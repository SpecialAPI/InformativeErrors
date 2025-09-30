using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace InformativeErrorsPatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];

        public static void Patch(AssemblyDefinition assembly)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("InformativeErrors");

            var module = assembly.MainModule;
            var typeDef = module.GetType("EffectInfo");
            var str = module.ImportReference(typeof(string));

            var method = new MethodDefinition("ToString", MethodAttributes.Public, str);
            var body = method.Body;
            var il = body.GetILProcessor();

            var effect = typeDef.FindField("effect");
            var entry = typeDef.FindField("entryVariable");
            var target = typeDef.FindField("targets");

            var effectSOType = effect.FieldType.Resolve();
            var objectType = effectSOType.BaseType.Resolve().BaseType.Resolve();
            var objectName = module.ImportReference(objectType.FindMethod("get_name"));
            var objectEq = module.ImportReference(objectType.FindMethod("op_Equality"));

            var concat = module.ImportReference(typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]));

            var objType = module.ImportReference(typeof(object));
            var getType = module.ImportReference(objType.Resolve().Methods.First(x => x.Is(typeof(object).GetMethod("GetType"))));

            var typeName = module.ImportReference(typeof(Type).GetMethod("get_Name"));

            Instruction eqInstr;
            Instruction retInstr;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, effect);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Callvirt, typeName);
            
            il.Emit(OpCodes.Ldstr, ", ");
            il.Emit(OpCodes.Call, concat);
            
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, entry);
            il.Emit(OpCodes.Call, module.ImportReference(typeof(Patcher).GetMethod("IntToString")));
            il.Emit(OpCodes.Call, concat);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, target);
            il.Emit(OpCodes.Ldnull);
            il.Append(eqInstr = il.Create(OpCodes.Call, objectEq));

            il.Emit(OpCodes.Ldstr, ", ");
            il.Emit(OpCodes.Call, concat);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, target);
            il.Emit(OpCodes.Callvirt, objectName);
            il.Emit(OpCodes.Call, concat);

            il.Append(retInstr = il.Create(OpCodes.Ret));
            il.InsertAfter(eqInstr, il.Create(OpCodes.Brtrue, retInstr));

            typeDef.Methods.Add(method);
        }

        public static string IntToString(int v)
        {
            return v.ToString();
        }
    }
}
