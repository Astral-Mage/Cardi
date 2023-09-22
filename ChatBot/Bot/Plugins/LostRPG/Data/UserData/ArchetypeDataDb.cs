using ChatBot.Bot.Plugins.LostRPG.CardSystem;
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
        public static ArchetypeDataDb ArcDb = new ArchetypeDataDb();
    }

    public class ArchetypeDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string ArchetypeTable = "ArcData";

        public int AddNewArchetype(Archetype spec)
        {
            try
            {
                string query = $"INSERT INTO {ArchetypeTable} (name, description, specs, stats, buffs, debuffs, rawstring, skills) VALUES (@name, @description, @specs, @stats, @buffs, @debuffs, @rawstring, @skills); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", spec.Name));
                        command.Parameters.Add(new SQLiteParameter("@rawstring", spec.RawString));
                        command.Parameters.Add(new SQLiteParameter("@description", spec.Description));
                        command.Parameters.Add(new SQLiteParameter("@buffs", string.Join(",", spec.Buffs)));
                        command.Parameters.Add(new SQLiteParameter("@debuffs", string.Join(",", spec.Debuffs)));
                        command.Parameters.Add(new SQLiteParameter("@skills", string.Join(",", spec.Skills)));
                        command.Parameters.Add(new SQLiteParameter("@specs", string.Join(",", spec.Specs)));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(spec.Stats)));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                spec.ArcId = Convert.ToInt32(reader["pk"]);
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

        public List<Archetype> GetAllArchetypes()
        {
            List<Archetype> toReturn = new List<Archetype>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT arcid, name, description, specs, stats, buffs, debuffs, rawstring, skills FROM {ArchetypeTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Archetype spec = new Archetype();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                spec.ArcId = Convert.ToInt32(reader["arcid"]);
                                Convert.ToString(reader["specs"]).Split(',').ToList().ForEach(x => spec.Specs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["skills"]))) Convert.ToString(reader["skills"]).Split(',').ToList().ForEach(x => spec.Skills.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["buffs"]))) Convert.ToString(reader["buffs"]).Split(',').ToList().ForEach(x => spec.Buffs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["debuffs"]))) Convert.ToString(reader["debuffs"]).Split(',').ToList().ForEach(x => spec.Debuffs.Add(Convert.ToInt32(x)));
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

        public Archetype GetArc(int specid)
        {
            Archetype toReturn = new Archetype();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT arcid, name, description, specs, stats, buffs, debuffs, rawstring, skills FROM {ArchetypeTable} WHERE arcid like @arcid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@arcid", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                toReturn.ArcId = Convert.ToInt32(reader["arcid"]);
                                Convert.ToString(reader["specs"]).Split(',').ToList().ForEach(x => toReturn.Specs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["skills"]))) Convert.ToString(reader["skills"]).Split(',').ToList().ForEach(x => toReturn.Skills.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["buffs"]))) Convert.ToString(reader["buffs"]).Split(',').ToList().ForEach(x => toReturn.Buffs.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["debuffs"]))) Convert.ToString(reader["debuffs"]).Split(',').ToList().ForEach(x => toReturn.Debuffs.Add(Convert.ToInt32(x)));
                                toReturn.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
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