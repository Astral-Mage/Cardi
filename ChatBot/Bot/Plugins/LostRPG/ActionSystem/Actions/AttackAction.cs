using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CombatSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class AttackAction : BaseAction
    {
        const string _secondaryCommandChar = "/";

        public AttackAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Message;
            AlternateNames.Add(ActionNames.a);
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            List<string> split = new List<string>();
            if (ao.Message.Contains(ao.CommandChar)) split = ao.Message.Split(ao.CommandChar.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).ToList();
            else if (ao.Message.ToLowerInvariant().Contains("with")) split = ao.Message.ToLowerInvariant().Split(new string[] { "with" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            else split.Add(ao.Message.Trim());

            // target checks
            if (split.Count < 1)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"You require a target to attack.", ao.User);
                return;
            }
            if (!DataDb.CardDb.UserExists(split.First().Trim()))
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"User {split.First()} not a valid target.", ao.User);
                return;
            }
            UserCard enemy = DataDb.CardDb.GetCard(split.First().Trim());
            
            // self attack check
            if (enemy.UserId == card.UserId)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"You may not attack yourself, {card.Name}.", ao.User);
                return;
            }

            // health checks
            if (card.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife) <= 0)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"You're too injured to fight anyone right now, {card.Alias}.", ao.User);
                return;
            }
            if (enemy.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife) <= 0)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Your target is too wounded to fight right now, {card.Alias}.", ao.User);
                return;
            }

            // check for skills
            Skill skillToUse = null;
            if (split.Count > 1)
            {
                string stu = split.Last();
                foreach (var s in card.GetUsableSkills())
                {
                    if (s.Name.ToLowerInvariant().Equals(stu.Trim().ToLowerInvariant()))
                    {
                        skillToUse = s;
                        break;
                    }
                    else if (int.TryParse(stu, out int skillid) && s.SkillId == skillid)
                    {
                        skillToUse = s;
                        break;
                    }
                }
            }

            string tosend = $"{card.Alias} is attacking {enemy.Alias}";
            if (skillToUse != null) tosend += $" with {skillToUse.Name}";
            tosend += $", whom may either -ignore, or -defend.";

            var enemyskills = enemy.GetUsableSkills();
            if (enemyskills.Any(x => x.Reaction))
            {
                tosend += $" Available Reactions: ";

                enemyskills.ForEach((x) =>
                {
                    if (x.Reaction)
                    {
                        tosend += $"{x.GetShortDescription()} ";
                    }
                });

            }

            if (!CombatController.CreateImpendingAttack(card, enemy, skillToUse))
            {
                return;
            }
            else
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, tosend, ao.User);
            }
        }
    }
}
