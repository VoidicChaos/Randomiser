using System;
using HarmonyLib;
// test
namespace RandomiserTOUM.Patches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First + 100)]
        public static bool Prefix(ChatController __instance)
        {
            string msg = __instance.freeChatField.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(msg)) return true;

            if (msg.StartsWith("/randomise", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("/randomize", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("/rand", StringComparison.OrdinalIgnoreCase))
            {
                if (!AmongUsClient.Instance.AmHost)
                {
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                        PlayerControl.LocalPlayer,
                        "<color=#FF0000>[Randomiser]</color> Only the host can use this command."
                    );
                    ClearChat(__instance);
                    return false;
                }

                Randomiser.GenerateAndApply();
                ClearChat(__instance);
                return false;
            }

            return true;
        }

        private static void ClearChat(ChatController chat)
        {
            chat.freeChatField.Clear();
            chat.quickChatMenu.Clear();
            chat.quickChatField.Clear();
            chat.UpdateChatMode();
        }
    }
}
