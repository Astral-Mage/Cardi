using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CombatSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class DefendAction : BaseAction
    {
        const string _secondaryCommandChar = "/";

        public DefendAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Message;
            AlternateNames.Add("d");
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            var split = ao.Message.Split(ao.CommandChar.ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).ToList();

            // check for skills
            Skill skillToUse = null;
            if (split.Count == 1)
            {
                var usableskills = card.GetUsableSkills();
                bool parsed = int.TryParse(ao.Message, out int skillid);
                foreach (var sk in usableskills)
                {
                    if (parsed && sk.SkillId == skillid)
                    {
                        skillToUse = sk;
                    }
                    else
                    {
                        if (sk.Name.ToLowerInvariant().Equals(split.First().ToLowerInvariant()))
                        {
                            skillToUse = sk;
                        }
                    }
                }
            }

            var foundattk = CombatController.ConfirmImpendingAttack(card, skillToUse);

            if (foundattk == null)
            {
                return;
            }
            else
            {
                CombatController.Attack(foundattk, ao.Channel);
            }
        }
    }
}