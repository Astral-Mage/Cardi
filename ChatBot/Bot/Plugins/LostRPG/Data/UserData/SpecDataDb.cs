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
        public static SpecDataDb SpecDb = new SpecDataDb();
    }

    public class SpecDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string SpecTable = "SpecData";

        public int AddNewSpec(Specialization spec)
        {
            try
            {
                string query = $"INSERT INTO {SpecTable} (name, description, tags, buffs, debuffs, stats) VALUES (@name, @description, @tags, @buffs, @debuffs, @stats); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", spec.Name));
                        command.Parameters.Add(new SQLiteParameter("@description", spec.Description));
                        command.Parameters.Add(new SQLiteParameter("@tags", string.Join(",", spec.Tags)));
                        command.Parameters.Add(new SQLiteParameter("@buffs", string.Join(",", spec.Buffs)));
                        command.Parameters.Add(new SQLiteParameter("@debuffs", string.Join(",", spec.Debuffs)));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(spec.Stats)));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                spec.SpecId = Convert.ToInt32(reader["pk"]);
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

        public List<Specialization> GetAllSpecs()
        {
            List<Specialization> toReturn = new List<Specialization>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT specid, name, description, tags, buffs, debuffs, stats FROM {SpecTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Specialization spec = new Specialization();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.SpecId = Convert.ToInt32(reader["specid"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                    Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["buffs"])))
                                    Convert.ToString(reader["buffs"]).Split(',').ToList().ForEach(x => spec.Buffs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["debuffs"])))
                                    Convert.ToString(reader["debuffs"]).Split(',').ToList().ForEach(x => spec.Debuffs.Add(Convert.ToInt32(x)));
                                spec.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
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

        public Specialization GetSpec(int specid)
        {
            Specialization toReturn = new Specialization();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT specid, name, description, tags, buffs, debuffs, stats FROM {SpecTable} WHERE specid like @specid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@specid", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => toReturn.Tags.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["buffs"])))
                                    Convert.ToString(reader["buffs"]).Split(',').ToList().ForEach(x => toReturn.Buffs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["debuffs"])))
                                    Convert.ToString(reader["debuffs"]).Split(',').ToList().ForEach(x => toReturn.Debuffs.Add(Convert.ToInt32(x)));

                                toReturn.Stats = (JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"])));
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