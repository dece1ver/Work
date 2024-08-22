namespace libeLog.Extensions
{
    public static class Focas
    {
        public static (char, char) GetName(this Focas1.ODBAXISNAME axisName, int index)
        {
            switch (index)
            {
                case 1:
                    return ((char)axisName.data1.name, (char)axisName.data1.suff);
                case 2:
                    return ((char)axisName.data2.name, (char)axisName.data2.suff);
                case 3:
                    return ((char)axisName.data3.name, (char)axisName.data3.suff);
                case 4:
                    return ((char)axisName.data4.name, (char)axisName.data4.suff);
                case 5:
                    return ((char)axisName.data5.name, (char)axisName.data5.suff);
                case 6:
                    return ((char)axisName.data6.name, (char)axisName.data6.suff);
                case 7:
                    return ((char)axisName.data7.name, (char)axisName.data7.suff);
                case 8:
                    return ((char)axisName.data8.name, (char)axisName.data8.suff);
                default:
                    return (char.MinValue, char.MinValue);

            }
        }
    }
}
