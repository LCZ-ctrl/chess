namespace ChessLogic
{
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;
        public override Player Color { get; }

        public Knight(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Knight copy = new Knight(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private static IEnumerable<Position> PotentialToPositions(Position from)
        {
            foreach (Direction vDir in new Direction[] { Direction.North, Direction.South })
            {
                foreach (Direction hDir in new Direction[] { Direction.West, Direction.East })
                {
                    yield return from + 2 * vDir + hDir;
                    yield return from + 2 * hDir + vDir;
                }
            }
        }

        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            foreach (Position pos in PotentialToPositions(from))
            {
                if (!Board.IsInside(pos))
                {
                    continue;
                }
                if (board.IsEmpty(pos) || board[pos].Color != Color)
                {
                    yield return pos;
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Position to in MovePositions(from, board))
            {
                yield return new NormalMove(from, to);
            }
        }
    }
}