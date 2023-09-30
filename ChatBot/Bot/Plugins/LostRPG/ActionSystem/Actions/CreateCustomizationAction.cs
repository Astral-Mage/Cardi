using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateCustomizationAction : BaseAction
    {
        public CreateCustomizationAction()
        {
            Description = "";
            SecurityType = CommandSecurity.Ops;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
            AlternateNames.Add("cc");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // setup spec
            BaseCustomization newArc = BaseCustomization.ReadRawString(ao.Message);
            if (newArc == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Spec Creation Failure", ao.User);
                return;
            }

            // save to db
            DataDb.CustomDb.AddNewCustomization(newArc);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created {newArc.Customization} (Id: {newArc.Id}) || Name: {newArc.Name}", ao.User);

            return;
        }
    }
}
