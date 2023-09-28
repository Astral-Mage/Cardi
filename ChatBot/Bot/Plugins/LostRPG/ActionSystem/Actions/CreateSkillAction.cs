using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateSkillAction : BaseAction
    {
        public CreateSkillAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Both;
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            ao.Message = ao.Message.Replace("&lt;", "<").Replace("&gt;", ">");

            // setup skill
            Skill newSkill = Skill.ReadRawString(ao.Message);
            if (newSkill == null)
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Skill Creation Failure: Missing skill creation tag or improperly constructed tag || Required tags: /level /damage /speed /effects /tags /name. Also note: No vertical slashes ( | ) allowed in names or descriptions.", ao.User);
                return;
            }



            // add up total skill points


            // check if skill is valid and acceptable


            // save to db
            newSkill.RawStr = ao.Message;
            DataDb.SkillsDb.AddNewSkill(newSkill);
            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"Created Skill (Id: {newSkill.SkillId}) || Name: {newSkill.Name} | Level: {newSkill.Level} | Effect Count: {newSkill.SkillEffects.Count} | Tags: {string.Join(", ", newSkill.Tags)} | Speed: {newSkill.Speed} | Reaction: {newSkill.Reaction} | Description: {newSkill.Description}", ao.User);

            return;
        }
    }
}
