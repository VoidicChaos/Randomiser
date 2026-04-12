using System;
using HarmonyLib;

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
            if (msg.StartsWith("/randomise") || msg.StartsWith("/randomize") || msg.StartsWith("/rand")) // added /rand (simpler) and /randomize (for american english)
            {
                var parts = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float? lower = null;
                float? upper = null;
                if (parts.Length >= 3 &&
                    float.TryParse(parts[1], out float l) &&
                    float.TryParse(parts[2], out float u))
                {
                    lower = l;
                    upper = u;
                }
                Randomiser.GenerateAndApply(lower, upper);
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                    PlayerControl.LocalPlayer,
                    lower.HasValue
                        ? $"<color=#87CEEB>[Random]</color> Applied with range {lower} to {upper}"
                        : "<color=#87CEEB>[Random]</color> Random config generated."
                );
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