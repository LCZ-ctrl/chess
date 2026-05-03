using ChessLogic;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI
{
    public partial class PromotionMenu : UserControl
    {
        public event Action<PieceType> PieceSelected;
        private readonly Player currentPlayer;

        public PromotionMenu(Player player)
        {
            InitializeComponent();

            currentPlayer = player;

            var img = Images.GetImage(player, PieceType.Queen);
            if (img != null) QueenImg.Source = img;
            img = Images.GetImage(player, PieceType.Rook);
            if (img != null) RookImg.Source = img;
            img = Images.GetImage(player, PieceType.Bishop);
            if (img != null) BishopImg.Source = img;
            img = Images.GetImage(player, PieceType.Knight);
            if (img != null) KnightImg.Source = img;

            UpdateLanguage();

            this.Loaded += (s, e) => LanguageManager.LanguageChanged += UpdateLanguage;
            this.Unloaded += (s, e) => LanguageManager.LanguageChanged -= UpdateLanguage;
        }

        private void UpdateLanguage()
        {
            switch (LanguageManager.CurrentLanguage)
            {
                case LanguageType.English:
                    TitleText.Text = "SELECT A PIECE";
                    break;
                case LanguageType.Chinese:
                    TitleText.Text = "选择棋子";
                    break;
                case LanguageType.Russian:
                    TitleText.Text = "ВЫБЕРИТЕ ФИГУРУ";
                    break;
            }
        }

        private void QueenImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen);
        }

        private void RookImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook);
        }

        private void BishopImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop);
        }

        private void KnightImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight);
        }
    }
}
