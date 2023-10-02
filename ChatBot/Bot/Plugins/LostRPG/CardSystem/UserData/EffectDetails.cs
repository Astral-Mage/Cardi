using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    [Serializable]
    public class EffectDetails
    {
        public int eid { get; set; }
        public DateTime CreationDate { get; set; }
        public TimeSpan Duration { get; set; }
        public EffectTypes EffectType { get; set; }
        public DamageTypes DamageType { get; set; }
        public int DamageSnapshotValue { get; set; }
        public int HealingSnapshotValue { get; set; }

        public EffectDetails()
        {
            eid = -1;
            CreationDate = DateTime.Now;
            Duration = new TimeSpan();
            EffectType = EffectTypes.Buff;
            DamageType = DamageTypes.None;
            DamageSnapshotValue = 0;
            HealingSnapshotValue = 0;
        }

        public EffectDetails(int _eid, TimeSpan duration, EffectTypes type)
        {
            eid = _eid;
            CreationDate = DateTime.Now;
            Duration = duration;
            EffectType = type;
            DamageType = DamageTypes.None;
            DamageSnapshotValue = 0;
            HealingSnapshotValue = 0;
        }

        public TimeSpan GetRemainingDuration()
        {
            if (Duration == TimeSpan.MaxValue) return Duration;

            DateTime now = DateTime.Now;
            TimeSpan elapsedtime = now - CreationDate;
            if (elapsedtime.TotalMilliseconds > Duration.TotalMilliseconds) return new TimeSpan(0, 0, 0);
            else return Duration - elapsedtime;
        }
    }
}