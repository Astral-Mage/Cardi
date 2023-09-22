using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData.Skills.SkillEffects;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateEffectAction : BaseAction
    {
        public CreateEffectAction()
        {
            Description = "";
            SecurityType = CommandSecurity.Ops;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
            AlternateNames.Add("ce");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // setup
            BaseEffect newEffect = BaseEffect.ReadRawString(ao.Message);

            if (newEffect == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Buff/Debuff Creation Failure: Missing buff/debuff creation tag or improperly constructed tag || ", ao.User);
                return;
            }

            // save to db
            DataDb.EffectDb.AddNewEffect(newEffect);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created new Effect (Id: {newEffect.EffectId}) || Name: {newEffect.Name} | Type: {newEffect.EffectType}", ao.User);

            return;
        }
    }
}
