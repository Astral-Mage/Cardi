using System;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    class ActiveSocket : Socket
    {
        public override string GetName()
        {
            return string.Empty;
        }

        public override string GetShortDescription()
        {
            throw new NotImplementedException();
        }

        public override string LevelUp()
        {
            throw new NotImplementedException();
        }
    }
}
