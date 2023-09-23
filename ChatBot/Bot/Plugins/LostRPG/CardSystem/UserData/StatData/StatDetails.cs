using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{

    [Serializable]
    public enum StatTypes
    {
        // tracking stats
        [Description("life")]
        Life,
        CurrentLife,
        ShieldHealth,
        Level,
        Experience,
        MaxStamina,
        CurrentStamina,

        // currency stats
        Kills,
        Gold,
        Stardust,

        // primary stats

        //physical
        [Description("str")]
        Strength,
        [Description("dex")]
        Dexterity,
        [Description("con")]
        Constitution,

        //mental
        [Description("int")]
        Intelligence,
        [Description("wis")]
        Wisdom,
        [Description("per")]
        Perception,

        // social
        [Description("lib")]
        Libido,
        [Description("cha")]
        Charisma,
        [Description("tui")]
        Intuition,

        // secondary stats



        /// other info
        Damage,
        MaxDamage,
        Healing,
        MaxHealing,
        DebuffAbsorb,
        Speed,
    }
}
