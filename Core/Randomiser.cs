using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Roles;
using UnityEngine;

namespace RandomiserTOUM
{
    public static class Randomiser
    {
        public static void GenerateAndApply(float? lower = null, float? upper = null)
        {
            ApplyVanilla();
            ApplyMira();
            ApplyRoles(lower, upper);
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
                foreach (var option in group.Children)
                    yield return option;
        }

        private static void ApplyMira() // apply the randomised setings to ToUM
        {
            foreach (var option in GetAllModdedOptions())
            {
                try
                {
                    if (option is ModdedNumberOption numOpt)
                    {
                        numOpt.SetValue(Rand(numOpt.Min, numOpt.Max));
                    }
                    else if (option is ModdedToggleOption togOpt)
                    {
                        togOpt.SetValue(RandInt(0, 1) == 0);
                    }
                    else if (option is ModdedEnumOption enumOpt)
                    {
                        enumOpt.SetValue(RandInt(0, enumOpt.Values.Length - 1));
                    }
                }
                catch { }
            }
        }

        private static void ApplyRoles(float? lower = null, float? upper = null) // randomise role chances in the limits
        {
            foreach (var role in CustomRoleManager.CustomRoleBehaviours)
            {
                try
                {
                    if (role is not ICustomRole customRole) continue;
                    var config = customRole.Configuration;
                    if (!config.CanModifyChance) continue;
                    int min = lower.HasValue ? (int)lower.Value : 0; // default lower if no value provided
                    int max = upper.HasValue ? (int)upper.Value : 70; // default higher if no value provided (no 100 because a guarenteed role is not fun)
                    if (min > max)
                        (min, max) = (max, min);
                    customRole.SetChance(RandInt(min, max)); // apply the chance within the range from earlier
                    int maxCount = config.MaxRoleCount; // role count limits
                    if (maxCount > 0)
                        customRole.SetCount(RandInt(1, maxCount));
                }
                catch { }
            }
        }

        private static float Rand(float min, float max) => UnityEngine.Random.Range(min, max); // random value generator
        private static int RandInt(int min, int max) => UnityEngine.Random.Range(min, max + 1); // random value generator (whole number)
    }
}
