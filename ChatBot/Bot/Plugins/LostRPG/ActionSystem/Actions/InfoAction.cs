using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class InfoAction : BaseAction
    {
        public InfoAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            string toSend = "\\n";

            if (string.IsNullOrWhiteSpace(ao.Message))
            {
                toSend += "Basic Info Return";
                SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
                return;
            }
            var splitmsg = ao.Message.Split(" ".ToCharArray(), 2).ToList();

            if (!Enum.TryParse(splitmsg.First(), true, out InfoTypes res))
            {
                SystemController.Instance.Respond(ao.Channel, "No information available for this category at the moment.", ao.User);
                return;
            }

            try
            {
                switch (res)
                {
                    case InfoTypes.Spec:
                        {
                            InformationSystem.InfoCore.GetSpecInfo(card, ao.Message, ao.Channel, splitmsg, ao.User);
                        }
                        break;
                    case InfoTypes.Arc:
                        {
                            InformationSystem.InfoCore.GetArcInfo(card, ao.Message, ao.Channel, splitmsg, ao.User);
                        }
                        break;
                    case InfoTypes.Tags:
                        {
                            InformationSystem.InfoCore.GetTagInfo(card, ao.Message, ao.Channel, splitmsg, ao.User);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                SystemController.Instance.Respond(ao.Channel, "Info return failure.", ao.User);
            }

        }
    }
}
