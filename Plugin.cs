using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace RandomiserTOUM
{
    [BepInPlugin("com.chaos.randomiser", "TOU Randomiser", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public static Harmony HarmonyInstance;
        public override void Load()
        {
            HarmonyInstance = new Harmony("com.chaos.randomiser");
            HarmonyInstance.PatchAll();
        }
    }
}
