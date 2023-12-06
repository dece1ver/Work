using libeLog.Extensions;
using Microsoft.Data.SqlClient;
using remeLog.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    public static class Database
    {
        public static ICollection<Part> ReadParts()
        {
            var parts = new List<Part>();
            throw new NotImplementedException();
        }

        public static ObservableCollection<Part> ReadPartsWithConditions(string conditions)
        {
            var parts = new ObservableCollection<Part>();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM Parts WHERE {conditions} ORDER BY StartSetupTime ASC;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    parts.FillParts(command);
                }
            }
            return parts;
        }

        public static ObservableCollection<Part> ReadPartByMachineAndShiftDate(string machine, DateTime shiftDate)
        {
            var parts = new ObservableCollection<Part>();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM Parts WHERE Machine = '{machine}' AND ShiftDate = '{shiftDate}' ORDER BY StartSetupTime ASC;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    parts.FillParts(command);
                }
            }

            return parts;
        }

        public static ObservableCollection<Part> ReadPartsByShiftDate(DateTime fromDate, DateTime toDate)
        {
            var parts = new ObservableCollection<Part>();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Parts WHERE ShiftDate BETWEEN @FromDate AND @ToDate ORDER BY StartSetupTime ASC;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);

                    parts.FillParts(command);
                }
            }

            return parts;
        }

        static void FillParts(this ICollection<Part> parts, SqlCommand command)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Part part = new Part(
                        reader.GetGuid(0),
                        reader.GetString(1),                        // станок
                        reader.GetDateTime(2),                      // дата смены
                        reader.GetString(3),                        // смена
                        reader.GetString(4),                        // оператор
                        reader.GetString(5),                        // деталь
                        reader.GetString(6),                        // м/л
                        reader.GetInt32(7),                         // установка
                        reader.GetInt32(8),                         // выполнено деталей
                        reader.GetInt32(9),                         // всего деталей
                        reader.GetDateTime(10),                     // начало наладки
                        reader.GetDateTime(11),                     // начало изготовления
                        reader.GetDouble(12),                       // фактическая наладка
                        reader.GetDateTime(13),                     // конец изготовления
                        reader.GetDouble(14),                       // норматив наладки
                        reader.GetDouble(15),                       // норматив наладки для отчета
                        reader.GetDouble(16),                       // норматив штучный
                        reader.GetDouble(17),                       // фактическое изготовление
                        reader.GetTimeSpan(18),                     // машинное время
                        reader.GetDouble(19),                       // простои в наладке
                        reader.GetDouble(20),                       // простои в изготовлении
                        reader.GetDouble(21),                       // частичная наладка
                        reader.GetDouble(22),                       // обслуживание
                        reader.GetDouble(23),                       // поиск инструмента
                        reader.GetDouble(24),                       // обучение
                        reader.GetDouble(25),                       // организацонные вопросы
                        reader.GetDouble(26),                       // изготовление оснастки
                        reader.GetDouble(27),                       // отказ оборудования
                        reader.GetString(28),                       // комментарий оператора
                        reader.GetValue(29).ToString() ?? "",       // причина невыполнения норматива наладки
                        reader.GetValue(30).ToString() ?? "",       // причина невыполнения норматива изготовления
                        reader.GetValue(31).ToString() ?? "",       // причина отмеченных простоев
                        reader.GetValue(32).ToString() ?? "",       // причина неотмеченных простоев
                        reader.GetValue(33).ToString() ?? "",       // комментарий техотдела
                        reader.GetValue(34).ToString() ?? ""        // комментарий техотдела
                        );
                    parts.Add(part);
                }
            }
        }
    }
}
