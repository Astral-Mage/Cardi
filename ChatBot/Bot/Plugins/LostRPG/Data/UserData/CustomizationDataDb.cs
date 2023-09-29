using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        public static CustomizationDataDb CustomDb = new CustomizationDataDb();
    }

    public class CustomizationDataDb : DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string CustomizationTable = "CustomizationData";

        public int AddNewCustomization(BaseCustomization custom)
        {
            try
            {
                string query = $"INSERT INTO {CustomizationTable} (name, description, tags, stats, effects, rawstring, skills, ctype) VALUES (@name, @description, @tags, @stats, @effects, @rawstring, @skills, @ctype); SELECT last_insert_rowid() as pk;";

                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@name", custom.Name));
                        command.Parameters.Add(new SQLiteParameter("@rawstring", custom.RawString));
                        command.Parameters.Add(new SQLiteParameter("@description", custom.Description));
                        command.Parameters.Add(new SQLiteParameter("@effects", string.Join(",", custom.Effects)));
                        command.Parameters.Add(new SQLiteParameter("@skills", string.Join(",", custom.Skills)));
                        command.Parameters.Add(new SQLiteParameter("@tags", string.Join(",", custom.Tags)));
                        command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(custom.Stats)));
                        command.Parameters.Add(new SQLiteParameter("@ctype", custom.Customization));


                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                custom.Id = Convert.ToInt32(reader["pk"]);
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

        public List<BaseCustomization> GetAllCustomizationsByType(CustomizationTypes type)
        {
            List<BaseCustomization> toReturn = new List<BaseCustomization>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT id, name, description, tags, stats, effects, rawstring, skills, ctype FROM {CustomizationTable} WHERE ctype = @type;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@type", (int)type));

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BaseCustomization spec = null;
                                var cType = (CustomizationTypes)Convert.ToInt32(reader["ctype"]);
                                switch (cType)
                                {
                                    case CustomizationTypes.Archetype:
                                        {
                                            spec = new Archetype();

                                        }
                                        break;
                                    case CustomizationTypes.Specialization:
                                        {
                                            spec = new Specialization();
                                        }
                                        break;
                                    case CustomizationTypes.Calling:
                                        {
                                            spec = new Calling();
                                        }
                                        break;
                                    default:
                                        throw new Exception();
                                }
                                spec.Customization = cType;
                                spec.Name = Convert.ToString(reader["name"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                 spec.Id = Convert.ToInt32(reader["id"]);
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["tags"]))) Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["skills"]))) Convert.ToString(reader["skills"]).Split(',').ToList().ForEach(x => spec.Skills.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["effects"]))) Convert.ToString(reader["effects"]).Split(',').ToList().ForEach(x => spec.Effects.Add(Convert.ToInt32(x)));
                                spec.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                spec.RawString = Convert.ToString(reader["rawstring"]);

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

        public List<BaseCustomization> GetAllCustomizations()
        {
            List<BaseCustomization> toReturn = new List<BaseCustomization>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT id, name, description, tags, stats, effects, rawstring, skills, ctype FROM {CustomizationTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BaseCustomization spec = null;
                                var cType = (CustomizationTypes)Convert.ToInt32(reader["ctype"]);
                                switch (cType)
                                {
                                    case CustomizationTypes.Archetype:
                                        {
                                            spec = new Archetype();

                                        }
                                        break;
                                    case CustomizationTypes.Specialization:
                                        {
                                            spec = new Specialization();
                                        }
                                        break;
                                    case CustomizationTypes.Calling:
                                        {
                                            spec = new Calling();
                                        }
                                        break;
                                    default:
                                        throw new Exception();
                                }
                                spec.Customization = cType;

                                spec.Name = Convert.ToString(reader["name"]);
                                spec.Description = Convert.ToString(reader["description"]);
                                spec.Id = Convert.ToInt32(reader["id"]);
                                Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => spec.Tags.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["skills"]))) Convert.ToString(reader["skills"]).Split(',').ToList().ForEach(x => spec.Skills.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["effects"]))) Convert.ToString(reader["effects"]).Split(',').ToList().ForEach(x => spec.Effects.Add(Convert.ToInt32(x)));
                                spec.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                spec.RawString = Convert.ToString(reader["rawstring"]);

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

        public BaseCustomization GetCustomizationById(int specid)
        {
            BaseCustomization toReturn = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connstr))
                {
                    connection.Open();
                    string sql = $"SELECT id, name, description, tags, stats, effects, rawstring, skills, ctype, rawstring FROM {CustomizationTable} WHERE id like @id;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@id", specid));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CustomizationTypes cType = (CustomizationTypes)Convert.ToInt32(reader["ctype"]);
                                switch(cType)
                                {
                                    case CustomizationTypes.Archetype:
                                        {
                                            toReturn = new Archetype();
                                        }
                                        break;
                                    case CustomizationTypes.Specialization:
                                        {
                                            toReturn = new Specialization();
                                        }
                                        break;
                                    case CustomizationTypes.Calling:
                                        {
                                            toReturn = new Calling();
                                        }
                                        break;
                                    default:
                                        throw new Exception();
                                }

                                toReturn.Customization = cType;
                                toReturn.Name = Convert.ToString(reader["name"]);
                                toReturn.Description = Convert.ToString(reader["description"]);
                                toReturn.Id = Convert.ToInt32(reader["id"]);
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["tags"]))) Convert.ToString(reader["tags"]).Split(',').ToList().ForEach(x => toReturn.Tags.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["skills"]))) Convert.ToString(reader["skills"]).Split(',').ToList().ForEach(x => toReturn.Skills.Add(Convert.ToInt32(x)));
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(reader["effects"]))) Convert.ToString(reader["effects"]).Split(',').ToList().ForEach(x => toReturn.Effects.Add(Convert.ToInt32(x)));
                                toReturn.Stats = JsonConvert.DeserializeObject<StatData>(Convert.ToString(reader["stats"]));
                                toReturn.RawString = Convert.ToString(reader["rawstring"]);
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