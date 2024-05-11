using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace BingoSyncExtension
{
    public class BingoSyncExtension : Mod
    {
        new public string GetName() => "BingoSyncExtension";
        public override string GetVersion() => "1.1";
        public override int LoadPriority() => 0;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            BingoSquareReader.Setup(Log);
            BingoSquareInjector.Setup(Log);
            VariableProxy.Setup(Log);
            GameModesManager.Setup(Log);
            MenuUI.Setup();

            Log("Initialized");
        }
    }
}