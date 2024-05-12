using MagicUI.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace BingoSyncExtension
{
    public static class GameModesManager
    {
        private static Action<string> Log;
        private static readonly List<GameMode> _gameModes = [];
        private static string activeGameMode = string.Empty;
        public static string room = string.Empty;
        public static string username = string.Empty;
        public static string password = string.Empty;
        public static void Setup(Action<string> log)
        {
            Log = log;
        }
        public static void AddGameMode(GameMode gameMode)
        {
            _gameModes.Add(gameMode);
        }
        public static List<string> GameModeNames()
        {
            List<string> names = [];
            foreach(GameMode gameMode in _gameModes)
            {
                names.Add(gameMode.GetName());
            }
            return names;
        }
        public static void SetActiveGameMode(string gameMode)
        {
            activeGameMode = gameMode;
        }
        public static void Generate(Button sender)
        {
            ExtractSessionInfo();
            Log("Generate button clicked");
            Thread boardSenderThread = new(() => {
                NewCardClient client = new();
                client.JoinRoom(room, password);
                while (client.GetState() == NewCardClient.State.Loading)
                {
                    Log("Loading...");
                    Thread.Sleep(500);
                }
                client.ChatMessage(room, $"{username} generated {activeGameMode} board");
                Thread.Sleep(100);
                string customJSON = GameMode.GetErrorBoard();
                if (activeGameMode != string.Empty)
                {
                    customJSON = _gameModes.Find(gameMode => gameMode.GetName() == activeGameMode).GenerateBoard();
                }
                client.NewCard(room, customJSON);
                Thread.Sleep(1000);
                client.ExitRoom();
            });
            boardSenderThread.Start();
        }
        public static void ExtractSessionInfo()
        {
            string hk_data = BingoSquareReader.GetHKDataFolderName();
            string path = @$".\{hk_data}\Managed\Mods\BingoSync\BingoSync.dll";
            Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
            Type bingoSyncClientType = assembly.GetType("BingoSync.BingoSyncClient");
            FieldInfo roomField = bingoSyncClientType.GetField("room", BindingFlags.Public | BindingFlags.Static);
            FieldInfo nicknameField = bingoSyncClientType.GetField("nickname", BindingFlags.Public | BindingFlags.Static);
            FieldInfo passwordField = bingoSyncClientType.GetField("password", BindingFlags.Public | BindingFlags.Static);
            room = (string) roomField.GetValue(room);
            username = (string)nicknameField.GetValue(room);
            password = (string)passwordField.GetValue(room);
        }
    }
}
