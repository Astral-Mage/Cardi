using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CombatSystem
{
    public static class CombatController
    {
        static List<ImpendingAttack> ImpendingAttacks = new List<ImpendingAttack>();

        public static bool CreateImpendingAttack(UserCard attacker, UserCard defender, Skill attackerSkill)
        {
            ImpendingAttack iat = new ImpendingAttack(attacker, defender, attackerSkill);
            foreach (var atk in ImpendingAttacks)
            {
                if (atk.Attacker.UserId == attacker.UserId)
                {
                    return false;
                }
            }

            ImpendingAttacks.Add(iat);
            return true;
        }

        static void CheckTimeouts()
        {
            List<ImpendingAttack> toDelete = new List<ImpendingAttack>();
            foreach (var v in ImpendingAttacks)
            {
                if (v.GetRemainingTime().TotalMilliseconds <= 0) toDelete.Add(v);
            }
            toDelete.ForEach(x => ImpendingAttacks.Remove(x));
        }

        public static ImpendingAttack ConfirmImpendingAttack(UserCard defender, Skill defenderSkill, bool decline = false)
        {
            CheckTimeouts();
            ImpendingAttack attack = null;
            foreach (var atk in ImpendingAttacks)
            {
                if (atk.Defender.UserId == defender.UserId)
                {
                    atk.DefenderSkill = defenderSkill;
                    attack = atk;
                }
            }

            if (attack == null)
            {
                return null;
            }

            ImpendingAttacks.Remove(attack);

            if (decline)
            {
                ImpendingAttacks.Remove(attack);
                return attack;

            }

            return attack;
        }

        public static void Attack(ImpendingAttack attack, string channel)
        {

            string outputstr = string.Empty;
            int damage = 300 + RNG.Seed.Next(0, 50);

            if (attack.AttackerSkill != null && attack.AttackerSkill.Stats.Stats.ContainsKey(CardSystem.UserData.StatTypes.Damage)) damage = (int)Math.Round( damage * (attack.AttackerSkill.Stats.GetStat(CardSystem.UserData.StatTypes.Damage) * .01), 0);

            int life = attack.Defender.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife);
            int shield = 0;
            if (attack.DefenderSkill != null)
            {
                if (attack.DefenderSkill.Stats.Stats.ContainsKey(CardSystem.UserData.StatTypes.Shield))
                {
                    shield = attack.DefenderSkill.Stats.GetStat(CardSystem.UserData.StatTypes.Shield);
                }
            }

            int postshielddmg = damage - shield;

            int absorbed = 0;
            if (damage > shield) absorbed = shield;
            else absorbed = damage;

            if (postshielddmg < 0) postshielddmg = 0;
            life -= postshielddmg;

            string skilladd = string.Empty;
            if (attack.AttackerSkill != null) skilladd += $" with ⟨ {attack.AttackerSkill.Name} ⟩.";
            if (attack.DefenderSkill != null) skilladd += $" {attack.Defender.Alias} defends with ⟨ {attack.DefenderSkill.Name} ⟩.";


            string effectadd = string.Empty;
            if (attack.AttackerSkill != null && attack.AttackerSkill.SkillEffects.Any())
            {
                foreach (var eff in attack.AttackerSkill.SkillEffects)
                {
                    var te = DataDb.EffectDb.GetEffect(eff);
                    effectadd += $" {attack.Defender.Alias} has been debuffed with ⟪ {te.Name} ⟫.";
                    te.CreationDate = DateTime.Now;
                    attack.Defender.ActiveEffects.Add(new CardSystem.UserData.EffectDetails(te.EffectId, te.Duration, te.EffectType));
                }
            }

            string dmgadd = $"for {postshielddmg} damage";
            if (shield != 0)
            {
                dmgadd += $" ({absorbed} shielded)";
            }

            outputstr += $"{attack.Attacker.Alias} attacks {attack.Defender.Alias}{skilladd} {dmgadd}.{effectadd}";
            
            if (life <= 0)
            {
                outputstr += $" {attack.Defender.Alias} has been downed and is no longer able to continue fighting. You did {life * -1} overkill damage, {attack.Attacker.Alias}";
                life = 0;
            }



            attack.Defender.Stats.SetStat(CardSystem.UserData.StatTypes.CurrentLife, life);
            
            DataDb.CardDb.UpdateUserCard(attack.Defender);
            DataDb.CardDb.UpdateUserCard(attack.Attacker);

            SystemController.Instance.Respond(channel, outputstr, null);
        }
    }
}
