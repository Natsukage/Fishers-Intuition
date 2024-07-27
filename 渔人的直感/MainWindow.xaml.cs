using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using 渔人的直感.Models;
using Debug = System.Diagnostics.Debug;

namespace 渔人的直感
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private Process GameProcess;
        private ProcessModule GameProcessMainModule;

        private BackgroundWorker Worker;
        private SigScanner Scanner;

        private readonly Fish Fish = new Fish();
        private readonly Status Status = new Status();

        public static MainWindow CurrentMainWindow;
        private Window _settingsWindow;

        private byte LastOceanFishingZone;
        private bool LastZoneHasSpectralCurrent;
        private bool CurrentZoneHadSpectralCurrent;
        private float CompensatedTime;

        public MainWindow()
        {
            InitializeComponent();
            if (!Initialize())
                Application.Current.Shutdown();
            LoadConfig();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        private bool Initialize()
        {
            var procs = Process.GetProcessesByName("ffxiv_dx11");

            switch (procs.Length)
            {
                case 0:
                    MessageBox.Show("没有找到FFXIV(DX11)的进程!", "上钩的FF14逃走了…");
                    return false;
                default:
                    MessageBox.Show($"发现{procs.Length}个FFXIV(DX11)的进程！", "这里的FF14现在警惕性很高…");
                    return false;
                case 1:
                    GameProcess = procs[0];
                    if (GameProcess == null)
                    {
                        MessageBox.Show("无法取得FFXIV的进程！", "渔人的直感", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    break;
            }

            GameProcessMainModule = GameProcess.MainModule;

            BiteProgressBar.DataContext = Fish;
            StatusProgressBar.DataContext = Status;

            GameProcess.EnableRaisingEvents = true;
            GameProcess.Exited += (_, e) =>
            {
                Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
            };

            Scanner = new SigScanner(GameProcess, GameProcessMainModule);
            Data.Initialize(Scanner);
            Worker = new BackgroundWorker();
            Worker.DoWork += OnWork;
            Worker.WorkerSupportsCancellation = true;
            Worker.RunWorkerAsync();

            CurrentMainWindow = this;
            Closing += SaveLocation;
            return true;
        }
        /// <summary>
        ///     读取配置文件并设置窗体尺寸等
        /// </summary>
        private void LoadConfig()
        {
            BiteProgressBar.Height = Properties.Settings.Default.Height;
            StatusProgressBar.Height = Properties.Settings.Default.Height;
            Height = Properties.Settings.Default.Height * 2 + 20;
            Width = Properties.Settings.Default.Width;

            if (Properties.Settings.Default.Location.X <= 0 ||
                Properties.Settings.Default.Location.X >= SystemParameters.FullPrimaryScreenWidth ||
                Properties.Settings.Default.Location.Y <= 0 ||
                Properties.Settings.Default.Location.Y >= SystemParameters.FullPrimaryScreenHeight)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Left = Properties.Settings.Default.Location.X;
                Top = Properties.Settings.Default.Location.Y;
                WindowStartupLocation = WindowStartupLocation.Manual;
            }
        }
        /// <summary>
        ///     设置窗体的隐藏与鼠标穿透属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStyleHelper.ExStyle |= 0x00000080; //ExtendedWindowStyles.WS_EX_TOOLWINDOW = 0x00000080
            if (Properties.Settings.Default.ClickThrough)
                WindowStyleHelper.ExStyle |= 0x00000020; //ExtendedWindowStyles.WS_EX_TRANSPARENT  = 0x00000020
        }

        /// <summary>
        ///     循环检测玩家状态.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWork(object sender, DoWorkEventArgs e)
        {
            var status = Scanner.ReadInt16(Data.StatusPtr);

            while (true)
            {
                if (Worker.CancellationPending)
                    e.Cancel = true;
                try
                {
                    var nStatus = Scanner.ReadInt16(Data.StatusPtr);
                    if (nStatus != status)
                    {
                        StatusChange(nStatus);
                        status = nStatus;
                    }

                    var localplayer = Scanner.ReadIntPtr(Data.ActorTable);
                    if (localplayer == IntPtr.Zero || !Data.IsGathering)
                    {
                        Reset();
                        Status.End();
                        Fish.Reset();
                        //挂机会导致cpu飙高
                        Thread.Sleep(50);
                        continue;
                    }

                    var buffTablePtr = localplayer + Data.UiStatusEffects;
                    BuffCheck(buffTablePtr);
                    OceanFishingZoneCheck();
                    WeatherCheck(Data.WeatherPtr);

                    if (Fish.State == FishingState.Casting) Fish.Update();
                    if (Status.IsActive) Status.Update();
                }
                catch (Exception)
                {
                    // ignored
                }

                Thread.Sleep(50);
            }
        }
        private void StatusChange(short newStatus)
        {
            switch (newStatus)
            {
                case 0x112: //抛竿动作1（常规鱼饵）
                case 0x113: //抛竿动作2（部分拟饵以小钓大等）
                case 0x114: //抛竿动作3（摇蚊等特殊鱼饵等）
                case 0xC49: //抛竿动作1（坐下时）
                case 0xC4A: //抛竿动作2（坐下时）
                case 0xC4B: //抛竿动作3（坐下时）
                    Fish.Cast();
                    break;
                case 0x124: //咬钩（轻杆）
                    Fish.Bite(TugType.Light);
                    break;
                case 0x125: //咬钩（中杆）
                    Fish.Bite(TugType.Medium);
                    break;
                case 0x126: //咬钩（鱼王杆）
                    Fish.Bite(TugType.Heavy);
                    break;
                case 0x11B: //脱钩
                case 0xC52: //脱钩（坐下时）
                    Fish.Bite(TugType.None);
                    break;
                case 0x111: //停止垂钓
                case 0xC48: //停止垂钓（坐下时）
                    Fish.Reset();
                    break;
            }

            // 原代码 虽然行数整齐一些但是太不直观了
            /*
            switch (newStatus)
            {
                case short n when new short[] { 0x112, 0x113, 0x114, 0xC49, 0xC4A, 0xC4B }.Any(x => n == x): 
                    Fish.Cast();
                    break;
                case short n when new short[] { 0x124, 0x125, 0x126 }.Any(x => n == x):
                    var tug = (TugType) (n - 0x123);
                    //0x124:TugType.Light 0x125:TugType.Medium 0x126:TugType.Heavy
                    Fish.Bite(tug);
                    break;
                case short n when new short[] { 0x11B, 0xC52 }.Any(x => n == x):
                    Fish.Bite(TugType.None);
                    break;
                case short n when new short[] { 0x111, 0xC48 }.Any(x => n == x):
                    Fish.Reset();
                    break;
                default:
                    break;
            }*/
        }
        private void BuffCheck(IntPtr buffTablePtr)
        {
            if (!Status.IsActive)
            {
                for (var i = 0; i < 30; i++)
                    if (Scanner.ReadInt16(buffTablePtr + i * 12) == Data.FishEyesBuffId)
                    {
                        Status.Start(Scanner.ReadFloat(buffTablePtr + i * 12 + 4));
                        /*Buff buff = new Buff
                        {
                            ID = Scanner.ReadInt16(BuffTablePtr + i * 12),
                            Stacks = Scanner.ReadInt16(BuffTablePtr + i * 12 + 2),
                            Duration = Scanner.ReadFloat(BuffTablePtr + i * 12 + 4),
                            Owner = Scanner.ReadInt32(BuffTablePtr + i * 12 + 8)
                        };*/
                        break;
                    }

                return;
            }

            if (Status.Type != Status.StatusType.FishEyes) return;
            
            var fishEyeIsActive = false;
            for (var i = 0; i < 30; i++)
            {
                if (Scanner.ReadInt16(buffTablePtr + i * 12) != Data.FishEyesBuffId)
                    continue;
                fishEyeIsActive = true;
                break;
            }

            if (!fishEyeIsActive)
                Status.End();
            
        }

        private void WeatherCheck(IntPtr weatherPtr)
        {
            var currentWeather = Scanner.ReadByte(weatherPtr);
            if (!Status.IsActive)
            {
                foreach (var weather in Data.SpecialWeathers)
                {
                    if (weather.Id != currentWeather) continue;

                    var duration = weather.Duration;
                    // 幻海流
                    if (weather.Id == 145)
                    {
                        CurrentZoneHadSpectralCurrent = true;

                        //获取当前海域剩余时间
                        var remainingTime = Data.OceanFishingRemainingTime;
                        var currentZone = Data.OceanFishingCurrentZone;
                        Debug.WriteLine($"Duration: {duration}, CompensatedTime: {CompensatedTime}, {remainingTime}");
                        
                        // 如果上个海域没幻海,而且不是刚进,加60秒
                        if (currentZone != 0)
                        {
                            if (!LastZoneHasSpectralCurrent)
                            {
                                duration += 60;
                            }
                            else
                            {
                                duration += CompensatedTime;
                            }

                            CompensatedTime = 0;
                        }

                        // 最多只有180秒
                        if (duration > 180)
                            duration = 180;

                        //如果这轮幻海吃不满
                        if (remainingTime - duration < 30)
                        {
                            //下一轮就要补这么多时间
                            CompensatedTime = Math.Abs(remainingTime - duration - 30);
                            Debug.WriteLine($"No full uptime. CompensatedTime: {CompensatedTime} / remainingTime: {remainingTime} / duration:{duration}");
                            if (CompensatedTime < 0)
                                CompensatedTime = 0;
                            else if (CompensatedTime > 60)
                                CompensatedTime = 60;
                            duration = remainingTime - 30;
                        }
                    }

                    Status.Start(weather, duration);
                    break;
                }
            }
            else if (Status.Type == Status.StatusType.Weather)
            {
                if (Data.SpecialWeathers.All(x => currentWeather != x.Id))
                    Status.End();
            }
        }

        private void OceanFishingZoneCheck()
        {
            // 检查是否在海钓里
            var territory = Data.TerritoryType;
            // 为什么换海域的时候territoryId会变成0啊啊啊啊啊
            if (territory != 0 && !Data.IsInOceanFishing)
            {
                Reset();
                // Console.WriteLine($"[OceanFishingZoneCheck] Reset. {territory}");
                return;
            }

            try
            {
                var currentZone = Data.OceanFishingCurrentZone;
                if (LastOceanFishingZone == currentZone)
                    return;

                //换海域了, 刚进来的时候海域是0,所以直接设成false
                Debug.WriteLine($"[{DateTime.Now}] Moving to next zone / {LastOceanFishingZone:X}:{currentZone:X} / {Data.OceanFishingRemainingTime}");
                LastZoneHasSpectralCurrent = currentZone != 0 && CurrentZoneHadSpectralCurrent;
                CurrentZoneHadSpectralCurrent = false;
                LastOceanFishingZone = currentZone;
            }
            catch
            {
                // 结算的时候是invalid的
                // 应该没必要,反正检查territory id了
                // Reset();
            }
        }

        private void WindowDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        //由于Win10的bug，全屏程序在托盘打开的ContextMenu可能不会正确失去焦点。这里令左键点击托盘图标时强制关闭其弹出的右键菜单。
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e) => CurrentMainWindow.TrayIcon.ContextMenu.IsOpen = false;

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new Settings();
                _settingsWindow.Closed += (_, e2) => { _settingsWindow = null; };
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.WindowState = WindowState.Normal;
                _settingsWindow.Activate();
            }
        }

        private void FishTracker_Click(object sender, RoutedEventArgs e) => Process.Start("http://fish.senriakane.com/");

        private void FishCake_Click(object sender, RoutedEventArgs e) => Process.Start("https://ricecake404.gitee.io/ff14-list");

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void SaveLocation(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                Properties.Settings.Default.Location = new System.Drawing.Point((int) Left, (int) Top);
            Properties.Settings.Default.Save();
        }

        private void Reset()
        {
            LastZoneHasSpectralCurrent = false;
            CurrentZoneHadSpectralCurrent = false;
            CompensatedTime = 0f;
            LastOceanFishingZone = byte.MaxValue;
        }
    }
}