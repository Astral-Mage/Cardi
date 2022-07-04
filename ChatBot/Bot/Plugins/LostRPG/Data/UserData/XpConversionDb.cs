using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        ///// <summary>
        ///// table name
        ///// </summary>
        //readonly string xpConversionTable = "XpConversionTable";
        //
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="user"></param>
        //public double ConvertKFToXp()
        //{
        //    double toReturn = 0;
        //    try
        //    {
        //        using (SQLiteConnection connection = new SQLiteConnection(connString))
        //        {
        //            connection.Open();
        //            string sql = $"SELECT magicid, color, description, name FROM {magicTable};";
        //            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
        //            {
        //                using (SQLiteDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //
        //                        tm.Color = Convert.ToString(reader["color"]);
        //                        tm.Description = Convert.ToString(reader["description"]);
        //                        tm.MagicId = Convert.ToInt32(reader["magicid"]);
        //                        tm.Name = Convert.ToString(reader["name"]);
        //
        //                    }
        //                    reader.Close();
        //                }
        //                command.Dispose();
        //            }
        //            connection.Close();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //
        //    return toReturn;
        //}
    }
}
