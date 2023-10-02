using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects;
using ChatBot.Core;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.EquipmentSystem
{
    public static class EquipmentController
    {
        public static Socket GenerateSocketItem(SocketTypes socketType, DamageTypes dtype = DamageTypes.None)
        {
            Socket newItem = new Socket();

            switch(socketType)
            {
                case SocketTypes.Weapon:
                    {
                        newItem = GenereateWeapon(dtype);
                    }
                    break;
                case SocketTypes.Armor:
                    {
                        newItem = GenerateArmor();
                    }
                    break;
                case SocketTypes.Passive:
                    {
                        newItem = GeneratePassive();
                    }
                    break;
                default:
                    throw new System.Exception("Bad SocketType");
            }

            return newItem;
        }

        static Socket GenereateWeapon(DamageTypes dtype = DamageTypes.None)
        {
            WeaponSocket newItem = new WeaponSocket();

            newItem.SocketType = SocketTypes.Weapon;
            newItem.WeaponType = WeaponTypes.Launcher;
            newItem.Stats.AddStat(StatTypes.Damage, 50);
            newItem.DamageType = (dtype == DamageTypes.None) ? (DamageTypes)RNG.Seed.Next(0, 3) : dtype;


            List<StatTypes> WeaponStats = new List<StatTypes>() { 
                StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution,
                StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception,
                StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition
            };

            int index = RNG.Seed.Next(0, WeaponStats.Count);

            newItem.Stats.AddStat(WeaponStats[index], 5);
            newItem.Stats.AddStat(WeaponStats[index], 5);

            return newItem;
        }

        static Socket GenerateArmor()
        {
            ArmorSocket newItem = new ArmorSocket();

            newItem.SocketType = SocketTypes.Armor;
            newItem.Stats.AddStat(StatTypes.Constitution, 20);
            newItem.GearType = ArmorTypes.Shield;


            List<StatTypes> ArmorStats = new List<StatTypes>() {
                StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution,
                StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception,
                StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition
            };

            int index = RNG.Seed.Next(0, ArmorStats.Count);

            newItem.Stats.AddStat(ArmorStats[index], 5);

            return newItem;
        }

        static Socket GeneratePassive()
        {
            PassiveSocket newItem = new PassiveSocket();

            newItem.SocketType = SocketTypes.Passive;

            List<StatTypes> ArmorStats = new List<StatTypes>() {
                StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution,
                StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception,
                StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition
            };

            int index = RNG.Seed.Next(0, ArmorStats.Count);
            int inde2 = RNG.Seed.Next(0, ArmorStats.Count);

            newItem.Stats.AddStat(ArmorStats[index], 15);
            newItem.Stats.AddStat(ArmorStats[inde2], 10);


            return newItem;
        }
    }
}
