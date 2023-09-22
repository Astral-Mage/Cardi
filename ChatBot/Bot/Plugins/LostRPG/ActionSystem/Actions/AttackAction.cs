using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
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
            var split = ao.Message.Split(_secondaryCommandChar.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).ToList();

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

            // attack
            if (!Attack(card, enemy, out string output))
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, output, ao.User);
                return;
            }

            // reply
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, output, ao.User);
        }

        private bool Attack(UserCard source, UserCard target, out string outputstr)
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

            int damage = 300 + RNG.Seed.Next(0, 50);
            int life = target.Stats.GetStat(CardSystem.UserData.StatTypes.CurrentLife);
            life = life - damage;

            outputstr += $"{source.Alias} attacks {target.Alias} for {damage} damage.";
            if (life <= 0)
            {
                outputstr += $" {target.Alias} has been downed and is no longer able to continue fighting. You did {life * -1} overkill damage, {source.Alias}";
                life = 0;
            }

            target.Stats.SetStat(CardSystem.UserData.StatTypes.CurrentLife, life);

            DataDb.CardDb.UpdateUserCard(target);
            DataDb.CardDb.UpdateUserCard(source);
            return true;
        }
    }
}
