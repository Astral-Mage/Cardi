using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateSpecializationAction : BaseAction
    {
        public CreateSpecializationAction()
        {
            Description = "";
            SecurityType = CommandSecurity.Ops;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
            AlternateNames.Add("cs");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // setup spec
            Specialization newSpec = Specialization.ReadRawString(ao.Message);
            if (newSpec == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Spec Creation Failure", ao.User);
                return;
            }

            // save to db
            DataDb.SpecDb.AddNewSpec(newSpec);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created Specialization (Id: {newSpec.SpecId}) || Name: {newSpec.Name}", ao.User);

            return;
        }
    }
}
