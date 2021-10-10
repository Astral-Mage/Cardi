using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChatBot.Bot.Plugins
{
    class GameDb
    {
        /// <summary>
        /// table name
        /// </summary>
        static readonly string playerTable = "Players";

        /// <summary>
        /// database location
        /// </summary>
#if DEBUG
        
        static readonly string connString = @"Data Source = ..\..\Databases\Debug\GameCore.db; Version=3;";
#else
        static readonly string connString = @"Data Source = ..\..\Databases\Release\GameCore.db; Version=3;";
#endif
        /// <summary>
        /// returns cards of all current players
        /// </summary>
        /// <returns>list of cards</returns>
        public static List<PlayerCard> GetAllCards()
        {
            List<PlayerCard> toReturn = new List<PlayerCard>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT * FROM {playerTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PlayerCard card = JsonConvert.DeserializeObject<PlayerCard>(Convert.ToString(reader["card"]));
                                toReturn.Add(card);
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();
                    return toReturn;
                }
            }
            catch (Exception)
            {
                return toReturn;
            }
        }

        /// <summary>
        /// deletes a specified user from the table
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>true if successful</returns>
        public static bool DeleteCard(string user)
        {
            int result = -1;
            using (SQLiteConnection connection = new SQLiteConnection(connString))
            {
                connection.Open();
                string sql = $"delete from {playerTable} WHERE name = '{user}'";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    result = command.ExecuteNonQuery();
                    command.Dispose();
                }
                connection.Close();
            }
            return result == 1;
        }

        /// <summary>
        /// Handles updating a user's card data
        /// </summary>
        /// <param name="pc">card of the user to update</param>
        /// <returns>true if successful</returns>
        public static bool UpdateCard(PlayerCard pc)
        {
            try
            {
                int result = -1;
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"UPDATE {playerTable} SET card = @card WHERE name = @name;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@card", JsonConvert.SerializeObject(pc)));
                        command.Parameters.Add(new SQLiteParameter("@name", pc.name));
                        result = command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                    return result == 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Handles grabbing a card of specified user
        /// </summary>
        /// <param name="user">user to get the card of</param>
        /// <returns>card, if found</returns>
        public static PlayerCard GetCard(string user)
        {
            //List<PlayerCard> cardsToConvert = GetAllCardsTemp();
            //foreach (PlayerCard card in cardsToConvert)
            //{
            //    AddNewUser(card);
            //}

            PlayerCard toReturn = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT name, card FROM {playerTable} WHERE name = @name;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", user));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn = JsonConvert.DeserializeObject<PlayerCard>(Convert.ToString(reader["card"]));
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();
                    return toReturn;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return toReturn;
            }
        }

        /// <summary>
        /// confirms if the user exists in the table
        /// </summary>
        /// <param name="user">user to check</param>
        /// <returns>true if user exists</returns>
        public static bool UserExists(string user)
        {
            int toReturn = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT COUNT(name) AS Count FROM {playerTable} WHERE name = '{user}';";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn = Convert.ToInt32(reader["Count"]);
                                break;
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();

                }
                return toReturn == 1;
            }
            catch (Exception)
            {
                return toReturn == 1;
            }
        }

        /// <summary>
        /// adds a new user to the table
        /// </summary>
        /// <param name="pc">card of the user to add</param>
        /// <returns>treu if successful</returns>
        public static bool AddNewUser(PlayerCard pc)
        {
            int toReturn = -1;
            try
            {
                string query = $"INSERT INTO {playerTable} (name, card) VALUES (@name, @card);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", pc.name));
                        command.Parameters.Add(new SQLiteParameter("@card", JsonConvert.SerializeObject(pc)));
                        toReturn = command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
                
                return toReturn == 1;
            }
            catch (Exception)
            {
                return toReturn == 1;
            }
        }
    }
}