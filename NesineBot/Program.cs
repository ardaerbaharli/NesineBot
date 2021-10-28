using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace NesineBot
{
    class Program
    {
        private static IWebDriver driver;

        private static Credential credential;
        static void Main(string[] args)
        {
            Config.Do();
            credential = Config.Credential;
            if (credential == null)
                return;

            DB.Create();

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appDataPath, @"Chastoca");
            path = Path.Combine(path, @"SeleniumDriver");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            driver = new ChromeDriver(path);
            driver.Url = "https://www.nesine.com/login";
            Login();
            Thread.Sleep(2000);

            var timer = new System.Timers.Timer(1000 * 60 * 30); // every 30 min
            timer.Elapsed += (sender, e) => Timer_Tick(sender, e);
            timer.Enabled = true;
            timer.Start();

            Scan();
        }

        private static void Scan()
        {
            driver.Url = "https://www.nesine.com/iddaa/populer-bahisler";
            Thread.Sleep(1000);

            var betsList = GetBets();

            var betsToPlay = Eliminate(betsList);

            PlayBets(betsToPlay);
        }

        private static void Timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            DB.RemoveExpiredBets();
            driver.Navigate().Refresh();
            Scan();
        }

        private static void PlayBets(List<Bet> betsToPlay)
        {
            try
            {
                IList<IWebElement> betLines = driver.FindElements(By.ClassName("betLine"));
                foreach (IWebElement betLine in betLines)
                {
                    var matchCode = betLine.FindElement(By.ClassName("matchCode")).Text;
                    if (betsToPlay.Any(x => x.MatchCode == matchCode))
                    {
                        var bet = betsToPlay.Find(x => x.MatchCode == matchCode);
                        DB.Add(bet);
                        Play(betLine);
                        Logger.Bets(bet);

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return;
            }

        }

        private static void Play(IWebElement betLine)
        {
            try
            {
                var btnParent = betLine.FindElement(By.ClassName("bet"));
                var btn = btnParent.FindElement(By.ClassName("odd"));
                btn.Click();

                driver.FindElement(By.Id("coupon-info")).Click();

                var betAmountSelector = driver.FindElement(By.Id("ddlMultiply"));
                var selectElement = new SelectElement(betAmountSelector);
                selectElement.SelectByValue("3");

                driver.FindElement(By.Id("btnPlay")).Click();
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return; ;
            }
        }

        private static List<Bet> Eliminate(List<Bet> betsList)
        {
            try
            {
                List<Bet> eliminatedBetList = betsList;

                foreach (var bet in betsList.ToList())
                {
                    if (bet.Mbs != 1)
                        eliminatedBetList.Remove(bet);
                    else if (bet.Rate > 1.5f)
                        eliminatedBetList.Remove(bet);
                    else if (bet.PlayedCount < 55000)
                        eliminatedBetList.Remove(bet);
                    else if (DB.DoesExist(bet))
                        eliminatedBetList.Remove(bet);
                }

                return eliminatedBetList;

            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return null;
            }
        }

        private static List<Bet> GetBets()
        {
            try
            {
                IList<IWebElement> betLines = driver.FindElements(By.ClassName("betLine")); // note the FindElements, plural.
                List<Bet> betList = new();
                foreach (IWebElement betLine in betLines)
                {
                    Bet bet = new();
                    bet.MatchCode = betLine.FindElement(By.ClassName("matchCode")).Text;
                    bet.MatchName = betLine.FindElement(By.ClassName("matchName")).Text;
                    var mbs = betLine.FindElement(By.ClassName("mbs")).FindElement(By.XPath(".//*")).GetAttribute("class");
                    bet.Mbs = int.Parse(Regex.Match(mbs, @"\d+").Value);
                    bet.BetType = betLine.FindElement(By.ClassName("bet")).Text;
                    bet.Rate = float.Parse(betLine.FindElement(By.ClassName("rate")).Text.Replace('.', ','));
                    bet.PlayedCount = int.Parse(betLine.FindElement(By.ClassName("playedCount")).Text.Replace(".", string.Empty));

                    var date = betLine.FindElement(By.ClassName("date")).Text;
                    if (!date.Contains("Yarın") && !date.Contains("Bugün"))
                        continue;

                    var today = DateTime.Today;
                    string separator = "\r\n";
                    var index = date.IndexOf(separator);
                    int hour = int.Parse(date.Substring(index + separator.Length, 2));
                    int min = int.Parse(date.Substring(index + separator.Length + "000".Length, 2));

                    if (date.Contains("Yarın"))
                        today.AddDays(1);

                    bet.Date = new DateTime(today.Year, today.Month, today.Day, hour, min, 0);
                    betList.Add(bet);
                }
                return betList;
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return null;
            }
        }

        private static void Login()
        {
            try
            {
                driver.FindElement(By.Id("userNameLP")).SendKeys(credential.Username);
                driver.FindElement(By.Id("realpassLP")).SendKeys(credential.Password);
                driver.FindElement(By.Id("btnLoginSubmitLP")).Click();
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return;
            }

        }
    }
}
