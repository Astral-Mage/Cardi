using ChatBot.Bot.Plugins.LostRPG.CardSystem;
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

        public void UpdateUserMagicData(Magic magic)
        {
            try
            {
                string query = $"UPDATE {userMagicTable} SET magicid = @mid WHERE userid = @uid;";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", magic.UserId));
                        command.Parameters.Add(new SQLiteParameter("@mid", magic.MagicId));
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

        public Magic GetUserMagicData(int userId)
        {
            Magic toReturn = new Magic();
            toReturn.UserId = userId;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT {magicTable}.name, {magicTable}.color, {magicTable}.description, name FROM {magicTable} INNER JOIN {userMagicTable} ON {magicTable}.magicid = {userMagicTable}.magicid WHERE {userMagicTable}.userid = @uid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@uid", userId));
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
                string query = $"INSERT INTO {userMagicTable} (userid, magicid) VALUES (@uid, @mid);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", card.UserId));
                        command.Parameters.Add(new SQLiteParameter("@mid", card.MainMagic.MagicId));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: addnewusermagictable");
            }
        }
    }
}
