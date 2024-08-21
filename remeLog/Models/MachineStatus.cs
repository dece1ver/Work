using libeLog.Base;

namespace remeLog.Models
{
    public class MachineStatus : ViewModel
    {
        private string _mode;
        private string _status;
        private bool _isOperating;
        private int _speed;
        private double _feed;
        private double _feedPerRevolution;

        // Добавляем новые свойства для координат по осям
        private double _x;
        private double _y;
        private double _z;

        public string Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsOperating
        {
            get => _isOperating;
            set
            {
                _isOperating = value;
                OnPropertyChanged(nameof(IsOperating));
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }

        public double Feed
        {
            get => _feed;
            set
            {
                _feed = value;
                OnPropertyChanged(nameof(Feed));
            }
        }

        public double FeedPerRevolution
        {
            get => _feedPerRevolution;
            set
            {
                _feedPerRevolution = value;
                OnPropertyChanged(nameof(FeedPerRevolution));
            }
        }

        public double X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        public double Z
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged(nameof(Z));
            }
        }
    }
}
