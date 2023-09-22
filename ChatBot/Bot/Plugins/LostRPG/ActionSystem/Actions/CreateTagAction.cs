using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateTagAction : BaseAction
    {
        public CreateTagAction()
        {
            Description = "";
            SecurityType = CommandSecurity.Ops;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = false;
            AlternateNames.Add("ct");
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // -ct pants
            var taglist = DataDb.TagsDb.GetAllTags();
            if (taglist.Any(x => x.Name.ToLowerInvariant().Equals(ao.Message.ToLowerInvariant())))
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Tag already exists.", ao.User);
                return;
            }

            // setup spec
            Tags tag = new Tags(ao.Message);

            // save to db
            DataDb.TagsDb.AddNewTag(tag);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created Tag (Id: {tag.TagId}) || Name: {tag.Name}", ao.User);
            return;
        }
    }
}
