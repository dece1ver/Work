using libeLog;
using libeLog.Base;
using libeLog.FanucApi;
using libeLog.Infrastructure;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
        private int _axisCount = 0;
        private double _scale = 1000.0;
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
                _axisCount = axisNames.Count(ax => !string.IsNullOrEmpty(ax));
                if (axisNames.Count >= 5)
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
                    UpdateAxisValues();
                    (Status.Speed, Status.Feed) = _fanucService.GetActRpmAndFeedrate(_handle);
                    Status.IsOperating = _fanucService.GetOpSignal(_handle);
                    Status.Mode = _fanucService.GetMode(_handle);
                    Status.Status = _fanucService.GetStatus(_handle);
                    (Status.UsedProgramms, Status.UnusedProgramms, Status.UsedMem, Status.UnusedMem) = _fanucService.GetProgramDataInfo(_handle);                   
                    Status.Alarms = _fanucService.GetAlarms(_handle);

                    OnPropertyChanged(nameof(Status));

                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Monitoring Error");
                }
            }
        }

        private void UpdateAxisValues()
        {           
            short num = Focas1.MAX_AXIS;
            var odb = new Focas1.ODBPOS();

            short ret = Focas1.cnc_rdposition(_handle, Focas1.ALL_AXES, ref num, odb);
            if (ret == Focas1.EW_OK)
            {
                for (int i = 1; i <= _axisCount; i++)
                {
                    switch (i)
                    {
                        case 1:
                            Status.FirstRelativeAxisValue = odb.p1.rel.data / _scale;
                            Status.FirstAbsoluteAxisValue = odb.p1.abs.data / _scale;
                            Status.FirstMachineAxisValue = odb.p1.mach.data / _scale;
                            Status.FirstDistanceToGoAxisValue = odb.p1.dist.data / _scale;
                            break;
                        case 2:
                            Status.SecondRelativeAxisValue = odb.p2.rel.data / _scale;
                            Status.SecondAbsoluteAxisValue = odb.p2.abs.data / _scale;
                            Status.SecondMachineAxisValue = odb.p2.mach.data / _scale;
                            Status.SecondDistanceToGoAxisValue = odb.p2.dist.data / _scale;
                            break;
                        case 3:
                            Status.ThirdRelativeAxisValue = odb.p3.rel.data / _scale;
                            Status.ThirdAbsoluteAxisValue = odb.p3.abs.data / _scale;
                            Status.ThirdMachineAxisValue = odb.p3.mach.data / _scale;
                            Status.ThirdDistanceToGoAxisValue = odb.p3.dist.data / _scale;
                            break;
                        case 4:
                            Status.FourthRelativeAxisValue = odb.p4.rel.data / _scale;
                            Status.FourthAbsoluteAxisValue = odb.p4.abs.data / _scale;
                            Status.FourthMachineAxisValue = odb.p4.mach.data / _scale;
                            Status.FourthDistanceToGoAxisValue = odb.p4.dist.data / _scale;
                            break;
                        case 5:
                            Status.FivethRelativeAxisValue = odb.p5.rel.data / _scale;
                            Status.FivethAbsoluteAxisValue = odb.p5.abs.data / _scale;
                            Status.FivethMachineAxisValue = odb.p5.mach.data / _scale;
                            Status.FivethDistanceToGoAxisValue = odb.p5.dist.data / _scale;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
