using Discord;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace NebulaMods
{
    public static class Extensions
    {
        public static string MaskIPAddress(this string str)
        {
            return IPAddress.Parse(str).AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? str.Replace(str.Split('.')[3], "*") : str.Replace(str.Split(':')[4], "*");
        }
        public static string MaskIPAddress(this IPAddress ip)
        {
            var ipaddy = ip.ToString();
            switch (ip.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetwork:
                    var idk1 = ipaddy.Split('.');
                return ipaddy.Replace(idk1[3], "*");
                default:
                    return "N/A";
            }
        }
        public static string MakeIPv4(this string str)
        {
            return Regex.Replace(str, "[^0-9./]+", "", RegexOptions.Compiled);
        }
        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9. ,:]+", "", RegexOptions.Compiled);
        }
        public static decimal SizeConverter(this long bytes)
        {
            if (bytes < 1024)
                return bytes;//Bytes
            else if (decimal.Divide(bytes, 1024) < 1024)
                return Math.Round(decimal.Divide(bytes, 1024), 2);//Kilobytes
            else if (decimal.Divide(bytes, 1024 * 1024) < 1024)
                return Math.Round(decimal.Divide(bytes, 1024 * 1024), 2);//Megabytes
            else
                return Math.Round(decimal.Divide(bytes, 1024 * 1024 * 1024), 2);//Gigabytes
        }
        public static decimal ThroughputConverter(this long bytes, bool round = true)
        {
            decimal result;
            if ((result = decimal.Divide(bytes * 8, 1000)) < 1000)
                return Math.Round(result);
            else if ((result = decimal.Divide(bytes * 8, 1000000)) < 1000)
                return Math.Round(result);
            else
                return Math.Round(decimal.Divide(bytes * 8, 1000000000), 2);
        }

        public static string LongConverter(this long size, bool round = true)
        {
            decimal result;
            if (size < 1000)
                return size.ToString();
            else if ((result = decimal.Divide(size, 1000)) < 1000)
                return $"{Math.Round(result, 2)}K";
            else if ((result = decimal.Divide(size, 100000)) < 100000)
                return $"{Math.Round(result, 2)}M";
            else
                return $"{Math.Round(decimal.Divide(size, 100000000), 2)}B";
        }
        public static string SizeDetector(this long bytes)
        {
            if (bytes < 1024)
                return "Bytes";
            else if (decimal.Divide(bytes, 1024) < 1024)
                return "Kilobytes";
            else if (decimal.Divide(bytes, 1024 * 1024) < 1024)
                return "Megabytes";
            else
                return "Gigabytes";
        }
        public static string ThroughputDetector(this long bytes)
        {
            if (decimal.Divide(bytes * 8, 1000) < 1000)
                return "Kilobits Per Second";
            else if (decimal.Divide(bytes * 8, 1000000) < 1000)
                return "Megabits Per Second";
            else
                return "Gigabits Per Second";
        }
    }
    public static class Utilities
    {
        public static string CommandExecuter(string FileName, string FileArgs, string FileLocation = "/usr/bin/", bool VerifyFile = true)
        {
            try
            {
                if (VerifyFile)
                {
                    if (!File.Exists($"{FileLocation}{FileName}"))
                        return $"{FileName} not found on server!";
                }
                var process = new Process();
                process.StartInfo.FileName = $"{FileLocation}{FileName}";
                process.StartInfo.Arguments = FileArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string Result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (string.IsNullOrEmpty(Result))
                    return "N/A";
                else
                    return Result;
            }
            catch(Exception error)
            {
                ErrorDetection(error);
                return "N/A";
            }
        }
        
        public static void ConsoleWhileLoop()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
        }

        public static void ErrorDetection(Exception error)
        {
            var database = new Services.DatabaseService();
            database.Errors.Add(new Database.LogsSchema.ErrorLogs()
            {
                Application = error.Source,
                Name = "idk",
                Location = error.TargetSite.Name,
                Reason = error.Message,
                ErrorTime = DateTime.Now
            });
            database.SaveChanges();
            Console.WriteLine("Error occured, please check the logs, thanks.");
        }

        public static Color RandomDiscordColour()
        {
            return new Color(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));
        }
    }

}
