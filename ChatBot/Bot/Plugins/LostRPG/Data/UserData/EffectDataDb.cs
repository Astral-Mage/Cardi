using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
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
        public static EffectDataDb EffectDb = new EffectDataDb();
    }

    public class EffectDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string EffectTable = "EffectData";

        public int AddNewEffect(Effect effect)
        {
            try
            {
                string query = $"INSERT INTO {EffectTable} (name, effecttype, description, tags, stats, duration, procchance, proctrigger, target, creationdate) VALUES (@name, @etype, @description, @tags, @stats, @dur, @procc, @proct, @target, @creationdate); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", effect.Name));
                        command.Parameters.Add(new SQLiteParameter("@etype", effect.EffectType));
                        command.Parameters.Add(new SQLiteParameter("@description", effect.Description));
                        command.Parameters.Add(new SQLiteParameter("@tags", string.Join(",", effect.Tags)));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(effect.Stats)));
                        command.Parameters.Add(new SQLiteParameter("@dur", effect.GlobalDuration.Ticks));
                        command.Parameters.Add(new SQLiteParameter("@procc", effect.ProcChance));
                        command.Parameters.Add(new SQLiteParameter("@proct", effect.ProcTrigger));
                        command.Parameters.Add(new SQLiteParameter("@target", effect.Target));
                        command.Parameters.Add(new SQLiteParameter("@creationdate", effect.CreationDate.ToString()));


                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                effect.EffectId = Convert.ToInt32(reader["pk"]);
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

        public List<Effect> GetAllEffectsByType(EffectTypes etype)
        {
            List<Effect> toReturn = new List<Effect>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT effectid, name, description, tags, stats, duration, procchance, proctrigger, effecttype, target, creationdate FROM {EffectTable} WHERE effecttype = '@etype';";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@etype", etype));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Effect spec = new Effect();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.EffectId = Convert.ToInt32(reader["effectid"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                //Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
                                spec.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                spec.ProcTrigger = (ProcTriggers)Convert.ToInt32(reader["proctrigger"]);
                                spec.EffectType = (EffectTypes)Convert.ToInt32(reader["effecttype"]);
                                spec.ProcChance = Convert.ToInt32(reader["procchance"]);
                                if (reader["duration"].ToString().Equals("9.22337203685478E+18"))
                                    spec.GlobalDuration = TimeSpan.MaxValue;
                                else
                                    spec.GlobalDuration = new TimeSpan(long.Parse(reader["duration"].ToString()));
                                spec.Target = (EffectTargets)Convert.ToInt32(reader["target"]);
                                spec.CreationDate = Convert.ToDateTime(reader["creationdate"]);


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

        public Effect GetEffect(int specid)
        {
            Effect toReturn = new Effect();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT effectid, name, description, tags, stats, duration, procchance, proctrigger, effecttype, target, creationdate FROM {EffectTable} WHERE effectid like @effectid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@effectid", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.EffectId = Convert.ToInt32(reader["effectid"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                //Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => toReturn.Tags.Add(Convert.ToInt32(x)));
                                toReturn.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                toReturn.ProcTrigger = (ProcTriggers)Convert.ToInt32(reader["proctrigger"]);
                                toReturn.ProcChance = Convert.ToInt32(reader["procchance"]);

                                var idk = reader["duration"].ToString();
                                if (reader["duration"].ToString().Equals("9.22337203685478E+18"))
                                    toReturn.GlobalDuration = TimeSpan.MaxValue;
                                else
                                    toReturn.GlobalDuration = new TimeSpan(long.Parse(reader["duration"].ToString()));
                                toReturn.Target = (EffectTargets)Convert.ToInt32(reader["target"]);
                                toReturn.EffectType = (EffectTypes)Convert.ToInt32(reader["effecttype"]);
                                toReturn.CreationDate = Convert.ToDateTime(reader["creationdate"]);
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