using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesineBot
{
    class Logger
    {
        public static void Log(Log log)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirectory = Path.Combine(appDataPath, @"Chastoca");
            logDirectory = Path.Combine(logDirectory, "Nesine");
            foreach (var item in log.FolderDirectory)
            {
                logDirectory = Path.Combine(logDirectory, item);
            }

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (log.LogName == null)
                log.LogName = "NO_LOG_NAME";

            Log formattedLog = new();
            formattedLog.LogName = $"{log.LogName} - {DateTime.Now.ToShortDateString()}";
            formattedLog.Message = $"{DateTime.Now.ToLongTimeString()} || {log.Message} {Environment.NewLine}";

            string path = $"{logDirectory}\\{formattedLog.LogName}.txt";
            File.AppendAllText(path, formattedLog.Message);
            Console.WriteLine(log.Message);
        }

        public static void CrashReport(Exception ex)
        {
            Log crashReport = new();
            crashReport.FolderDirectory = new string[] { "CrashReports" };
            crashReport.LogName = "CrashReport";
            crashReport.Message = string.Format("Exception message: {0}\nStack trace:\n{1}", ex.Message, ex.StackTrace);
            crashReport.TimeStamp = true;
            Log(crashReport);
        }

        public static void Bets(Bet bet)
        {
            Log betLog = new();
            betLog.FolderDirectory = new string[] { "BetsPlayed" };
            betLog.LogName = "BetsPlayed";
            betLog.Message = $"{bet.MatchCode} || {bet.Date} || {bet.MatchName} || {bet.BetType} || {bet.Rate} || {bet.PlayedCount}";
            betLog.TimeStamp = false;
            Log(betLog);
        }
    }
}
