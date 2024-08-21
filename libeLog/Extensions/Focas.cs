namespace libeLog.Extensions
{
    public static class Focas
    {
        public static char GetName(this Focas1.ODBAXISNAME axisName, int index)
        {
            switch (index)
            {
                case 1:
                    return (char)axisName.data1.name;
                case 2:
                    return (char)axisName.data2.name;
                case 3:
                    return (char)axisName.data3.name;
                case 4:
                    return (char)axisName.data4.name;
                case 5:
                    return (char)axisName.data5.name;
                default:
                    return char.MinValue;

            }
        }
    }
}
