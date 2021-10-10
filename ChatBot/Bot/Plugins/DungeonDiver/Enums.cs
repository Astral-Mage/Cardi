namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// Gilded items
    /// </summary>
    public enum ItemGild
    {
        Normal = 0,
        Gilded = 1,

    }

    /// <summary>
    /// Your item types
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Gear,
        Special,
    }

    /// <summary>
    /// Your unique card types
    /// </summary>
    public enum CardType
    {
        Name,
        Nickname,
        Species,
        Class,
        Signature,
    }

    /// <summary>
    /// Different types of dungeon events
    /// </summary>
    public enum EventType
    {
        Enemy,
        Npc,
        Floor,
        Loot,
        Misc,
        Gold,
        Gilded,
        Xp,
    }
}
