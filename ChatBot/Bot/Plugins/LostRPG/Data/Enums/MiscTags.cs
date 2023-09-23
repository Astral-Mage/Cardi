using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    public enum RequiredSkillTags
    {
        tags,
        level,
        effects,
        speed,
        name,
    }

    public enum OptionalSkillTags
    {
        reaction,
        description,
    }

    public enum EffectTypes
    {
        [Description("b")]
        Buff,
        [Description("db")]
        Debuff,
    }
}