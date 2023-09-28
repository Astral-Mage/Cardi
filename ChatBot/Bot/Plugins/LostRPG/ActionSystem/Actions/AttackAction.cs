using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CombatSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
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
            var split = ao.Message.Split(ao.CommandChar.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).ToList();

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

            // check for skills
            Skill skillToUse = null;
            if (split.Count > 1)
            {
                string stu = split.Last();
                foreach (var s in card.GetUsableSkills())
                {
                    if (s.Name.Equals(stu))
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

            // attack
            if (!Attack(card, enemy, skillToUse, out string output))
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, output, ao.User);
                return;
            }

            // reply
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, output, ao.User);
        }

        private bool Attack(UserCard source, UserCard target, Skill skilltouse, out string outputstr)
        {
            outputstr = string.Empty;
            if (source.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife) <=0)
            {
                outputstr += $"You're too injured to fight anyone right now, {source.Alias}.";
                return false;
            }

            if (target.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife) <= 0)
            {
                outputstr += $"Your target is too wounded to fight right now, {source.Alias}.";
                return false;
            }

            CombatController.Attack(source, target, skilltouse, out outputstr);
            return true;
        }
    }
}
