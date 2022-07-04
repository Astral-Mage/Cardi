using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.IO;

namespace ChatBot.Bot.Plugins.LostRPG.Data
{
    /// <summary>
    /// clean, threadsafe singleton database storage system
    /// </summary>
    public partial class DataDb
    {
        /// <summary>
        /// our instance
        /// </summary>
        private static DataDb instance;

        /// <summary>
        /// requesting the instance
        /// </summary>
        /// <returns>a valid, non-null db instance</returns>
        public static DataDb Instance
        {
            get
            {
                lock (threadLock)
                {
                    if (instance == null)
                    {
                        instance = new DataDb();
                    }

                    CheckAndStartTimer();
                    return instance;
                }
            }

        }

        /// <summary>
        /// our hidden constructor
        /// </summary>
        private DataDb() { }

        /// <summary>
        /// db name
        /// </summary>
        static readonly string databaseName = "LostRPG.db";
        static readonly string datetimeArg = "{DTN}";
        static readonly string nameSplitArg = ".bak";
        static readonly string datetimeSaveTemplate = "yyyy-dd-M--HH-mm-ss";

        /// <summary>
        /// backup variables
        /// </summary>
        static readonly string backupNameArg = $"{databaseName}{nameSplitArg}{datetimeArg}";
        static readonly int backupMaxCount = 500;
        static readonly TimeSpan backupInterval = new TimeSpan(1, 0, 0);
        static readonly string backupFolderPath = @"..\..\Databases\Backups\" + $"{databaseName}\\";
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

        /// <summary>
        /// checks for the automatic backup system, starting it if not already running
        /// </summary>
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

        /// <summary>
        /// handles automatic backups
        /// </summary>
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

        /// <summary>
        /// triggered when it's time to backup our data
        /// </summary>
        /// <param name="sender">triggering event</param>
        /// <param name="e">our event arguments</param>
        private static void BackupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerStuffInternal();
        }
    }
}