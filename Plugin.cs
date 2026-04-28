using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Utilities;

namespace RandomiserTOUM
{
    [BepInPlugin("chaos.randomiser", "Mira Randomiser", "1.0.1")]
    [BepInDependency("auavengers.tou.mira")]
    [BepInDependency("mira.api")]
    public class MiraRandomiser : BasePlugin, IMiraPlugin
    {
        public static Harmony HarmonyInstance;
        public string ID => "chaos.randomiser";
        public string Name => "Mira Randomiser";
        public string OptionsTitleText => "Randomiser";
        public ConfigFile GetConfigFile() => Config;

        public override void Load()
        {
            Log.LogInfo($"MiraRandomiser v1.0.1 loading...");

            HarmonyInstance = new Harmony("chaos.randomiser");
            HarmonyInstance.PatchAll();

            Log.LogInfo("MiraRandomiser Loaded.");
            ReactorCredits.Register<MiraRandomiser>(ReactorCredits.AlwaysShow);
        }
    }
}
