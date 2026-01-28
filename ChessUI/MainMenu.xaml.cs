using System;
using System.Windows;
using System.Windows.Media;

namespace ChessUI
{
    public partial class MainMenu : Window
    {
        private bool _instructionVisible = false;
        private ChessLogic.Player? selectedSide;
        private Difficulty? selectedDifficulty;
        private enum Difficulty { Beginner, Easy, Medium, Hard, Master }

        private Brush normalBrush;
        private Brush selectedBrush;

        public MainMenu()
        {
            InitializeComponent();
            normalBrush = (SolidColorBrush)FindResource("ButtonColor");
            selectedBrush = (SolidColorBrush)FindResource("SelectedButtonColor");
            SetLanguage(LanguageManager.CurrentLanguage);
            this.IsVisibleChanged += (s, e) =>
            {
                if (this.IsVisible)
                {
                    MusicManager.PlayMenuMusic();

                    MainMenuPanel.Visibility = Visibility.Visible;
                    ComputerMenuPanel.Visibility = Visibility.Collapsed;
                    LanguageMenuPanel.Visibility = Visibility.Collapsed;
                    InstructionBox.Visibility = Visibility.Collapsed;

                    selectedSide = null;
                    selectedDifficulty = null;
                    UpdateSideButtons();
                    UpdateDifficultyButtons();
                    UpdateStartEnabled();

                    _instructionVisible = false;
                }
            };
            LanguageManager.LanguageChanged += OnLanguageChanged;
            this.Closed += (s, e) => LanguageManager.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            Dispatcher.BeginInvoke(new Action(() => SetLanguage(LanguageManager.CurrentLanguage)));
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MusicManager.Stop();

            if (WindowManager.GameWindow is MainWindow mainWindow)
            {
                mainWindow.ResetToHumanVsHuman();
            }

            if (WindowManager.MainWindow != null)
            {
                try
                {
                    WindowManager.MainWindow.RestartGame();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("RestartGame failed: " + ex);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: MainWindow is null when Play clicked.");
            }
            WindowManager.Show(WindowManager.GameWindow, this, 0);
        }

        private void ComputerButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenuPanel.Visibility = Visibility.Collapsed;
            ComputerMenuPanel.Visibility = Visibility.Visible;
        }

        private void WhiteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSide == ChessLogic.Player.White)
            {
                selectedSide = null;  
            }
            else
            {
                selectedSide = ChessLogic.Player.White;
            }
            UpdateSideButtons();
            UpdateStartEnabled();
        }

        private void BlackButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSide == ChessLogic.Player.Black)
            {
                selectedSide = null;
            }
            else
            {
                selectedSide = ChessLogic.Player.Black;
            }
            UpdateSideButtons();
            UpdateStartEnabled();
        }

        private void BeginnerButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDifficulty == Difficulty.Beginner)
            {
                selectedDifficulty = null;
            }
            else
            {
                selectedDifficulty = Difficulty.Beginner;
            }
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void EasyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDifficulty == Difficulty.Easy)
            {
                selectedDifficulty = null;
            }
            else
            {
                selectedDifficulty = Difficulty.Easy;
            }
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDifficulty == Difficulty.Medium)
            {
                selectedDifficulty = null;
            }
            else
            {
                selectedDifficulty = Difficulty.Medium;
            }
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDifficulty == Difficulty.Hard)
            {
                selectedDifficulty = null;
            }
            else
            {
                selectedDifficulty = Difficulty.Hard;
            }
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void MasterButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDifficulty == Difficulty.Master)
            {
                selectedDifficulty = null;
            }
            else
            {
                selectedDifficulty = Difficulty.Master;
            }
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void UpdateSideButtons()
        {
            WhiteButton.Background = selectedSide == ChessLogic.Player.White ? selectedBrush : normalBrush;
            BlackButton.Background = selectedSide == ChessLogic.Player.Black ? selectedBrush : normalBrush;
        }

        private void UpdateDifficultyButtons()
        {
            BeginnerButton.Background = selectedDifficulty == Difficulty.Beginner ? selectedBrush : normalBrush;
            EasyButton.Background = selectedDifficulty == Difficulty.Easy ? selectedBrush : normalBrush;
            MediumButton.Background = selectedDifficulty == Difficulty.Medium ? selectedBrush : normalBrush;
            HardButton.Background = selectedDifficulty == Difficulty.Hard ? selectedBrush : normalBrush;
            MasterButton.Background = selectedDifficulty == Difficulty.Master ? selectedBrush : normalBrush;
        }

        private void UpdateStartEnabled()
        {
            StartComputerButton.IsEnabled = selectedSide.HasValue && selectedDifficulty.HasValue;
        }

        private void StartComputerButton_Click(object sender, RoutedEventArgs e)
        {
            MusicManager.Stop();
            int skillLevel = selectedDifficulty.Value switch
            {
                Difficulty.Beginner => 0,   
                Difficulty.Easy => 5,   
                Difficulty.Medium => 10,  
                Difficulty.Hard => 15,  
                Difficulty.Master => 18,  
                _ => 3
            };

            if (WindowManager.GameWindow is MainWindow mainWindow)
            {
                mainWindow.StartComputerGame(selectedSide.Value, skillLevel);
            }

            WindowManager.Show(WindowManager.GameWindow, this, 0);
        }

        private void BackFromComputerButton_Click(object sender, RoutedEventArgs e)
        {
            ComputerMenuPanel.Visibility = Visibility.Collapsed;
            MainMenuPanel.Visibility = Visibility.Visible;
            selectedSide = null;
            selectedDifficulty = null;
            UpdateSideButtons();
            UpdateDifficultyButtons();
            UpdateStartEnabled();
        }

        private void InstructionButton_Click(object sender, RoutedEventArgs e)
        {
            _instructionVisible = !_instructionVisible;
            InstructionBox.Visibility = _instructionVisible ? Visibility.Visible : Visibility.Collapsed;
            if (_instructionVisible)
                UpdateInstructionText();
        }

        private void CloseInstructionButton_Click(object sender, RoutedEventArgs e)
        {
            _instructionVisible = false;
            InstructionBox.Visibility = Visibility.Collapsed;
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenuPanel.Visibility = Visibility.Collapsed;
            LanguageMenuPanel.Visibility = Visibility.Visible;
            UpdateLanguageTitle();
        }

        private void BackFromLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageMenuPanel.Visibility = Visibility.Collapsed;
            MainMenuPanel.Visibility = Visibility.Visible;
        }

        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(LanguageType.English);
            BackFromLanguageButton_Click(sender, e);
        }

        private void ChineseButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(LanguageType.Chinese);
            BackFromLanguageButton_Click(sender, e);
        }

        private void RussianButton_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(LanguageType.Russian);
            BackFromLanguageButton_Click(sender, e);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void SetLanguage(LanguageType lang)
        {
            switch (lang)
            {
                case LanguageType.English:
                    TitleText.Text = "CHESS";
                    PlayButton.Content = "PLAY(2P)";
                    ComputerButton.Content = "VS COMPUTER";
                    InstructionButton.Content = "INSTRUCTION";
                    LanguageButton.Content = "LANGUAGE";
                    ExitButton.Content = "EXIT";
                    BackFromLanguageButton.Content = "← BACK";
                    ComputerTitleText.Text = "VS COMPUTER";
                    SideText.Text = "CHOOSE SIDE";
                    DifficultyText.Text = "CHOOSE DIFFICULTY";
                    WhiteButton.Content = "WHITE";
                    BlackButton.Content = "BLACK";
                    BeginnerButton.Content = "BEGINNER";
                    EasyButton.Content = "EASY";
                    MediumButton.Content = "MEDIUM";
                    HardButton.Content = "HARD";
                    MasterButton.Content = "MASTER";
                    StartComputerButton.Content = "START GAME";
                    BackFromComputerButton.Content = "← BACK";
                    break;
                case LanguageType.Chinese:
                    TitleText.Text = "国际象棋";
                    PlayButton.Content = "开始游玩(2P)";
                    ComputerButton.Content = "与电脑对战";
                    InstructionButton.Content = "规则说明";
                    LanguageButton.Content = "语言";
                    ExitButton.Content = "退出";
                    BackFromLanguageButton.Content = "← 返回";
                    ComputerTitleText.Text = "电脑对战";
                    SideText.Text = "选择下棋方";
                    DifficultyText.Text = "选择难度";
                    WhiteButton.Content = "白方";
                    BlackButton.Content = "黑方";
                    BeginnerButton.Content = "初学者";
                    EasyButton.Content = "简单";
                    MediumButton.Content = "中等";
                    HardButton.Content = "困难";
                    MasterButton.Content = "大师";
                    StartComputerButton.Content = "开始游戏";
                    BackFromComputerButton.Content = "← 返回";
                    break;
                case LanguageType.Russian:
                    TitleText.Text = "ШАХМАТЫ";
                    PlayButton.Content = "ИГРАТЬ(2P)";
                    ComputerButton.Content = "С КОМПЬЮТЕРОМ";
                    InstructionButton.Content = "ПРАВИЛА";
                    LanguageButton.Content = "ЯЗЫК";
                    ExitButton.Content = "ВЫХОД";
                    BackFromLanguageButton.Content = "← НАЗАД";
                    ComputerTitleText.Text = "С КОМПЬЮТЕРОМ";
                    SideText.Text = "ВЫБЕРИТЕ СТОРОНУ";
                    DifficultyText.Text = "ВЫБЕРИТЕ СЛОЖНОСТЬ";
                    WhiteButton.Content = "БЕЛЫЕ";
                    BlackButton.Content = "ЧЁРНЫЕ";
                    BeginnerButton.Content = "НОВИЧОК";
                    EasyButton.Content = "ЛЁГКИЙ";
                    MediumButton.Content = "СРЕДНИЙ";
                    HardButton.Content = "СЛОЖНЫЙ";
                    MasterButton.Content = "МАСТЕР";
                    StartComputerButton.Content = "НАЧАТЬ ИГРУ";
                    BackFromComputerButton.Content = "← НАЗАД";
                    break;
            }
        }

        private void UpdateLanguageTitle()
        {
            LanguageTitleText.Text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English => "LANGUAGE",
                LanguageType.Chinese => "语言",
                LanguageType.Russian => "ЯЗЫК",
                _ => "LANGUAGE"
            };
        }

        private void UpdateInstructionText()
        {
            string text = LanguageManager.CurrentLanguage switch
            {
                LanguageType.English =>
@"Rules:
1. White moves first. Players take turns. The mouse cursor color indicates whose turn it is.
2. Checkmate the opponent to win.
3. Stalemate or insufficient material results in a draw.
4. Threefold repetition causes a draw.
5. Fifty-move rule: If 50 consecutive turns pass without pawn move or capture, it's a draw.

Enjoy the game!

Contact me :)
https://github.com/LCZ-ctrl
",
                LanguageType.Chinese =>
@"规则说明:
1. 白方先手，每轮玩家轮流移动棋子，鼠标的颜色决定轮到哪一方下棋
2. 将死对方者获胜
3. 僵局或者棋力不足将导致平局
4. 如果有三次重复局面出现，将导致平局
5. 如果连续50回合，没有兵被移动，也没有吃子将触发50步规则导致平局

玩的开心！

联系我 :)
https://github.com/LCZ-ctrl
",
                LanguageType.Russian =>
@"Правила:
1. Белые ходят первыми. Игроки ходят по очереди. Цвет курсора мыши показывает, чей ход.
2. Побеждает тот, кто ставит мат противнику.
3. Пат или недостаточно фигур приводит к ничьей.
4. При трёхкратном повторении позиции объявляется ничья.
5. Правило 50-ходов: если 50 ходов подряд нет движения пешки и взятия фигур, объявляется ничья.

Удачной игры!

Свяжитесь со мной :)
https://github.com/LCZ-ctrl
",
                _ => ""
            };
            InstructionText.Text = text;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}