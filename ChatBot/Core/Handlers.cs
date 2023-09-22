using ChatApi;
using System.Linq;

namespace ChatBot
{
    public partial class Program
    {
        /// <summary>
        /// Takes care of pushing messages to the bot
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleMessageReceived(object sender, MessageEventArgs e)
        {
            bool isOp = Ops.Any(x => x.Equals(e.user, System.StringComparison.InvariantCultureIgnoreCase));
            string command = string.Empty;
            if (e.message.StartsWith(CommandChar))
            {
                e.message = e.message.TrimStart(CommandChar.ToCharArray());
                if (e.message.Split(' ').Length > 1)
                {
                    command = e.message.Split(' ').First();
                }
                else
                {
                    command = e.message;
                }
            }

            Bot.HandleMessage(e.channel, Utility.ReplaceFirst(e.message, command, "").TrimStart(), e.user, command.ToLowerInvariant(), isOp);
        }

        static void ConnectedToChat(object sender, ChannelEventArgs e)
        {

        }

        /// <summary>
        /// We've joined a channel
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleJoinedChannel(object sender, ChannelEventArgs e)
        {
            if (e.userJoining.Equals(CharacterName))
            {
                //Chat.SetStatus(ChatStatus.DND, $"[session={e.name}]{(string.IsNullOrWhiteSpace(e.code) ? e.name : e.code)}[/session] [color=pink]DM me with {CommandChar}{"help"} to get started![/color]", CharacterName);
                Bot.HandleJoinedChannel(string.IsNullOrWhiteSpace(e.code) ? e.name : e.code);
            }
        }

        static void HandleCreatedChannel(object sender, ChannelEventArgs e)
        {
            //Chat.Mod_InviteUserToChannel("Astral Mage", e.code);
            //Chat.SetStatus(ChatStatus.DND, $"[session={e.name}]{(string.IsNullOrWhiteSpace(e.code) ? e.name : e.code)}[/session] [color=green]Welcome to the testing grounds.[/color]", CharacterName);

        }

        /// <summary>
        /// We've left a channel
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandleLeftChannel(object sender, ChannelEventArgs e)
        {

        }

        /// <summary>
        /// We got a private list of channels
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandlePrivateChannelsReceived(object sender, ChannelEventArgs e)
        {
            var privateChannels = Chat.RequestChannelList(ChannelType.Private);

            // check and join starting channel here
            if (privateChannels.Any(x => x.Code.Equals(StartingChannel, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                Chat.JoinChannel(StartingChannel);
            }
            else if (privateChannels.Any(x => x.Name.Equals(StartingChannel, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                Chat.JoinChannel(privateChannels.First(x => x.Name.Equals(StartingChannel, System.StringComparison.InvariantCultureIgnoreCase)).Code);
            }

#if DEBUG
            //string roomname = "Aelia's Secret Testing Ground";
            //if (!Chat.RequestChannelList(ChannelType.Private).Any(x => x.Code.Equals("adh-1a7c52c105ef5420b73b", System.StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    Chat.CreateChannel(roomname);
            //}
            //else
            //{
            //    Chat.JoinChannel(roomname);
            //}
#endif
        }

        /// <summary>
        /// We got a public list of channels
        /// </summary>
        /// <param name="sender">our sending object</param>
        /// <param name="e">our event args</param>
        static void HandlePublicChannelsReceived(object sender, ChannelEventArgs e)
        {
        }
    }
}