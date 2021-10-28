using System;
using System.IO;

namespace NesineBot
{
    class Config
    {
        private static string CredentialsPath;
        private static Credential credential;

        public static Credential Credential { get => credential; set => credential = value; }

        public static void Do()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appDataPath, @"Chastoca");
            path = Path.Combine(path, "Nesine");
            path = Path.Combine(path, "Settings");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            CredentialsPath = Path.Combine(path, "Credentials.txt");

            Credential = GetCredentials();
        }

        public static Credential GetCredentials()
        {
            try
            {
                if (File.Exists(CredentialsPath))
                {
                    string[] text = File.ReadAllLines(CredentialsPath);
                    if (text.Length < 1)
                    {
                        Console.WriteLine("Credentials file empty");
                        return null;
                    }
                    Credential c = new();
                    c.Username = text[0]["username: ".Length..];
                    c.Password = text[1]["password: ".Length..];
                    return c;
                }
                else
                {
                    Console.WriteLine("Credentials file empty");
                    File.Create(CredentialsPath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.CrashReport(ex);
                return null;
            }
        }

    }
}
