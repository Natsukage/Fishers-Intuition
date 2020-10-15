using System;
using System.ComponentModel;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 鱼眼、特殊天气等状态
    /// </summary>
    public class Status : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsActive;
        public DateTime StartTime { get; set; }
        public StatusType Type = StatusType.FishEyes;

        private float _duration = 1f;
        private string _weather = "";
        public string Color => Properties.Settings.Default.StatusColor;

        public void Start(float statusDuration, StatusType type = StatusType.FishEyes, string weatherName = "")
        {
            StartTime = DateTime.Now;
            Type = type;
            _duration = statusDuration;
            if (type == StatusType.Weather)
                _weather = weatherName;
            IsActive = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Duration"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
        }

        public void Start(SpecialWeather weather) //特殊天气触发
        {
            Start(weather.Duration, StatusType.Weather, weather.Name);
        }

        public void End()
        {
            IsActive = false;
            _duration = 0f;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
        }

        public void Update()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressBarValue"));
        }

        public string Text
        {
            get
            {
                var str = "";
                var buffRemain = StartTime + TimeSpan.FromSeconds(_duration) - DateTime.Now;
                if (buffRemain.TotalSeconds * 3 > _duration) //剩余时间足够长时添加buff类型或天气名前缀
                    str += Type == StatusType.FishEyes ? "鱼眼 : " : _weather + " : ";
                if (buffRemain.Minutes > 0) //剩余时间大于1分钟时显示剩余分钟部分。
                    str += buffRemain.Minutes + "m";
                str += buffRemain.ToString("ss") + "s";
                return str;
            }
        }

        public double ProgressBarValue =>
            IsActive ? (StartTime + TimeSpan.FromSeconds(_duration) - DateTime.Now).TotalSeconds : 0;

        public double Duration => _duration;

        public System.Windows.Visibility Visibility =>
            IsActive ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public enum StatusType
        {
            FishEyes, //鱼眼
            Weather //特殊天气
        }
    }
}