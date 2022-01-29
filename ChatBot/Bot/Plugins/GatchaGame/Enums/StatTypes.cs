using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    [Serializable]
    public enum StatTypes
    {
        Atk, // attack chance
        Pdf, // physical def
        Mdf, // magic def
        Con, // temp hp, basically
        Spd, // attack speed
        Crt, // crit dmg multiplier
        Dmg, // base raw danage
        Vit, // health
        Eva, // evasion
        Int, // magic damage boost
        Prg, // progress
        Sta, // stamina
        StM, // stamina max
        Kil, // kills
        KiB, // boss kills
        Lvl, // level
        Gld, // gold
        Sds, // stardust
        Rom, // rooms cleared
        Adr, // auto-delete rarity threshold
        Dex, // phyiscal dmg boost
        Cs1, // class 1
        Sps, // species
        Exp, // experience
        Dff, // default floor
        Crc, // crit chance
        Ats, // attack spread modifier
        Upg, // number of level-up upgrades available
        Upe, // number of level-up upgrades equipped
        Bly, // times successfully bullied a target
        Sbm, // times submitted to bullying
        Dwi, // duels won
        Dlo, // duels lost
        Pvr, // pvp rank
        Foc, // focus stat
    }

    [Serializable]
    public enum StatPrefixes
    {
        [Description("Raging")]
        Atk,
        [Description("Bold")]
        Pdf,
        [Description("Confident")]
        Mdf,
        [Description("Certain")]
        Con,
        [Description("Optimistic")]
        Spd,
        [Description("Critical")]
        Crt, // crit dmg multiplier
        [Description("-")]
        Dmg,
        [Description("Tenacious")]
        Vit, // health
        [Description("Desperate")]
        Eva, // evasion
        [Description("Powerful")]
        Int,
        [Description("-")]
        Prg, // progress
        [Description("-")]
        Sta, // stamina
        [Description("-")]
        StM, // stamina max
        [Description("-")]
        Kil, // kills
        [Description("-")]
        KiB, // boss kills
        [Description("-")]
        Lvl, // level
        [Description("-")]
        Gld, // gold
        [Description("-")]
        Sds, // stardust
        [Description("-")]
        Rom, // rooms cleared
        [Description("-")]
        Adr, // auto-delete rarity threshold
        [Description("Nimble")]
        Dex,
        [Description("-")]
        Cs1, // class 1
        [Description("-")]
        Sps, // species
        [Description("-")]
        Exp, // experience
        [Description("-")]
        Dff, // default floor
        [Description("Focused")]
        Crc, // crit chance
        [Description("Determined")]
        Ats, // attack spread modifier
        Upg, // number of level-up upgrades available
        Upe, // number of level-up upgrades equipped
        Bly, // times successfully bullied a target
        Sbm, // times submitted to bullying
        Dwi, // duels won
        Dlo, // duels lost
        Pvr, // pvp rank
        Foc, // focus stat
    }

    [Serializable]
    public enum StatSuffixes
    {
        [Description("Longing")]
        Atk,
        [Description("Resolve")]
        Pdf,
        [Description("Perserverence")]
        Mdf,
        [Description("Belief")]
        Con,
        [Description("Finality")]
        Spd,
        [Description("Elegy")]
        Crt, // crit dmg multiplier
        Dmg,
        [Description("Hunger")]
        Vit, // health
        [Description("Cowardice")]
        Eva, // evasion
        [Description("Inspiration")]
        Int,
        Prg, // progress
        Sta, // stamina
        StM, // stamina max
        Kil, // kills
        KiB, // boss kills
        Lvl, // level
        Gld, // gold
        Sds, // stardust
        Rom, // rooms cleared
        Adr, // auto-delete rarity threshold
        [Description("Wit")]
        Dex,
        Cs1, // class 1
        Sps, // species
        Exp, // experience
        Dff, // default floor
        [Description("Devastation")]
        Crc, // crit chance
        [Description("Rampage")]
        Ats, // attack spread modifier
        Upg, // number of level-up upgrades available
        Upe, // number of level-up upgrades equipped
        Bly, // times successfully bullied a target
        Sbm, // times submitted to bullying
        Dwi, // duels won
        Dlo, // duels lost
        Pvr, // pvp rank
        Foc, // focus stat
    }
}
