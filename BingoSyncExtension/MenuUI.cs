using MagicUI.Core;
using MagicUI.Elements;
using Newtonsoft.Json;
using Settings;
using System.Collections.Generic;
using Modding.Converters;
using InControl;


namespace BingoSyncExtension
{
    public static class MenuUI {
        private static Hotkeys HotkeySettings { get; set; } = new();

        public static LayoutRoot layoutRoot = new(true, "Persistent layout");
        public static StackLayout layout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Vertical,
                Padding = new Padding(0, 50, 20, 0),
            };
        private static Button GenerateBoardButton;
        private static List<Button> GameModeButtons = [];

        private static int buttonSize = 173;
        private static int inputSize = buttonSize * 3 + 20;

        private static bool GenerationUiVisible = true;

        public static void Setup()
        {
            GenerateBoardButton = new(layoutRoot, "generateBoardButton")
            {
                Content = "Generate Board",
                FontSize = 22,
                Margin = 20,
                MinWidth = inputSize,
            };
            GenerateBoardButton.Click += GameModesManager.Generate;

            layout.Children.Add(GenerateBoardButton);

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
        }

        public static Button CreateGameModeButton(string name)
        {
            Button button = new(layoutRoot, name)
            {
                Content = name,
                FontSize = 15,
                Margin = 20,
                MinWidth = buttonSize,
            };
            button.Click += SelectGameMode;
            return button;

        }
        public static void SelectGameMode(Button sender)
        {
            GameModesManager.SetActiveGameMode(sender.Name);
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
