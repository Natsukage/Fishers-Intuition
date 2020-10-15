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
            if (!long.TryParse(Properties.Settings.Default.Offset ,System.Globalization.NumberStyles.AllowHexSpecifier, null, out _))
            {
                MessageBox.Show("偏移坐标格式不合法！\n应为相对偏移的十六进制数值", "渔人的直感", MessageBoxButton.OK, MessageBoxImage.Error);
                LockOffset.IsChecked = true;
                TextOffset.SelectAll();
                TextOffset.Focus();
                return;
            }

            Properties.Settings.Default.Offset = Properties.Settings.Default.Offset.ToUpper();
            Properties.Settings.Default.Save();
            if (MessageBox.Show("设置已保存\n将在下次启动应用后生效\n是否现在重启应用？", "渔人的直感", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Application.Current.Shutdown();
            }
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
        }
    }
}
