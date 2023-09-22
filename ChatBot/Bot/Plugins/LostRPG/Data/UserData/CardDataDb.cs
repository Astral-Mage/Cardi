﻿using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        public static CardDataDb CardDb = new CardDataDb();
    }

    public class CardDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string CardTable = "UserData";

        /// <summary>
        /// Handles grabbing a card of specified user
        /// </summary>
        /// <param name="user">user to get the card of</param>
        /// <returns>card, if found</returns>
        public UserCard GetCard(string user)
        {
            UserCard toReturn = new UserCard(user);

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT name, alias, userid, stats, title, skills, spec, archetype FROM {CardTable} WHERE name like @name;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", user));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Alias = Convert.ToString(reader["alias"]);
                                toReturn.UserId = Convert.ToInt32(reader["userid"]);
                                toReturn.CurrentTitle = reader["title"].ToString();
                                toReturn.SetStats(JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"])));
                                var pls = Convert.ToString(reader["skills"]);
                                if (!string.IsNullOrWhiteSpace(pls))
                                {
                                    pls.Split(',').ToList().ForEach((x) =>
                                    {
                                        toReturn.Skills.Add(Convert.ToInt32(x));
                                        return;
                                    });
                                }

                                int specid = Convert.ToInt32(reader["spec"]);
                                toReturn.Spec = DataDb.SpecDb.GetSpec(specid);
                                int arcid = Convert.ToInt32(reader["archetype"]);
                                toReturn.Archetype = DataDb.ArcDb.GetArc(arcid);
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            toReturn.RpData = DataDb.RpDb.GetUserRoleplayData(toReturn.UserId);
            return toReturn;
        }

        /// <summary>
        /// deletes a specified user from the table
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>true if successful</returns>
        public bool DeleteCard(UserCard user)
        {
            int result = -1;
            using (SQLiteConnection connection = new SQLiteConnection(connstr))
            {
                connection.Open();
                string sql = $"delete from {CardTable} WHERE name like '{user}'";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    result = command.ExecuteNonQuery();
                    command.Dispose();
                }
                connection.Close();
            }

            DataDb.RpDb.DeleteUserRoleplayData(user.UserId);
            return result == 1;
        }

        public void UpdateUserCard(UserCard card)
        {
            try
            {
                string query = $"UPDATE {CardTable} SET alias = @tali, stats = @stats, title = @title, name = @name, skills = @skills, spec = @spec, archetype = @arc WHERE userid = @uid;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", card.UserId));
                        command.Parameters.Add(new SQLiteParameter("@tali", card.Alias));
                        command.Parameters.Add(new SQLiteParameter("@title", card.CurrentTitle));
                        command.Parameters.Add(new SQLiteParameter("@name", card.Name));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(card.Stats)));
                        command.Parameters.Add(new SQLiteParameter("@skills", string.Join(",", card.Skills.ToArray())));
                        command.Parameters.Add(new SQLiteParameter("@spec", card.Spec.SpecId));
                        command.Parameters.Add(new SQLiteParameter("@arc", card.Archetype.ArcId));

                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: UpdateUserCard");
            }

            DataDb.RpDb.UpdateUserRoleplayData(card.RpData);
        }

        public int AddNewUserData(UserCard card)
        {
            try
            {
                string query = $"INSERT INTO {CardTable} (name, alias, stats, title, skills, spec, archetype) VALUES (@name, @alias, @stats, @title, @skills, @spec, @arc);";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", card.Name));
                        command.Parameters.Add(new SQLiteParameter("@alias", card.Alias));
                        command.Parameters.Add(new SQLiteParameter("@title", card.CurrentTitle));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(card.Stats)));
                        command.Parameters.Add(new SQLiteParameter("@skills", string.Join(",", card.Skills.ToArray())));
                        command.Parameters.Add(new SQLiteParameter("@spec", card.Spec.SpecId));
                        command.Parameters.Add(new SQLiteParameter("@arc", card.Archetype.ArcId));


                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: addnewuser");
            }

            return GetUserId(card.Name);
        }

        /// <summary>
        /// Attempts to get a user id by string name
        /// </summary>
        /// <param name="user">user name to check</param>
        /// <returns>id of user, if any</returns>
        public int GetUserId(string user)
        {
            int toReturn = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT userid FROM {CardTable} WHERE name like @name;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", user));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn = Convert.ToInt32(reader["userid"]);
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
        public bool UserExists(string user)
        {
            int toReturn = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT COUNT(name) AS Count FROM {CardTable} WHERE name like '{user}';";
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
        public void AddNewUser(UserCard card)
        {
            card.UserId = DataDb.CardDb.AddNewUserData(card);
            DataDb.RpDb.AddNewUserRoleplayData(card.UserId);
            
        }
    }
}
