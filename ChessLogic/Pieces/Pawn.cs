using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Pawn : Piece
    {
        public override PieceType Type => PieceType.Pawn;
        public override Player Color { get; }

        private readonly Direction forward;

        public Pawn(Player color)
        {
            Color = color;

            if (color == Player.White)
            {
                forward = Direction.North;
            }
            else if (color == Player.Black)
            {
                forward = Direction.South;
            }
        }

        public override Piece Copy()
        {
            Pawn copy = new Pawn(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private static bool CanMoveTo(Position pos, Board board)
        {
            return Board.IsInside(pos) && board.IsEmpty(pos);
        }

        private bool CanCaptureAt(Position pos, Board board)
        {
            if (!Board.IsInside(pos) || board.IsEmpty(pos))
            {
                return false;
            }

            return board[pos].Color != Color;
        }

        private static IEnumerable<Move> PromotionMoves(Position from, Position to)
        {
            yield return new PawnPromotion(from, to, PieceType.Knight);
            yield return new PawnPromotion(from, to, PieceType.Bishop);
            yield return new PawnPromotion(from, to, PieceType.Rook);
            yield return new PawnPromotion(from, to, PieceType.Queen);
        }

        private IEnumerable<Move> ForwardMoves(Position from, Board board)
        {
            Position oneMovePos = from + forward;

            if (CanMoveTo(oneMovePos, board))
            {
                if (oneMovePos.Row == 0 || oneMovePos.Row == 7)
                {
                    foreach (Move proMove in PromotionMoves(from, oneMovePos))
                    {
                        yield return proMove;
                    }
                }
                else
                {
                    yield return new NormalMove(from, oneMovePos);
                }

                Position twoMovePos = oneMovePos + forward;

                if (!HasMoved && CanMoveTo(twoMovePos, board))
                {
                    yield return new DoublePawn(from, twoMovePos);
                }
            }
        }

        private IEnumerable<Move> DiagonalMoves(Position from, Board board)
        {
            foreach (Direction dir in new Direction[] { Direction.West, Direction.East })
            {
                Position to = from + forward + dir;

                Position skipPos = board.GetPawnSkipPosition(Color.Opponent());
                if (skipPos != null && to == skipPos)
                {
                    yield return new EnPassant(from, to);
                }
                else if (CanCaptureAt(to, board))
                {
                    if (to.Row == 0 || to.Row == 7)
                    {
                        foreach (Move proMove in PromotionMoves(from, to))
                        {
                            yield return proMove;
                        }
                    }
                    else
                    {
                        yield return new NormalMove(from, to);
                    }
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Move m in ForwardMoves(from, board))
            {
                yield return m;
            }
            foreach (Move m in DiagonalMoves(from, board))
            {
                yield return m;
            }
        }

        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            foreach (Move move in DiagonalMoves(from, board))
            {
                Piece piece = board[move.ToPos];
                if (piece != null && piece.Type == PieceType.King)
                {
                    return true;
                }
            }
            return false;
        }
    }
}