using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace NesineBot
{
    class DB
    {
        private static string connectionString;
        public static bool Create()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dbDirectory = Path.Combine(appDataPath, "Chastoca");
                dbDirectory = Path.Combine(dbDirectory, "Nesine");
                string dbPath = Path.Combine(dbDirectory, "nesineBets.db");
                connectionString = string.Format("Data Source={0}", dbPath);
                if (!File.Exists(dbPath))
                {
                    Directory.CreateDirectory(dbDirectory);
                    SQLiteConnection.CreateFile(dbPath);

                    SQLiteConnection con = new(connectionString);
                    con.Open();

                    string sql;
                    sql = @"CREATE TABLE Bets(matchCode TEXT NOT NULL, matchName TEXT NOT NULL, mbs INTEGER NOT NULL, betType TEXT NOT NULL, rate REAL NOT NULL, playedCount INTEGER NOT NULL, date TEXT NOT NULL)";

                    SQLiteCommand cmd = new(con);
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return false;
            }
            return false;
        }

        public static bool Add(Bet bet)
        {
            if (!DoesExist(bet))
            {
                SQLiteConnection con = new(connectionString);
                try
                {
                    int isSuccessful;
                    con.Open();
                    SQLiteCommand cmd;
                    cmd = con.CreateCommand();

                    cmd.CommandText = @"INSERT INTO Bets (matchCode, matchName, mbs, betType, rate, playedCount, date) VALUES ('" +
                    bet.MatchCode + "','" + bet.MatchName + "','" + bet.Mbs + "','" + bet.BetType + "','" + bet.Rate + "','" + bet.PlayedCount + "','" + bet.Date + "');";

                    isSuccessful = cmd.ExecuteNonQuery();
                    con.Close();
                    if (isSuccessful == 0)
                        return false;
                    else
                        return true;
                }
                catch (Exception ex)
                {
                    con.Close();
                    Logger.CrashReport(ex);
                    return false;
                }
            }
            else
                return false;
        }

        public static void RemoveExpiredBets()
        {
            var bets = GetBets();
            foreach (var bet in bets.ToList())
            {
                var res = DateTime.Compare(DateTime.Now, bet.Date);
                if (res < 0)
                    Remove(bet);
            }
        }

        public static List<Bet> GetBets()
        {
            SQLiteConnection con = new(connectionString);
            try
            {
                con.Open();
                SQLiteCommand cmd;
                SQLiteDataReader reader;
                cmd = con.CreateCommand();
                cmd.CommandText = "SELECT * FROM Bets";
                reader = cmd.ExecuteReader();

                List<Bet> bets = new();

                while (reader.Read())
                {
                    Bet bet = new();
                    bet.MatchCode = reader[0].ToString().TrimEnd();
                    bet.MatchName = reader[1].ToString().TrimEnd();
                    bet.Mbs = int.Parse(reader[2].ToString().TrimEnd());
                    bet.BetType = reader[3].ToString().TrimEnd();
                    bet.Rate = float.Parse(reader[4].ToString().TrimEnd());
                    bet.PlayedCount = int.Parse(reader[5].ToString().TrimEnd());
                    bet.Date = DateTime.Parse(reader[6].ToString().TrimEnd());

                    bets.Add(bet);
                }

                con.Close();
                return bets;
            }
            catch (Exception ex)
            {
                con.Close();
                Logger.CrashReport(ex);
                return null;
            }
        }
        public static bool Remove(Bet bet)
        {
            if (DoesExist(bet))
            {
                SQLiteConnection con = new(connectionString);
                try
                {
                    int isSuccessful;
                    con.Open();
                    SQLiteCommand cmd;
                    cmd = con.CreateCommand();
                    cmd.CommandText = @"DELETE FROM Bets WHERE matchCode='" + bet.MatchCode + "'";
                    isSuccessful = cmd.ExecuteNonQuery();
                    con.Close();
                    if (isSuccessful == 0)
                        return false;
                    else
                        return true;
                }
                catch (Exception ex)
                {
                    con.Close();
                    Logger.CrashReport(ex);
                    return false;
                }
            }
            else
                return false;
        }
        public static bool DoesExist(Bet bet)
        {
            SQLiteConnection con = new(connectionString);
            try
            {
                con.Open();
                SQLiteCommand cmd;
                cmd = con.CreateCommand();
                cmd.CommandText = @"SELECT EXISTS(SELECT * FROM Bets WHERE matchCode='" + bet.MatchCode + "');";
                int commandExist = int.Parse(cmd.ExecuteScalar().ToString());
                con.Close();
                if (commandExist > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                con.Close();
                Logger.CrashReport(ex);
                return false;
            }
        }
    }
}
