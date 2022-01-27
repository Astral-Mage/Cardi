using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards
{
    public class EnemyCard : BaseCard
    {
        public string StatusReason;

        public EnemyCard() : base(CardTypes.EnemyCard)
        {
            LevelScaleValue = 1.2;
        }

        public override int GetStat(StatTypes type, bool includeModifiers = true, bool includeEquipment = true, bool includePassives = true, bool includeLevels = false)
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
                    type == StatTypes.Int ||
                    type == StatTypes.Dex ||
                    type == StatTypes.Dmg ||
                    type == StatTypes.Crt ||
                    type == StatTypes.Crc ||
                    type == StatTypes.Eva)
                {
                    stat = (int)(stat * GetStat(StatTypes.Lvl) * LevelScaleValue);
                }
                else if (type == StatTypes.Vit)
                {
                    stat = (int)(stat * GetStat(StatTypes.Lvl) * (LevelScaleValue + .2));
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
    }
}
