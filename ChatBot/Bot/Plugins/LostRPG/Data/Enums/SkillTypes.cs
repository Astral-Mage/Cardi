using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public enum SkillTypes
    {
        damage,
        healing,
        speed,
        buff,
        debuff,
        reaction,
        shield,
        misc,
        area,
        aura,
        name,
        level,
        tags,
        cooldown,
    }
}

public enum SkillTags
{
    fire,
    ice,
    air,
    gravity,
    time,
    elemental,
    physical,
    force,
    energy,
    love,
    charge,
}

public enum EffectTypes
{
    [Description("b")]
    Buff,
    [Description("db")]
    Debuff,
}