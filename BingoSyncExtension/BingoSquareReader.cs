using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BingoSyncExtension
{
    public static class BingoSquareReader
    {
        private static Action<string> Log;
        private static string HKDataFolder = string.Empty;
        public static void Setup(Action<string> log)
        {
            Log = log;

            const string steam = "hollow_knight_Data";
            const string GOG = "Hollow Knight_Data";
            if (Directory.Exists($".\\{steam}\\"))
            {
                HKDataFolder = steam;
            }
            else if (Directory.Exists($".\\{GOG}\\"))
            {
                HKDataFolder = GOG;
            }
            else
            {
                Log("Neither steam path nor GOG path exists");
            }
        }
        public static List<LocalBingoSquare> ReadFromFile(string filepath)
        {
            List<LocalBingoSquare> squares = [];
            using (StreamReader reader = new(filepath))
            using (JsonTextReader jsonReader = new(reader))
            {
                JsonSerializer ser = new();
                var squaresSer = ser.Deserialize<List<LocalBingoSquare>>(jsonReader);
                squares.AddRange(squaresSer);
            }
            return squares;
        }

        [Obsolete("Deprecated, please use Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) instead")]
        public static string GetHKDataFolderName()
        {
            return HKDataFolder;
        }
    }
}
