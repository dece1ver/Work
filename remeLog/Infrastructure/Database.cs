using DocumentFormat.OpenXml.VariantTypes;
using libeLog.Extensions;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using remeLog.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public static DbResult UpdatePart(this Part part)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Parts SET " +
                        "Machine = @Machine, " +
                        "Shift = @Shift, " +
                        "ShiftDate = @ShiftDate, " +
                        "Operator = @Operator, " +
                        "PartName = @PartName, " +
                        "[Order] = @Order, " +
                        "Setup = @Setup, " +
                        "FinishedCount = @FinishedCount, " +
                        "TotalCount = @TotalCount, " +
                        "StartSetupTime = @StartSetupTime, " +
                        "StartMachiningTime = @StartMachiningTime, " +
                        "EndMachiningTime = @EndMachiningTime, " +
                        "SetupTimeFact = @SetupTimeFact, " +
                        "SetupTimePlan = @SetupTimePlan, " +
                        "SetupTimePlanForReport = @SetupTimePlanForReport, " +
                        "SingleProductionTimePlan = @SingleProductionTimePlan, " +
                        "ProductionTimeFact = @ProductionTimeFact, " +
                        "MachiningTime = @MachiningTime, " +
                        "SetupDowntimes = @SetupDowntimes, " +
                        "MachiningDowntimes = @MachiningDowntimes, " +
                        "PartialSetupTime = @PartialSetupTime, " +
                        "MaintenanceTime = @MaintenanceTime, " +
                        "ToolSearchingTime = @ToolSearchingTime, " +
                        "MentoringTime = @MentoringTime, " +
                        "ContactingDepartmentsTime = @ContactingDepartmentsTime, " +
                        "FixtureMakingTime = @FixtureMakingTime, " +
                        "HardwareFailureTime = @HardwareFailureTime, " +
                        "OperatorComment = @OperatorComment, " +
                        "MasterSetupComment = @MasterSetupComment, " +
                        "MasterMachiningComment = @MasterMachiningComment, " +
                        "SpecifiedDowntimesComment = @SpecifiedDowntimesComment, " +
                        "UnspecifiedDowntimeComment = @UnspecifiedDowntimeComment, " +
                        "MasterComment = @MasterComment, " +
                        "FixedSetupTimePlan = @FixedSetupTimePlan, " +
                        "FixedProductionTimePlan = @FixedProductionTimePlan, " +
                        "EngineerComment = @EngineerComment " +
                        "WHERE Guid = @Guid";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Guid", part.Guid);
                        cmd.Parameters.AddWithValue("@Machine", part.Machine);
                        cmd.Parameters.AddWithValue("@Shift", part.Shift);
                        cmd.Parameters.AddWithValue("@ShiftDate", part.ShiftDate);
                        cmd.Parameters.AddWithValue("@Operator", part.Operator);
                        cmd.Parameters.AddWithValue("@PartName", part.PartName);
                        cmd.Parameters.AddWithValue("@Order", part.Order);
                        cmd.Parameters.AddWithValue("@Setup", part.Setup);
                        cmd.Parameters.AddWithValue("@FinishedCount", part.FinishedCount);
                        cmd.Parameters.AddWithValue("@TotalCount", part.TotalCount);
                        cmd.Parameters.AddWithValue("@StartSetupTime", part.StartSetupTime);
                        cmd.Parameters.AddWithValue("@StartMachiningTime", part.StartMachiningTime);
                        cmd.Parameters.AddWithValue("@SetupTimeFact", part.SetupTimeFact);
                        cmd.Parameters.AddWithValue("@EndMachiningTime", part.EndMachiningTime);
                        cmd.Parameters.AddWithValue("@SetupTimePlan", part.SetupTimePlan);
                        cmd.Parameters.AddWithValue("@SetupTimePlanForReport", part.SetupTimePlanForReport);
                        cmd.Parameters.AddWithValue("@SingleProductionTimePlan", part.SingleProductionTimePlan);
                        cmd.Parameters.AddWithValue("@ProductionTimeFact", part.ProductionTimeFact);
                        cmd.Parameters.AddWithValue("@MachiningTime", part.MachiningTime);
                        cmd.Parameters.AddWithValue("@SetupDowntimes", part.SetupDowntimes);
                        cmd.Parameters.AddWithValue("@MachiningDowntimes", part.MachiningDowntimes);
                        cmd.Parameters.AddWithValue("@PartialSetupTime", part.PartialSetupTime);
                        cmd.Parameters.AddWithValue("@MaintenanceTime", part.MaintenanceTime);
                        cmd.Parameters.AddWithValue("@ToolSearchingTime", part.ToolSearchingTime);
                        cmd.Parameters.AddWithValue("@MentoringTime", part.MentoringTime);
                        cmd.Parameters.AddWithValue("@ContactingDepartmentsTime", part.ContactingDepartmentsTime);
                        cmd.Parameters.AddWithValue("@FixtureMakingTime", part.FixtureMakingTime);
                        cmd.Parameters.AddWithValue("@HardwareFailureTime", part.HardwareFailureTime);
                        cmd.Parameters.AddWithValue("@OperatorComment", part.OperatorComment);
                        cmd.Parameters.AddWithValue("@MasterSetupComment", part.MasterSetupComment);
                        cmd.Parameters.AddWithValue("@MasterMachiningComment", part.MasterMachiningComment);
                        cmd.Parameters.AddWithValue("@SpecifiedDowntimesComment", part.SpecifiedDowntimesComment);
                        cmd.Parameters.AddWithValue("@UnspecifiedDowntimeComment", part.UnspecifiedDowntimesComment);
                        cmd.Parameters.AddWithValue("@MasterComment", part.MasterComment);
                        cmd.Parameters.AddWithValue("@FixedSetupTimePlan", part.MasterComment);
                        cmd.Parameters.AddWithValue("@FixedProductionTimePlan", part.MasterComment);
                        cmd.Parameters.AddWithValue("@EngineerComment", part.EngineerComment);

                        var execureResult = cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                    return DbResult.Ok;
                }
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 18456:
                        Util.WriteLog(sqlEx, $"Ошибка №{sqlEx.Number}:\nОшибка авторизации.");
                        return DbResult.AuthError;
                    default:
                        Util.WriteLog(sqlEx, $"Ошибка №{sqlEx.Number}:");
                        return DbResult.Error;
                }
            }
            catch (Exception ex)
            {
                Util.WriteLog(ex);
                return DbResult.Error;
            }
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
                        reader.GetString(2),                        // смена
                        reader.GetDateTime(3),                      // дата смены
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
