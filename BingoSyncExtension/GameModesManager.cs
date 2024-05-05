using MagicUI.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BingoSyncExtension
{
    public static class GameModesManager
    {
        private static Action<string> Log;
        private static readonly List<GameMode> _gameModes = [];
        public static void Setup(Action<string> log)
        {
            Log = log;
        }
        public static void AddGameMode(GameMode gameMode)
        {
            _gameModes.Add(gameMode);
        }
        public static void Generate(Button sender)
        {
            Log("Generate button clicked");
            Thread boardSenderThread = new(() => {
                NewCardClient client = new();
                client.JoinRoom();
                while (client.GetState() == NewCardClient.State.Loading)
                {
                    Log("Loading...");
                    Thread.Sleep(500);
                }
                client.ChatMessage($"{client.nickname} generated Custom board");
                Thread.Sleep(100);
                client.NewCard(GameMode.GetErrorBoard());
                Thread.Sleep(1000);
                client.ExitRoom();
            });
            boardSenderThread.Start();
        }
    }
}
