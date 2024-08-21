using DocumentFormat.OpenXml.VariantTypes;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using remeLog.Models;
using remeLog.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace remeLog.ViewModels
{
    class FanucMonitorViewModel : ViewModel
    {
        private ushort _handle = 0;
        private short _ret = 0;
        private bool _isConnected = false;

        private string _ipAddress;
        private MachineStatus _status;

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
        public FanucMonitorViewModel()
        {
            Status = new MachineStatus();
            ConnectCommand = new LambdaCommand(OnConnectCommandExecuted, CanConnectCommandExecute);
        }

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

        #region Connect
        public ICommand ConnectCommand { get; }
        private void OnConnectCommandExecuted(object p)
        {
            ConnectToMachine();
        }

        private static bool CanConnectCommandExecute(object p) => true;
        #endregion

        private void MonitorMachine()
        {
            while (IsConnected)
            {
                var odbspeed = new Focas1.ODBSPEED();
                var _ = Focas1.cnc_rdspeed(_handle, -1, odbspeed);
                Status.Speed = odbspeed.acts.data;
                Status.Feed = odbspeed.actf.data;
                Status.FeedPerRevolution = odbspeed.acts.data != 0 ? (double)odbspeed.actf.data / odbspeed.acts.data : 0;
                Status.IsOperating = GetOpSignal(_handle);
                Status.Mode = GetMode(_handle);
                Status.Status = GetStatus(_handle);

                (Status.X, Status.Y, Status.Z) = GetAllAxisAbsolutePositions(_handle);

                OnPropertyChanged(nameof(Status));
                Thread.Sleep(250);
            }
        }

        public static bool GetOpSignal(ushort _handle)
        {
            if (_handle == 0)
            {
                MessageBox.Show("Error: Please obtain a handle before calling this method");
                return false;
            }

            short addr_kind = 1; // F
            short data_type = 0; // Byte
            ushort start = 0;
            ushort end = 0;
            ushort data_length = 9; // 8 + N
            Focas1.IODBPMC0 pmc = new Focas1.IODBPMC0();
            var _ret = Focas1.pmc_rdpmcrng(_handle, addr_kind, data_type, start, end, data_length, pmc);
            if (_ret != Focas1.EW_OK)
            {
                MessageBox.Show($"Error: Unable to ontain the OP Signal");
                return false;
            }
            return pmc.cdata[0].GetBit(7);
        }

        public static (double x, double y, double z) GetAllAxisAbsolutePositions(ushort _handle)
        {
            if (_handle == 0)
                return (0, 0, 0);
            var _scale = 1000;
            try
            {
                Focas1.ODBAXIS _axisPositionMachine = new Focas1.ODBAXIS();

                //for (short i = 4; i < short.MaxValue; i++)
                //{
                //    var _ret = Focas1.cnc_absolute(_handle, -1, i, _axisPositionMachine);
                //    Debug.Print($"i = {i} | ret = {_ret}");
                //    if (_ret == Focas1.EW_OK) MessageBox.Show($"i = {i} | ret = {_ret}");
                //    Thread.Sleep(500);
                //}
                short _ret = 0;
                _ret = Focas1.cnc_absolute(_handle, 1, 8, _axisPositionMachine);

                if (_ret != Focas1.EW_OK)
                    return (0, 0, 0);
                double x = _axisPositionMachine.data[0];

                _ret = Focas1.cnc_absolute(_handle, 2, 8, _axisPositionMachine);

                if (_ret != Focas1.EW_OK)
                    return (0, 0, 0);
                double y = _axisPositionMachine.data[0];

                _ret = Focas1.cnc_absolute(_handle, 3, 8, _axisPositionMachine);

                if (_ret != Focas1.EW_OK)
                    return (0, 0, 0);
                double z = _axisPositionMachine.data[0];

                return (x / _scale, y / _scale, z / _scale);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return (0, 0, 0);
        }

        public static string GetMode(ushort _handle)
        {
            if (_handle == 0)
            {
                MessageBox.Show("Error: Please obtain a handle before calling this method");
                return "";
            }

            Focas1.ODBST Mode = new Focas1.ODBST();

            var _ret = Focas1.cnc_statinfo(_handle, Mode);

            if (_ret != 0)
            {
                MessageBox.Show($"Error: Unable to obtain mode.\nReturn Code: {_ret}");
                return "";
            }

            return ModeNumberToString(Mode.aut);
        }

        public static string ModeNumberToString(int num)
        {
            switch (num)
            {
                case 0: { return "MDI"; }
                case 1: { return "Memory"; }
                case 3: { return "Edit"; }
                case 4: { return "Handle"; }
                case 5: { return "JOG"; }
                case 6: { return "Teach in JOG"; }
                case 7: { return "Teach in HND"; }
                case 8: { return "INC"; }
                case 9: { return "REF"; }
                case 10: { return "RMT"; }
                default: { return "UNAVAILABLE"; }
            }
        }

        public static string GetStatus(ushort _handle)
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return "";
            }

            Focas1.ODBST Status = new Focas1.ODBST();

            var _ret = Focas1.cnc_statinfo(_handle, Status);

            if (_ret != 0)
            {
                Console.WriteLine($"Error: Unable to obtain status.\nReturn Code: {_ret}");
                return "";
            }

            return StatusNumberToString(Status.run);
        }

        public static string StatusNumberToString(int num)
        {
            switch (num)
            {
                case 0: { return "****"; }
                case 1: { return "STOP"; }
                case 2: { return "HOLD"; }
                case 3: { return "STRT"; }
                case 4: { return "MSTR"; }
                default: { return "UNAVAILABLE"; }
            }
        }


    }
}
