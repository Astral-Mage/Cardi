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

            card.Restore();

            toSend += $"Alias: {card.Alias} | UserId: {card.UserId}'s health has been restored and all active debuffs have been removed.";
            SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
        }
    }
}
