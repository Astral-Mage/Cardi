using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.MagicSystem;
using System;
using System.Data.SQLite;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string userMagicTable = "UserMagicData";

        public void DeleteUserMagicData(int userId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connString))
            {
                connection.Open();
                string sql = $"delete from {userMagicTable} WHERE userid like '{userId}'";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connection.Close();
            }
        }

        public void UpdateUserMagicData(MagicData magic)
        {
            try
            {
                string query = $"UPDATE {userMagicTable} SET magicid = @mid, secondarymagicid = @mid2 WHERE userid = @uid;";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", magic.UserId));
                        command.Parameters.Add(new SQLiteParameter("@mid", magic.PrimaryMagic.MagicId));
                        command.Parameters.Add(new SQLiteParameter("@mid2", magic.SecondaryMagic.MagicId));

                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: UpdateUserRoleplayData");
            }
        }

        public MagicData GetUserMagicData(int userId)
        {
            MagicData toReturn = new MagicData
            {
                UserId = userId
            };
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT magicid, secondarymagicid FROM {userMagicTable} WHERE userid = @uid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@uid", userId));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.PrimaryMagic = new Magic();
                                toReturn.SecondaryMagic = new Magic();

                                toReturn.PrimaryMagic.MagicId = Convert.ToInt32(reader["magicid"]);
                                toReturn.SecondaryMagic.MagicId = Convert.ToInt32(reader["secondarymagicid"]);
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
            }

            toReturn.PrimaryMagic = GetMagicById(toReturn.PrimaryMagic.MagicId);
            toReturn.SecondaryMagic = GetMagicById(toReturn.SecondaryMagic.MagicId);
            return toReturn;
        }

        public Magic GetMagicById(int magicid)
        {
            Magic toReturn = new Magic();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT name, description, color FROM {magicTable} WHERE magicid = @mid";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@mid", magicid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Color = Convert.ToString(reader["color"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                toReturn.Name = Convert.ToString(reader["name"]);
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
                return null;
            }
        }

        private void AddNewUserMagicData(UserCard card)
        {
            try
            {
                string query = $"INSERT INTO {userMagicTable} (userid, magicid, secondarymagicid) VALUES (@uid, @mid, @mid2);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", card.UserId));
                        command.Parameters.Add(new SQLiteParameter("@mid", card.MagicData.PrimaryMagic.MagicId));
                        command.Parameters.Add(new SQLiteParameter("@mid2", null));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Db write error: addnewusermagictable: {e}");
            }
        }
    }
}
