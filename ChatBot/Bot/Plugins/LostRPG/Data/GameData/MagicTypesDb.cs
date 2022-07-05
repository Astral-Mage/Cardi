using ChatBot.Bot.Plugins.LostRPG.MagicSystem;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string magicTable = "MagicTypes";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public List<Magic> GetMagic()
        {
            List<Magic> toReturn = new List<Magic>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT magicid, color, description, name FROM {magicTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Magic tm = new Magic
                                {
                                    Color = Convert.ToString(reader["color"]),
                                    Description = Convert.ToString(reader["description"]),
                                    MagicId = Convert.ToInt32(reader["magicid"]),
                                    Name = Convert.ToString(reader["name"])
                                };

                                toReturn.Add(tm);
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
    }
}
