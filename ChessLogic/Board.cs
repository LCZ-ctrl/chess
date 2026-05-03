using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Board
    {
        private readonly Piece[,] pieces = new Piece[8, 8];

        // Stores en-passant target square (pawn skip) for each player
        private readonly Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>
        {
            {Player.White, null },
            {Player.Black, null }
        };

        // Indexer by row/column
        public Piece this[int row, int col]
        {
            get { return pieces[row, col]; }
            set { pieces[row, col] = value; }
        }

        // Indexer by Position
        public Piece this[Position pos]
        {
            get { return this[pos.Row, pos.Column]; }
            set { this[pos.Row, pos.Column] = value; }
        }

        public Position GetPawnSkipPosition(Player player)
        {
            return pawnSkipPositions[player];
        }

        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        public static Board Initial()
        {
            Board board = new Board();
            board.AddStartPieces();
            return board;
        }

        private void AddStartPieces()
        {
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            for (int c = 0; c < 8; c++)
            {
                this[1, c] = new Pawn(Player.Black);
                this[6, c] = new Pawn(Player.White);
            }
        }

        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        // Enumerate all positions that currently contain a piece
        public IEnumerable<Position> PiecePositions()
        {
            for(int r = 0; r < 8; r++)
            {
                for(int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        // Enumerate positions that contain a piece belonging to the given player
        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            foreach (Position pos in PiecePositions())
            {
                if (this[pos].Color == player)
                {
                    yield return pos;
                }
            }
        }

        public bool IsInCheck(Player player)
        {
            Player opponent = player.Opponent();

            foreach (Position pos in PiecePositionsFor(opponent))
            {
                Piece piece = this[pos];
                if (piece.CanCaptureOpponentKing(pos, this))
                {
                    return true;
                }
            }
            return false;
        }

        // Deep copy of the board and pieces
        public Board Copy()
        {
            Board copy = new Board();
            foreach(Position pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }

            copy.pawnSkipPositions[Player.White] = this.pawnSkipPositions[Player.White];
            copy.pawnSkipPositions[Player.Black] = this.pawnSkipPositions[Player.Black];
            return copy;
        }

        public Counting CountPieces()
        {
            Counting counting = new Counting();
            foreach(Position pos in PiecePositions())
            {
                Piece piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }
            return counting;
        }

        public bool InsufficientMaterial()
        {
            Counting counting = CountPieces();

            // King vs King
            if (IsKingVKing(counting)) return true;

            // King + Bishop vs King
            if (IsKingBishopVKing(counting)) return true;

            // King + Knight vs King
            if (IsKingKnightVKing(counting)) return true;

            // King + Bishop vs King + Bishop (both bishops on the same color)
            if (IsKingBishopVKingBishop(counting)) return true;

            return false;
        }

        public bool IsKingVKing(Counting counting)
        {
            return counting.TotalCount == 2;
        }

        public bool IsKingBishopVKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
        }

        public bool IsKingKnightVKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
        }

        public bool IsKingBishopVKingBishop(Counting counting)
        {
            if(counting.TotalCount != 4)
            {
                return false;
            }
            if(counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1)
            {
                return false;
            }
            if (!TryFindPiece(Player.White, PieceType.Bishop, out Position wBishopPos))
            {
                return false;
            }
            if (!TryFindPiece(Player.Black, PieceType.Bishop, out Position bBishopPos))
            {
                return false;
            }
            return wBishopPos.SquareColor() == bBishopPos.SquareColor();
        }

        // Try to find the position of the specified piece type for the given player
        public bool TryFindPiece(Player color, PieceType type, out Position pos)
        {
            foreach (Position p in PiecePositionsFor(color))
            {
                if (this[p].Type == type)
                {
                    pos = p;
                    return true;
                }
            }

            pos = null;
            return false;
        }

        public Position FindPieceOrNull(Player color, PieceType type)
        {
            if (TryFindPiece(color, type, out Position pos))
            {
                return pos;
            }
            return null;
        }

        public bool IsUnmovedKingAndRook(Position kingPos, Position rookPos)
        {
            if(IsEmpty(kingPos) || IsEmpty(rookPos))
            {
                return false;
            }
            Piece king = this[kingPos];
            Piece rook = this[rookPos];
            return king.Type == PieceType.King && rook.Type == PieceType.Rook &&
                   !king.HasMoved && !rook.HasMoved;
        }

        public bool CastleRightKS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                _ => false
            };
        }

        public bool CastleRightQS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 0)),
                _ => false
            };
        }

        // Check if the player has a pawn in any of the provided pawnPositions that can capture en-passant to skipPos
        public bool HasPawnInPosition(Player player, Position[] pawnPositions, Position skipPos)
        {
            foreach(Position pos in pawnPositions)
            {
                if (!IsInside(pos))
                {
                    continue;
                }

                Piece piece = this[pos];
                if(piece == null || piece.Color != player || piece.Type != PieceType.Pawn)
                {
                    continue;
                }

                // Create an EnPassant move candidate and test legality
                EnPassant move = new EnPassant(pos, skipPos);
                if (move.IsLegal(this))
                {
                    return true;
                }
            }
            return false;
        }

        // Determine if the player can capture en-passant given the current pawn-skip position
        public bool CanCaptureEnPassant(Player player)
        {
            Position skipPos = GetPawnSkipPosition(player.Opponent());
            if(skipPos == null)
            {
                return false;
            }

            Position[] pawnPositions = player switch
            {
                Player.White => new Position[] { skipPos + Direction.SouthWest, skipPos + Direction.SouthEast },
                Player.Black => new Position[] { skipPos + Direction.NorthWest, skipPos + Direction.NorthEast },
                _ => Array.Empty<Position>()
            };

            return HasPawnInPosition(player, pawnPositions, skipPos);
        }
    }
}
