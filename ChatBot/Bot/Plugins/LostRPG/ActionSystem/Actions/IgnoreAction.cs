using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CombatSystem;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class IgnoreAction : BaseAction
    {
        const string _secondaryCommandChar = "/";

        public IgnoreAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Message;
            AlternateNames.Add("i");
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            var atk = CombatController.ConfirmImpendingAttack(card, null, true);
            SystemController.Instance.Respond(ao.Channel, $"{card.Alias} has denied your attack, {atk.Attacker.Alias}.", card.Name);
        }
    }
}
