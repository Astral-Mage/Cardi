using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    [Serializable]
    public enum WeaponTypes
    {
        [Description("Spear")]
        Spear,

        [Description("Javelin")]
        Javelin,

        [Description("Sword")]
        Sword,

        [Description("Staff")]
        Staff,

        [Description("Magic Book")]
        MagicBook,

        [Description("Orb")]
        Orb,

        [Description("Knife")]
        Knife,

        [Description("Rifle")]
        Rifle,

        [Description("Pistol")]
        Pistol,

        [Description("Launcher")]
        Launcher,

        [Description("Mace")]
        Mace,
    }

    [Serializable]
    public enum ArmorTypes
    {
        [Description("Tunic")]
        Tunic,
        [Description("Brigadine")]
        Brigadine,
        [Description("Shield")]
        Shield,
        [Description("Armguard")]
        ArmGuard,
    }
}
