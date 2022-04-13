using ChatApi;

namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// Our base plugin for others to derive off of
    /// </summary>
    public class PluginBase
    {
        /// <summary>
        /// Our api connection
        /// </summary>
        public ApiConnection Api;

        /// <summary>
        /// 
        /// </summary>
        public string CommandChar;

        /// <summary>
        /// hidden constructor
        /// </summary>
        private PluginBase() { }

        /// <summary>
        /// Sets our api con in the constructor
        /// </summary>
        /// <param name="api"></param>
        public PluginBase(ApiConnection api, string commandChar) { Api = api; CommandChar = commandChar; }

        /// <summary>
        /// how we should handle a recieved message
        /// </summary>
        /// <param name="commander">command sent</param>
        /// <param name="channel">source channel, if any</param>
        /// <param name="message">base message, if any</param>
        /// <param name="sendingUser">sending user</param>
        /// <param name="isOp">if the user is an op</param>
        public virtual void HandleRecievedMessage(string command, string channel, string message, string sendingUser, bool isOp) { }

        public virtual void HandleJoinedChannel(string channel) { }

        /// <summary>
        /// shuts down any volatile variables
        /// </summary>
        public virtual void Shutdown() { }

        /// <summary>
        /// Handlse responding
        /// </summary>
        /// <param name="channel">target channel, if any</param>
        /// <param name="message">base message, if any</param>
        /// <param name="recipient">who to send the reply to, if a dm</param>
        public virtual void Respond(string channel, string message, string sendingUser) { }
    }
}
