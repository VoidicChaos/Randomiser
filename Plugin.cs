using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;

namespace RandomiserTOUM
{
    [BepInPlugin("chaos.randomiser", "Mira Randomiser", "1.0.1")]
    [BepInDependency("auavengers.tou.mira")]
    [BepInDependency("mira.api")]
    public class Plugin : BasePlugin, IMiraPlugin
    {
        public static Harmony HarmonyInstance;
        public string ID => "com.colin.randomiser";
        public string Name => "Mira Randomiser";
        public string OptionsTitleText => "Mira Randomiser";
        public ConfigFile GetConfigFile() => Config;
        public override void Load()
        {
            HarmonyInstance = new Harmony("chaos.randomiser");
            HarmonyInstance.PatchAll();
        }
    }
}
