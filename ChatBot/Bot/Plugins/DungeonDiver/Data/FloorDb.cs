using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ChatBot.Bot.Plugins
{
    class FloorDb
    {
        /// <summary>
        /// our floor table
        /// </summary>
        static string floorTable = "Floors";

        /// <summary>
        /// path to our database location
        /// </summary>
#if DEBUG
        
        static readonly string connString = @"Data Source = ..\..\Databases\Debug\GameCore.db; Version=3;";
#else
        static readonly string connString = @"Data Source = ..\..\Databases\Release\GameCore.db; Version=3;";
#endif
        /// <summary>
        /// Handles updating a user's card data
        /// </summary>
        /// <param name="fc">card of the user to update</param>
        /// <returns>true if successful</returns>
        public static bool UpdateFloor(FloorCard fc)
        {
            try
            {
                int result = -1;
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"UPDATE {floorTable} SET data = @data WHERE level = @level;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(fc)));
                        command.Parameters.Add(new SQLiteParameter("@level", fc.floor));
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
        /// returns cards of all current players
        /// </summary>
        /// <returns>list of cards</returns>
        public static List<FloorCard> GetAllFloors()
        {
            List<FloorCard> toReturn = new List<FloorCard>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT * FROM {floorTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FloorCard card = JsonConvert.DeserializeObject<FloorCard>(Convert.ToString(reader["data"]));
                                toReturn.Add(card);
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();
                    return toReturn.OrderBy(x => x.floor).ToList();
                }
            }
            catch (Exception)
            {
                return toReturn;
            }
        }

        /// <summary>
        /// Handles grabbing a card of specified user
        /// </summary>
        /// <param name="user">user to get the card of</param>
        /// <returns>card, if found</returns>
        public static FloorCard GetFloor()
        {
            //List<FloorCard> cardsToConvert = GetAllFloorsTemp();
            //foreach (FloorCard card in cardsToConvert)
            //{
            //    AddNewFloor(card);
            //}

            FloorCard toReturn = null;
            List<FloorCard> theList = new List<FloorCard>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT level, data FROM {floorTable} ORDER BY level;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                theList.Add(JsonConvert.DeserializeObject<FloorCard>(Convert.ToString(reader["data"])));
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();

                    foreach(var fc in theList)
                    {
                        if (fc.currentxp >= fc.neededxp)
                        {
                            continue;
                        }

                        toReturn = fc;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return toReturn;
            }

            return toReturn;
        }

        /// <summary>
        /// adds a new user to the table
        /// </summary>
        /// <param name="fc">card of the user to add</param>
        /// <returns>treu if successful</returns>
        public static bool AddNewFloor(FloorCard fc)
        {
            int toReturn = -1;
            try
            {
                string query = $"INSERT INTO {floorTable} (data) VALUES (@data);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(fc)));
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