using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        public static SkillsDataDb SkillsDb = new SkillsDataDb();
    }

    public class SkillsDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string SkillsTable = "SkillData";

        public int AddNewSkill(Skill skill)
        {
            try
            {
                string query = $"INSERT INTO {SkillsTable} (name, reaction, speed, level, stamina, cost, description, effects, tags, rawstr) VALUES (@name, @reaction, @speed, @level, @stamina, @cost, @description, @effects, @tags, @rawstr); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", skill.Name));
                        command.Parameters.Add(new SQLiteParameter("@reaction", skill.Reaction));
                        command.Parameters.Add(new SQLiteParameter("@speed", skill.Speed));
                        command.Parameters.Add(new SQLiteParameter("@level", skill.Level));
                        command.Parameters.Add(new SQLiteParameter("@stamina", skill.Stamina));
                        command.Parameters.Add(new SQLiteParameter("@cost", skill.Cost));
                        command.Parameters.Add(new SQLiteParameter("@description", skill.Description));
                        command.Parameters.Add(new SQLiteParameter("@effects", JsonConvert.SerializeObject(skill.SkillEffects)));
                        command.Parameters.Add(new SQLiteParameter("@tags", JsonConvert.SerializeObject(skill.Tags)));
                        command.Parameters.Add(new SQLiteParameter("@rawstr", skill.RawStr));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                skill.SkillId = Convert.ToInt32(reader["pk"]);
                            }
                            reader.Close();
                        }

                        command.Dispose();
                    }
                    connection.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: addnewuser");
            }

            return 1;
        }

        public Skill GetSkill(int skillid)
        {
            Skill toReturn = new Skill();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT skillid, name, reaction, speed, level, stamina, cost, description, effects, tags, rawstr FROM {SkillsTable} WHERE skillid like @skillid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@skillid", skillid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.SkillId = Convert.ToInt32(reader["skillid"]);
                                toReturn.Reaction = Convert.ToBoolean(reader["reaction"]);
                                toReturn.Speed = Convert.ToInt32(reader["speed"]);
                                toReturn.Level = Convert.ToInt32(reader["level"]);
                                toReturn.Stamina = Convert.ToInt32(reader["stamina"]);
                                toReturn.Cost = Convert.ToInt32(reader["cost"]);
                                toReturn.Description = reader["description"].ToString();

                                toReturn.SkillEffects = JsonConvert.DeserializeObject<List<Effect>>(Convert.ToString(reader["effects"]));
                                toReturn.Tags = (JsonConvert.DeserializeObject<List<string>>(Convert.ToString(reader["tags"])));
                                toReturn.RawStr = reader["rawstr"].ToString();

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