using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Generation
{
    public static class RngGeneration
    {
        public readonly static Random Rng = new Random();

        private const int BaseGatchaItemMaxClip = 30;

        public static List<EnemyCard> GenerateEnemyCluster(FloorCard fc, Cards.PlayerCard pc, int maxEnemies = 3)
        {
            List<EnemyCard> toReturn = new List<EnemyCard>();

            int numEnemies = Rng.Next(1, maxEnemies);

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

            return ec;
        }

        public static void GenerateEnemyEquipment(FloorCard fc, Cards.PlayerCard pc, EnemyCard ec)
        {

        }

        public static void GenerateEnemyLoot(FloorCard fc, Cards.PlayerCard pc, EnemyCard ec)
        {

        }

        public static void GenerateNewCharacterStats(Cards.PlayerCard pc)
        {
            BaseStats baseStats = new BaseStats();

            // random
            baseStats.AddStat(StatTypes.Atk, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Con, 0);
            baseStats.AddStat(StatTypes.Spd, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Crt, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Mdf, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Pdf, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Vit, Rng.Next(70, 81));
            baseStats.AddStat(StatTypes.Int, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Dex, Rng.Next(3, 7));
            baseStats.AddStat(StatTypes.Exp, 0);
            baseStats.AddStat(StatTypes.Crc, 0);
            baseStats.AddStat(StatTypes.Ats, 0);



            // static
            baseStats.AddStat(StatTypes.Eva, 0);
            baseStats.AddStat(StatTypes.Dmg, Rng.Next(8, 11));
            baseStats.AddStat(StatTypes.Gld, 0);
            baseStats.AddStat(StatTypes.Sds, 50);
            baseStats.AddStat(StatTypes.Lvl, 1);
            baseStats.AddStat(StatTypes.Cs1, 0);
            baseStats.AddStat(StatTypes.Sps, 0);

            // player-specific
            baseStats.AddStat(StatTypes.Prg, 0);
            baseStats.AddStat(StatTypes.KiB, 0);
            baseStats.AddStat(StatTypes.Kil, 0);
            baseStats.AddStat(StatTypes.Rom, 0);
            baseStats.AddStat(StatTypes.Adr, 0);
            baseStats.AddStat(StatTypes.Sta, 54000);
            baseStats.AddStat(StatTypes.StM, 108000);
            baseStats.AddStat(StatTypes.Dff, 0);


            // set
            pc.SetStats(baseStats);
        }

        public static void GenerateEnemyStats(Cards.EnemyCard ec, int baseLevel, CardTypes type)
        {
            BaseStats baseStats = new BaseStats();

            // random
            baseStats.AddStat(StatTypes.Atk, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Con, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Spd, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Crt, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Mdf, Rng.Next(2, 15));
            baseStats.AddStat(StatTypes.Pdf, Rng.Next(2, 15));
            baseStats.AddStat(StatTypes.Vit, Rng.Next(30, 46));
            baseStats.AddStat(StatTypes.Int, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Dex, Rng.Next(2, 5));
            baseStats.AddStat(StatTypes.Cs1, 0);
            baseStats.AddStat(StatTypes.Sps, 0);
            baseStats.AddStat(StatTypes.Exp, (int)(1.6 * baseLevel) * 1);

            // static
            baseStats.AddStat(StatTypes.Eva, 1);
            baseStats.AddStat(StatTypes.Dmg, 18);
            baseStats.AddStat(StatTypes.Gld, 2);

            if (Rng.Next(5) == 0)
                baseStats.AddStat(StatTypes.Sds, 1);
            else
                baseStats.AddStat(StatTypes.Sds, 0);


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

        public static Socket GenerateRandomPassive(out int valRolled)
        {
            PassiveSocket toReturn = new PassiveSocket();
            int roll = Rng.Next(1, BaseGatchaItemMaxClip);
            valRolled = roll;
            int rarity = (int)Math.Pow(Math.Log10(0.3f * roll), 10) + 1;
            toReturn.SocketRarity = (RarityTypes)rarity;
            toReturn.SocketDescription = "";

            int lowVal = (rarity * 5) + 20;
            int highVal = (rarity * 5) + lowVal + 10;

            List<StatTypes> AvailableStats = new List<StatTypes>();
            AvailableStats.Add(StatTypes.Con);
            AvailableStats.Add(StatTypes.Spd);
            AvailableStats.Add(StatTypes.Atk);
            AvailableStats.Add(StatTypes.Dex);
            AvailableStats.Add(StatTypes.Int);
            AvailableStats.Add(StatTypes.Vit);
            AvailableStats.Add(StatTypes.Mdf);
            AvailableStats.Add(StatTypes.Pdf);
            AvailableStats.Add(StatTypes.Crc);
            AvailableStats.Add(StatTypes.Ats);
            AvailableStats.Add(StatTypes.Atk);
            AvailableStats.Add(StatTypes.Crt);

            for (int x = 0; x < 2; x++)
            {
                int lv = 10 + (int)(0.5f * rarity) * 2;
                int hv = 10 + (int)(1.5f * rarity + 5) * 2;

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

            return toReturn;
        }

        public static Socket GenerateRandomEquipment(out int valRolled)
        {

            EquipmentSocket toReturn = ((Rng.Next(2) == 0) ? (EquipmentSocket)new WeaponSocket() : new ArmorSocket());
            int roll = Rng.Next(1, BaseGatchaItemMaxClip);
            valRolled = roll;
            int rarity = (int)Math.Pow(Math.Log10(0.3f * roll), 10) + 1;
            toReturn.SocketRarity = (RarityTypes)rarity;
            toReturn.SocketDescription = "";
            toReturn.Prefix = EquipmentPrefixes.None;
            toReturn.Suffix = EquipmentSuffixes.None;

            int idk = Rng.Next(0, 10);
            if (idk < rarity)
            {
                toReturn.Prefix = (EquipmentPrefixes)Rng.Next(Enum.GetValues(typeof(EquipmentPrefixes)).Length);
            }
            idk = Rng.Next(0, 10);
            if (idk < rarity)
            {
                toReturn.Suffix = (EquipmentSuffixes)Rng.Next(Enum.GetValues(typeof(EquipmentSuffixes)).Length);
            }

            List<StatTypes> AvailableStats = new List<StatTypes>();

            switch (toReturn.ItemType)
            {
                case EquipmentTypes.Weapon:
                    {
                        WeaponSocket ws = (WeaponSocket)toReturn;
                        ws.DamageType = (DamageTypes)Rng.Next(Enum.GetNames(typeof(DamageTypes)).Length);
                        ws.WeaponType = (WeaponTypes)Rng.Next(Enum.GetNames(typeof(WeaponTypes)).Length);

                        int tRarity = rarity;
                        int lowVal = (tRarity * 5)               + 20;
                        int highVal = (tRarity * 5) + lowVal     + 10;

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
                        ars.GearType = (ArmorTypes)Rng.Next(Enum.GetNames(typeof(ArmorTypes)).Length);
                        ars.SocketType = SocketTypes.Armor;

                        int tRarity = rarity;
                        int lowVal = (tRarity * 4) + 15;
                        int highVal = (tRarity * 5) + lowVal + 10;


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
                bool addBonus = Rng.Next(2) == 0;

                if (addBonus && toReturn.Bonuses <= rarity)
                {
                    int lv = 1 + (int)(0.5f * rarity);
                    int hv = 1 + (int)(1.5f * rarity + 5);

                    var theStat = AvailableStats[Rng.Next(AvailableStats.Count)];
                    int value = Rng.Next(lv, hv);
                    if (toReturn.StatModifiers.ContainsKey(theStat))
                    {
                        toReturn.StatModifiers[theStat] = toReturn.StatModifiers[theStat] + value;
                    }
                    else
                    {
                        toReturn.StatModifiers.Add(theStat, value);
                    }
                    toReturn.Bonuses++;
                }
            }

            return toReturn;
        }
    }
}
