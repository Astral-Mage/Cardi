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

        public int AddNewEffect(BaseEffect effect)
        {
            try
            {
                string query = $"INSERT INTO {EffectTable} (name, effecttype, description, tags, stats, duration, procchance, proctrigger, target) VALUES (@name, @etype, @description, @tags, @stats, @dur, @procc, @proct, @target); SELECT last_insert_rowid() as pk;";

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

        public List<BaseEffect> GetAllEffectsByType(EffectTypes etype)
        {
            List<BaseEffect> toReturn = new List<BaseEffect>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT effectid, name, description, tags, stats, duration, procchance, proctrigger, effecttype, target FROM {EffectTable} WHERE ;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BaseEffect spec = new BaseEffect();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.EffectId = Convert.ToInt32(reader["effectid"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
                                spec.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                spec.ProcTrigger = (ProcTriggers)Convert.ToInt32(reader["proctrigger"]);
                                spec.EffectType = (EffectTypes)Convert.ToInt32(reader["effecttype"]);
                                spec.ProcChance = Convert.ToInt32(reader["procchance"]);
                                spec.GlobalDuration = new TimeSpan(Convert.ToInt32(reader["duration"]));
                                spec.Target = (EffectTargets)Convert.ToInt32(reader["target"]);


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

        public BaseEffect GetEffect(int specid)
        {
            BaseEffect toReturn = new BaseEffect();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT effectid, name, description, tags, stats, duration, procchance, proctrigger, effecttype, target FROM {EffectTable} WHERE specid like @effectid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@specid", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.EffectId = Convert.ToInt32(reader["effectid"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => toReturn.Tags.Add(Convert.ToInt32(x)));
                                toReturn.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                toReturn.ProcTrigger = (ProcTriggers)Convert.ToInt32(reader["proctrigger"]);
                                toReturn.ProcChance = Convert.ToInt32(reader["procchance"]);
                                toReturn.GlobalDuration = new TimeSpan(Convert.ToInt32(reader["duration"]));
                                toReturn.Target = (EffectTargets)Convert.ToInt32(reader["target"]);
                                toReturn.EffectType = (EffectTypes)Convert.ToInt32(reader["effecttype"]);
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