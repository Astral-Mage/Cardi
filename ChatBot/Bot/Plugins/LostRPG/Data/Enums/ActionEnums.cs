namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    public enum ChatTypeRestriction
    {
        Whisper,
        Message,
        Both
    }

    /// <summary>
    /// whether or not there's security access required for this command
    /// </summary>
    public enum CommandSecurity
    {
        Ops,
        None
    }
}
