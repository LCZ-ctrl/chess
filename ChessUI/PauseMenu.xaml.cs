using System;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    public partial class PauseMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public PauseMenu()
        {
            InitializeComponent();

            this.Loaded += (s, e) => LanguageManager.LanguageChanged += UpdateLanguage;
            this.Unloaded += (s, e) => LanguageManager.LanguageChanged -= UpdateLanguage;

            UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            switch (LanguageManager.CurrentLanguage)
            {
                case LanguageType.English:
                    PauseTitle.Text = "Pause";
                    ContinueText.Text = "CONTINUE";
                    RestartText.Text = "RESTART";
                    MenuText.Text = "MENU";
                    break;
                case LanguageType.Chinese:
                    PauseTitle.Text = "暂停";
                    ContinueText.Text = "继续";
                    RestartText.Text = "重新开始";
                    MenuText.Text = "菜单";
                    break;
                case LanguageType.Russian:
                    PauseTitle.Text = "Пауза";
                    ContinueText.Text = "ПРОДОЛЖИТЬ";
                    RestartText.Text = "ПЕРЕЗАПУСК";
                    MenuText.Text = "МЕНЮ";
                    break;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Continue);
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var wnd = Window.GetWindow(this) as MainWindow;
            if (wnd != null)
            {
                if (wnd.MenuContainer != null)
                {
                    wnd.MenuContainer.Content = null;
                }

                WindowManager.Show(WindowManager.MainMenu, wnd, durationMs: 0);
            }
            else
            {
                WindowManager.Show(WindowManager.MainMenu, null, durationMs: 0);
            }
        }
    }
}
