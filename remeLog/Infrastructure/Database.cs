using DocumentFormat.OpenXml.VariantTypes;
using libeLog.Extensions;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using remeLog.Infrastructure.Extensions;
using remeLog.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Part = remeLog.Models.Part;

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
                        cmd.Parameters.AddWithValue("@FixedSetupTimePlan", part.FixedSetupTimePlan);
                        cmd.Parameters.AddWithValue("@FixedProductionTimePlan", part.FixedProductionTimePlan);
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
                    var guid = reader.GetGuid(0);
                    var machine = reader.GetString(1);                  
                    var shift = reader.GetString(2);                  
                    var shiftDate = reader.GetDateTime(3);                
                    var @operator = reader.GetString(4);                  
                    var partName = reader.GetString(5);                  
                    var order = reader.GetString(6);                  
                    var setup = reader.GetInt32(7);                   
                    var finishedCount = reader.GetInt32(8);
                    var totalCount = reader.GetInt32(9);                   
                    var startSetupTime = reader.GetDateTime(10);               
                    var startMachiningTime = reader.GetDateTime(11);
                    var setupTimeFact = reader.GetDouble(12);                 
                    var endMachiningTime = reader.GetDateTime(13);               
                    var setupTimePlan = reader.GetDouble(14);                 
                    var setupTimePlanForReport = reader.GetDouble(15);                 
                    var singleProductionTimePlan = reader.GetDouble(16);                 
                    var productionTimeFact = reader.GetDouble(17);                 
                    var machininhTime = reader.GetTimeSpan(18);               
                    var setupDowntimes = reader.GetDouble(19);                 
                    var machiningDowntimes = reader.GetDouble(20);                 
                    var partialSetupTime = reader.GetDouble(21);                 
                    var maintenanceTime = reader.GetDouble(22);                 
                    var toolSearchingTime = reader.GetDouble(23);                 
                    var mentoringTime = reader.GetDouble(24);                 
                    var contactiongDepartmentsTime = reader.GetDouble(25);                 
                    var fixtureMakingTime = reader.GetDouble(26);                 
                    var hardwareFailureTime = reader.GetDouble(27);                 
                    var operatorComment = reader.GetString(28);                 
                    var masterSetupComment = reader.GetValue(29).ToString() ?? ""; 
                    var masterMachiningComment = reader.GetValue(30).ToString() ?? ""; 
                    var specifiedDowntimesComment = reader.GetValue(31).ToString() ?? ""; 
                    var unspecifiedDowntimesComment = reader.GetValue(32).ToString() ?? ""; 
                    var masterComment = reader.GetValue(33).ToString() ?? ""; 
                    var fixedSetupComment = reader.GetValue(34).GetDouble();      
                    var fixedProductionComment = reader.GetValue(35).GetDouble();     
                    var engineerComment = reader.GetValue(36).ToString() ?? "";

                    Part part = new Part(
                        guid,
                        machine,                   
                        shift,
                        shiftDate, 
                        @operator, 
                        partName, 
                        order, 
                        setup, 
                        finishedCount, 
                        totalCount, 
                        startSetupTime, 
                        startMachiningTime, 
                        setupTimeFact, 
                        endMachiningTime, 
                        setupTimePlan, 
                        setupTimePlanForReport, 
                        singleProductionTimePlan, 
                        productionTimeFact, 
                        machininhTime,
                        setupDowntimes, 
                        machiningDowntimes, 
                        partialSetupTime, 
                        maintenanceTime, 
                        toolSearchingTime, 
                        mentoringTime, 
                        contactiongDepartmentsTime,
                        fixtureMakingTime,
                        hardwareFailureTime, 
                        operatorComment,
                        masterSetupComment, 
                        masterMachiningComment, 
                        specifiedDowntimesComment, 
                        unspecifiedDowntimesComment,
                        masterComment, 
                        fixedSetupComment, 
                        fixedProductionComment, 
                        engineerComment);
                    parts.Add(part);
                }
            }
        }
    }
}
