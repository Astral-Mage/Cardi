using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public class PointsAction
    {
        public int actionId { get; set; }
        public int actionCost { get; set; }
        public string actionName { get; set; }
        public List<string> actionNicknames { get; set; }

        public PointsAction()
        {
            actionId = -1;
            actionCost = 0;
            actionName = "Null";
            actionNicknames = new List<string>();
        }
    }



    public partial class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string PointActionsTable = "PointActionsTable";

        public List<PointsAction> GetPointActionsDictionary()
        {
            List<PointsAction> toReturn = new List<PointsAction>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT * FROM {PointActionsTable};";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PointsAction tpa = new PointsAction();
                                tpa.actionId = Convert.ToInt32(reader["actionId"]);
                                tpa.actionCost = Convert.ToInt32(reader["actionCost"]);
                                tpa.actionName = Convert.ToString(reader["actionName"]);
                                tpa.actionNicknames = Convert.ToString(reader["actionNicknames"]).Split(',').ToList();
                                toReturn.Add(tpa);
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
