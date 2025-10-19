using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InformativeErrorsPatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];

        public static ManualLogSource Log = Logger.CreateLogSource("Informative Errors Patcher");

        public static bool TryLoadPluginModule(out ModuleDefinition mod)
        {
            var patcherDllPath = typeof(Patcher).Assembly.Location;
            var patchersPath = Path.GetDirectoryName(patcherDllPath);
            var wmitfRootPath = Path.GetDirectoryName(patchersPath);
            var pluginsPath = Path.Combine(wmitfRootPath, "plugins");
            var pluginDllPath = Path.Combine(pluginsPath, "InformativeErrors.dll");

            try
            {
                mod = ModuleDefinition.ReadModule(pluginDllPath);
                return true;
            }
            catch (Exception ex)
            {
                mod = null;
                Log.LogError($"Failed to read plugin dll: {ex}");
                return false;
            }
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            if (!TryLoadPluginModule(out var pluginModule))
                return;

            var module = assembly.MainModule;
            var str = module.ImportReference(typeof(string));

            var basegameToStringExtensions = pluginModule.GetType("InformativeErrors.BasegameToStringExtensions");
            var classesToAddToStringTo = new Dictionary<string, string>()
            {
                ["EffectInfo"] = "EffectInfoToString"
            };

            foreach(var kvp in classesToAddToStringTo)
            {
                var type = module.GetType(kvp.Key);

                if(type == null)
                {
                    Log.LogError($"No type with the name {kvp.Key} exists.");
                    continue;
                }

                if(type.FindMethod("ToString") != null)
                {
                    Log.LogError($"{type.FullName} already has a ToString method.");
                    continue;
                }

                var toStringExtMethod = basegameToStringExtensions.FindMethod(kvp.Value);
                if(toStringExtMethod == null)
                {
                    Log.LogError($"{basegameToStringExtensions.Name} doesn't have a method with the name {kvp.Value}");
                    continue;
                }

                var method = new MethodDefinition("ToString", MethodAttributes.Public, str);
                var body = method.Body;
                var il = body.GetILProcessor();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, toStringExtMethod);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(method);
            }
        }
    }
}
