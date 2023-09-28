using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class RestoreAction : BaseAction
    {
        public RestoreAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = true;
            AlternateNames.Add("rest");
            AlternateNames.Add("r");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            string toSend = string.Empty;

            card.Stats.SetStat(CardSystem.UserData.StatTypes.CurrentLife, card.Stats.GetStat(CardSystem.UserData.StatTypes.Life));
            List<Effect> toRemove = new List<Effect>();
            foreach (var eff in card.ActiveEffects)
            {
                if (eff.GetRemainingDuration().TotalMilliseconds <= 0)
                    toRemove.Add(eff);
            }
            toRemove.ForEach(x => card.ActiveEffects.Remove(x));

            DataDb.CardDb.UpdateUserCard(card);




            toSend += $"Alias: {card.Alias} | UserId: {card.UserId}'s health has been restored.";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
