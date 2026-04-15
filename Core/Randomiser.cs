using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Roles;
using UnityEngine;

namespace RandomiserTOUM
{
    public static class Randomiser
    {
        private static readonly int RoleListOptionCount = 25;

        private static readonly HashSet<string> ModifierGroups = new()
        {
            "Alliance Modifiers", "Crewmate Modifiers", "Hider Modifiers",
            "Seeker Modifiers", "Impostor Modifiers", "Neutral Modifiers",
            "Universal Modifiers"
        };

        private static readonly HashSet<string> GameSettingsGroups = new()
        {
            "Assassin Options", "Game Mechanics", "End Game Timer", "General",
            "Host-Specific Options", "Postmortem Options", "Task Tracking",
            "Vanilla Tweaks"
        };

        private static readonly HashSet<string> MapSettingsGroups = new()
        {
            "Advanced Sabotages", "Advanced Utilities", "Better Airship",
            "Better Fungle", "Better Level Impostor", "Better Mira HQ",
            "Better Polus", "Better Skeld", "Better Submerged",
            "Global Better Maps", "Randomized Door Mode", "Random Map Choice"
        };

        private static RandomiserOptions Options => OptionGroupSingleton<RandomiserOptions>.Instance;

        private static RandomiserMIRAOptions MIRAOptions => OptionGroupSingleton<RandomiserMIRAOptions>.Instance;

        private static RandomiserLimitOptions Limits => OptionGroupSingleton<RandomiserLimitOptions>.Instance;

        public static void GenerateAndApply()
        {
            HudManager.Instance.StartCoroutine(GenerateAndApplyCoroutine().WrapToIl2Cpp());
        }

        private static IEnumerator GenerateAndApplyCoroutine()
        {
            var chat = DestroyableSingleton<HudManager>.Instance.Chat;
            var player = PlayerControl.LocalPlayer;

            void Notify(string msg) => chat.AddChat(player,
                $"<color=#87CEEB>[Randomiser]</color> {msg}");

            if (Options.RandomiseVanilla)
            {
                Notify("Randomising Vanilla Settings...");
                yield return new WaitForSeconds(0.5f);
                ApplyVanilla();
            }

            if (Options.RandomiseMira)
            {
                Notify("Randomising Mira Settings...");
                yield return new WaitForSeconds(0.5f);
                ApplyMira();
            }

            if (Options.RandomiseMira && (MIRAOptions.RandomiseRoleChances))
            {
                Notify("Randomising Role Chances...");
                yield return new WaitForSeconds(0.5f);
                ApplyRoles();
            }

            if (Options.RandomiseMira && MIRAOptions.RandomiseRoleSlots)
            {
                Notify("Randomising Buckets...");
                yield return new WaitForSeconds(0.5f);
                ApplyRoleSlots();
            }

            Notify("Syncing to Players...");
            yield return new WaitForSeconds(0.5f);
            SyncToAll();

            Notify("<color=#2ECC71>Settings Randomised.</color>");
        }

        private static void ApplyVanilla() // randomise vanilla settings
        {
            var opt = GameOptionsManager.Instance.CurrentGameOptions;

            opt.SetFloat(FloatOptionNames.PlayerSpeedMod, Rand(0.5f, 3f));
            opt.SetFloat(FloatOptionNames.CrewLightMod, Rand(0.25f, 5f));
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, Rand(0.25f, 5f));
            opt.SetFloat(FloatOptionNames.KillCooldown, Rand(5f, 60f));

            opt.SetInt(Int32OptionNames.NumCommonTasks, RandInt(0, 2));
            opt.SetInt(Int32OptionNames.NumLongTasks, RandInt(0, 3));
            opt.SetInt(Int32OptionNames.NumShortTasks, RandInt(0, 5));
        }

        private static IEnumerable<IModdedOption> GetAllModdedOptions() // fetch Mira options
        {
            var groupsField = typeof(ModdedOptionsManager)
                .GetField("Groups", BindingFlags.NonPublic | BindingFlags.Static);

            if (groupsField?.GetValue(null) is not List<AbstractOptionGroup> groups)
                yield break;

            foreach (var group in groups)
            {
                if (group.GroupName == "Role Settings") continue;
                if (group.GroupName == "ToUM Randomiser") continue;
                if (group is RandomiserOptions or RandomiserMIRAOptions or RandomiserLimitOptions) continue;
                if (group.GroupName.StartsWith("Randomiser:", StringComparison.OrdinalIgnoreCase)) continue;

                if (ModifierGroups.Contains(group.GroupName))
                {
                    if (MIRAOptions.RandomiseModifierChances)
                        foreach (var o in group.Children) yield return o;
                    continue;
                }

                if (GameSettingsGroups.Contains(group.GroupName))
                {
                    if (MIRAOptions.RandomiseGameSettings)
                        foreach (var o in group.Children) yield return o;
                    continue;
                }

                if (MapSettingsGroups.Contains(group.GroupName))
                {
                    if (MIRAOptions.RandomiseMapSettings)
                        foreach (var o in group.Children) yield return o;
                    continue;
                }

                foreach (var option in group.Children)
                    yield return option;
            }
        }

        private static void ApplyMira() // apply the randomised setings to ToUM
        {
            foreach (var option in GetAllModdedOptions())
            {
                try
                {
                    if (option is ModdedNumberOption numOpt) {
                        numOpt.SetValue(Rand(numOpt.Min, numOpt.Max));
                    }
                    else if (option is ModdedToggleOption togOpt) {
                        togOpt.SetValue(RandInt(0, 1) == 0);
                    }
                    else if (option is ModdedEnumOption enumOpt) {
                        enumOpt.SetValue(RandInt(0, enumOpt.Values.Length - 1));
                    }
                }
                catch { }
            }
        }

        private static void ApplyRoleSlots() // randomise the role slots
        {
            try
            {
                var touAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "TownOfUsMira");
                if (touAssembly == null) return;

                var roleOptionsType = touAssembly.GetType("TownOfUs.Options.RoleOptions");
                if (roleOptionsType == null) return;

                var singletonType = typeof(OptionGroupSingleton<>).MakeGenericType(roleOptionsType);
                var instanceProp = singletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = instanceProp?.GetValue(null);
                if (instance == null) return;

                for (int i = 1; i <= 15; i++)
                {
                    try
                    {
                        var slotProp = roleOptionsType.GetProperty($"Slot{i}", BindingFlags.Public | BindingFlags.Instance);
                        if (slotProp == null) continue;

                        var configEntry = slotProp.GetValue(instance);
                        if (configEntry == null) continue;

                        var valueProp = configEntry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                        if (valueProp == null) continue;

                        var enumType = valueProp.PropertyType;
                        var randomValue = Enum.ToObject(enumType, RandInt(0, RoleListOptionCount - 1));
                        valueProp.SetValue(configEntry, randomValue);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static (int minSteps, int maxSteps) GetChanceSteps(bool limitsEnabled, float min, float max) // role chance rounding
        {
            if (MIRAOptions.RoundRoleChances) {
                int minChance = limitsEnabled ? (int)(Math.Round(min / 10f) * 10) : 0;
                int maxChance = limitsEnabled ? (int)(Math.Round(max / 10f) * 10) : 100;
                if (minChance > maxChance) minChance = maxChance;
                return (minChance / 10, maxChance / 10);
            } else
            {
                return ((int)min, (int)max);
            }
        }

        private static void ApplyRoles() // randomise role chances/count in the limits
        {

            var (crewMinSteps, crewMaxSteps) = GetChanceSteps(Limits.CrewChanceLimitsEnabled, Limits.MinCrewChance, Limits.MaxCrewChance);
            var (neutMinSteps, neutMaxSteps) = GetChanceSteps(Limits.NeutralChanceLimitsEnabled, Limits.MinNeutralChance, Limits.MaxNeutralChance);
            var (impMinSteps, impMaxSteps) = GetChanceSteps(Limits.ImpostorChanceLimitsEnabled, Limits.MinImpostorChance, Limits.MaxImpostorChance);

            foreach (var role in CustomRoleManager.CustomRoleBehaviours)
            {
                try
                {
                    if (role is not ICustomRole customRole) continue;
                    if (customRole.Configuration.HideSettings) continue;

                    if (MIRAOptions.RandomiseRoleChances && customRole.Configuration.CanModifyChance)
                    {
                        int minSteps, maxSteps;
                        int minCount, maxCount;
                        switch (customRole.Team)
                        {
                            case ModdedRoleTeams.Impostor:
                                (minSteps, maxSteps) = (impMinSteps, impMaxSteps);
                                minCount = (int)Limits.MinImpostorCount;
                                maxCount = (int)Limits.MaxImpostorCount;
                                break;
                            case ModdedRoleTeams.Custom:
                                (minSteps, maxSteps) = (neutMinSteps, neutMaxSteps);
                                minCount = (int)Limits.MinNeutralCount;
                                maxCount = (int)Limits.MaxNeutralCount;
                                break;
                            default:
                                (minSteps, maxSteps) = (crewMinSteps, crewMaxSteps);
                                minCount = (int)Limits.MinCrewCount;
                                maxCount = (int)Limits.MaxCrewCount;
                                break;
                        }

                        if (minSteps > maxSteps) (minSteps, maxSteps) = (maxSteps, minSteps);

                        if (minCount > maxCount) (minCount, maxCount) = (maxCount, minCount);

                        customRole.SetChance(RandInt(minSteps, maxSteps) * 10);

                        customRole.SetCount(RandInt(minCount, maxCount));
                    }

                }
                catch { }
            }
        }

        private static void SyncToAll()
        {
            try
            {
                var syncAllOptions = typeof(ModdedOptionsManager)
                    .GetMethod("SyncAllOptions", BindingFlags.NonPublic | BindingFlags.Static);
                syncAllOptions?.Invoke(null, new object[] { -1 });
            }
            catch { }

            try
            {
                var syncAllRoleSettings = typeof(CustomRoleManager)
                    .GetMethod("SyncAllRoleSettings", BindingFlags.NonPublic | BindingFlags.Static);
                syncAllRoleSettings?.Invoke(null, new object[] { -1 });
            }
            catch { }

            try
            {
                GameManager.Instance.LogicOptions.SyncOptions();
            }
            catch { }
        }

        private static float Rand(float min, float max) => UnityEngine.Random.Range(min, max); // random value generator

        private static int RandInt(int min, int max) => UnityEngine.Random.Range(min, max + 1); // random value generator (whole number)
    }
}
