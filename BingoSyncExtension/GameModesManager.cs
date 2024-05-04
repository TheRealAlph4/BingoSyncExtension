using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoSyncExtension
{
    public static class GameModesManager
    {
        private static Action<string> Log;
        private static List<GameMode> _gameModes = new();
        public static void Setup(Action<string> log)
        {
            Log = log;
        }
        public static void AddGameMode(GameMode gameMode)
        {
            _gameModes.Add(gameMode);
        }
    }
}
