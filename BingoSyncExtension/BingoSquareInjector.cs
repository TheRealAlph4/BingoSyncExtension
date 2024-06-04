using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BingoSyncExtension
{
    public static class BingoSquareInjector
    {
        private static Action<string> Log;
        public static Assembly _bingoSyncAssembly;
        private static Type _bingoTrackerType;
        private static Type _bingoSquareType;
        private static Type _conditionObjType;
        private static Type _conditionTypeEnumType;
        private static Type _conditionStateEnumType;

        private static FieldInfo _bingoSquareNameField;
        private static FieldInfo _bingoSquareConditionField;
        private static FieldInfo _bingoSquareCanUnmarkField;

        private static FieldInfo _conditionTypeField;
        private static FieldInfo _conditionAmountField;
        private static FieldInfo _conditionSolvedField;
        private static FieldInfo _conditionVariableNameField;
        private static FieldInfo _conditionStateField;
        private static FieldInfo _conditionExpectedQuantityField;
        private static FieldInfo _conditionExpectedValueField;
        private static FieldInfo _conditionConditionsField;

        public static void Setup(Action<string> log)
        {
            Log = log;
            Log("getting bingosync");
            foreach(IMod loadedmod in ModHooks.GetAllMods())
            {
                Log(loadedmod.GetName());
            }
            if (ModHooks.GetMod("BingoSync") is { } mod)
            {
                Log("setting up stuff");
                SetupReflection(mod);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetupReflection(IMod mod)
        {
            _bingoSyncAssembly = mod.GetType().Assembly;

            _bingoTrackerType = _bingoSyncAssembly.GetType("BingoSync.BingoTracker");
            _bingoSquareType = _bingoSyncAssembly.GetType("BingoSync.BingoSquare");
            _conditionObjType = _bingoSyncAssembly.GetType("BingoSync.Condition");
            _conditionTypeEnumType = _bingoSyncAssembly.GetType("BingoSync.ConditionType");
            _conditionStateEnumType = _bingoSyncAssembly.GetType("BingoSync.BingoRequirementState");

            _bingoSquareNameField = _bingoSquareType.GetField("Name");
            _bingoSquareConditionField = _bingoSquareType.GetField("Condition");
            _bingoSquareCanUnmarkField = _bingoSquareType.GetField("CanUnmark");

            _conditionTypeField = _conditionObjType.GetField("Type");
            _conditionAmountField = _conditionObjType.GetField("Amount");
            _conditionSolvedField = _conditionObjType.GetField("Solved");
            _conditionVariableNameField = _conditionObjType.GetField("VariableName");
            _conditionStateField = _conditionObjType.GetField("State");
            _conditionExpectedQuantityField = _conditionObjType.GetField("ExpectedQuantity");
            _conditionExpectedValueField = _conditionObjType.GetField("ExpectedValue");
            _conditionConditionsField = _conditionObjType.GetField("Conditions");
        }

        public static Dictionary<string, BingoGoal> ProcessGoalsFile(string filepath)
        {
            Dictionary<string, BingoGoal> goals = [];
            List<LocalBingoSquare> squares = BingoSquareReader.ReadFromFile(filepath);
            FieldInfo allSquaresField = _bingoTrackerType.GetField("_allPossibleSquares", BindingFlags.NonPublic | BindingFlags.Static);
            IList allSquaresList = (IList) allSquaresField.GetValue(null);

            foreach (LocalBingoSquare square in squares)
            {
                goals.Add(square.Name, new BingoGoal(square.Name, []));
                allSquaresList.Add(ConvertSquare(square));
            }

            allSquaresField.SetValue(null, allSquaresList);
            
            return goals;
        }

        private static object ConvertSquare(LocalBingoSquare square)
        {
            object newSquare = Activator.CreateInstance(_bingoSquareType);
            _bingoSquareNameField.SetValue(newSquare, square.Name);
            _bingoSquareConditionField.SetValue(newSquare, ConvertCondition(square.Condition));
            _bingoSquareCanUnmarkField.SetValue(newSquare, square.CanUnmark);
            return newSquare;
        }
        
        private static object ConvertCondition(LocalCondition condition)
        {
            object newCondition = Activator.CreateInstance(_conditionObjType);

            _conditionTypeField.SetValue(newCondition, ConvertConditionType(condition.Type));
            _conditionAmountField.SetValue(newCondition, condition.Amount);
            _conditionSolvedField.SetValue(newCondition, condition.Solved);
            _conditionVariableNameField.SetValue(newCondition, condition.VariableName);
            _conditionStateField.SetValue(newCondition, ConvertConditionState(condition.State));
            _conditionExpectedQuantityField.SetValue(newCondition, condition.ExpectedQuantity);
            _conditionExpectedValueField.SetValue(newCondition, condition.ExpectedValue);
            IList subconditions = (IList)_conditionConditionsField.GetValue(newCondition);
            foreach (LocalCondition subcondition in condition.Conditions)
            {
                subconditions.Add(ConvertCondition(subcondition));
            }
            _conditionConditionsField.SetValue(newCondition, subconditions);

            return newCondition;
        }

        private static object ConvertConditionType(LocalConditionType conditionType)
        {
            object newConditionType = Activator.CreateInstance(_conditionTypeEnumType);

            switch (conditionType)
            {
                case LocalConditionType.Bool: 
                    newConditionType = _conditionTypeEnumType.GetEnumValues().GetValue(0); 
                    break;
                case LocalConditionType.Int:
                    newConditionType = _conditionTypeEnumType.GetEnumValues().GetValue(1); 
                    break;
                case LocalConditionType.Or:
                    newConditionType = _conditionTypeEnumType.GetEnumValues().GetValue(2); 
                    break;
                case LocalConditionType.And:
                    newConditionType = _conditionTypeEnumType.GetEnumValues().GetValue(3); 
                    break;
                case LocalConditionType.Some:
                    newConditionType = _conditionTypeEnumType.GetEnumValues().GetValue(4);
                    break;
            }

            return newConditionType;
        }

        public static object ConvertConditionState(LocalBingoRequirementState state)
        {
            object newConditionState = Activator.CreateInstance(_conditionStateEnumType);

            switch (state)
            {
                case LocalBingoRequirementState.Current:
                    newConditionState = _conditionStateEnumType.GetEnumValues().GetValue(0);
                    break;
                case LocalBingoRequirementState.Added:
                    newConditionState = _conditionStateEnumType.GetEnumValues().GetValue(1);
                    break;
                case LocalBingoRequirementState.Removed:
                    newConditionState = _conditionStateEnumType.GetEnumValues().GetValue(2);
                    break;
            }

            
            return newConditionState;
        }
    }
}
