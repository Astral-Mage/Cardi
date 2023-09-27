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
        public static CallingsDataDb CallingDb = new CallingsDataDb();
    }

    public class CallingsDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string CallingTable = "CallingData";

        public int AddNewCalling(Calling call)
        {
            try
            {
                string query = $"INSERT INTO {CallingTable} (name, description, tags, buffs, debuffs, stats, skills) VALUES (@name, @description, @tags, @buffs, @debuffs, @stats, @skills); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", call.Name));
                        command.Parameters.Add(new SQLiteParameter("@description", call.Description));
                        command.Parameters.Add(new SQLiteParameter("@tags", string.Join(",", call.Tags)));
                        if (call.Buff != -1) command.Parameters.Add(new SQLiteParameter("@buffs", call.Buff));
                        else command.Parameters.Add(new SQLiteParameter("@buffs", string.Empty));
                        if (call.Debuff != -1) command.Parameters.Add(new SQLiteParameter("@debuffs", call.Debuff));
                        else command.Parameters.Add(new SQLiteParameter("@debuffs", string.Empty));
                        if (call.Skill != -1) command.Parameters.Add(new SQLiteParameter("@skills", call.Skill));
                        else command.Parameters.Add(new SQLiteParameter("@skills", string.Empty));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(call.Stats)));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                call.CallingId = Convert.ToInt32(reader["pk"]);
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

        public List<Calling> GetAllCallings()
        {
            List<Calling> toReturn = new List<Calling>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT callid, name, description, tags, buffs, debuffs, stats, skills FROM {CallingTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Calling spec = new Calling();

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.CallingId = Convert.ToInt32(reader["callid"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                if (!string.IsNullOrWhiteSpace(reader["buffs"].ToString())) spec.Buff = Convert.ToInt32(reader["buffs"]);
                                if (!string.IsNullOrWhiteSpace(reader["debuffs"].ToString())) spec.Debuff = Convert.ToInt32(reader["debuffs"]);
                                if (!string.IsNullOrWhiteSpace(reader["skills"].ToString())) spec.Skill = Convert.ToInt32(reader["skills"]);
                                if (!string.IsNullOrWhiteSpace(reader["tags"].ToString())) Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
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

        public Calling GetCalling(int callid)
        {
            Calling toReturn = new Calling();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT callid, name, description, tags, buffs, debuffs, stats, skills FROM {CallingTable} WHERE callid like @callid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@callid", callid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                if (!string.IsNullOrWhiteSpace(reader["tags"].ToString())) Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => toReturn.Tags.Add(Convert.ToInt32(x)));
                                toReturn.Stats = (JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"])));
                                toReturn.CallingId = Convert.ToInt32(reader["callid"]);
                                if (!string.IsNullOrWhiteSpace(reader["buffs"].ToString())) toReturn.Buff = Convert.ToInt32(reader["buffs"]);
                                if (!string.IsNullOrWhiteSpace(reader["debuffs"].ToString())) toReturn.Debuff = Convert.ToInt32(reader["debuffs"]);
                                if (!string.IsNullOrWhiteSpace(reader["skills"].ToString())) toReturn.Skill = Convert.ToInt32(reader["skills"]);

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