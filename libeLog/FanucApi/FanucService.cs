using libeLog.Extensions;
using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace libeLog.FanucApi
{
    public class FanucService
{
    public int GetMaxRpm(ushort handle)
    {
        var odspn = new Focas1.ODBSPN();
        return Focas1.cnc_rdspmaxrpm(handle, 1, odspn) == Focas1.EW_OK ? odspn.data[0] : 0;
    }

    public (int speed, int feed) GetActRpmAndFeedrate(ushort handle)
    {
        var odbspeed = new Focas1.ODBSPEED();
        var _ret = Focas1.cnc_rdspeed(handle, -1, odbspeed);
        if (_ret != Focas1.EW_OK) MessageBox.Show(_ret.ToString(), "cnc_rdspeed");
        return (odbspeed.acts.data, odbspeed.actf.data);
    }

    public bool GetOpSignal(ushort handle)
    {
        return handle != 0 && GetPmcSignal(handle, 1, 0, 0, 0, 9).cdata[0].GetBit(7);
    }

    private Focas1.IODBPMC0 GetPmcSignal(ushort handle, short addrKind, short dataType, ushort start, ushort end, ushort dataLength)
    {
        var pmc = new Focas1.IODBPMC0();
        Focas1.pmc_rdpmcrng(handle, addrKind, dataType, start, end, dataLength, pmc);
        return pmc;
    }

    public List<char> GetAllAxisNames(ushort handle, short axisCount = 5)
    {
        List<char> result = new();
        var axn = new Focas1.ODBAXISNAME();
        for (short i = 1; i <= axisCount; i++)
        {
            short a = i;
            if (Focas1.cnc_rdaxisname(handle, ref a, axn) == Focas1.EW_OK)
            {
                result.Add(axn.GetName(i));
            }
        }
        return result;
    }

    public List<double> GetAxisPositions(ushort handle, int axisCount, AxisPositionType positionType)
    {
        var result = new List<double>();
        var scale = 1000.0;

        for (short i = 1; i <= axisCount; i++)
        {
            var axisData = new Focas1.ODBAXIS();
            var _ret = positionType switch
            {
                AxisPositionType.Relative => Focas1.cnc_relative(handle, i, 8, axisData),
                AxisPositionType.Absolute => Focas1.cnc_absolute(handle, i, 8, axisData),
                AxisPositionType.Machine => Focas1.cnc_machine(handle, i, 8, axisData),
                AxisPositionType.DistanceToGo => Focas1.cnc_distance(handle, i, 8, axisData),
                _ => 0
            };

            if (_ret == Focas1.EW_OK)
            {
                result.Add(axisData.data[0] / scale);
            }
        }
        return result;
    }

    public string GetMode(ushort handle)
    {
        return handle == 0 ? string.Empty : ModeNumberToString(GetStatusData(handle).aut);
    }

    public string GetStatus(ushort handle)
    {
        return handle == 0 ? string.Empty : StatusNumberToString(GetStatusData(handle).run);
    }

    private Focas1.ODBST GetStatusData(ushort handle)
    {
        var status = new Focas1.ODBST();
        Focas1.cnc_statinfo(handle, status);
        return status;
    }

    private string ModeNumberToString(int num) => num switch
    {
        0 => "MDI",
        1 => "Memory",
        3 => "Edit",
        4 => "Handle",
        5 => "JOG",
        6 => "Teach in JOG",
        7 => "Teach in HND",
        8 => "INC",
        9 => "REF",
        10 => "RMT",
        _ => "UNAVAILABLE"
    };

    private string StatusNumberToString(int num) => num switch
    {
        0 => "****",
        1 => "STOP",
        2 => "HOLD",
        3 => "STRT",
        4 => "MSTR",
        _ => "UNAVAILABLE"
    };
}

}
