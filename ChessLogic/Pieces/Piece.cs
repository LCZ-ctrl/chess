using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public abstract class Piece
    {
        public abstract PieceType Type { get; }
        public abstract Player Color { get; }
        public bool HasMoved { get; set; } = false;
        public abstract Piece Copy();
        public abstract IEnumerable<Move> GetMoves(Position from, Board board);

        // All legal positions moving from 'from' along 'dir'
        // Includes capture on first enemy piece
        protected IEnumerable<Position> MovePositionsInDir(Position from, Board board, Direction dir)
        {
            for(Position pos = from + dir; Board.IsInside(pos); pos += dir)
            {
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }

                Piece piece = board[pos];
                if(piece.Color != Color)
                {
                    yield return pos;
                }
                yield break;
            }
        }

        // Expand multiple directions into a single sequence of positions
        protected IEnumerable<Position> MovePositionsInDirs(Position from, Board board, Direction[] dirs)
        {
            foreach (Direction dir in dirs)
            {
                foreach (Position p in MovePositionsInDir(from, board, dir))
                {
                    yield return p;
                }
            }
        }

        public virtual bool CanCaptureOpponentKing(Position from, Board board)
        {
            foreach (Move move in GetMoves(from, board))
            {
                Piece target = board[move.ToPos];
                if (target != null && target.Type == PieceType.King)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
