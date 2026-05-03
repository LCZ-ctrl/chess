using System.Windows;

namespace ChessUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            WindowManager.Init();

            WindowManager.Show(WindowManager.MainMenu);
        }
    }
}