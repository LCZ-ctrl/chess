using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();
        private GameState gameState;
        private Position selectedPos = null;
        private Move lastMove = null;
        private readonly Rectangle[,] lastMoveRects = new Rectangle[8, 8];
        private Position lastFrom = null;
        private Position lastTo = null;

        // VS Computer fields
        private bool vsComputerMode;
        private Player humanPlayer;
        private int engineSkillLevel;
        private StockfishEngine engine;
        private CancellationTokenSource computerCancel;
        private readonly List<string> uciHistory = new();
        private bool computerThinking;

        // Whether to flip the chessboard
        private bool boardFlipped = false;
        private int? lastVisualFromRow = null;
        private int? lastVisualToRow = null;
        private int? lastVisualFromCol = null;
        private int? lastVisualToCol = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        public void StartComputerGame(Player humanSide, int skillLevel)
        {
            vsComputerMode = true;
            humanPlayer = humanSide;
            engineSkillLevel = skillLevel;  
            boardFlipped = (humanSide == Player.Black);

            if (engine == null)
            {
                engine = new StockfishEngine();
            }
            computerCancel?.Cancel();
            computerCancel?.Dispose();
            computerCancel = new CancellationTokenSource();
            RestartGame();
        }

        private Player GetComputerPlayer() => humanPlayer.Opponent();

        private async Task ComputerTurnAsync()
        {
            if (!vsComputerMode || computerThinking) return;
            computerThinking = true;
            Dispatcher.Invoke(() =>
            {
                HideHightlights();
                selectedPos = null;
                moveCache.Clear();
                SetCursor(gameState.CurrentPlayer);
            });

            try
            {
                await Task.Delay(2000, computerCancel.Token);
                var legals = GetLegalMoves().ToList();
                if (legals.Count == 0) return;
                string movesUci = string.Join(" ", uciHistory);
                string bestUci = await engine.GetBestMoveUciAsync(movesUci, engineSkillLevel, computerCancel.Token);
                Move? bestMove = legals.FirstOrDefault(m => MoveToUci(m) == bestUci);
                if (bestMove != null)
                {
                    Dispatcher.Invoke(() => HandleMove(bestMove));
                }
            }
            catch (OperationCanceledException)
            {
                // Paused or cancelled
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ComputerTurnAsync error: " + ex);
            }
            finally
            {
                computerThinking = false;
            }
        }

        private IEnumerable<Move> GetLegalMoves()
        {
            return Enumerable.Range(0, 8)
                .SelectMany(r => Enumerable.Range(0, 8)
                .Select(c => new Position(r, c)))
                .Where(p => !gameState.Board.IsEmpty(p) && gameState.Board[p].Color == gameState.CurrentPlayer)
                .SelectMany(p => gameState.LegalMoveForPiece(p));
        }

        private static string PosToUci(Position pos)
        {
            return $"{(char)('a' + pos.Column)}{(char)('1' + 7 - pos.Row)}";
        }

        private static char PieceTypeToPromChar(PieceType pt)
        {
            return pt switch
            {
                PieceType.Queen => 'q',
                PieceType.Rook => 'r',
                PieceType.Bishop => 'b',
                PieceType.Knight => 'n',
                _ => throw new ArgumentException($"Invalid promotion: {pt}")
            };
        }

        private string MoveToUci(Move move)
        {
            string from = PosToUci(move.FromPos);
            string to = PosToUci(move.ToPos);
            if (move.Type == MoveType.PawnPromotion)
            {
                var pp = (PawnPromotion)move;
                return from + to + PieceTypeToPromChar(pp.PromotedTo);
            }
            return from + to;
        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image
                    {
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    image.RenderTransform = new ScaleTransform(0.8, 0.8);
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle lastRect = new Rectangle
                    {
                        Fill = Brushes.Transparent,
                        IsHitTestVisible = false
                    };
                    lastMoveRects[r, c] = lastRect;
                    LastMoveGrid.Children.Add(lastRect);

                    Rectangle highlight = new Rectangle
                    {
                        Fill = Brushes.Transparent,
                        IsHitTestVisible = false
                    };
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            if (board == null) return;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    int boardRow = boardFlipped ? 7 - r : r;
                    int boardCol = boardFlipped ? 7 - c : c;

                    Piece piece = board[boardRow, boardCol];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen() || computerThinking || (vsComputerMode && gameState.CurrentPlayer != humanPlayer))
            {
                return;
            }
            try
            {
                Point point = e.GetPosition(BoardGrid);
                bool ok = TryGetSquareFromPoint(point, out Position pos);
                if (!ok)
                {
                    return;
                }
                if (selectedPos == null)
                {
                    OnFromPositionSelected(pos);
                }
                else
                {
                    OnToPositionSelected(pos);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BoardGrid_MouseDown error: " + ex);
            }
        }

        private bool TryGetSquareFromPoint(Point point, out Position pos)
        {
            pos = null!;
            double width = BoardGrid.RenderSize.Width;
            double height = BoardGrid.RenderSize.Height;
            if (double.IsNaN(width) || double.IsNaN(height) || width <= 0.0 || height <= 0.0)
            {
                return false;
            }
            double squareSize = Math.Min(width, height) / 8.0;
            double x = point.X;
            double y = point.Y;
            if (x < -1 || y < -1 || x > width + 1 || y > height + 1)
            {
                return false;
            }
            x = Math.Max(0, Math.Min(x, width - 1e-6));
            y = Math.Max(0, Math.Min(y, height - 1e-6));
            int visualCol = (int)(x / squareSize);
            int boardCol = boardFlipped ? 7 - visualCol : visualCol;

            int visualRow = (int)(y / squareSize);
            int boardRow = boardFlipped ? 7 - visualRow : visualRow;

            if (visualRow < 0 || visualRow > 7 || visualCol < 0 || visualCol > 7) return false;
            pos = new Position(boardRow, boardCol);
            return true;
        }

        private void OnFromPositionSelected(Position pos)
        {
            var movesList = gameState.LegalMoveForPiece(pos).ToList();
            if (movesList.Count > 0)
            {
                selectedPos = pos;
                CacheMoves(movesList);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHightlights();
            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;
            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;
            Action<PieceType> handler = null!;
            handler = type =>
            {
                try
                {
                    MenuContainer.Content = null;
                    Move promMove = new PawnPromotion(from, to, type);
                    HandleMove(promMove);
                }
                finally
                {
                    promMenu.PieceSelected -= handler;
                }
            };
            promMenu.PieceSelected += handler;
        }

        private void HandleMove(Move move)
        {
            bool wasCapture = false;
            try
            {
                if (move == null)
                {
                    wasCapture = false;
                }
                else if (move.Type == MoveType.EnPassant)
                {
                    Position capturedPawnPos = new Position(move.FromPos.Row, move.ToPos.Column);
                    if (Board.IsInside(capturedPawnPos))
                        wasCapture = !gameState.Board.IsEmpty(capturedPawnPos);
                }
                else
                {
                    if (Board.IsInside(move.ToPos))
                        wasCapture = !gameState.Board.IsEmpty(move.ToPos);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Capture detection failed: " + ex);
                wasCapture = false;
            }

            bool movedOk = false;
            try
            {
                movedOk = gameState.MakeMove(move);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MakeMove threw exception: " + ex);
                movedOk = false;
            }

            if (!movedOk)
            {
                DrawBoard(gameState.Board);
                SetCursor(gameState.CurrentPlayer);
                HideHightlights();
                return;
            }

            // Add to history after successful move
            uciHistory.Add(MoveToUci(move));

            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            ShowLastMove(move.FromPos, move.ToPos);

            try
            {
                if (gameState.IsGameOver())
                {
                    try
                    {
                        MusicManager.PlaySound("gameover");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Play gameover sound failed: " + ex);
                    }
                    computerCancel?.Cancel();
                    computerThinking = false;
                    ShowGameOver();
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("IsGameOver check failed: " + ex);
            }

            bool gaveCheck = false;
            try
            {
                if (gameState.Board.IsInCheck(gameState.CurrentPlayer))
                    gaveCheck = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("IsInCheck check failed: " + ex);
                gaveCheck = false;
            }

            if (gaveCheck)
            {
                MusicManager.PlaySound("check");
            }
            else
            {
                switch (move.Type)
                {
                    case MoveType.CastleKS:
                    case MoveType.CastleQS:
                        MusicManager.PlaySound("castle");
                        break;
                    case MoveType.PawnPromotion:
                        MusicManager.PlaySound("promote");
                        break;
                    default:
                        if (wasCapture)
                            MusicManager.PlaySound("capture");
                        else
                            MusicManager.PlaySound("move");
                        break;
                }
            }

            // Computer's turn?
            if (vsComputerMode && gameState.CurrentPlayer == GetComputerPlayer())
            {
                _ = ComputerTurnAsync();
            }
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowLastMove(Position from, Position to)
        {
            ClearLastMove(); 

            if (from == null || to == null) return;
            if (!Board.IsInside(from) || !Board.IsInside(to)) return;

            int visualFromRow = boardFlipped ? 7 - from.Row : from.Row;
            int visualToRow = boardFlipped ? 7 - to.Row : to.Row;

            int visualFromCol = boardFlipped ? 7 - from.Column : from.Column;
            int visualToCol = boardFlipped ? 7 - to.Column : to.Column;

            Color lmColor = Color.FromArgb(180, 250, 128, 114);
            var lmBrush = new SolidColorBrush(lmColor);
            lmBrush.Freeze();

            lastMoveRects[visualFromRow, visualFromCol].Fill = lmBrush;
            lastMoveRects[visualToRow, visualToCol].Fill = lmBrush;

            lastFrom = from;
            lastTo = to;
            lastVisualFromRow = visualFromRow;
            lastVisualToRow = visualToRow;
            lastVisualFromCol = visualFromCol;
            lastVisualToCol = visualToCol;
        }

        private void ClearLastMove()
        {
            if (lastVisualFromRow.HasValue && lastVisualFromCol.HasValue)
            {
                lastMoveRects[lastVisualFromRow.Value, lastVisualFromCol.Value].Fill = Brushes.Transparent;
            }
            if (lastVisualToRow.HasValue && lastVisualToCol.HasValue)
            {
                lastMoveRects[lastVisualToRow.Value, lastVisualToCol.Value].Fill = Brushes.Transparent;
            }

            lastFrom = null;
            lastTo = null;
            lastVisualFromRow = null;
            lastVisualToRow = null;
            lastVisualFromCol = null;
            lastVisualToCol = null;
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125);
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    highlights[r, c].Fill = Brushes.Transparent;
                }
            }

            foreach (Position to in moveCache.Keys)
            {
                if (to != null && to.Row >= 0 && to.Row < 8 && to.Column >= 0 && to.Column < 8)
                {
                    int visualRow = boardFlipped ? 7 - to.Row : to.Row;
                    int visualCol = boardFlipped ? 7 - to.Column : to.Column;
                    highlights[visualRow, visualCol].Fill = brush;
                }
            }
        }

        private void HideHightlights()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    highlights[r, c].Fill = Brushes.Transparent;
                }
            }
        }

        private void SetCursor(Player player)
        {
            try
            {
                if (player == Player.White)
                {
                    Cursor = ChessCursors.WhiteCursor ?? Cursors.Arrow;
                }
                else
                {
                    Cursor = ChessCursors.BlackCursor ?? Cursors.Arrow;
                }
            }
            catch
            {
                Cursor = Cursors.Arrow;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;
            Action<Option> handler = null!;
            handler = option =>
            {
                try
                {
                    MenuContainer.Content = null;
                    switch (option)
                    {
                        case Option.Restart:
                            RestartGame();
                            break;
                        case Option.Menu:
                            OpenMainMenu();
                            break;
                        case Option.Exit:
                            Application.Current.Shutdown();
                            break;
                    }
                }
                finally
                {
                    gameOverMenu.OptionSelected -= handler;
                }
            };
            gameOverMenu.OptionSelected += handler;
        }

        private void OpenMainMenu()
        {
            ClearLastMove();
            WindowManager.Show(WindowManager.MainMenu, this);
            MusicManager.PlayMenuMusic();
        }

        public void RestartGame()
        {
            selectedPos = null;
            HideHightlights();
            moveCache.Clear();
            ClearLastMove();
            uciHistory.Clear();
            computerCancel?.Cancel();
            computerCancel?.Dispose();
            computerCancel = vsComputerMode ? new CancellationTokenSource() : null;
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
            // Auto-start computer if their turn
            if (vsComputerMode && gameState.CurrentPlayer == GetComputerPlayer())
            {
                _ = ComputerTurnAsync();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!IsMenuOnScreen() && e.Key == Key.Escape)
                {
                    ShowPauseMenu();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Window_KeyDown error: " + ex);
            }
        }

        private void ShowPauseMenu()
        {
            // Stop computer thinking
            computerCancel?.Cancel();
            computerThinking = false;
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;
            Action<Option> handler = null!;
            handler = option =>
            {
                try
                {
                    MenuContainer.Content = null;
                    switch (option)
                    {
                        case Option.Continue:
                            if (vsComputerMode && gameState.CurrentPlayer == GetComputerPlayer() && !computerThinking)
                            {
                                computerCancel = new CancellationTokenSource();
                                _ = ComputerTurnAsync();
                            }
                            break;
                        case Option.Restart:
                            RestartGame();
                            break;
                        case Option.Menu:
                            OpenMainMenu();
                            break;
                    }
                }
                finally
                {
                    pauseMenu.OptionSelected -= handler;
                }
            };
            pauseMenu.OptionSelected += handler;
        }

        protected override void OnClosed(EventArgs e)
        {
            engine?.Dispose();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        public void ResetToHumanVsHuman()
        {
            vsComputerMode = false;
            boardFlipped = false;

            // Stop and release the engine (to save resources)
            computerCancel?.Cancel();
            computerCancel?.Dispose();
            computerCancel = null;
            // engine?.Dispose();  // completely unleash the engine
            uciHistory.Clear();
            computerThinking = false;

            RestartGame();
        }
    }
}