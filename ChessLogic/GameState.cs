using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        public int noCaptureOrPawnMoves = 0;
        public string stateString;

        // History of state strings (for threefold repetition)
        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        // Return all legal moves for the piece at the given position
        public IEnumerable<Move> LegalMoveForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return new List<Move>();
            }

            Piece piece = Board[pos];

            List<Move> legalMoves = new List<Move>();
            foreach (Move candidate in piece.GetMoves(pos, Board))
            {
                if (candidate.IsLegal(Board))
                {
                    legalMoves.Add(candidate);
                }
            }
            return legalMoves;
        }

        public bool MakeMove(Move move)
        {
            if (!move.IsLegal(Board))
            {
                return false;
            }

            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawn = move.Execute(Board);

            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
                // stateHistory.Clear();
            }
            else
            {
                noCaptureOrPawnMoves++;
            }

            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver();

            return true;
        }

        // Return all legal moves for the given player
        public IEnumerable<Move> AllLegalMoveFor(Player player)
        {
            List<Move> legalMoves = new List<Move>();
            foreach (Position pos in Board.PiecePositionsFor(player))
            {
                Piece piece = Board[pos];

                foreach (Move candidate in piece.GetMoves(pos, Board))
                {
                    if (candidate.IsLegal(Board))
                    {
                        legalMoves.Add(candidate);
                    }
                }
            }

            return legalMoves;
        }

        private void CheckForGameOver()
        {
            bool hasAnyLegalMove = false;
            foreach (Move m in AllLegalMoveFor(CurrentPlayer))
            {
                hasAnyLegalMove = true;
                break;
            }

            if (!hasAnyLegalMove)
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result = Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
                return;
            }

            if (Board.InsufficientMaterial())
            {
                Result = Result.Draw(EndReason.InsufficientMaterial);
                return;
            }

            if (FiftyMoveRule())
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
                return;
            }

            if (ThreefoldRepetition())
            {
                Result = Result.Draw(EndReason.ThreefoldRepetition);
                return;
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }

        private bool ThreefoldRepetition()
        {
            if (stateHistory.TryGetValue(stateString, out int count))
            {
                return count == 3;
            }
            return false;
        }

        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();

            if (!stateHistory.ContainsKey(stateString))
            {
                stateHistory[stateString] = 1;
            }
            else
            {
                stateHistory[stateString]++;
            }
        }
    }
}
