﻿using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
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

            List<EffectDetails> tokill = new List<EffectDetails>();
            foreach (var ae in card.ActiveEffects)
            {
                if (ae.EffectType == EffectTypes.Debuff) tokill.Add(ae);
            }
            tokill.ForEach(x => card.ActiveEffects.Remove(x));

            DataDb.CardDb.UpdateUserCard(card);

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId}'s health has been restored and all active debuffs have been removed.";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
