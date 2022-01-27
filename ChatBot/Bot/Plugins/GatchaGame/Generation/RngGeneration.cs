using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.GatchaGame.Generation
{
    internal static class RngGeneration
    {
        public readonly static Random Rng = new Random();

        public const int BaseGatchaItemMaxClip = 42;

        public const double XPMULT = .00556;

        public static List<EnemyCard> GenerateEnemyCluster(FloorCard fc, Cards.PlayerCard pc, int maxEnemies = 3)
        {
            List<EnemyCard> toReturn = new List<EnemyCard>();

            int numEnemies = Rng.Next(2, 1 + maxEnemies);

            for (int i = 0; i < numEnemies; i++)
            {
                toReturn.Add(GenerateEnemy(fc, pc));
            }

            return toReturn;
        }

        public static EnemyCard GenerateEnemy(FloorCard fc, Cards.PlayerCard pc)
        {
            EnemyCard ec = new EnemyCard();
            GenerateEnemyStats(ec, fc.floor, CardTypes.EnemyCard);
            ec.CurrentVitality = ec.GetStat(StatTypes.Vit);
            ec.Status = CharacterStatusTypes.Alive;

            // NYI
            GenerateEnemyLoot(fc, pc, ec);
            GenerateEnemyEquipment(fc, pc, ec);
            //

            List<string> EnemyNames = new List<string>()
            {
                "Skeleton",
                "Abberation",
                "Shade",
                "Gloop",
                "Slu Kathras",
                "Demon",
                "Sinful",
                "Mercco",

            };

            ec.DisplayName = EnemyNames[Rng.Next(0, EnemyNames.Count)];
            ec.Name = ec.DisplayName;

            return ec;
        }

        public static void GenerateEnemyEquipment(FloorCard fc, Cards.PlayerCard pc, EnemyCard ec)
        {

        }

        public static void GenerateEnemyLoot(FloorCard fc, Cards.PlayerCard pc, EnemyCard ec)
        {
            ec.AddStat(StatTypes.Gld, Convert.ToInt32(Rng.Next(1, 5) * (1 + (.2 * fc.floor))), false, false, false);

            if (Rng.Next(4) == 0)
                ec.AddStat(StatTypes.Sds, 1, false, false, false);
            else
                ec.AddStat(StatTypes.Sds, 0, false, false, false);
        }

        public static List<StatTypes> GetAllFocusableStats()
        {
            List<StatTypes> toSend = new List<StatTypes>()
            {
                StatTypes.Vit,
                StatTypes.Atk,
                StatTypes.Dmg,
                StatTypes.Pdf,
                StatTypes.Mdf,
                StatTypes.Dex,
                StatTypes.Int,
                StatTypes.Spd,
                StatTypes.Con,
                StatTypes.Eva,
                StatTypes.Crc,
                StatTypes.Crt,
                StatTypes.Ats,
            };

            return toSend;
        }

        public static StatTypes GetRandomFocusableStat()
        {
            return GetAllFocusableStats()[Rng.Next(0, GetAllFocusableStats().Count-1)];
        }

        public static void GenerateNewCharacterStats(Cards.PlayerCard pc)
        {
            BaseStats baseStats = new BaseStats();

            // random
            baseStats.AddStat(StatTypes.Vit, Rng.Next(125, 157));

            baseStats.AddStat(StatTypes.Atk, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Dmg, Rng.Next(8, 11));

            baseStats.AddStat(StatTypes.Mdf, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Pdf, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Con, Rng.Next(1, 3));

            baseStats.AddStat(StatTypes.Spd, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Crt, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Int, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Dex, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Crc, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Ats, Rng.Next(1, 2));
            baseStats.AddStat(StatTypes.Eva, Rng.Next(1, 3));


            // static
            baseStats.AddStat(StatTypes.Gld, 0);
            baseStats.AddStat(StatTypes.Sds, 70);
            baseStats.AddStat(StatTypes.Lvl, 1);
            baseStats.AddStat(StatTypes.Cs1, 0);
            baseStats.AddStat(StatTypes.Sps, 0);
            baseStats.AddStat(StatTypes.Exp, 0);


            // player-specific
            baseStats.AddStat(StatTypes.Sta, Convert.ToInt32(new TimeSpan(3, 0, 0).TotalSeconds));
            baseStats.AddStat(StatTypes.StM, Convert.ToInt32(new TimeSpan(4, 30, 0).TotalSeconds));
            baseStats.AddStat(StatTypes.Prg, 0);
            baseStats.AddStat(StatTypes.KiB, 0);
            baseStats.AddStat(StatTypes.Kil, 0);
            baseStats.AddStat(StatTypes.Rom, 0);
            baseStats.AddStat(StatTypes.Adr, 0);
            baseStats.AddStat(StatTypes.Dff, 0);
            baseStats.AddStat(StatTypes.Bly, 0);
            baseStats.AddStat(StatTypes.Sbm, 0);
            baseStats.AddStat(StatTypes.Dwi, 0);
            baseStats.AddStat(StatTypes.Dlo, 0);
            baseStats.AddStat(StatTypes.Foc, (int)GetRandomFocusableStat());


            // set
            pc.SetStats(baseStats);

            // set other vars
            pc.MaxInventory = 12;
        }

        public static void GenerateEnemyStats(Cards.EnemyCard ec, int baseLevel, CardTypes type)
        {
            BaseStats baseStats = new BaseStats();
            // random
            baseStats.AddStat(StatTypes.Atk, Convert.ToInt32(Math.Floor((double)Rng.Next(3, 6) * baseLevel)));
            baseStats.AddStat(StatTypes.Con, Convert.ToInt32(Math.Floor((double)Rng.Next(1, 4) * baseLevel)));
            baseStats.AddStat(StatTypes.Spd, Convert.ToInt32(Math.Floor((double)Rng.Next(1, 5) * baseLevel)));
            baseStats.AddStat(StatTypes.Crt, Convert.ToInt32(Math.Floor((double)Rng.Next(1, 5) * baseLevel)));
            baseStats.AddStat(StatTypes.Mdf, Convert.ToInt32(Math.Floor(Rng.Next(10, 15) + (double)Rng.Next(1, 5) * baseLevel)));
            baseStats.AddStat(StatTypes.Pdf, Convert.ToInt32(Math.Floor(Rng.Next(10, 15) + (double)Rng.Next(1, 5) * baseLevel)));
            baseStats.AddStat(StatTypes.Vit, Convert.ToInt32(Math.Floor(Rng.Next(50, 65) + (double)Rng.Next(6, 10) * baseLevel)));
            baseStats.AddStat(StatTypes.Int, Convert.ToInt32(Math.Floor((double)Rng.Next(2, 6) * baseLevel)));
            baseStats.AddStat(StatTypes.Dex, Convert.ToInt32(Math.Floor((double)Rng.Next(1, 5) * baseLevel)));
            baseStats.AddStat(StatTypes.Cs1, 0);
            baseStats.AddStat(StatTypes.Sps, 0);
            baseStats.AddStat(StatTypes.Exp, Rng.Next(1, 3) * baseLevel);

            // static
            baseStats.AddStat(StatTypes.Eva, Convert.ToInt32(Math.Floor((double)Rng.Next(1, 4) * baseLevel)));
            baseStats.AddStat(StatTypes.Dmg, Convert.ToInt32(Math.Floor(Rng.Next(2, 6) + ((double)Rng.Next(3, 6) * baseLevel))));

            baseStats.AddStat(StatTypes.Lvl, baseLevel);

            // set
            ec.SetStats(baseStats);
        }

        public static bool TryGetCard(string user, out Cards.PlayerCard card)
        {
            card = new Cards.PlayerCard();

            try
            {
                card = Data.DataDb.GetCard(user);
                DateTime now = DateTime.Now;

                // stamina update check on every card get
                if (card.LastStaUpdate + TimeSpan.FromSeconds(10) <= now)
                {
                    // it's been at least 1s so update stamina
                    int curSta = card.GetStat(StatTypes.Sta);
                    int maxSta = card.GetStat(StatTypes.StM);

                    int secondsElapsed = Convert.ToInt32((now - card.LastStaUpdate).TotalSeconds);
                    curSta += secondsElapsed;
                    if (curSta > maxSta)
                        curSta = maxSta;

                    card.LastStaUpdate = now;
                    card.SetStat(StatTypes.Sta, curSta);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Socket GenerateRandomPassive(out int valRolled, int rarityOverride = -1)
        {
            PassiveSocket toReturn = new PassiveSocket();
            int roll = Rng.Next(1, BaseGatchaItemMaxClip);
            valRolled = roll;

            int rarity = 1;
            if (valRolled > 30)
            {
                rarity = 1 + (int)Math.Floor(.1 * Math.Pow((-30 + valRolled), 1.5));
            }

            if (rarityOverride > 0) rarity = rarityOverride;

            toReturn.SocketRarity = 0;
            toReturn.SocketDescription = "";

            List<StatTypes> AvailableStats = new List<StatTypes>
            {
                StatTypes.Con,
                StatTypes.Spd,
                StatTypes.Atk,
                StatTypes.Dex,
                StatTypes.Int,
                StatTypes.Vit,
                StatTypes.Mdf,
                StatTypes.Pdf,
                StatTypes.Crc,
                StatTypes.Ats,
                StatTypes.Atk,
                StatTypes.Crt
            };
            for (int x = 0; x < 2; x++)
            {
                int lv = 15 + (x * 10);
                int hv = 20 + (x * 12);

                var theStat = AvailableStats[Rng.Next(AvailableStats.Count)];
                AvailableStats.Remove(theStat);
                int value = Rng.Next(lv, hv);
                if (toReturn.StatModifiers.ContainsKey(theStat))
                {
                    toReturn.StatModifiers[theStat] = toReturn.StatModifiers[theStat] + value;
                }
                else
                {
                    toReturn.StatModifiers.Add(theStat, value);
                }
            }

            for (int x = 0; x < rarity; x++)
            {
                toReturn.LevelUp();
            }

            return toReturn;
        }

        public static Socket GenerateRandomEquipment(out int valRolled, int rarityOverride = -1)
        {
            return GenerateRandomEquipment(out valRolled, (EquipmentTypes)Rng.Next(0, 1), rarityOverride);
        }

        public static Socket GenerateRandomEquipment(out int valRolled, EquipmentTypes typetouse, int rarityOverride = -1)
        {
            EquipmentSocket toReturn = ((typetouse == EquipmentTypes.Weapon) ? (EquipmentSocket)new WeaponSocket() : new ArmorSocket());
            int roll = Rng.Next(1, BaseGatchaItemMaxClip);
            valRolled = roll;
            int rarity = 1;
            if (valRolled > 30)
            {
                rarity = 1 + (int)Math.Floor(.1 * Math.Pow((-30 + valRolled), 1.5));
            }

            if (rarityOverride > 0) rarity = rarityOverride;

            toReturn.SocketRarity = 0;
            toReturn.SocketDescription = "";
            toReturn.Prefix = EquipmentPrefixes.None;
            toReturn.Suffix = EquipmentSuffixes.None;

            int idk = Rng.Next(0, 10);
            if (idk < rarity)
            {
                toReturn.Prefix = (EquipmentPrefixes)Rng.Next(1, Enum.GetValues(typeof(EquipmentPrefixes)).Length);
            }
            idk = Rng.Next(0, 10);
            if (idk < rarity)
            {
                toReturn.Suffix = (EquipmentSuffixes)Rng.Next(1, Enum.GetValues(typeof(EquipmentSuffixes)).Length);
            }

            List<StatTypes> AvailableStats = new List<StatTypes>();

            switch (toReturn.ItemType)
            {
                case EquipmentTypes.Weapon:
                    {
                        WeaponSocket ws = (WeaponSocket)toReturn;
                        ws.DamageType = DamageTypes.Physical;

                        if (ws.WeaponType == WeaponTypes.MagicBook || ws.WeaponType == WeaponTypes.Staff)
                        {
                            ws.DamageType = (DamageTypes)Rng.Next(1, Enum.GetNames(typeof(DamageTypes)).Length - 1);
                        }

                        if (valRolled > BaseGatchaItemMaxClip - 5)
                        {
                            if (ws.WeaponType == WeaponTypes.MagicBook || ws.WeaponType == WeaponTypes.Staff)
                            {
                                ws.DamageType = 0;
                            }
                            else
                                ws.DamageType = (DamageTypes)Rng.Next(1, Enum.GetNames(typeof(DamageTypes)).Length - 1);
                        }

                        ws.WeaponType = (WeaponTypes)Rng.Next(0, Enum.GetNames(typeof(WeaponTypes)).Length);
                        ws.SecondaryDamageType = DamageTypes.None;

                        int lowVal = 30;
                        int highVal = 40;

                        ws.StatModifiers.Add(StatTypes.Dmg, Rng.Next(lowVal, highVal));
                        ws.SocketType = SocketTypes.Weapon;

                        AvailableStats.Add(StatTypes.Atk);
                        AvailableStats.Add(StatTypes.Spd);
                        AvailableStats.Add(StatTypes.Dex);
                        AvailableStats.Add(StatTypes.Dmg);
                        AvailableStats.Add(StatTypes.Int);
                        AvailableStats.Add(StatTypes.Crc);
                        AvailableStats.Add(StatTypes.Ats);
                    }
                    break;
                case EquipmentTypes.Armor:
                    {
                        ArmorSocket ars = (ArmorSocket)toReturn;
                        ars.GearType = (ArmorTypes)Rng.Next(0, Enum.GetNames(typeof(ArmorTypes)).Length);
                        ars.SocketType = SocketTypes.Armor;

                        int lowVal = 45;
                        int highVal = 60;


                        if (Rng.Next(2) == 0) ars.StatModifiers.Add(StatTypes.Pdf, Rng.Next(lowVal, highVal));
                        else ars.StatModifiers.Add(StatTypes.Mdf, Rng.Next(lowVal, highVal));



                        AvailableStats.Add(StatTypes.Con);
                        AvailableStats.Add(StatTypes.Spd);
                        AvailableStats.Add(StatTypes.Dex);
                        AvailableStats.Add(StatTypes.Dmg);
                        AvailableStats.Add(StatTypes.Int);
                        AvailableStats.Add(StatTypes.Vit);
                        AvailableStats.Add(StatTypes.Atk);
                        AvailableStats.Add(StatTypes.Mdf);
                        AvailableStats.Add(StatTypes.Pdf);
                        AvailableStats.Add(StatTypes.Crc);
                        AvailableStats.Add(StatTypes.Ats);
                    }
                    break;
            }

            for (int x = 0; x < rarity; x++)
            {
                toReturn.LevelUp();
            }

            return toReturn;
        }
    }
}
