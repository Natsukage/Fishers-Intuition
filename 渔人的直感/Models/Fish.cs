using System;
using System.ComponentModel;
using System.IO;
using System.Media;

namespace 渔人的直感.Models
{
	/// <summary>
	/// 抛竿咬钩等钓鱼相关状态
	/// </summary>
    public class Fish : INotifyPropertyChanged
    {
		public event PropertyChangedEventHandler PropertyChanged;
		public FishingState State { get; set; }
		private DateTime StartTime { get; set; }
        public string Color { get; set; }

        private TugType _tug;
		/// <summary>
		/// 抛竿动作（包括以小钓大）
		/// </summary>
		public void Cast()
        {
            StartTime = DateTime.Now;
			State = FishingState.Casting;
            Color = Properties.Settings.Default.TimerColor;
            _tug = TugType.None;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
		}
		/// <summary>
		/// 抛竿动作（包括以小钓大）
		/// </summary>
        internal void Bite(TugType tug)
        {
            State = FishingState.Holding;
			_tug = tug;
            if (tug == TugType.None)
                return;
            var soundPlayer = new SoundPlayer();
			switch (tug)
			{
				case TugType.Light:
					Color = Properties.Settings.Default.LTugColor;
					soundPlayer.SoundLocation = "轻杆.wav";
					break;
				case TugType.Medium:
					Color = Properties.Settings.Default.MTugColor;
					soundPlayer.SoundLocation = "中杆.wav";
					break;
				case TugType.Heavy:
					Color = Properties.Settings.Default.HTugColor;
					soundPlayer.SoundLocation = "鱼王杆.wav";
					break;
                case TugType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tug), tug, null);
            }
            //提杆或放杆后，界面保持显示最后状态。因此最后一次更新显示。
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
			Update();

			if (File.Exists(soundPlayer.SoundLocation))
			{
				//m_mediaPlayer.Play();
				soundPlayer.LoadAsync();
				soundPlayer.Play();
			}
		}
		public void Reset()
		{
			//起身后，界面重置。
			State = FishingState.None;
			Update();
		}

		public void Update()
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("Text"));
				PropertyChanged(this, new PropertyChangedEventArgs("ProgressBarValue"));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
			}
		}
		/// <summary>
		/// 抛竿计时条上显示的文本
		/// </summary>
		public string Text
		{
			get
			{
                if (State == FishingState.None)
                    return "";

                var totalSeconds = StartTime == default ? 0 : (DateTime.Now - StartTime).TotalSeconds;

				var text = $"{totalSeconds:F1}s";
                if (totalSeconds > 5.0 || totalSeconds == 0)
                {
                    switch (_tug)
                    {
                        case TugType.Light:
                            text = "轻杆!" + text;
                            break;
                        case TugType.Medium:
                            text = "中杆!!" + text;
                            break;
                        case TugType.Heavy:
                            text = "鱼王杆!!!" + text;
                            break;
                    }
                }
				return text;
			}
		}
		/// <summary>
		/// 抛竿计时条的进度（0-70+）
		/// </summary>
		public double ProgressBarValue
		{
			get
			{
				if (State == FishingState.None)
					return 0.0;

				var totalSeconds = (DateTime.Now - StartTime).TotalSeconds;

				
				if (totalSeconds < 10.0)
					return totalSeconds * 3.0; //10秒内的计时条增速3倍，提高幻海流中不同鱼类的区分度。

                return 30.0 + (totalSeconds - 10.0); //10秒后计时条增长速度恢复原速（计时条最大值为70，即50s）
			}
		}
        public System.Windows.Visibility Visibility =>
            Properties.Settings.Default.HideWhenNotActived && State == FishingState.None
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
    }

	internal enum TugType : byte { None, Light, Medium, Heavy }
	public enum FishingState
	{
		None, //未在垂钓，未抛竿或起身后，进度条不显示
		Casting, //抛竿后等待咬钩，进度条随时间推进
		Holding  //鱼上钩、提钩、放钩后，进度条冻结
	}

}
