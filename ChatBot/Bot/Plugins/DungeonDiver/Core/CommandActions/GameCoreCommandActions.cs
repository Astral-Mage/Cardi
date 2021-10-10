using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        /// <summary>
        /// Sends a default help string
        /// </summary>
        /// <param name="sendingUser">user requesting help</param>
        public void HelpAction(string sendingUser)
        {
            string toSend = string.Empty;

            toSend += "\\nType [color=pink]-create[/color] to register as an Adventurer!" +
                        "\\nThis will give you a card, which you can see by typing[color=pink] -card[/color]!" +
                        "\\n\\nSome parts of your card can be customized. Learn more by typing[color=pink] -set help[/color]!" +
                        "\\nDungeon progress is shared between all adventurers!Check the current floor by typing[color=pink] -prog[/color]." +
                        "\\n\\nTyping[color=pink] -dive[/color] lets you dive into the dungeon. This earns you experience and candies. You may find secret rooms or meet monsters while you're down there!" +
                        "\\nEarn enough gold and you can upgrade your gear! Type[color=pink] -upgrade help[/color] to see what you can upgrade." +
                        "\\n\\nSee my profile here: [user]Cardinal System[/user] to learn how to use more commands or see frequently asked questions!" +
                        "\\n\\nSome more useful commands include:\\n" +
                        "[color=yellow]-gold[/color] to view how much gold you have.\\n" +
                        "[color=green]-cd[/color] to view your current cooldown.\\n" +
                        "[color=red]-lb[/color] to view the leaderboard.\\n";

            Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// Returns the card of a user
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="pc">card of user making the request</param>
        /// <param name="requestedUser">user being requested, self if null</param>
        public void CardAction(string channel, PlayerCard pc, string requestedUser)
        {
            string usertouse = string.IsNullOrWhiteSpace(requestedUser) ? pc.name : requestedUser;
            if (GameDb.UserExists(usertouse))
            {
                PlayerCard gc = GameDb.GetCard(usertouse);
                string nickname = (string.IsNullOrWhiteSpace(gc.nickname)) ? $"[color=white]{gc.name}[/color]" : $"[color=white]{gc.nickname}[/color][/b][b]";
                string toSend = $"\\n   [color={gc.colortheme}][b]Name: {nickname}" + $"\\n    Level: [color={lvlcolor}]{gc.level}[/color][/b] (Next: [color={xpcolor}]{GetXpToLevel(gc)}[/color])[b]" + $" | Gold: [color={goldcolor}]{gc.gold}[/color] | Defeated: [color={enemycolor}]{gc.killed}[/color][/b]";
                toSend += $"\\n[b]Species:[/b] {gc.species}\\n";
                toSend += $"    [b]Class:[/b] {gc.mainClass}";

                // append items
                string wpnbonussymbol = $"{(gc.weaponperklvl == 1 ? "[color=cyan]⁎ " : "[color=white]+")}{gc.weaponlvl}:[/color]";
                string gearbonussymbol = $"{(gc.gearperklvl == 1 ? "[color=cyan]⁎ " : "[color=white]+")}{gc.gearlvl}:[/color]";
                string specialbonussymbol = $"{(gc.specialperklvl == 1 ? "[color=cyan]⁎ " : "[color=white]+")}{gc.speciallvl}:[/color]";
                toSend += $"\\n   [b]🗡️{wpnbonussymbol}[/b] {gc.weapon}\\n    [b]🛡️{gearbonussymbol}[/b] {gc.gear}\\n   [b]✨{specialbonussymbol}[/b] {gc.special}";

                // append signature
                if (!string.IsNullOrWhiteSpace(gc.signature)) toSend += $"\\n{gc.signature}[/color]";
                else toSend += $"[/color]";

                Respond(channel, toSend, pc.name);
            }
            else
            {
                string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";
                Respond(channel, $"Sorry, {nickname}. No card with a user of name: {usertouse} could be found.", pc.name);
            }
        }

        /// <summary>
        /// Returns a leaderboard of monster kills
        /// </summary>
        /// <param name="channel">the source channel</param>
        /// <param name="pc">requesting user's card</param>
        public void LeaderboardAction(string channel, PlayerCard pc)
        {
            var dtc = DateTime.Now - leaderboardLastChecked;
            if (dtc.Ticks > leaderboardCooldown.Ticks && !string.IsNullOrWhiteSpace(channel))
            {
                leaderboardLastChecked = DateTime.Now;
            }
            else if (!string.IsNullOrWhiteSpace(channel))
            {
                var diff = leaderboardCooldown - dtc;
                Respond(channel, $"Public leaderboard cooldown remaining: {diff.Minutes} minutes, {diff.Seconds} seconds.", pc.name);
                channel = null;
            }
            List<PlayerCard> lb = GameDb.GetAllCards();
            lb = lb.OrderByDescending(x => x.killed).ToList();

            string leaderboard =
                "\\n[color=white][b]The[/b][/color] [color=red][b]Monsters Defeated[/b][/color] [color=white][b]Leaderboard[/b][/color]\\n" +
                "\\n";

            int displaylimit = lb.Count;
            if (displaylimit > 10)
            {
                displaylimit = 10;
            }

            for (int counter = 1; counter <= displaylimit; counter++)
            {
                string nickname = (string.IsNullOrWhiteSpace(lb[counter - 1].nickname)) ? $"[color=white]{lb[counter - 1].name}[/color]" : $"[color=white]{lb[counter - 1].nickname}[/color] ({lb[counter - 1].name})";
                leaderboard += $"[color=green][b]{counter}[/b][/color] - [color=red][b]{lb[counter - 1].killed}:[/b][/color] {nickname}\\n";
            }
            Respond(channel, leaderboard, pc.name);
        }

        /// <summary>
        /// handles setting various things in a submenu
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="message">our base message</param>
        /// <param name="pc">the user sending the request</param>
        public void SetAction(string channel, string message, PlayerCard pc)
        {
            Command cmder;
            string cmd = message.Split(' ').First();

            try
            {
                cmder = GetSetSubCommandList().First(x => x.command.Equals(cmd));
                if (cmder == null)
                {
                    Respond(null, $"Sorry, {pc.name}, but I didn't understand your command. Use -set help to see all available commands!", pc.name);
                }

                if (message.Split(' ').Length > 1)
                {
                    message = message.Split(new[] { ' ' }, 2).Last();
                }
                else
                {
                    message = "";
                }
            }
            catch
            {
                Respond(null, $"Sorry, {pc.name}, but I didn't understand your command. Use -set help to see all available commands!", pc.name);
                return;
            }

            SetSubMenu(GetSetSubCommandList().First(x => x.command.Equals(cmd)), message, pc);
        }

        /// <summary>
        /// handles creating and adding a new floor through ui
        /// </summary>
        /// <param name="pc">commanding user</param>
        /// <param name="message">combined floor command</param>
        public void JoinChannelAction(PlayerCard pc, string message)
        {
            int minutesToJoinFor = -1;
            try
            {
                minutesToJoinFor = Convert.ToInt32(message.Split(' ').Last());
            }
            catch
            {
                Respond(null, $"Invalid format. Expected: -joinchannel Cardinal's Cathedral 120", pc.name);
                return;
            }

            message = message.Substring(0, message.Length - (minutesToJoinFor.ToString().Length + 1));
            Api.JoinChannel(message);
            Respond(null, $"Attempted to join {message}", pc.name);
        }

        /// <summary>
        /// handles creating and adding a new floor through ui
        /// </summary>
        /// <param name="pc">commanding user</param>
        /// <param name="message">combined floor command</param>
        public void LeaveChannelAction(PlayerCard pc, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Respond(null, $"Invalid format. Expected: -leavechannel Cardinal's Cathedral", pc.name);
                return;
            }

            Api.LeaveChannel(message);
            Respond(null, $"Attempted to leave {message}", pc.name);
        }

        /// <summary>
        /// handles upgrading various things in a submenu
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="message">our base message</param>
        /// <param name="pc">the user sending the request</param>
        public void UpgradeAction(string channel, string message, PlayerCard pc)
        {
            Command cmder;
            string cmd = message.Split(' ').First();

            try
            {
                cmder = GetUpgradeSubCommandList().First(x => x.command.Equals(cmd));
                if (cmder == null)
                {
                    Respond(null, $"Sorry, {pc.name}, but I didn't understand your command. Use -upgrade help to see all available commands!", pc.name);
                }

                if (message.Split(' ').Length > 1)
                {
                    message = message.Split(new[] { ' ' }, 2).Last();
                }
                else
                {
                    message = "";
                }
            }
            catch
            {
                Respond(null, $"Sorry, {pc.name}, but I didn't understand your command. Use -upgrade help to see all available commands!", pc.name);
                return;
            }

            UpgradeSubMenu(GetUpgradeSubCommandList().First(x => x.command.Equals(cmd)), channel, pc);
        }

        /// <summary>
        /// handles showing the floor progress
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="pc">the user sending the request</param>
        public void ProgAction(string channel, PlayerCard pc)
        {
            FloorCard fc = FloorDb.GetFloor();
            string append = (string.IsNullOrWhiteSpace(fc.notes)) ? "" : $"- {fc.notes} ";
            Respond(channel, $"{fc.name} {append}[color=green]([b]Floor {fc.floor}: {fc.currentxp}[/b]/[b]{fc.neededxp}[/b])[/color]", pc.name);
        }

        /// <summary>
        /// handles showing the player their cooldown
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="pc">the user sending the request</param>
        public void CooldownAction(string channel, PlayerCard pc)
        {
            var cooldown = (new TimeSpan(0, BaseDiveCooldown, 0) - new TimeSpan(DateTime.Now.Ticks - pc.lastDive.Ticks));
            string cooldownstring = (cooldown.Ticks > 0) ? $"[b]{cooldown.Hours}[/b] hours, [b]{cooldown.Minutes}[/b] minutes, [b]{cooldown.Seconds}[/b] seconds of cooldown remaining." : "[color=green]Cooldown Ready![/color]";
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";
            Respond(null, $"[b]{nickname}[/b], your current dive cooldown is: {cooldownstring}", pc.name);
        }

        /// <summary>
        /// handles gifting gold between players
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="pc">the user sending the request</param>
        public void GiftAction(string channel, string message, PlayerCard pc)
        {
            PlayerCard gc;
            int goldAmount = 0;
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";

            // public only
            if (channel == null)
            {
                Respond(null, $"Sorry, {nickname}, but you can only gift in public channels!", pc.name);
                return;
            }

            try
            {
                goldAmount = Convert.ToInt32(message.Split(' ').Last());
                string targetName = message.Replace(goldAmount.ToString(), "").TrimEnd();
                gc = GameDb.GetCard(targetName);
                if (gc == null)
                {
                    Respond(channel, $"Sorry, {nickname}, but {targetName} isn't a valid user! Check your spelling or casing.", pc.name);
                    return;
                }
                else if (goldAmount <= 0)
                {
                    Respond(channel, $"Sorry, {nickname}, but you can only give positive amounts of gold to other people!", pc.name);
                    return;
                }
                else if (goldAmount > pc.gold)
                {
                    Respond(channel, $"Sorry, {nickname}, but you can only give as much gold as you have ({pc.gold}) to someone!", pc.name);
                    return;
                }
                else if (pc.name.Equals(gc.name))
                {
                    Respond(channel, $"Sorry, {nickname}, but you can't gift yourself!", pc.name);
                    return;
                }
            }
            catch
            {
                gc = new PlayerCard();
            }

            if (string.IsNullOrWhiteSpace(gc.name))
            {
                Respond(channel, $"Sorry, {nickname}, but unable to find user: {message}. Expected format: -gift Rng 34    -gift Cardinal System 14", pc.name);
            }
            else
            {
                gc.gold += goldAmount;
                pc.gold -= goldAmount;
                GameDb.UpdateCard(gc);

                GameDb.UpdateCard(pc);
                string nicknameTwo = (string.IsNullOrWhiteSpace(gc.nickname)) ? $"[color=white]{gc.name}[/color]" : $"[color=white]{gc.nickname}[/color]";
                Respond(channel, $"[b]{nickname}[/b] has gifted [b]{nicknameTwo}[/b] [color=yellow][b]{goldAmount}[/b][/color] gold!", pc.name);
            }
        }

        /// <summary>
        /// handles showing the player their gold
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="pc">the user sending the request</param>
        public void GoldAction(string channel, PlayerCard pc)
        {
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";
            Respond(channel, $"[b]{nickname}[/b]: You check your coinpurse and count out [color=yellow][b]{pc.gold}[/b][/color] gold.", pc.name);
        }

        /// <summary>
        /// handles creating a new character
        /// </summary>
        /// <param name="channel">originating channel</param>
        /// <param name="sendingUser">the user sending the request</param>
        public void CreateAction(string channel, string sendingUser)
        {
            if (!GameDb.UserExists(sendingUser))
            {
                PlayerCard newPlayer = new PlayerCard();
                newPlayer.name = sendingUser;
                if (GameDb.AddNewUser(newPlayer))
                {
                    Respond(channel, $"Welcome, {sendingUser}. Thanks for playing! Use -card to see your player card. Use -dive to make your first dungeon run!\\nJust so you know, talking in the channel reduces your cooldown. Roleplaying reduces it even faster! Use -help to learn more commands.", sendingUser);
                }
            }
            else
            {
                Respond(channel, $"You already have an account, {sendingUser}.", sendingUser);
            }
        }

        /// <summary>
        /// handles creating and adding a new floor through ui
        /// </summary>
        /// <param name="pc">commanding user</param>
        /// <param name="message">combined floor command</param>
        public void AddFloorAction(PlayerCard pc, string message)
        {
            string floorName;
            string description;

            try
            {
                if (!message.Contains("&gt;"))
                {
                    throw new Exception();
                }
                else
                {
                    message = message.Replace("&gt;", ">");
                }

                floorName = message.Split('>').First();
                description = message.Split('>').Last();

                if (string.IsNullOrWhiteSpace(floorName))
                {
                    throw new Exception();
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = string.Empty;
                }
            }
            catch (Exception)
            {
                Respond(null, $"Incorrect format, please try: -addfloor Floor Name>Floor Description", pc.name);
                return;
            }

            FloorCard pfc = FloorDb.GetAllFloors().Last();
            FloorCard fc = new FloorCard();
            fc.firstseen = DateTime.Now;
            fc.floor = pfc.floor + 1;
            fc.enemies = new List<string>();
            fc.notes = description;
            fc.rawenemies = string.Empty;
            fc.currentxp = 0;
            fc.neededxp = (int)(pfc.neededxp * 1.2);
            fc.name = floorName;
            FloorDb.AddNewFloor(fc);

            Respond(null, $"You've successfully created floor [b]{fc.floor}:[/b] {fc.name} - {fc.notes}", pc.name);
        }

        /// <summary>
        /// handles globally muting or unmuting the bot
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="message">source message</param>
        /// <param name="pc">player card</param>
        public void MuteAction(string channel, string message, PlayerCard pc)
        {
            string firstWord;
            try { firstWord = message.Split(' ').First(); }
            catch { firstWord = null; }
            if (firstWord != null)
            {
                if (firstWord.ToLowerInvariant().Equals("on"))
                {
                    isMuted = true;
                    Respond(null, "Muted.", pc.name);
                }
                else if (firstWord.ToLowerInvariant().Equals("off"))
                {
                    isMuted = false;
                    Respond(channel, "Unmuted.", pc.name);
                }
            }
        }

        /// <summary>
        /// handles changing the base cooldown
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="message">source message</param>
        /// <param name="pc">player card</param>
        public void BaseCooldownAction(string channel, string message, PlayerCard pc)
        {
            try
            {
                int cd = Convert.ToInt32(message);
                BaseDiveCooldown = cd;
                Respond(channel, $"[b]Base cooldown now set to: [color=green]{BaseDiveCooldown}[/color] minutes. Happy hunting![/b]", pc.name);
            }
            catch
            {
                Respond(channel, $"Invalid format. Try again! ex: -bcd 2 (for 2 hours)", pc.name);
            }
        }

        /// <summary>
        /// handles smiting a player
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="message">source message</param>
        /// <param name="pc">player card</param>
        public void SmiteAction(string channel, string message, PlayerCard pc)
        {
            PlayerCard gc;
            int goldAmount = 0;
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";
            try
            {
                goldAmount = Convert.ToInt32(message.Split(' ').Last());
                string targetName = message.Replace(goldAmount.ToString(), "").TrimEnd();
                gc = GetActiveUser(targetName);
                if (gc == null)
                {
                    Respond(channel, $"Sorry, {nickname}, but {targetName} isn't a valid user! Check your spelling or casing.", pc.name);
                    return;
                }
            }
            catch
            {
                gc = new PlayerCard();
            }

            if (string.IsNullOrWhiteSpace(gc.name))
            {
                Respond(channel, $"Sorry, {nickname}, but unable to find user: {message}. Expected format: -smite Cardinal System 34", pc.name);
            }
            else
            {
                gc.gold -= goldAmount;
                GameDb.UpdateCard(gc);
                string nicknameTwo = (string.IsNullOrWhiteSpace(gc.nickname)) ? $"[color=white]{gc.name}[/color]" : $"[color=white]{gc.nickname}[/color]";
                if (goldAmount < 0)
                {
                    Respond(channel, $"[b]{nickname}[/b] has smited [b]{nicknameTwo}[/b] for [color=yellow][b]{-goldAmount}[/b][/color] gold gained! Wait... gained!?", pc.name);
                }
                else
                {
                    Respond(channel, $"[b]{nickname}[/b] has smited [b]{nicknameTwo}[/b] for [color=yellow][b]{goldAmount}[/b][/color] gold loss!", pc.name);
                }
            }
        }

        /// <summary>
        /// handles executing a player
        /// </summary>
        /// <param name="channel">source channel</param>
        /// <param name="message">source message</param>
        /// <param name="pc">player card</param>
        public void ExecuteAction(string channel, string message, PlayerCard pc)
        {
            if (message != null)
            {
                PlayerCard tc = GameDb.GetCard(message);
                if (tc == null)
                {
                    Respond(channel, $"Sorry, can't find {message} to execute. Check spelling or casing and try again.", pc.name);
                }
                else
                {
                    if (GameDb.DeleteCard(tc.name))
                    {
                        Respond(channel, $"Executed {tc.name}. All traces they were once here have been deleted.", pc.name);
                    }
                    else
                    {
                        Respond(channel, $"Sorry, can't find {message} to execute even though we could. Something went wrong! :c", pc.name);
                    }
                }
            }
        }
    }
}
