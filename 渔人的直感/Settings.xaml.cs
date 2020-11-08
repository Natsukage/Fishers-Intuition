using System.Windows;

namespace 渔人的直感
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            if (MessageBox.Show("设置已保存\n将在下次启动应用后生效\n是否现在重启应用？", "渔人的直感", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Application.Current.Shutdown();
            }
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentMainWindow.Left = -1;
            MainWindow.CurrentMainWindow.Top = -1;
            Properties.Settings.Default.Reset();
        }
    }
}
