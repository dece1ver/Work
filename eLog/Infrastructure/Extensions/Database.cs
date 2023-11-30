using DocumentFormat.OpenXml.Office2010.PowerPoint;
using eLog.Models;
using libeLog.Extensions;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    public static class Database
    {
        

        /// <summary>
        /// Запись информации о детали в БЖ
        /// </summary>
        /// <param name="part">Деталь</param>
        /// <param name="passive">Нужно ли присваивать Id, false используется для одновременной работы с двумя источниками, тогда Id назначается при записи в XL</param>
        /// <returns></returns>
        public static DbResult WritePart(this Part part, bool passive = false)
        {
            if (AppSettings.Instance.DebugMode) Util.WriteLog(part, "Добавление информации об изготовлении в БД.");
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnetctionString))
                {
                    connection.Open();
                    if (AppSettings.Instance.DebugMode) Util.WriteLog("Соединение к БД открыто.");
                    var partIndex = AppSettings.Instance.Parts.IndexOf(part);
                    var prevPart = partIndex != -1 && AppSettings.Instance.Parts.Count > partIndex + 1 ? AppSettings.Instance.Parts[partIndex + 1] : null;
                    string insertQuery = "INSERT INTO Parts (" +
                        "Guid, " +
                        "Machine, " +
                        "Shift, " +
                        "ShiftDate, " +
                        "Operator, " +
                        "PartName, " +
                        "[Order], " +
                        "Setup, " +
                        "FinishedCount, " +
                        "TotalCount, " +
                        "StartSetupTime, " +
                        "StartMachiningTime, " +
                        "EndMachiningTime, " +
                        "SetupTimePlan, " +
                        "SetupTimePlanForReport, " +
                        "SingleProductionTimePlan, " +
                        "MachiningTime) " +
                        "VALUES (" +
                        "@Guid, " +
                        "@Machine, " +
                        "@Shift, " +
                        "@ShiftDate, " +
                        "@Operator, " +
                        "@PartName, " +
                        "@Order, " +
                        "@Setup, " +
                        "@FinishedCount, " +
                        "@TotalCount, " +
                        "@StartSetupTime, " +
                        "@StartMachiningTime, " +
                        "@EndMachiningTime, " +
                        "@SetupTimePlan, " +
                        "@SetupTimePlanForReport, " +
                        "@SingleProductionTimePlan, " +
                        "@MachiningTime" +
                        "@SetupDowntimes" +
                        "@MachiningDowntimes" +
                        "@PartialSetupTime" +
                        "@MaintenanceTime" +
                        "@ToolSearchingTime" +
                        "@MentoringTime" +
                        "@ContactingDepartmentsTime" +
                        "@FixtureMakingTime" +
                        "@HardwareFailureTime" +
                        "); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Guid", part.Guid);
                        cmd.Parameters.AddWithValue("@Machine", AppSettings.Instance.Machine.Name);
                        cmd.Parameters.AddWithValue("@Shift", part.Shift);
                        var needDiscrease = part.Shift == Text.NightShift && part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(8);
                        var shiftDate = needDiscrease
                            ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                            : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                        cmd.Parameters.AddWithValue("@ShiftDate", shiftDate);
                        cmd.Parameters.AddWithValue("@Operator", part.Operator.FullName);
                        cmd.Parameters.AddWithValue("@PartName", part.FullName);
                        cmd.Parameters.AddWithValue("@Order", part.Order);
                        cmd.Parameters.AddWithValue("@Setup", part.Setup);
                        cmd.Parameters.AddWithValue("@FinishedCount", part.FinishedCount);
                        cmd.Parameters.AddWithValue("@TotalCount", part.TotalCount);
                        cmd.Parameters.AddWithValue("@StartSetupTime", part.StartSetupTime);
                        cmd.Parameters.AddWithValue("@StartMachiningTime", part.StartMachiningTime);
                        cmd.Parameters.AddWithValue("@EndMachiningTime", part.EndMachiningTime);
                        cmd.Parameters.AddWithValue("@SetupTimePlan", part.SetupTimePlan);
                        cmd.Parameters.AddWithValue("@SingleProductionTimePlan", part.SingleProductionTimePlan);
                        cmd.Parameters.AddWithValue("@MachiningTime", part.MachineTime);
                        cmd.Parameters.AddWithValue("@SetupDowntimes", Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MachiningDowntimes", Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@PartialSetupTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.PartialSetup }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MaintenanceTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Maintenance }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@ToolSearchingTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ToolSearching }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MentoringTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Mentoring }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@ContactingDepartmentsTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ContactingDepartments }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@FixtureMakingTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.FixtureMaking }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@HardwareFailureTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.HardwareFailure }).TotalMinutes(), 0));
                        var partSetupTimePlanReport = prevPart != null && prevPart.Order == part.Order && prevPart.Setup == part.Setup ? 0 : part.SetupTimePlan;
                        if (partSetupTimePlanReport == 0 && part.SetupTimeFact.TotalMinutes > 0) partSetupTimePlanReport = part.SetupTimeFact.TotalMinutes;
                        cmd.Parameters.AddWithValue("@SetupTimePlanForReport", partSetupTimePlanReport);
                        if (AppSettings.Instance.DebugMode) Util.WriteLog("Запись...");
                        var execureResult = cmd.ExecuteNonQuery();
                        if (!passive)
                        {
                            using (SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Parts", connection))
                            {
                                part.Id = (int)countCmd.ExecuteScalar();
                            }
                        }
                        if (AppSettings.Instance.DebugMode) Util.WriteLog($"Записно строк: {execureResult}\n{(passive ? "Оставлен" : "Присвоен")} Id: {part.Id}");
                    }
                    connection.Close();
                    return DbResult.Ok;
                }
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 2601 or 2627:
                        Util.WriteLog(sqlEx, $"Ошибка №{sqlEx.Number}:\nЗапись в БД уже существует.");
                        return UpdatePart(part);
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

        /// <summary>
        /// Обновление информации о детали в БД
        /// </summary>
        /// <param name="part">Деталь</param>
        /// <param name="passive">Нужно ли присваивать Id, false используется для одновременной работы с двумя источниками, тогда Id назначается при записи в XL. 
        /// В этом методе используется только для передачи в метод WritePart.</param>
        /// <returns></returns>
        public static DbResult UpdatePart(this Part part, bool passive = false)
        {
            if (AppSettings.Instance.DebugMode) Util.WriteLog(part, "Обновление информации об изготовлении в БД.");
            var partIndex = AppSettings.Instance.Parts.IndexOf(part);
            var prevPart = partIndex != -1 && AppSettings.Instance.Parts.Count > partIndex + 1 ? AppSettings.Instance.Parts[partIndex + 1] : null;
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnetctionString))
                {
                    connection.Open();
                    if (AppSettings.Instance.DebugMode) Util.WriteLog("Соединение к БД открыто.");
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
                        "SetupTimePlan = @SetupTimePlan, " +
                        "SetupTimePlanForReport = @SetupTimePlanForReport, " +
                        "SingleProductionTimePlan = @SingleProductionTimePlan, " +
                        "MachiningTime = @MachiningTime " +
                        "SetupDowntimes = @SetupDowntimes " +
                        "MachiningDowntimes = @MachiningDowntimes " +
                        "PartialSetupTime = @PartialSetupTime " +
                        "MaintenanceTime = @MaintenanceTime " +
                        "ToolSearchingTime = @ToolSearchingTime " +
                        "MentoringTime = @MentoringTime " +
                        "ContactingDepartmentsTime = @ContactingDepartmentsTime " +
                        "FixtureMakingTime = @FixtureMakingTime " +
                        "HardwareFailureTime = @HardwareFailureTime " +
                        "WHERE Guid = @Guid";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Guid", part.Guid);
                        cmd.Parameters.AddWithValue("@Machine", AppSettings.Instance.Machine.Name);
                        cmd.Parameters.AddWithValue("@Shift", part.Shift);
                        var needDiscrease = part.Shift == Text.NightShift && part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(8);
                        var shiftDate = needDiscrease
                            ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                            : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                        cmd.Parameters.AddWithValue("@ShiftDate", shiftDate);
                        cmd.Parameters.AddWithValue("@Operator", part.Operator.FullName);
                        cmd.Parameters.AddWithValue("@PartName", part.FullName);
                        cmd.Parameters.AddWithValue("@Order", part.Order);
                        cmd.Parameters.AddWithValue("@Setup", part.Setup);
                        cmd.Parameters.AddWithValue("@FinishedCount", part.FinishedCount);
                        cmd.Parameters.AddWithValue("@TotalCount", part.TotalCount);
                        cmd.Parameters.AddWithValue("@StartSetupTime", part.StartSetupTime);
                        cmd.Parameters.AddWithValue("@StartMachiningTime", part.StartMachiningTime);
                        cmd.Parameters.AddWithValue("@EndMachiningTime", part.EndMachiningTime);
                        cmd.Parameters.AddWithValue("@SetupTimePlan", part.SetupTimePlan);
                        var partSetupTimePlanReport = prevPart != null && prevPart.Order == part.Order && prevPart.Setup == part.Setup ? 0 : part.SetupTimePlan;
                        if (partSetupTimePlanReport == 0 && part.SetupTimeFact.TotalMinutes > 0) partSetupTimePlanReport = part.SetupTimeFact.TotalMinutes;
                        cmd.Parameters.AddWithValue("@SetupTimePlanForReport", partSetupTimePlanReport);
                        cmd.Parameters.AddWithValue("@SingleProductionTimePlan", part.SingleProductionTimePlan);
                        cmd.Parameters.AddWithValue("@MachiningTime", part.MachineTime);
                        cmd.Parameters.AddWithValue("@SetupDowntimes", Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MachiningDowntimes", Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@PartialSetupTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.PartialSetup }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MaintenanceTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Maintenance }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@ToolSearchingTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ToolSearching }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@MentoringTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Mentoring }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@ContactingDepartmentsTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ContactingDepartments }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@FixtureMakingTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.FixtureMaking }).TotalMinutes(), 0));
                        cmd.Parameters.AddWithValue("@HardwareFailureTime", Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.HardwareFailure }).TotalMinutes(), 0));

                        if (AppSettings.Instance.DebugMode) Util.WriteLog("Запись...");
                        var execureResult = cmd.ExecuteNonQuery();
                        if (AppSettings.Instance.DebugMode) Util.WriteLog($"Изменено строк: {execureResult}");
                        if (execureResult == 0)
                        {
                            Util.WriteLog("Деталь не найдена, добавение новой.");
                            return WritePart(part, passive);
                        }
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
    }
}
