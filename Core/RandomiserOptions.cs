using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace RandomiserTOUM
{
    public sealed class RandomiserOptions : AbstractOptionGroup
    {
        public override string GroupName => "Main Randomiser Settings";
        public override uint GroupPriority => 0; // have it be the first page



        [ModdedToggleOption("Randomise Vanilla Settings")]
        public bool RandomiseVanilla { get; set; } = true;



        [ModdedToggleOption("Randomise Mira Settings")]
        public bool RandomiseMira { get; set; } = true;
    }

    public sealed class RandomiserMIRAOptions : AbstractOptionGroup
    {
        public override string GroupName => "Randomiser Options";
        public override uint GroupPriority => 1; // have it be the second page
        public override Func<bool> GroupVisible => () =>
            OptionGroupSingleton<RandomiserOptions>.Instance.RandomiseMira;



        [ModdedToggleOption("Randomise Role Chances")]
        public bool RandomiseRoleChances { get; set; } = true;



        [ModdedToggleOption("Round Role Chances")]
        public bool RoundRoleChances { get; set; } = true;



        [ModdedToggleOption("Randomise Role Slots")]
        public bool RandomiseRoleSlots { get; set; } = true;



        [ModdedToggleOption("Randomise Modifier Chances")]
        public bool RandomiseModifierChances { get; set; } = true;



        [ModdedToggleOption("Randomise Game Settings")]
        public bool RandomiseGameSettings { get; set; } = true;



        [ModdedToggleOption("Randomise Map Settings")]
        public bool RandomiseMapSettings { get; set; } = true;
    }

    public sealed class RandomiserLimitOptions : AbstractOptionGroup
    {
        public override string GroupName => "Limits";
        public override uint GroupPriority => 2; // have it be the third page
        public override Func<bool> GroupVisible => () =>
            OptionGroupSingleton<RandomiserOptions>.Instance.RandomiseMira;

        // Crewmate Roles (Chance)
        [ModdedToggleOption("Crewmate Limiting Enabled")]
        public bool CrewChanceLimitsEnabled { get; set; } = true;



        [ModdedNumberOption("Min Crewmate Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MinCrewChance { get; set; } = 0f;



        [ModdedNumberOption("Max Crewmate Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MaxCrewChance { get; set; } = 100f;



        // Neutral Role (Chance)
        [ModdedToggleOption("Neutral Limiting Enabled")]
        public bool NeutralChanceLimitsEnabled { get; set; } = true;



        [ModdedNumberOption("Min Neutral Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MinNeutralChance { get; set; } = 0f;



        [ModdedNumberOption("Max Neutral Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MaxNeutralChance { get; set; } = 100f;



        // Impostor Roles (Chance)
        [ModdedToggleOption("Impostor Limiting Enabled")]
        public bool ImpostorChanceLimitsEnabled { get; set; } = true;



        [ModdedNumberOption("Min Impostor Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MinImpostorChance { get; set; } = 0f;



        [ModdedNumberOption("Max Impostor Role Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
        public float MaxImpostorChance { get; set; } = 100f;
    }
}
