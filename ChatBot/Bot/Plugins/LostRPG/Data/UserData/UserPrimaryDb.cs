using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Enums;
using System;
using System.Data.SQLite;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string userPrimaryTable = "UserPrimaryData";

        /// <summary>
        /// deletes a specified user from the table
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>true if successful</returns>
        public bool DeleteCard(UserCard user)
        {
            int result = -1;
            using (SQLiteConnection connection = new SQLiteConnection(connString))
            {
                connection.Open();
                string sql = $"delete from {userPrimaryTable} WHERE name like '{user}'";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    result = command.ExecuteNonQuery();
                    command.Dispose();
                }
                connection.Close();
            }

            DeleteUserMagicData(user.UserId);
            DeleteUserRoleplayData(user.UserId);
            return result == 1;
        }

        public int GetUserId(string user)
        {
            int toReturn = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT userid FROM {userPrimaryTable} WHERE name like @name;";
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
        /// Handles grabbing a card of specified user
        /// </summary>
        /// <param name="user">user to get the card of</param>
        /// <returns>card, if found</returns>
        public UserCard GetCard(string user)
        {
            UserCard toReturn = new UserCard(user);
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT name, status, alias, userid FROM {userPrimaryTable} WHERE name like @name;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@name", user));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Alias = Convert.ToString(reader["alias"]);
                                toReturn.Status = (UserStatus)Convert.ToInt32(reader["status"]);
                                toReturn.UserId = Convert.ToInt32(reader["userid"]);
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

            toReturn.MagicData = GetUserMagicData(toReturn.UserId);
            toReturn.RpData = GetUserRoleplayData(toReturn.UserId);
            return toReturn;
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
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT COUNT(name) AS Count FROM {userPrimaryTable} WHERE name like '{user}';";
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

        public void UpdateUserCard(UserCard card)
        {
            try
            {
                string query = $"UPDATE {userPrimaryTable} SET status = @tsta, alias = @tali WHERE userid = @uid;";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", card.UserId));
                        command.Parameters.Add(new SQLiteParameter("@tsta", card.Status));
                        command.Parameters.Add(new SQLiteParameter("@tali", card.Alias));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Db write error: UpdateUserCard");
            }

            UpdateUserRoleplayData(card.RpData);
            UpdateUserMagicData(card.MagicData);
        }

        private int AddNewUserData(UserCard card)
        {
            try
            {
                string query = $"INSERT INTO {userPrimaryTable} (name, status, alias) VALUES (@name, @status, @alias);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", card.Name));
                        command.Parameters.Add(new SQLiteParameter("@status", card.Status));
                        command.Parameters.Add(new SQLiteParameter("@alias", card.Alias));
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
        /// adds a new user to the table
        /// </summary>
        /// <param name="pc">card of the user to add</param>
        /// <returns>treu if successful</returns>
        public void AddNewUser(UserCard card)
        {
            card.UserId = AddNewUserData(card);
            AddNewUserMagicData(card);
            AddNewUserRoleplayData(card.UserId);
        }
    }
}
