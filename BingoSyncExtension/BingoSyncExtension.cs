﻿using Modding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace BingoSyncExtension
{
    public class BingoSyncExtension : Mod
    {
        new public string GetName() => "BingoSyncExtension";
        public override string GetVersion() => "1.1.2";
        public override int LoadPriority() => 0;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            BingoSquareInjector.Setup(Log);
            BingoSquareReader.Setup(Log);
            VariableProxy.Setup(Log);
            GameModesManager.Setup(Log);
            MenuUI.Setup();

            On.UIManager.ContinueGame += ContinueGame;
            On.UIManager.StartNewGame += StartNewGame;
            On.UIManager.FadeInCanvasGroup += FadeIn;
            On.UIManager.FadeOutCanvasGroup += FadeOut;

            ModHooks.FinishedLoadingModsHook += MenuUI.SetupGameModeButtons;


            Log("Initialized");
        }
        private IEnumerator FadeIn(On.UIManager.orig_FadeInCanvasGroup orig, UIManager self, CanvasGroup cg)
        {
            if (cg.name == "MainMenuScreen")
            {
                MenuUI.SetUIVisible(true);
                MenuUI.SetGenerateButtonEnabled(true);
            }
            return orig(self, cg);
        }
        private IEnumerator FadeOut(On.UIManager.orig_FadeOutCanvasGroup orig, UIManager self, CanvasGroup cg)
        {
            if (cg.name == "MainMenuScreen")
            {
                MenuUI.SetUIVisible(false);
                MenuUI.SetGenerateButtonEnabled(false);
            }
            return orig(self, cg);
        }
        private void ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
        {
            MenuUI.SetUIVisible(false);
            MenuUI.SetGenerateButtonEnabled(false);
            orig(self);
        }
        private void StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            MenuUI.SetUIVisible(false);
            MenuUI.SetGenerateButtonEnabled(false);
            orig(self, permaDeath, bossRush);
        }
    }
}