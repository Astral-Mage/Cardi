using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards
{
    [Serializable]
    public class BaseCard
    {
        public string Name;
        public string DisplayName;
        public List<Socket> ActiveSockets;
        public List<SocketTypes> AvailableSockets;
        public CardTypes CardType;
        public double LevelScaleValue;
        public List<Socket> Inventory;
        public int MaxInventory;

        public BaseStats Stats;
        public int CurrentVitality;

        public List<BoonTypes> BoonsEarned;

        public BaseStats GetStats()
        {
            return Stats;
        }

        public void SetStats(BaseStats stats)
        {
            Stats = stats;
        }

        public void AddStat(StatTypes type, int value)
        {
            SetStat(type, GetStat(type) + value);
        }

        public void SetStat(StatTypes type, int value)
        {
            Stats.Stats[type] = value;
        }

        public virtual int GetStat(StatTypes type, bool includeModifiers = true, bool includeEquipment = true, bool includePassives = true, bool includeLevels = true)
        {
            if (!Stats.TryGetStat(type, out int stat))
            {
                Stats.AddStat(type, 0);
                stat = 1;
            }

            if (!includeModifiers && !includeEquipment && !includePassives)
                return stat;

            if (includePassives)
            {
                foreach (var s in ActiveSockets.Where(x => (x.SocketType == SocketTypes.Passive) && x.StatModifiers.ContainsKey(type)))
                {
                    stat += s.StatModifiers[type];
                    if (stat <= 1)
                        stat = 1;
                }
            }

            if (includeLevels)
            {
                if (type == StatTypes.Atk ||
                    type == StatTypes.Con ||
                    type == StatTypes.Spd ||
                    type == StatTypes.Mdf ||
                    type == StatTypes.Pdf ||
                    type == StatTypes.Vit ||
                    type == StatTypes.Int ||
                    type == StatTypes.Dex ||
                    type == StatTypes.Dmg ||
                    type == StatTypes.Crt ||
                    type == StatTypes.Crc ||
                    type == StatTypes.Eva)
                {
                    stat = (int)(stat * GetStat(StatTypes.Lvl) * LevelScaleValue);
                }
            }

            if (includeEquipment)
            {
                foreach (var s in ActiveSockets.Where(x => (x.SocketType == SocketTypes.Weapon || x.SocketType == SocketTypes.Armor || x.SocketType == SocketTypes.Equipment) && x.StatModifiers.ContainsKey(type)))
                {
                    stat += s.StatModifiers[type];
                    if (stat <= 1)
                        stat = 1;
                }
            }

            return stat;
        }

        public void Update(TurnSteps turnStep)
        {
            var modsToUpdate = Stats.Modifiers.Where(x => x.UpdateStep == turnStep).ToList();
            modsToUpdate.ForEach(x => x.TurnDuration--);
            var modsToRemove = modsToUpdate.Where(x => x.TurnDuration <= 0).ToList();
            modsToRemove.ForEach(x => Stats.Modifiers.Remove(x));
        }

        protected BaseCard(CardTypes cardType)
        {
            CardType = cardType;
            ActiveSockets = new List<Socket>();
            AvailableSockets = new List<SocketTypes>();
            Name = "Undefined";
            DisplayName = Name;

            MaxInventory = 0;
            Inventory = new List<Socket>();

            Stats = new BaseStats();
            CurrentVitality = 0;

            LevelScaleValue = 1.6;
            BoonsEarned = new List<BoonTypes>();
        }

        private BaseCard()
        {
        }
    }
}
