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

        public static LayoutRoot layoutRoot;
        private static Button GenerateBoardButton;
        private static List<Button> GameModeButtons;

        private static int buttonSize = 100;
        private static int inputSize = buttonSize * 5 + 40;

        private static bool GenerationUiVisible = true;

        public static void Setup()
        {
            layoutRoot = new(true, "Persistent layout");
            StackLayout layout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Vertical,
                Padding = new Padding(0, 50, 20, 0),
            };

            GenerateBoardButton = new(layoutRoot, "generateBoardButton")
            {
                Content = "Generate Board",
                FontSize = 22,
                Margin = 20,
                MinWidth = inputSize,
            };
            GenerateBoardButton.Click += GameModesManager.Generate;

//            layout.Children.Add(CreateButtons());
            layout.Children.Add(GenerateBoardButton);

            layoutRoot.VisibilityCondition = () => {
                return GenerationUiVisible;
            };
            layoutRoot.ListenForPlayerAction(HotkeySettings.Keybinds.HideUI, () => {
                GenerationUiVisible = !GenerationUiVisible;
            });

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
