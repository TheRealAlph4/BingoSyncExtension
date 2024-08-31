using System;
using System.Collections.Generic;
using System.Reflection;

namespace BingoSyncExtension
{
    [Obsolete("Deprecated, use BingoSync.Variables.* instead")]
    public static class VariableProxy
    {
        private static Action<string> Log;
        private static readonly HashSet<string> trackedVariables = new HashSet<string>();
        private static Assembly _assembly;
        private static Type _bingoTrackerType;
        
        public static void Setup(Action<string> log)
        {
            Log = log;
            _bingoTrackerType = BingoSquareInjector._bingoSyncAssembly.GetType("BingoSync.BingoTracker");
        }

        public static void TrackVariable(string variableName)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }
            
            trackedVariables.Add(variableName);
        }

        public static void UntrackVariable(string variableName)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }
            
            trackedVariables.Remove(variableName);
        }

        public static int GetInteger(string variableName)
        {
            if (BingoSyncExtension.deprecated)
            {
                return -1;
            }
            
            int value = (int)_bingoTrackerType.GetMethod("GetInteger").Invoke(null, new object[] { variableName });
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetInteger: {variableName} = {value}");
            }
            return value;
        }

        public static void UpdateInteger(string variableName, int value)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }

            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateInteger: {variableName} = {value}");
            }
            _bingoTrackerType.GetMethod("UpdateInteger", new[] { typeof(string), typeof(int) }).Invoke(null, new object[] { variableName, value });
        }

        public static void SetInteger(string variableName, int value)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }

            UpdateInteger(variableName, value);
        }

        public static void Increment(string variableName, int amount = 1)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }

            SetInteger(variableName, GetInteger(variableName) + amount);
        }

        public static bool GetBoolean(string variableName)
        {
            if (BingoSyncExtension.deprecated)
            {
                return false;
            }

            bool value = (bool)_bingoTrackerType.GetMethod("GetBoolean").Invoke(null, new object[] { variableName });
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetBoolean: {variableName} = {value}");
            }
            return value;
        }

        public static void UpdateBoolean(string variableName, bool value)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }

            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateBoolean: {variableName} = {value}");
            }
            _bingoTrackerType.GetMethod("UpdateBoolean").Invoke(null, new object[] { variableName, value });
        }

        public static void SetBoolean(string variableName, bool value)
        {
            if (BingoSyncExtension.deprecated)
            {
                return;
            }

            UpdateBoolean(variableName, value);
        }
    }
}
