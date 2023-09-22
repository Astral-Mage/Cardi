﻿using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateArchetypeAction : BaseAction
    {
        public CreateArchetypeAction()
        {
            Description = "";
            SecurityType = CommandSecurity.Ops;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
            AlternateNames.Add("createarc");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // setup spec
            Archetype newArc = Archetype.ReadRawString(ao.Message);
            if (newArc == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Spec Creation Failure", ao.User);
                return;
            }

            // save to db
            DataDb.ArcDb.AddNewArchetype(newArc);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created Archetype (Id: {newArc.ArcId}) || Name: {newArc.Name}", ao.User);

            return;
        }
    }
}