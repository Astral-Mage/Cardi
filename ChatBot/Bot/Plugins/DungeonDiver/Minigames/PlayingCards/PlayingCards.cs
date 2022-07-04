using ChatApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Accord;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.DungeonDiver.Minigames.PlayingCardGame
{
    public static class Util
    {
        private static readonly Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class PlayingCards : PluginBase
    {
        public PlayingCards(ApiConnection api, string commandChar) : base(api, commandChar)
        {
            Games = new List<Game>();
        }

        readonly List<Game> Games = null;

        public List<PlayingCard> CreateNewDeck(bool includeJokers)
        {
            List<PlayingCard> toReturn = new List<PlayingCard>();

            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                if (rank == Rank.Joker) continue;

                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    PlayingCard tpc = new PlayingCard() { _rank = rank, _suit = suit };
                    toReturn.Add(tpc);
                }
            }

            if (includeJokers)
            {
                PlayingCard joker1 = new PlayingCard() { _rank = Rank.Joker, _suit = Suit.Hearts };
                PlayingCard joker2 = new PlayingCard() { _rank = Rank.Joker, _suit = Suit.Hearts };
                toReturn.Add(joker1);
                toReturn.Add(joker2);
            }

            return toReturn;
        }

        public Game GetGame(string playername)
        {
            try
            {
                Game toReturn = new Game();
                toReturn = Games.First(x => x.Players.Any(y => y.player.name.Equals(playername)));
                return toReturn;
            }
            catch
            {
                throw new Exception($"No open game found for user: {playername}");
            }
        }

        public string GetTableString(Game _game)
        {
            string tosend = "[b]-- Current Table State --[/b]\\n";

            foreach (Player player in _game.Players)
            {
                tosend += $"[b]{player.hand.Count()}[/b] : {player.player.GetPublicName()}'s cards in hand.";
                tosend += "\\n";
            }
            tosend += "\\n";
            tosend += "[b]Live Cards on Table:[/b] ";

            foreach (PlayingCard card in _game.Table)
            {
                tosend += $"{card.GetDescription()} ";
            }
            tosend += "\\n";

            tosend += "     [b]Cards in Discard:[/b]  ";

            foreach (PlayingCard card in _game.Discards)
            {
                tosend += $"{card.GetDescription()} ";
            }

            tosend += "\\n";
            tosend += $" [b]Remaining in Deck:[/b] {_game.Deck.Count()}";

            return tosend;
        }
        public string GetHandString(Player player)
        {
            string tosend = $"[color=white]{player.player.GetPublicName()}'s Hand ({player.hand.Count()}):     ";
            if (player.hand.Count() == 0)
            {
                tosend = $"{player.player.GetPublicName()} has no cards in hand.";
            }

            int counter = 0;
            foreach (PlayingCard card in player.hand)
            {
                if (counter != 0)
                {
                    tosend += "        ";
                }
                counter++;
                tosend += $" [sub]{counter}:[/sub] {card.GetDescription()}";
            }
            tosend += "[/color]";
            return tosend;
        }

        public void HandleUserCommands(string command, string channel, string message, PlayerCard pc)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                SystemController.Instance.Respond(channel, $"Sorry, {pc.name}, but please reformat your command.", pc.name);
                return;
            }

            switch (command)
            {
                case CommandStrings.Help:
                    {
                        string tosend = string.Empty;

                        tosend += "This is the experimental Card Game system! Yaaaay!" +
                            "\\n\\nYou can type [color=pink]-deck new[/color] to start a new game." +
                            "\\nYou can override this with players by typing -deck new Player1,Player2,Player3,etc..." +
                            "\\nYou can also type [color=pink]-deck addplayer PlayerName[/color] to add players one by one!" +
                            "\\n[color=pink]-deck removeplayer PlayerName[/color] can remove a player." +
                            "\\n\\nOther commands include..." +
                            "\\n[color=pink]-deck draw[/color]: to draw a card into your hand." +
                            "\\n[color=pink]-deck discard #[/color]: to discard a specific card from your hand." +
                            "\\n[color=pink]-deck flip[/color]: to flip a card from the top of the deck onto the table." +
                            "\\n[color=pink]-deck flipdead[/color]: to move a card from the top of the deck to discard." +
                            "\\n[color=pink]-deck leavegame[/color]: to leave a game you're current in." +
                            "\\n[color=pink]-deck stopgame[/color]: to completely stop a game you're in." +
                            "\\n[color=pink]-deck players[/color]: to list the players of the game you're currently in." +
                            "\\n[color=pink]-deck table[/color]: to see the current game's table." +
                            "\\n\\nNote: Not all features currently implemented.";

                        SystemController.Instance.Respond(null, tosend, pc.name);
                    }
                    break;
                case CommandStrings.PlayingCards_NewGame:
                    {
                        // check if user is already in a game
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);
                            SystemController.Instance.Respond(channel, $"Sorry, but you're already listed as in a game, [color=white]{pc.GetPublicName()}[/color]. Please leave it first with -leavegame or -stopgame.", pc.name);
                            return;
                        }
                        catch { }

                        if (string.IsNullOrWhiteSpace(channel))
                        {
                            SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                            return;
                        }

                        // personal game
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            Player player = new Player
                            {
                                player = pc
                            };
                            _game = new Game() { Players = new List<Player>() { player }, Deck = CreateNewDeck(false), Table = new List<PlayingCard>() };
                            Games.Add(_game);
                        }

                        // private game
                        else
                        {
                            List<Player> players = new List<Player>();

                            foreach (string s in message.Split(','))
                            {
                                PlayerCard tpc = GameDb.GetCard(s);
                                if (tpc != null)
                                {
                                    Player toAdd = new Player() { hand = new List<PlayingCard>(), player = tpc };
                                    players.Add(toAdd);
                                }
                            }
                            _game = new Game() { Players = players, Deck = CreateNewDeck(false), Table = new List<PlayingCard>() };
                            Games.Add(_game);
                        }

                        SystemController.Instance.Respond(channel, $"your new game of cards has been created, [color=white]{pc.GetPublicName()}[/color]!", pc.name);
                        Util.Shuffle(_game.Deck);
                    }
                    break;
                case "leavegame":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            Player player = _game.Players.First(x => x.player.name.Equals(pc.name));
                            _game.Players.Remove(player);
                            if (_game.Players.Count() == 0)
                            {
                                Games.Remove(_game);
                                SystemController.Instance.Respond(channel, $"You were the last remaining member, [color=white]{pc.GetPublicName()}[/color]. The game has been destroyed.", pc.name);
                                return;
                            }
                            SystemController.Instance.Respond(channel, $"You have left the game, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games to leave, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "stopgame":
                    {
                        // stops a game entirely, booting all players
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            Games.Remove(_game);
                            SystemController.Instance.Respond(channel, $"You have stopped the game, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games to stop, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "addplayer":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(message))
                            {
                                SystemController.Instance.Respond(channel, $"You must include a player to add, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            PlayerCard tpc = GameDb.GetCard(message);
                            if (tpc == null)
                            {
                                SystemController.Instance.Respond(channel, $"{message} isn't a registered adventurer, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            if (_game.Players.Any(x => x.player.name.Equals(message)))
                            {
                                SystemController.Instance.Respond(channel, $"{tpc.GetPublicName()} is already in a game, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            Player newPlayer = new Player
                            {
                                player = tpc,
                                hand = new List<PlayingCard>()
                            };
                            _game.Players.Add(newPlayer);
                            SystemController.Instance.Respond(channel, $"You have successfully added {tpc.GetPublicName()}, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You must be in a game to use this command, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "players":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            string tosend = "";

                            for (int i = 0; i < _game.Players.Count(); i++)
                            {
                                tosend += $"{_game.Players[i].player.GetPublicName()}";
                                if (i != _game.Players.Count() - 1)
                                {
                                    tosend += ", ";
                                }
                            }
                            SystemController.Instance.Respond(channel, $"Current players: {tosend}.", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"No games found that you're a member of, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "removeplayer":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(message))
                            {
                                SystemController.Instance.Respond(channel, $"You must include a player to remove, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            PlayerCard tpc = GameDb.GetCard(message);
                            if (tpc == null)
                            {
                                SystemController.Instance.Respond(channel, $"{message} isn't a registered adventurer, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            if (!_game.Players.Any(x => x.player.name.Equals(message)))
                            {
                                SystemController.Instance.Respond(channel, $"{tpc.GetPublicName()} isn't in a game, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            Player toRemove = _game.Players.First(x => x.player.name.Equals(tpc.name));
                            _game.Players.Remove(toRemove);
                            SystemController.Instance.Respond(channel, $"You have successfully removed {tpc.GetPublicName()}, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You must be in a game to use this command, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "hand":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);
                            SystemController.Instance.Respond(channel, GetHandString(_game.Players.First(x => x.player.name.Equals(pc.name))), pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "draw":
                    {
                        // draws x cards from the deck
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            // determine how many cards to draw
                            int cardsToDraw = 1;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(message))
                                {
                                    cardsToDraw = Convert.ToInt32(message);
                                }
                            }
                            catch
                            {
                                cardsToDraw = 1;
                            }

                            // draw cards
                            if (_game.Deck.Count() == 0)
                            {
                                SystemController.Instance.Respond(channel, $"Deck is empty, [color=white]{pc.GetPublicName()}[/color]. Can't draw any more cards!", pc.name);
                                return;
                            }
                            else if (_game.Deck.Count() < cardsToDraw)
                            {
                                SystemController.Instance.Respond(channel, $"Deck does not contain enough cards ({_game.Deck.Count()}), [color=white]{pc.GetPublicName()}[/color]. Can't draw that many cards!", pc.name);
                                return;
                            }

                            for (int i = 0; i < cardsToDraw; i++)
                            {
                                PlayingCard tcard = _game.Deck.First();
                                _game.Deck.Remove(tcard);
                                _game.Players.First(x => x.player.name.Equals(pc.name)).hand.Add(tcard);
                            }

                            SystemController.Instance.Respond(null, GetHandString(_game.Players.First(x => x.player.name.Equals(pc.name))), pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                            return;
                        }
                    }
                    break;
                case "discard":
                    {
                        // discards a card
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            int cardtodiscard = 0;
                            try
                            {
                                cardtodiscard = Convert.ToInt32(message);
                            }
                            catch
                            {
                                SystemController.Instance.Respond(channel, $"Please select a valid card number!", pc.name);
                                return;
                            }

                            if (cardtodiscard <= 0)
                            {
                                SystemController.Instance.Respond(channel, $"Please select a valid card number!", pc.name);
                                return;
                            }

                            Player player = _game.Players.First(x => x.player.name.Equals(pc.name));
                            if (cardtodiscard > player.hand.Count())
                            {
                                SystemController.Instance.Respond(channel, $"Please select a valid card number!", pc.name);
                                return;
                            }

                            PlayingCard tpc = player.hand[cardtodiscard-1];
                            player.hand.Remove(tpc);
                            _game.Discards.Add(tpc);
                            SystemController.Instance.Respond(channel, $"\\nYou discard a {tpc.GetDescription()} from your hand.", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "shuffle":
                    {
                        // shuffles cards in the deck
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            Game newGame = new Game() { Players = new List<Player>(_game.Players), Deck = CreateNewDeck(false), Table = new List<PlayingCard>(), Discards = new List<PlayingCard>() };
                            _game = newGame;
                            SystemController.Instance.Respond(channel, $"\\nYou shuffle up and start a fresh game.", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "flip":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            PlayingCard tpc = _game.Deck.First();
                            _game.Deck.Remove(tpc);
                            _game.Table.Add(tpc);
                            SystemController.Instance.Respond(channel, $"You flip a {tpc.GetDescription()} onto the table.", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "flipdead":
                    {
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);

                            if (string.IsNullOrWhiteSpace(channel))
                            {
                                SystemController.Instance.Respond(channel, $"This command must be done in public, {pc.GetPublicName()}.", pc.name);
                                return;
                            }

                            PlayingCard tpc = _game.Deck.First();
                            _game.Deck.Remove(tpc);
                            _game.Discards.Add(tpc);
                            SystemController.Instance.Respond(channel, $"\\nYou flip a {tpc.GetDescription()} into the discard pile.", pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "table":
                    {
                        // shows the current visible table
                        Game _game;
                        try
                        {
                            _game = GetGame(pc.name);
                            SystemController.Instance.Respond(channel, GetTableString(_game), pc.name);
                        }
                        catch
                        {
                            SystemController.Instance.Respond(channel, $"You aren't in any games, [color=white]{pc.GetPublicName()}[/color].", pc.name);
                        }
                    }
                    break;
                case "insert":
                    {
                        // inserts card into a specific spot in the deck if able

                    }
                    break;
                case "undo":
                    {
                        // undoes last action

                    }
                    break;
                case "return":
                    {
                        // returns a card to the bottom of the deck

                    }
                    break;
            }

        }
    }
}
