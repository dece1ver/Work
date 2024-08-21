using libeLog;
using libeLog.Base;
using libeLog.FanucApi;
using libeLog.Infrastructure;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace remeLog.ViewModels
{
    public class FanucMonitorViewModel : ViewModel
    {
        private readonly FanucService _fanucService;
        private ushort _handle = 0;
        private short _ret = 0;
        private bool _isConnected = false;
        private string _ipAddress;
        private MachineStatus _status;
        private List<char> _axisNames = new();
        private int _axisCount = 0;

        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IPAddress));
            }
        }

        public MachineStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        
        public ICommand ConnectCommand { get; }

        public FanucMonitorViewModel()
        {
            _fanucService = new FanucService();
            _ipAddress = "127.0.0.1";
            _status = new MachineStatus();
            ConnectCommand = new LambdaCommand(OnConnectCommandExecuted, CanConnectCommandExecute);
        }

        private void OnConnectCommandExecuted(object p)
        {
            ConnectToMachine();
        }

        private static bool CanConnectCommandExecute(object p) => true;

        private void ConnectToMachine()
        {
            Task.Run(() =>
            {
                _ret = Focas1.cnc_allclibhndl3(IPAddress, 8193, 6, out _handle);
                if (_ret == Focas1.EW_OK)
                {
                    IsConnected = true;
                    MonitorMachine();
                }
                else
                {
                    IsConnected = false;
                    MessageBox.Show($"Unable to connect. Return Code: {_ret}");
                }
            });
        }

        private void MonitorMachine()
        {
            try
            {
                Status.MaxRpm = _fanucService.GetMaxRpm(_handle);
                var axisNames = _fanucService.GetAllAxisNames(_handle);
                _axisCount = axisNames.Count(ax => ax != char.MinValue);
                if (axisNames.Count == 5)
                {
                    Status.FirstAxisName = axisNames[0].ToString();
                    Status.SecondAxisName = axisNames[1].ToString();
                    Status.ThirdAxisName = axisNames[2].ToString();
                    Status.FourthAxisName = axisNames[3].ToString();
                    Status.FivethAxisName = axisNames[4].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            while (IsConnected)
            {
                try
                {
                    UpdateAxisValues(_axisCount);
                    (Status.Speed, Status.Feed) = _fanucService.GetActRpmAndFeedrate(_handle);
                    Status.IsOperating = _fanucService.GetOpSignal(_handle);
                    Status.Mode = _fanucService.GetMode(_handle);
                    Status.Status = _fanucService.GetStatus(_handle);

                    OnPropertyChanged(nameof(Status));

                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Monitoring Error");
                }
            }
        }

        private void UpdateAxisValues(int count)
        {
            var axisRelativeValues = _fanucService.GetAxisPositions(_handle, count, AxisPositionType.Relative);
            var axisAbsoluteValues = _fanucService.GetAxisPositions(_handle, count, AxisPositionType.Absolute);
            var axisMachineValues = _fanucService.GetAxisPositions(_handle, count, AxisPositionType.Machine);
            var axisDistanceToGoValues = _fanucService.GetAxisPositions(_handle, count, AxisPositionType.DistanceToGo);
            Status.SetAxisValues(AxisPositionType.Relative, axisRelativeValues.Take(count).ToList());
            Status.SetAxisValues(AxisPositionType.Absolute, axisAbsoluteValues.Take(count).ToList());
            Status.SetAxisValues(AxisPositionType.Machine, axisMachineValues.Take(count).ToList());
            Status.SetAxisValues(AxisPositionType.DistanceToGo, axisDistanceToGoValues.Take(count).ToList());
        }
    }
}
