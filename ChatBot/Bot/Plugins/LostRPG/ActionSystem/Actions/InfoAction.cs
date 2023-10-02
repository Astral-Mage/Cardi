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
                if (card == null)
                {
                    toSend += "Basic Info Return";
                    SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
                }
                else
                {
                    foreach (var custom in card.GetActiveCustomizations().OrderBy(x => x.Customization))
                    {
                        toSend += custom.GetInfo() + "\\n";
                    }
                    SystemController.Instance.Respond(ao.Channel, toSend, ao.User);
                }
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
                            InformationSystem.InfoCore.GetCustomizationInfo(card, ao.Message, ao.Channel, splitmsg, ao.User, CustomizationTypes.Specialization);
                        }
                        break;
                    case InfoTypes.Arc:
                        {
                            InformationSystem.InfoCore.GetCustomizationInfo(card, ao.Message, ao.Channel, splitmsg, ao.User, CustomizationTypes.Archetype);

                        }
                        break;
                    case InfoTypes.Tags:
                        {
                            InformationSystem.InfoCore.GetTagInfo(card, ao.Channel, splitmsg, ao.User);
                        }
                        break;
                    case InfoTypes.Calling:
                        {
                            InformationSystem.InfoCore.GetCustomizationInfo(card, ao.Message, ao.Channel, splitmsg, ao.User, CustomizationTypes.Calling);
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
