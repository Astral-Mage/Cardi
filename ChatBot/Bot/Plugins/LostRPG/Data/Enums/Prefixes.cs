using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    [Serializable]
    public enum EquipmentPrefixes
    {
        [Description("Mighty")]
        Strength,
        [Description("Shocking")]
        Dexterity,
        [Description("Blazing")]
        Constitution,
        [Description("Longing")]
        Intelligence,
        [Description("Destroying")]
        Wisdom,
        [Description("Quick")]
        Perception,
        [Description("Slippery")]
        Libido,
        [Description("Quirky")]
        Charisma,
        [Description("Seeing")]
        Intuition,
    }

    [Serializable]
    public enum EquipmentSuffixes
    {
        [Description("of Strength")]
        Strength,
        [Description("of Haste")]
        Dexterity,
        [Description("of Resiliance")]
        Constitution,
        [Description("of the Mind")]
        Intelligence,
        [Description("of Wittiness")]
        Wisdom,
        [Description("of Rippling Death")]
        Perception,
        [Description("of Perversion")]
        Libido,
        [Description("of Smooth Talking")]
        Charisma,
        [Description("of Futuresight")]
        Intuition,
    }
}
