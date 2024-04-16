using System.Collections.Generic;

namespace BingoSyncExtension
{
    public class LocalBingoSquare
    {
        public string Name = string.Empty;
        public LocalCondition Condition = new LocalCondition();
        public bool CanUnmark = false;
    }

    public class LocalCondition
    {
        public LocalConditionType Type = LocalConditionType.And;
        public int Amount = 0;
        public bool Solved = false;
        public string VariableName = string.Empty;
        public LocalBingoRequirementState State = LocalBingoRequirementState.Current;
        public int ExpectedQuantity = 0;
        public bool ExpectedValue = false;
        public List<LocalCondition> Conditions = new List<LocalCondition>();
    }

    public enum LocalConditionType
    {
        Bool,
        Int,
        Or,
        And,
        Some,
    }
    public enum LocalBingoRequirementState
    {
        Current,
        Added,
        Removed,
    }

}
