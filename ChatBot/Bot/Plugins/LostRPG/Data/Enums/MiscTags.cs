using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    [Serializable]
    public enum RequiredSkillTags
    {
        tags,
        level,
        effects,
        speed,
        name,
    }

    [Serializable]
    public enum OptionalSkillTags
    {
        reaction,
        description,
    }

    [Serializable]
    public enum EffectTypes
    {
        [Description("b")]
        Buff,
        [Description("db")]
        Debuff,
    }

    [Serializable]
    public enum CustomizationTypes
    {
        Archetype,
        Specialization,
        Calling,
    }
}