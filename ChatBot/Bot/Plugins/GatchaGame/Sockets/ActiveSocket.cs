using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
