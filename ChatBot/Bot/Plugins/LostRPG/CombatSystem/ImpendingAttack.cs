using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using System;

namespace ChatBot.Bot.Plugins.LostRPG.CombatSystem
{
    public class ImpendingAttack
    {
        public DateTime CreationDate { get; set; }
        public UserCard Attacker { get; set; }
        public UserCard Defender { get; set; }
        public Skill AttackerSkill { get; set; }
        public Skill DefenderSkill { get; set; }
        public TimeSpan AttackTimeout { get; set; }

        public ImpendingAttack()
        {
            CreationDate = DateTime.Now;
            Attacker = null;
            Defender = null;
            AttackerSkill = null;
            DefenderSkill = null;
            AttackTimeout = new TimeSpan(0, 5, 0);
        }

        public ImpendingAttack(UserCard attacker, UserCard defender, Skill attackerSkill)
        {
            CreationDate = DateTime.Now;
            Attacker = attacker;
            Defender = defender;
            AttackerSkill = attackerSkill;
            DefenderSkill = null;
            AttackTimeout = new TimeSpan(0, 5, 0);
        }

        public TimeSpan GetRemainingTime()
        {
            var diff = DateTime.Now - CreationDate;
            return AttackTimeout - diff;
        }
    }
}
