using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        public static TagsDataDb TagsDb = new TagsDataDb();
    }

    public class TagsDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string TagsTable = "TagsData";

        public int AddNewTag(Tags spec)
        {
            try
            {
                string query = $"INSERT INTO {TagsTable} (name) VALUES (@name); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", spec.Name));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                spec.TagId = Convert.ToInt32(reader["pk"]);
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
                Console.WriteLine($"Db write error: addnewuser: {e}");
            }

            return 1;
        }

        public List<Tags> GetAllTags()
        {
            List<Tags> toReturn = new List<Tags>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT tagid, name FROM {TagsTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tags spec = new Tags();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.TagId = Convert.ToInt32(reader["tagid"]);
                                toReturn.Add(spec);
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
            return toReturn;
        }

        public Tags GetTag(int specid)
        {
            Tags toReturn = new Tags();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT tagid, name FROM {TagsTable} WHERE tagid like @tagid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@tagid", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.TagId = Convert.ToInt32(reader["tagid"]);
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

            return toReturn;
        }
    }
}