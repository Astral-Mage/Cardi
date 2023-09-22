using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class OrgasmAction : BaseAction
    {
        public OrgasmAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            string toSend = string.Empty;

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} Orgasm...";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
