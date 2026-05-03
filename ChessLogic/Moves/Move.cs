using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public abstract class Move
    {
        public abstract MoveType Type { get; }
        public abstract Position FromPos { get; }
        public abstract Position ToPos { get; }

        // Execute the move on the given board
        public abstract bool Execute(Board board);

        public virtual bool IsLegal(Board board)
        {
            if (!Board.IsInside(FromPos) || board.IsEmpty(FromPos))
            {
                return false;
            }

            Player player = board[FromPos].Color;

            // Use a copy of the board, perform the move, and test whether king is in check
            Board boardCopy = board.Copy();
            Execute(boardCopy);
            return !boardCopy.IsInCheck(player);
        }
    }
}
