using MagicUI.Core;
using MagicUI.Elements;
using Newtonsoft.Json;
using Settings;
using System.Collections.Generic;
using Modding.Converters;
using InControl;
using UnityEngine;


namespace BingoSyncExtension
{
    public static class MenuUI {
        private static Hotkeys HotkeySettings { get; set; } = new();

        private static readonly int menuWidth = 540; // match BingoSync
        private static readonly int gameModeButtonSize = (menuWidth - 30) / 3;
        private static readonly int generateButtonSize = 400;
        private static readonly int lockoutButtonSize = menuWidth - generateButtonSize - 20;

        public static LayoutRoot layoutRoot = new(true, "Persistent layout");
        public static StackLayout layout = new(layoutRoot)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Spacing = 15,
            Orientation = Orientation.Vertical,
            Padding = new Padding(0, 50, 20, 15),
        };
        private static readonly Button GenerateBoardButton = new(layoutRoot, "generateBoardButton")
        {
            Content = "Generate Board",
            FontSize = 22,
            Margin = 20,
            MinWidth = generateButtonSize,
            MinHeight = 50,
        };
        private static readonly Button LockoutToggleButton = new(layoutRoot, "lockoutToggleButton")
        {
            Content = "Lockout",
            FontSize = 15,
            Margin = 20,
            MinWidth = lockoutButtonSize,
            MinHeight = 50,
        };
        private static readonly List<Button> GameModeButtons = [];

        private static bool GenerationUiVisible = true;

        public static void Setup()
        {
            GenerateBoardButton.Click += GameModesManager.Generate;

            StackLayout bottomRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            bottomRow.Children.Add(GenerateBoardButton);
            bottomRow.Children.Add(LockoutToggleButton);


            layout.Children.Add(bottomRow);

            layoutRoot.VisibilityCondition = () => {
                return GenerationUiVisible;
            };
            layoutRoot.ListenForPlayerAction(HotkeySettings.Keybinds.HideUI, () => {
                GenerationUiVisible = !GenerationUiVisible;
            });

        }

        public static void SetupGameModeButtons()
        {
            StackLayout buttonLayout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Vertical,
            };

            StackLayout row = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

//            List<string> names = GameModesManager.GameModeNames();
            foreach (string gameMode in GameModesManager.GameModeNames())
            {
                if(row.Children.Count >= 3)
                {
                    buttonLayout.Children.Insert(0, row);
                    row = new(layoutRoot)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Spacing = 10,
                        Orientation = Orientation.Horizontal,
                    };
                }
                Button gameModeButton = CreateGameModeButton(gameMode);
                GameModeButtons.Add(gameModeButton);
                row.Children.Add(gameModeButton);
            }
            buttonLayout.Children.Insert(0, row);
            layout.Children.Insert(0, buttonLayout);
            SelectGameMode(GameModeButtons[0]);
        }

        public static Button CreateGameModeButton(string name)
        {
            Button button = new(layoutRoot, name)
            {
                Content = name,
                FontSize = 15,
                Margin = 20,
                MinWidth = gameModeButtonSize,
            };
            button.Click += SelectGameMode;
            return button;

        }

        public static void SelectGameMode(Button sender)
        {
            GameModesManager.SetActiveGameMode(sender.Name);
            foreach(Button gameMode in GameModeButtons)
            {
                gameMode.BorderColor = Color.white;
            }
            sender.BorderColor = Color.red;
        }

        public static void SetUIVisible(bool visible)
        {
            GenerationUiVisible = visible;
        }
    }

    internal class Hotkeys
    {
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public HideKeybind Keybinds = new();
    }
    internal class HideKeybind : PlayerActionSet
    {
        public PlayerAction HideUI;
        public HideKeybind()
        {
            HideUI = CreatePlayerAction("HideBoardGenerationUI");
            HideUI.AddDefaultBinding(Key.H);
        }

    }
}
