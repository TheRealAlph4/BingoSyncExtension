using System;
using System.Collections.Generic;
using System.Reflection;

namespace BingoSyncExtension
{
    public static class VariableProxy
    {
        private static Action<string> Log;
        private static readonly HashSet<string> trackedVariables = new HashSet<string>();
        private static Assembly _assembly;
        private static Type _bingoTrackerType;
        
        public static void Setup(Action<string> log)
        {
            string hk_data = BingoSquareReader.GetHKDataFolderName();
            string path = @$".\{hk_data}\Managed\Mods\BingoSync\BingoSync.dll";
            _assembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
            _bingoTrackerType = _assembly.GetType("BingoSync.BingoTracker");
            Log = log;
        }

        public static void TrackVariable(string variableName)
        {
            trackedVariables.Add(variableName);
        }

        public static void UntrackVariable(string variableName)
        {
            trackedVariables.Remove(variableName);
        }

        public static int GetInteger(string variableName)
        {
            int value = (int)_bingoTrackerType.GetMethod("GetInteger").Invoke(null, new object[] { variableName });
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetInteger: {variableName} = {value}");
            }
            return value;
        }

        public static void UpdateInteger(string variableName, int value)
        {
            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateInteger: {variableName} = {value}");
            }
            _bingoTrackerType.GetMethod("UpdateInteger", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { variableName, value });
        }

        public static void SetInteger(string variableName, int value)
        {
            UpdateInteger(variableName, value);
        }

        public static void Increment(string variableName, int amount = 1)
        {
            SetInteger(variableName, GetInteger(variableName) + amount);
        }

        public static bool GetBoolean(string variableName)
        {
            bool value = (bool)_bingoTrackerType.GetMethod("GetBoolean").Invoke(null, new object[] { variableName });
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetBoolean: {variableName} = {value}");
            }
            return value;
        }

        public static void UpdateBoolean(string variableName, bool value)
        {
            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateBoolean: {variableName} = {value}");
            }
            _bingoTrackerType.GetMethod("UpdateBoolean").Invoke(null, new object[] { variableName, value });
        }

        public static void SetBoolean(string variableName, bool value)
        {
            UpdateBoolean(variableName, value);
        }
    }
}
