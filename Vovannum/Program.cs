using System;
using System.Reflection;
using System.Threading;
using static Focas1;

namespace Vovannum
{
    class Program
    {
        static ushort _handle = 0;
        static short _ret = 0;

        static bool _exit = false;

        static void Main(string[] args)
        {

            Thread t = new Thread(new ThreadStart(ExitCheck));
            t.Start();

            _ret = Focas1.cnc_allclibhndl3("127.0.0.1", 8193, 6, out _handle);

            if (_ret != Focas1.EW_OK)
            {
                Console.WriteLine($"Unable to connect to 127.0.0.1 on port 8193\n\nReturn Code: {_ret}\n\nExiting....");
                Console.Read();
            }
            else
            {
                Console.WriteLine($"Focas handle is {_handle}");

                while (!_exit)
                {
                    Console.Clear();
                    var odbspeed = new ODBSPEED();
                    var bbb = cnc_rdspeed(_handle, -1, odbspeed);
                    Console.Write($"{(GetOpSignal() ? "Работает" : "Стоит")} в режиме {GetMode()} ({GetStatus()}) " +
                        $"| Обороты: {odbspeed.acts.data} | Подача: {odbspeed.actf.data} мм/мин ({(double)odbspeed.actf.data/(double)odbspeed.acts.data:0.000} мм/об)   ");

                    //for (short i = short.MinValue; i < short.MaxValue; i++)
                    //{
                    //    Console.Clear();
                    //    Console.WriteLine(i);

                    //    if (sops.blck_del != 0)
                    //    {

                    //        Console.WriteLine("GGGGGGGGGGGG");
                    //    }
                    //}

                    var sops = new IODBSGNL();
                    var aaa = cnc_rdopnlsgnl(_handle, -1, sops);
                    PrintMembers(sops);
                    Thread.Sleep(5000);
                }


            }
        }

        private static void ExitCheck()
        {
            while (Console.ReadLine() != "exit")
            {
                continue;
            }

            _exit = true;
        }


        public static bool GetOpSignal()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return false;
            }

            short addr_kind = 1; // F
            short data_type = 0; // Byte
            ushort start = 0;
            ushort end = 0;
            ushort data_length = 9; // 8 + N
            Focas1.IODBPMC0 pmc = new Focas1.IODBPMC0();

            _ret = Focas1.pmc_rdpmcrng(_handle, addr_kind, data_type, start, end, data_length, pmc);

            if (_ret != Focas1.EW_OK)
            {
                Console.WriteLine($"Error: Unable to ontain the OP Signal");
                return false;
            }

            return pmc.cdata[0].GetBit(7);

        }

        public static string GetMode()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return "";
            }

            Focas1.ODBST Mode = new Focas1.ODBST();

            _ret = Focas1.cnc_statinfo(_handle, Mode);

            if (_ret != 0)
            {
                Console.WriteLine($"Error: Unable to obtain mode.\nReturn Code: {_ret}");
                return "";
            }

            return ModeNumberToString(Mode.aut);
        }

        public static string ModeNumberToString(int num)
        {
            switch (num)
            {
                case 0: { return "MDI"; }
                case 1: { return "MEM"; }
                case 3: { return "EDIT"; }
                case 4: { return "HND"; }
                case 5: { return "JOG"; }
                case 6: { return "Teach in JOG"; }
                case 7: { return "Teach in HND"; }
                case 8: { return "INC"; }
                case 9: { return "REF"; }
                case 10: { return "RMT"; }
                default: { return "UNAVAILABLE"; }
            }
        }

        public static string GetStatus()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return "";
            }

            Focas1.ODBST Status = new Focas1.ODBST();

            _ret = Focas1.cnc_statinfo(_handle, Status);

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
        public static void PrintMembers(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("null");
                return;
            }

            Type type = obj.GetType();

            Console.WriteLine($"{type.Name}:");

            // Вывод свойств
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj, null);
                Console.WriteLine($"Property {property.Name}: {value}");
            }

            // Вывод полей
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(obj);
                Console.WriteLine($"Field {field.Name}: {value}");
            }
        }
    }
}
