using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class DoublePawn : Move
    {
        public override MoveType Type => MoveType.DoublePawn;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        // The square that the pawn skips over (used for en-passant)
        private readonly Position skippedPos;

        public DoublePawn(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
            skippedPos = new Position((from.Row + to.Row) / 2, from.Column);
        }

        public override bool Execute(Board board)
        {
            Player player = board[FromPos].Color;

            // Record the en-passant target square for the player who moved the pawn
            board.SetPawnSkipPosition(player, skippedPos);
            new NormalMove(FromPos, ToPos).Execute(board);

            return true;
        }
    }
}