using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Timers;
using System.IO;
using ChatBot.Bot.Plugins.GatchaGame.Quests;

namespace ChatBot.Bot.Plugins.GatchaGame.Data
{
    public static class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        static readonly string playerTable = "Players";
        static readonly string floorTable = "Floors";
        static readonly string questTable = "Quests";
        static readonly string blurbTable = "TextBlurbs";

        /// <summary>
        /// db name
        /// </summary>
        static readonly string databaseName = "GatchaGame.db";
        static readonly string datetimeArg = "{DTN}";
        static readonly string nameSplitArg = ".bak";
        static readonly string datetimeSaveTemplate = "yyyy-dd-M--HH-mm-ss";

        /// <summary>
        /// backup variables
        /// </summary>
        static readonly string backupNameArg = $"{databaseName}{nameSplitArg}{datetimeArg}";
        static readonly int backupMaxCount = 500;
        static readonly TimeSpan backupInterval = new TimeSpan(1, 0, 0);
        static readonly string backupFolderPath = @"..\..\Databases\Backups\";
        static Timer backupTimer;

        /// <summary>
        /// threading locker
        /// </summary>
        static readonly object threadLock = new object();

        /// <summary>
        /// database location
        /// </summary>
#if DEBUG
        static readonly string connectionPath = @"..\..\Databases\Debug\";
        static readonly string connString = @"Data Source = " + connectionPath + databaseName + @"; Version=3;";
#else
        static readonly string connectionPath = @"..\..\Databases\Release\";
        static readonly string connString = @"Data Source = " + connectionPath + databaseName + @"; Version=3;";
#endif

        private static void CheckAndStartTimer()
        {
            if (backupTimer == null)
                backupTimer = new Timer();

            if (backupTimer.Enabled)
                return;

            backupTimer = new Timer(backupInterval.TotalMilliseconds);
            backupTimer.Elapsed += BackupTimer_Elapsed;
            backupTimer.Enabled = true;

            TimerStuffInternal();

            backupTimer.Start();
        }

        private static void TimerStuffInternal()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;


            // check for cleanup
            if (!Directory.Exists(basePath + backupFolderPath))
                Directory.CreateDirectory(basePath + backupFolderPath);

            var tbf = Directory.GetFiles(basePath + backupFolderPath);
            List<string> backupFiles = new List<string>();
            foreach (var v in tbf)
            {
                if (Path.GetFileName(v).StartsWith($"{databaseName}{nameSplitArg}"))
                {
                    backupFiles.Add(v);
                }
            }
            if (backupFiles.Count >= backupMaxCount)
            {
                backupFiles = backupFiles.OrderBy(x => File.GetCreationTime(x).Ticks).ToList();
                File.Delete(backupFiles.First());
            }

            // create backup
            File.Copy(basePath + connectionPath + databaseName, basePath + backupFolderPath + backupNameArg.Replace(datetimeArg, DateTime.Now.ToString(datetimeSaveTemplate)));
        }

        private static void BackupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerStuffInternal();
        }

        /// <summary>
        /// returns cards of all current players
        /// </summary>
        /// <returns>list of cards</returns>
        public static List<Cards.PlayerCard> GetAllCards()
        {
            lock(threadLock)
            {
                CheckAndStartTimer();

                List<Cards.PlayerCard> toReturn = new List<Cards.PlayerCard>();

                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT * FROM {playerTable};";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {

                                    Cards.PlayerCard card = JsonConvert.DeserializeObject<Cards.PlayerCard>(Convert.ToString(reader["data"]));
                                    card.SetStats(JsonConvert.DeserializeObject<BaseStats>(Convert.ToString(reader["stats"])));

                                    toReturn.Add(card);
                                }
                                reader.Close();
                            }
                            command.Dispose();
                        }
                        connection.Close();
                        return toReturn;
                    }
                }
                catch (Exception)
                {
                    return toReturn;
                }
            }
        }

        /// <summary>
        /// deletes a specified user from the table
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>true if successful</returns>
        public static bool DeleteCard(string user)
        {
            lock(threadLock)
            {
                CheckAndStartTimer();

                int result = -1;
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"delete from {playerTable} WHERE name like '{user}'";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        result = command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
                return result == 1;
            }
        }

        private static List<Socket> ConvertJObjectToInventory(JArray jObj)
        {
            List<Socket> toReturn = new List<Socket>();

            if (jObj.Count == 0)
            {
                return toReturn;
            }

            KeyValuePair<string, string>[] ugh = JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(jObj.ToString());
            List<KeyValuePair<string, string>> boop = new List<KeyValuePair<string, string>>(ugh);

            foreach (var v in boop)
            {
                Type socketType = Type.GetType(v.Key);
                string socketData = v.Value;
                Socket socket;

                if (socketType == typeof(WeaponSocket))
                {
                    socket = JsonConvert.DeserializeObject<WeaponSocket>(socketData);
                }
                else if (socketType == typeof(ArmorSocket))
                {
                    socket = JsonConvert.DeserializeObject<ArmorSocket>(socketData);
                }
                else if (socketType == typeof(PassiveSocket))
                {
                    socket = JsonConvert.DeserializeObject<PassiveSocket>(socketData);
                }
                else if (socketType == typeof(ActiveSocket))
                {
                    socket = JsonConvert.DeserializeObject<ActiveSocket>(socketData);
                }
                else
                    throw new Exception();

                toReturn.Add(socket);
            }

            return toReturn;
        }

        private static JArray ConvertInventoryToJObject(List<Socket> socketList)
        {
            List<KeyValuePair<string, string>> ugh = new List<KeyValuePair<string, string>>();
            for (int x = 0; x < socketList.Count; x++)
            {
                string iStr;
                Type iType;
                switch (socketList[x].SocketType)
                {
                    case SocketTypes.Active:
                        throw new NotImplementedException();
                    case SocketTypes.Passive:
                        {
                            PassiveSocket ps = (PassiveSocket)socketList[x];
                            iStr = JsonConvert.SerializeObject(ps);
                            iType = ps.GetType();
                        }
                        break;
                    case SocketTypes.Equipment:
                        {
                            EquipmentSocket es = (EquipmentSocket)socketList[x];
            
                            if (es.ItemType == EquipmentTypes.Weapon)
                            {
                                WeaponSocket ws = (WeaponSocket)socketList[x];
                                iStr = JsonConvert.SerializeObject(ws);
                                iType = ws.GetType();
                            }
                            else if (es.ItemType == EquipmentTypes.Armor)
                            {
                                ArmorSocket ws = (ArmorSocket)socketList[x];
                                iStr = JsonConvert.SerializeObject(ws);
                                iType = ws.GetType();
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        break;
                    case SocketTypes.Weapon:
                        {
                            WeaponSocket ws = (WeaponSocket)socketList[x];
                            iStr = JsonConvert.SerializeObject(ws);
                            iType = ws.GetType();
                        }
                        break;
                    case SocketTypes.Armor:
                        {
                            ArmorSocket ws = (ArmorSocket)socketList[x];
                            iStr = JsonConvert.SerializeObject(ws);
                            iType = ws.GetType();
                        }
                        break;
                    default:
                        throw new Exception();
                }
                ugh.Add(new KeyValuePair<string, string>(iType.ToString(), iStr));
            }
            try
            {
                string pants = JsonConvert.SerializeObject(ugh.ToArray());
                return (ugh.Count > 0) ? JArray.Parse(pants) : new JArray();
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Handles updating a user's card data
        /// </summary>
        /// <param name="pc">card of the user to update</param>
        /// <returns>true if successful</returns>
        public static bool UpdateCard(Cards.PlayerCard pc, bool ignoreNoEquipCheck = true)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                try
                {
                    int result = -1;
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"UPDATE {playerTable} SET data = @data, stats = @stats, box = @box, activesockets = @activesockets WHERE name like @name;";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@box", ConvertInventoryToJObject(pc.Inventory).ToString()));
                            pc.Inventory = new List<Socket>();

                            if (!ignoreNoEquipCheck && pc.ActiveSockets.Count < 1)
                            {
                                try
                                {
                                    throw new Exception();
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine($"wtf something emptied out {pc.Name}'s active sockets: {e.ToString()}");
                                    return false;
                                }
                            }

                            command.Parameters.Add(new SQLiteParameter("@activesockets", ConvertInventoryToJObject(pc.ActiveSockets).ToString()));
                            command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(pc.GetStats())));
                            command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(pc)));
                            command.Parameters.Add(new SQLiteParameter("@name", pc.Name));
                            result = command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        connection.Close();
                    }
                    return result == 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

        }

        public static List<string> GetBlurbs(BlurbTypes blurbType)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                List<string> toReturn = new List<string>();
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT ALL Blurb FROM {blurbTable} WHERE BlurbType = @blurb;";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@blurb", (int)blurbType));
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    toReturn.Add(reader["blurb"].ToString());
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
                    return toReturn;
                }
            }

        }

        /// <summary>
        /// Handles grabbing a card of specified user
        /// </summary>
        /// <param name="user">user to get the card of</param>
        /// <returns>card, if found</returns>
        public static Cards.PlayerCard GetCard(string user)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                Cards.PlayerCard toReturn = null;
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT name, data, stats, box, activesockets FROM {playerTable} WHERE name like @name;";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@name", user));
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    toReturn = JsonConvert.DeserializeObject<Cards.PlayerCard>(Convert.ToString(reader["data"]));
                                    toReturn.SetStats(JsonConvert.DeserializeObject<BaseStats>(Convert.ToString(reader["stats"])));
                                    toReturn.Inventory = ConvertJObjectToInventory(JArray.Parse(Convert.ToString(reader["box"])));
                                    toReturn.ActiveSockets = ConvertJObjectToInventory(JArray.Parse(Convert.ToString(reader["activesockets"])));
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
                    return toReturn;
                }
            }

        }

        /// <summary>
        /// confirms if the user exists in the table
        /// </summary>
        /// <param name="user">user to check</param>
        /// <returns>true if user exists</returns>
        public static bool UserExists(string user)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                int toReturn = -1;
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT COUNT(name) AS Count FROM {playerTable} WHERE name like '{user}';";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    toReturn = Convert.ToInt32(reader["Count"]);
                                    break;
                                }
                                reader.Close();
                            }
                            command.Dispose();
                        }
                        connection.Close();

                    }
                    return toReturn == 1;
                }
                catch (Exception)
                {
                    return toReturn == 1;
                }
            }

        }

        /// <summary>
        /// adds a new user to the table
        /// </summary>
        /// <param name="pc">card of the user to add</param>
        /// <returns>treu if successful</returns>
        public static bool AddNewUser(Cards.PlayerCard pc)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                int toReturn = -1;
                try
                {
                    string query = $"INSERT INTO {playerTable} (name, data, stats, box, activesockets) VALUES (@name, @data, @stats, @box, @activesockets);";

                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.CommandText = query;
                            command.CommandType = System.Data.CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@box", ConvertInventoryToJObject(pc.Inventory)));
                            pc.Inventory = new List<Socket>();

                            command.Parameters.Add(new SQLiteParameter("@activesockets", ConvertInventoryToJObject(pc.ActiveSockets)));
                            pc.ActiveSockets = new List<Socket>();

                            command.Parameters.Add(new SQLiteParameter("@name", pc.Name));
                            command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(pc)));
                            command.Parameters.Add(new SQLiteParameter("@stats", JsonConvert.SerializeObject(pc.GetStats())));




                            toReturn = command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        connection.Close();
                    }

                    return toReturn == 1;
                }
                catch (Exception)
                {
                    return toReturn == 1;
                }
            }

        }

        ///////////////////////////////////////

        /// <summary>
        /// Handles updating a user's card data
        /// </summary>
        /// <param name="fc">card of the user to update</param>
        /// <returns>true if successful</returns>
        public static bool UpdateFloor(FloorCard fc)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                try
                {
                    int result = -1;
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"UPDATE {floorTable} SET data = @data WHERE level = @level;";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(fc)));
                            command.Parameters.Add(new SQLiteParameter("@level", fc.floor));
                            result = command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        connection.Close();
                        return result == 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

        }

        /// <summary>
        /// adds a new user to the table
        /// </summary>
        /// <param name="fc">card of the user to add</param>
        /// <returns>treu if successful</returns>
        public static bool AddNewFloor(FloorCard fc)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                int toReturn = -1;
                try
                {
                    string query = $"INSERT INTO {floorTable} (data) VALUES (@data);";

                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.CommandText = query;
                            command.CommandType = System.Data.CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@data", JsonConvert.SerializeObject(fc)));
                            toReturn = command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        connection.Close();
                    }

                    return toReturn == 1;
                }
                catch (Exception)
                {
                    return toReturn == 1;
                }
            }
        }

        /// <summary>
        /// returns cards of all current players
        /// </summary>
        /// <returns>list of cards</returns>
        public static List<FloorCard> GetAllFloors()
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                List<FloorCard> toReturn = new List<FloorCard>();

                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT * FROM {floorTable};";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    FloorCard card = JsonConvert.DeserializeObject<FloorCard>(Convert.ToString(reader["data"]));
                                    toReturn.Add(card);
                                }
                                reader.Close();
                            }
                            command.Dispose();
                        }
                        connection.Close();
                        return toReturn.OrderBy(x => x.floor).ToList();
                    }
                }
                catch (Exception)
                {
                    return toReturn;
                }
            }

        }

        public static bool AddNewQuest(Quest quest)
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                int toReturn = -1;
                try
                {
                    string query = $"INSERT INTO {questTable} (id, data) VALUES (@id, @data);";

                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.CommandText = query;
                            command.CommandType = System.Data.CommandType.Text;
                            command.Parameters.Add(new SQLiteParameter("@id", quest.QuestId));
                            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                            string str = JsonConvert.SerializeObject(quest, settings);
                            command.Parameters.Add(new SQLiteParameter("@data", str));
                            toReturn = command.ExecuteNonQuery();
                            command.Dispose();
                        }
                        connection.Close();
                    }

                    return toReturn == 1;
                }
                catch (Exception)
                {
                    return toReturn == 1;
                }
            }
        }

        public static List<Quest> LoadQuests()
        {
            lock (threadLock)
            {
                CheckAndStartTimer();

                List<Quest> toReturn = new List<Quest>();

                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connString))
                    {
                        connection.Open();
                        string sql = $"SELECT * FROM {questTable};";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                                    Quest quest = JsonConvert.DeserializeObject<Quest>(Convert.ToString(reader["data"]), settings);
                                    toReturn.Add(quest);
                                }
                                reader.Close();
                            }
                            command.Dispose();
                        }
                        connection.Close();
                        return toReturn;
                    }
                }
                catch (Exception)
                {
                    return toReturn;
                }
            }

        }
    }
}