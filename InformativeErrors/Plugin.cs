using BepInEx;
using HarmonyLib;
using System;

namespace InformativeErrors
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string MOD_GUID = "SpecialAPI.InformativeErrors";
        public const string MOD_NAME = "Informative Errors";
        public const string MOD_VERSION = "0.0.0";

        public void Awake()
        {
            var harmony = new Harmony(MOD_GUID);
            harmony.PatchAll();
        }
    }
}
