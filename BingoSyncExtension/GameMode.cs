using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoSyncExtension
{
    public class GameMode
    {
        private readonly string name;
        private List<BingoGoal> goals;
        
        public GameMode(string name, List<BingoGoal> goals)
        {
            this.name = name;
            this.goals = goals;
        }
        public List<BingoGoal> GetGoals()
        {
            return goals;
        }
        public void SetGoals(List<BingoGoal> goals)
        {
            this.goals = goals;
        }
        public string GetName()
        {
            return name;
        }
        public string GenerateBoard()
        {
            List<BingoGoal> board = new();
            List<BingoGoal> availableGoals = new(goals);
            Random r = new();

            while (board.Count < 25) {
                int index = r.Next(availableGoals.Count);
                BingoGoal proposedGoal = availableGoals[index];
                bool valid = true;
                foreach(BingoGoal existing in board)
                {
                    if(existing.Excludes(proposedGoal) || proposedGoal.Excludes(existing))
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    board.Add(proposedGoal);
                }
                availableGoals.Remove(proposedGoal);
                if(availableGoals.Count == 0)
                {
                    return GetErrorBoard();
                }
            }

            return Jsonify(board);
        }
        public string Jsonify(List<BingoGoal> board)
        {
            string output = "[";
            for (int i = 0; i < board.Count; i++)
            {
                output += "{\"name\": \"" + board.ElementAt(i).name + "\"}" + (i < 24 ? "," : "");
            }
            output += "]";
            return output;
        }
        private string GetErrorBoard()
        {
            return "[{\"name\": \"Error generating board\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"},{\"name\": \"-\"}]";
        }
    }
}
