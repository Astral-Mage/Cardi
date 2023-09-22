using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{

    [Serializable]
    public enum StatTypes
    {
        //Atk, // attack chance
        //Pdf, // physical def
        //Mdf, // magic def
        //Con, // temp hp, basically
        //Spd, // attack speed
        //Crt, // crit dmg multiplier
        //Dmg, // base raw danage
        //Vit, // health
        //Eva, // evasion
        //Int, // magic damage boost
        //Prg, // progress
        //Sta, // stamina
        //StM, // stamina max
        //Kil, // kills
        //KiB, // boss kills
        //Lvl, // level
        //Gld, // gold
        //Sds, // stardust
        //Rom, // rooms cleared
        //Adr, // auto-delete rarity threshold
        //Dex, // phyiscal dmg boost
        //Cs1, // class 1
        //Sps, // species
        //Exp, // experience
        //Dff, // default floor
        //Crc, // crit chance
        //Ats, // attack spread modifier
        //Upg, // number of level-up upgrades available
        //Upe, // number of level-up upgrades equipped
        //Bly, // times successfully bullied a target
        //Sbm, // times submitted to bullying
        //Dwi, // duels won
        //Dlo, // duels lost
        //Pvr, // pvp rank
        //Foc, // focus stat
        //Bcc, // base crit chance



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
        Healing,
        DebuffAbsorb,
        Speed,
    }
}
