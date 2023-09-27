using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    public enum StatPrefixes
    {
        [Description("Raging")]
        Strength,
        [Description("Bold")]
        Dexterity,
        [Description("Confident")]
        Constitution,
        [Description("Certain")]
        Intelligence,
        [Description("Optimistic")]
        Wisdom,
        [Description("Critical")]
        Perception, // crit dmg multiplier
        [Description("Powerful")]
        Libido,
        [Description("Tenacious")]
        Charisma, // health
        [Description("Desperate")]
        Intuition, // evasion
    }

    public enum StatSuffixes
    {
        [Description("Longing")]
        Strength,
        [Description("Resolve")]
        Dexterity,
        [Description("Perserverence")]
        Constitution,
        [Description("Belief")]
        Intelligence,
        [Description("Finality")]
        Wisdom,
        [Description("Elegy")]
        Perception, // crit dmg multiplier
        [Description("Hunger")]
        Libido, // health
        [Description("Cowardice")]
        Charisma, // evasion
        [Description("Inspiration")]
        Intuition,
    }
}
