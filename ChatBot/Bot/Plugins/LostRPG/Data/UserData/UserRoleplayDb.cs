using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using System;
using System.Data.SQLite;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    public partial class DataDb
    {
        /// <summary>
        /// table name
        /// </summary>
        readonly string userRoleplayTable = "UserRoleplayData";

        public RoleplayData GetUserRoleplayData(int userId)
        {
            RoleplayData toReturn = new RoleplayData();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    string sql = $"SELECT totalsentences, totalwords, totalparagraphs, totalsyllables, posthistory, totalposts, totalmisspellings, totalexperience FROM {userRoleplayTable} WHERE userid = @uid;";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@uid", userId));
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                toReturn.TotalSentences = Convert.ToInt32(reader["totalsentences"]);
                                toReturn.TotalWords = Convert.ToInt32(reader["totalwords"]);
                                toReturn.TotalPosts = Convert.ToInt32(reader["totalposts"]);
                                toReturn.TotalSyllables = Convert.ToInt32(reader["totalsyllables"]);
                                toReturn.TotalParagraphs = Convert.ToInt32(reader["totalparagraphs"]);
                                toReturn.TotalMisspellings = Convert.ToInt32(reader["totalmisspellings"]);
                                toReturn.TotalExperience = Convert.ToInt32(reader["totalexperience"]);

                                toReturn.PostHistory = Convert.ToString(reader["posthistory"]);

                                toReturn.UserId = userId;
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

        public void UpdateUserRoleplayData(RoleplayData rpData)
        {
            try
            {
                string query = $"UPDATE {userRoleplayTable} SET totalsentences = @tsent, totalwords = @tword, totalposts = @tpost, totalsyllables = @tsyl, totalparagraphs = @tpara, posthistory = @phis, totalmisspellings = @tmisp, totalexperience = @txp WHERE userid = @uid;";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", rpData.UserId));
                        command.Parameters.Add(new SQLiteParameter("@tsent", rpData.TotalSentences));
                        command.Parameters.Add(new SQLiteParameter("@tword", rpData.TotalWords));
                        command.Parameters.Add(new SQLiteParameter("@tpost", rpData.TotalPosts));
                        command.Parameters.Add(new SQLiteParameter("@tsyl", rpData.TotalSyllables));
                        command.Parameters.Add(new SQLiteParameter("@tpara", rpData.TotalParagraphs));
                        command.Parameters.Add(new SQLiteParameter("@tmisp", rpData.TotalMisspellings));
                        command.Parameters.Add(new SQLiteParameter("@txp", rpData.TotalExperience));

                        command.Parameters.Add(new SQLiteParameter("@phis", rpData.PostHistory));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: UpdateUserRoleplayData");
            }
        }

        private RoleplayData AddNewUserRoleplayData(int userId)
        {
            RoleplayData toReturn = new RoleplayData();
            toReturn.UserId = userId;

            try
            {
                string query = $"INSERT INTO {userRoleplayTable} (userid, totalsentences, totalwords, totalposts, totalsyllables, totalparagraphs, posthistory, totalmisspellings, totalexperience) VALUES (@uid, @tsent, @tword, @tpost, @tsyl, @tpara, @this, @tmisp, @txp);";

                using (SQLiteConnection connection = new SQLiteConnection(connString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.CommandText = query;
                        command.CommandType = System.Data.CommandType.Text;
                        command.Parameters.Add(new SQLiteParameter("@uid", userId));
                        command.Parameters.Add(new SQLiteParameter("@tsent", toReturn.TotalSentences));
                        command.Parameters.Add(new SQLiteParameter("@tword", toReturn.TotalWords));
                        command.Parameters.Add(new SQLiteParameter("@tpost", toReturn.TotalPosts));
                        command.Parameters.Add(new SQLiteParameter("@tsyl", toReturn.TotalSyllables));
                        command.Parameters.Add(new SQLiteParameter("@tpara", toReturn.TotalParagraphs));
                        command.Parameters.Add(new SQLiteParameter("@tmisp", toReturn.TotalMisspellings));
                        command.Parameters.Add(new SQLiteParameter("@txp", toReturn.TotalExperience));
                        command.Parameters.Add(new SQLiteParameter("@this", toReturn.PostHistory));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                    connection.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Db write error: AddNewUserRoleplayData");
                return null;
            }
            return toReturn;
        }

        public void DeleteUserRoleplayData(int userId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connString))
            {
                connection.Open();
                string sql = $"delete from {userRoleplayTable} WHERE userid like '{userId}'";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                connection.Close();
            }
        }
    }
}
