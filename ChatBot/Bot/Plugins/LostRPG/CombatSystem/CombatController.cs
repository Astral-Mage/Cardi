using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CombatSystem
{
    public static class CombatController
    {
        public static void Attack(UserCard attacker, UserCard defender, Skill skilltouse, out string outputstr)
        {

            outputstr = string.Empty;
            int damage = 300 + RNG.Seed.Next(0, 50);

            if (skilltouse != null && skilltouse.Stats.Stats.ContainsKey(CardSystem.UserData.StatTypes.Damage)) damage = (int)Math.Round( damage * (skilltouse.Stats.GetStat(CardSystem.UserData.StatTypes.Damage) * .01), 0);

            int life = defender.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife);
            life -= damage;

            string skilladd = string.Empty;
            if (skilltouse != null) skilladd += $" with ⟨ {skilltouse.Name} ⟩";



            string effectadd = string.Empty;
            if (skilltouse != null && skilltouse.SkillEffects.Any())
            {
                foreach (var eff in skilltouse.SkillEffects)
                {
                    effectadd += $" {defender.Alias} has been debuffed with ⟪ {eff.Name} ⟫.";
                    eff.CreationDate = DateTime.Now;
                    defender.ActiveEffects.Add(eff);
                }
            }


            outputstr += $"{attacker.Alias} attacks {defender.Alias}{skilladd} for {damage} damage.{effectadd}";
            
            if (life <= 0)
            {
                outputstr += $" {defender.Alias} has been downed and is no longer able to continue fighting. You did {life * -1} overkill damage, {attacker.Alias}";
                life = 0;
            }



            defender.Stats.SetStat(CardSystem.UserData.StatTypes.CurrentLife, life);
            
            DataDb.CardDb.UpdateUserCard(defender);
            DataDb.CardDb.UpdateUserCard(attacker);
        }
    }
}
