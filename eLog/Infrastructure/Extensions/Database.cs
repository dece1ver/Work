using DocumentFormat.OpenXml.Office2010.PowerPoint;
using eLog.Models;
using libeLog.Extensions;
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
        public static bool WritePart(this Part part)
        {
            if (AppSettings.Instance.DebugMode) Util.WriteLog(part, "Добавление информации об изготовлении в БД.");
            var result = false;
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
                        "@MachiningTime); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Guid", part.Guid);
                        cmd.Parameters.AddWithValue("@Machine", AppSettings.Instance.Machine.Name);
                        cmd.Parameters.AddWithValue("@Shift", part.Shift);
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
                        var partSetupTimePlanReport = prevPart != null && prevPart.Order == part.Order && prevPart.Setup == part.Setup ? 0 : part.SetupTimePlan;
                        if (partSetupTimePlanReport == 0 && part.SetupTimeFact.TotalMinutes > 0) partSetupTimePlanReport = part.SetupTimeFact.TotalMinutes;
                        cmd.Parameters.AddWithValue("@SetupTimePlanForReport", partSetupTimePlanReport);
                        if (AppSettings.Instance.DebugMode) Util.WriteLog("Запись...");
                        var execureResult = cmd.ExecuteNonQuery();
                        using (SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Parts", connection))
                        {
                            part.Id = (int)countCmd.ExecuteScalar();
                        }
                        if (AppSettings.Instance.DebugMode) Util.WriteLog($"Записно строк: {execureResult}\nПрисвоен Id: {part.Id}");
                        result = true;
                    }
                    connection.Close();
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number is 2601 or 2627)
                {
                    Util.WriteLog("Запись уже существует.");
                    return UpdatePart(part);
                }
                else
                {
                    Util.WriteLog(sqlEx);
                }
            }
            catch (Exception ex)
            {
                Util.WriteLog(ex);
            }
            return result;
        }

        public static bool UpdatePart(this Part part)
        {
            if (AppSettings.Instance.DebugMode) Util.WriteLog(part, "Обновление информации об изготовлении в БД.");
            var partIndex = AppSettings.Instance.Parts.IndexOf(part);
            var prevPart = partIndex != -1 && AppSettings.Instance.Parts.Count > partIndex + 1 ? AppSettings.Instance.Parts[partIndex + 1] : null;
            var result = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnetctionString))
                {
                    connection.Open();
                    if (AppSettings.Instance.DebugMode) Util.WriteLog("Соединение к БД открыто.");
                    string updateQuery = "UPDATE Parts SET " +
                        "Machine = @Machine, " +
                        "Shift = @Shift, " +
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
                        "WHERE Guid = @Guid";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Guid", part.Guid);
                        cmd.Parameters.AddWithValue("@Machine", AppSettings.Instance.Machine.Name);
                        cmd.Parameters.AddWithValue("@Shift", part.Shift);
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

                        if (AppSettings.Instance.DebugMode) Util.WriteLog("Запись...");
                        var execureResult = cmd.ExecuteNonQuery();
                        if (AppSettings.Instance.DebugMode) Util.WriteLog($"Изменено строк: {execureResult}");
                        if (execureResult == 0)
                        {
                            Util.WriteLog("Деталь не найдена, добавение новой.");
                            return WritePart(part);
                        }
                        result = true;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Util.WriteLog(ex);
            }
            return result;
        }
    }
}
