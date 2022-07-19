using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CardAction : BaseAction
    {
        public CardAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
        }

        public override void Execute(ActionObject ao)
        {
            if (!DataDb.Instance.UserExists(ao.User))
                return;

            UserCard card = DataDb.Instance.GetCard(ao.User);

            string toSend = string.Empty;

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId} | Magic: [color={card.MagicData.PrimaryMagic.Color}][b]{card.MagicData.PrimaryMagic.Name}[/b][/color]";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
