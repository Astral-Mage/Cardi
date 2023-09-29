using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateCallingAction : BaseAction
    {
        public CreateCallingAction()
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
            BaseCustomization newCalling = Calling.ReadRawString(ao.Message);
            if (newCalling == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Calling Creation Failure", ao.User);
                return;
            }

            // save to db
            DataDb.CustomDb.AddNewCustomization(newCalling);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created Calling (Id: {newCalling.Id}) || Name: {newCalling.Name}", ao.User);

            return;
        }
    }
}
