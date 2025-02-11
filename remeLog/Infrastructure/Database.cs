using libeLog.Extensions;
using libeLog.Infrastructure;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using remeLog.Infrastructure.Extensions;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Part = remeLog.Models.Part;

namespace remeLog.Infrastructure
{
    public static class Database
    {
        public static string GetLicenseKey(string licenseName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT license_key FROM licensing where license_name = '{licenseName}';";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader.GetString(0);

                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex) 
            {
                Util.WriteLog(ex);
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }

        public static List<OperatorInfo> GetOperators()
        {
            List<OperatorInfo> operators = new();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM cnc_operators ORDER BY Name ASC;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            operators.Add(new OperatorInfo(
                                reader.GetInt32(0),
                                reader.GetString(1).Trim(),
                                reader.GetInt32(2),
                                reader.GetBoolean(3)));
                        }
                    }
                }
            }
            return operators;
        }

        public async static Task<List<OperatorInfo>> GetOperatorsAsync(IProgress<string> progress)
        {
            List<OperatorInfo> operators = new();
            
            await Task.Run(async () =>
            {
                progress.Report("Подключение к БД...");
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = $"SELECT * FROM cnc_operators ORDER BY Name ASC;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            progress.Report("Чтение данных из БД...");
                            while (reader.Read())
                            {
                                operators.Add(new OperatorInfo(
                                    reader.GetInt32(0), 
                                    reader.GetString(1).Trim(), 
                                    reader.GetInt32(2), 
                                    reader.GetBoolean(3)));
                            }
                        }
                    }
                }
                progress.Report("Чтение завершено");
            });
            return operators;
        }

        /// <summary>
        /// Сохраняет информацию об операторе в базе данных.
        /// Если оператор с заданным Id существует, выполняется обновление его данных,
        /// если нет - создается новый оператор.
        /// </summary>
        /// <param name="operatorInfo">Объект оператора, содержащий данные для сохранения.</param>
        /// <param name="progress">Прогресс для отслеживания состояния выполнения.</param>
        /// <returns>Асинхронная задача, представляющая операцию сохранения.</returns>
        public static async Task SaveOperatorAsync(OperatorInfo operatorInfo, IProgress<string> progress)
        {
            
            string query = "IF EXISTS (SELECT 1 FROM cnc_operators WHERE Id = @Id) " +
                           "BEGIN " +
                           "    UPDATE cnc_operators SET Name = @Name, Qualification = @Qualification, IsActive = @IsActive WHERE Id = @Id; " +
                           "END " +
                           "ELSE " +
                           "BEGIN " +
                           "    INSERT INTO cnc_operators (Name, Qualification, IsActive) VALUES (@Name, @Qualification, @IsActive); " +
                           "END;";
            progress.Report("Подключение к БД...");
            using (var connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", operatorInfo.Id);
                    command.Parameters.AddWithValue("@Name", operatorInfo.Name);
                    command.Parameters.AddWithValue("@Qualification", operatorInfo.Qualification);
                    command.Parameters.AddWithValue("@IsActive", operatorInfo.IsActive);

                    progress.Report($"Сохранение оператора '{operatorInfo.Name}' в БД...");
                    await command.ExecuteNonQueryAsync();
                    progress.Report($"Оператор '{operatorInfo.Name}' успешно сохранен.");
                }
            }
        }

        /// <summary>
        /// Сохраняет список операторов в базе данных.
        /// Для каждого оператора в списке вызывается метод SaveOperatorAsync,
        /// который проверяет существование оператора и выполняет соответствующее действие.
        /// </summary>
        /// <param name="operators">Список операторов для сохранения.</param>
        /// <param name="progress">Прогресс для отслеживания состояния выполнения.</param>
        /// <returns>Асинхронная задача, представляющая операцию сохранения.</returns>
        public static async Task SaveOperatorsAsync(IEnumerable<OperatorInfo> operators, IProgress<string> progress)
        {
            progress.Report("Сохранение операторов в БД");
            foreach (var operatorInfo in operators)
            {
                await SaveOperatorAsync(operatorInfo, progress);
            }
            progress.Report("Сохранение операторов в БД выполнено");
        }

        /// <summary>
        /// Удаляет оператора из базы данных по указанному идентификатору.
        /// Если оператор с данным Id существует, он будет удален.
        /// </summary>
        /// <param name="operatorId">Уникальный идентификатор оператора, которого необходимо удалить.</param>
        /// <param name="progress">Прогресс для отслеживания состояния выполнения.</param>
        /// <returns>Асинхронная задача, представляющая операцию удаления.</returns>
        public static async Task DeleteOperatorAsync(int operatorId, IProgress<string> progress)
        {
            string query = "DELETE FROM cnc_operators WHERE Id = @Id;";

            using (var connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", operatorId);

                    progress.Report("Удаление оператора из БД...");
                    int rowsAffected = await command.ExecuteNonQueryAsync(); 

                    if (rowsAffected > 0)
                    {
                        progress.Report("Оператор успешно удален.");
                    }
                    else
                    {
                        progress.Report("Оператор не найден.");
                    }
                }
            }
        }

        public async static Task<List<Part>> ReadPartsWithConditions(string conditions, CancellationToken cancellationToken)
        {
            List<Part> parts = new();
            await Task.Run(async () =>
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    string query = $"SELECT * FROM Parts WHERE {conditions} ORDER BY StartSetupTime ASC;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await FillPartsAsync(parts, command, cancellationToken);
                    }
                }
            }, cancellationToken);
            return parts;
        }

        public async static Task<ObservableCollection<Part>> ReadPartsByShiftDateAndMachine(DateTime fromDate, DateTime toDate, string machine, CancellationToken cancellationToken)
        {
            ObservableCollection<Part> parts = new();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Parts WHERE ShiftDate BETWEEN @FromDate AND @ToDate AND Machine = @Machine ORDER BY StartSetupTime ASC;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);
                    command.Parameters.AddWithValue("@Machine", machine);

                    await parts.FillPartsAsync(command, cancellationToken);
                }
            }
            return parts;
        }

        public async static Task<ObservableCollection<Part>> ReadPartsByPartNameAndOrder(string[] partNames, string[] orders, CancellationToken cancellationToken)
        {
            ObservableCollection<Part> parts = new();
            using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Parts WHERE PartName IN ('" + string.Join("','", partNames) + "') AND [Order] IN ('" + string.Join("','", orders) + "')";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await parts.FillPartsAsync(command, cancellationToken);
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
                        "EngineerComment = @EngineerComment, " +
                        "ExcludeFromReports = @ExcludeFromReports " +
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
                        cmd.Parameters.AddWithValue("@ExcludeFromReports", part.ExcludeFromReports);

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

        static async Task FillPartsAsync(this ICollection<Part> parts, SqlCommand command, CancellationToken cancellationToken)
        {
            using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var guid = await reader.GetFieldValueAsync<Guid>(0);
                    var machine = await reader.GetFieldValueAsync<string>(1);
                    var shift = await reader.GetFieldValueAsync<string>(2);
                    var shiftDate = await reader.GetFieldValueAsync<DateTime>(3);
                    var @operator = await reader.GetFieldValueAsync<string>(4);
                    var partName = await reader.GetFieldValueAsync<string>(5);
                    var order = await reader.GetFieldValueAsync<string>(6);
                    var setup = await reader.GetFieldValueAsync<int>(7);
                    var finishedCount = await reader.GetFieldValueAsync<int>(8);
                    var totalCount = await reader.GetFieldValueAsync<int>(9);
                    var startSetupTime = await reader.GetFieldValueAsync<DateTime>(10);
                    var startMachiningTime = await reader.GetFieldValueAsync<DateTime>(11);
                    var setupTimeFact = await reader.GetFieldValueAsync<double>(12);
                    var endMachiningTime = await reader.GetFieldValueAsync<DateTime>(13);
                    var setupTimePlan = await reader.GetFieldValueAsync<double>(14);
                    var setupTimePlanForReport = await reader.GetFieldValueAsync<double>(15);
                    var singleProductionTimePlan = await reader.GetFieldValueAsync<double>(16);
                    var productionTimeFact = await reader.GetFieldValueAsync<double>(17);
                    var machininhTime = await reader.GetFieldValueAsync<TimeSpan>(18);
                    var setupDowntimes = await reader.GetFieldValueAsync<double>(19);
                    var machiningDowntimes = await reader.GetFieldValueAsync<double>(20);
                    var partialSetupTime = await reader.GetFieldValueAsync<double>(21);
                    var createNcProgramTime = await reader.GetFieldValueAsync<double>(22);
                    var maintenanceTime = await reader.GetFieldValueAsync<double>(23);
                    var toolSearchingTime = await reader.GetFieldValueAsync<double>(24);
                    var toolChangingTime = await reader.GetFieldValueAsync<double>(25);
                    var mentoringTime = await reader.GetFieldValueAsync<double>(26);
                    var contactiongDepartmentsTime = await reader.GetFieldValueAsync<double>(27);
                    var fixtureMakingTime = await reader.GetFieldValueAsync<double>(28);
                    var hardwareFailureTime = await reader.GetFieldValueAsync<double>(29);
                    var operatorComment = await reader.GetFieldValueAsync<string>(30);
                    var masterSetupComment = reader.GetValue(31)?.ToString() ?? "";
                    var masterMachiningComment = reader.GetValue(32)?.ToString() ?? "";
                    var specifiedDowntimesComment = reader.GetValue(33)?.ToString() ?? "";
                    var unspecifiedDowntimesComment = reader.GetValue(34)?.ToString() ?? "";
                    var masterComment = reader.GetValue(35)?.ToString() ?? "";
                    var fixedSetupComment = reader.GetValue(36).GetDouble();
                    var fixedProductionComment = reader.GetValue(37).GetDouble();
                    var engineerComment = reader.GetValue(38)?.ToString() ?? "";
                    var excludeFromReports = reader.GetValue(39) == DBNull.Value ? false : (bool)reader.GetValue(39);

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
                        createNcProgramTime,
                        maintenanceTime,
                        toolSearchingTime,
                        toolChangingTime,
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
                        engineerComment,
                        excludeFromReports);
                    parts.Add(part);
                }
            }
        }

        public static DbResult ReadMasters(this ICollection<string> masters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT FullName FROM masters WHERE IsActive = 1 ORDER BY FullName ASC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                masters.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult ReadMachines(this ICollection<string> machines)
        {
            machines.Clear();
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT Name FROM cnc_machines WHERE IsActive = 1 ORDER BY Name ASC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                machines.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public async static Task<DbResult> ReadMachines(this ICollection<MachineFilter> machines)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = $"SELECT Name, Type FROM cnc_machines WHERE IsActive = 1 ORDER BY Name ASC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                machines.Add(new(await reader.GetFieldValueAsync<string>(0), await reader.GetFieldValueAsync<string>(1), false));
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult ReadDeviationReasons(this ICollection<(string, bool)> reasons, DeviationReasonType type)
        {
            try
            {
                reasons.Clear();
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    var typeCondition = type switch
                    {
                        DeviationReasonType.Setup => "Type = 'Setup'",
                        DeviationReasonType.Machining => "Type = 'Machining'",
                        _ => throw new ArgumentException("Неверный аргумент в типе причин."),
                    };
                    connection.Open();
                    string query = $"SELECT Reason, RequireComment FROM cnc_deviation_reasons WHERE Type IS NULL OR {typeCondition} ORDER BY Reason ASC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reasons.Add((reader.GetString(0), reader.GetBoolean(1)));
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult ReadDowntimeReasons(this ICollection<string> reasons)
        {
            try
            {
                reasons.Clear();
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT Reason FROM cnc_downtime_reasons ORDER BY Reason ASC";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reasons.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult WriteShiftInfo(ShiftInfo shiftInfo)
        {
            try
            {
                ReadShiftInfo(shiftInfo, out var shifts);
                if (shifts is { Count: 1 })
                {
                    return UpdateShiftInfo(shiftInfo);
                }
                else if (shifts.Count > 1)
                {
                    MessageBox.Show("Найдена больше чем одна запись за смену, сообщите разработчику.", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    Util.WriteLog("Найдена больше чем одна запись за смену.");
                    return DbResult.Error;
                }
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    if (AppSettings.Instance.DebugMode) Util.WriteLog("Запись в БД информации о смене.");
                    connection.Open();
                    string query = $"INSERT INTO cnc_shifts (ShiftDate, Shift, Machine, Master, UnspecifiedDowntimes, DowntimesComment, CommonComment, IsChecked) " +
                        $"VALUES (@ShiftDate, @Shift, @Machine, @Master, @UnspecifiedDowntimes, @DowntimesComment, @CommonComment, @IsChecked); SELECT SCOPE_IDENTITY()";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("ShiftDate", shiftInfo.ShiftDate);
                        command.Parameters.AddWithValue("Shift", shiftInfo.Shift);
                        command.Parameters.AddWithValue("Machine", shiftInfo.Machine);
                        command.Parameters.AddWithValue("Master", shiftInfo.Master);
                        command.Parameters.AddWithValue("UnspecifiedDowntimes", shiftInfo.UnspecifiedDowntimes);
                        command.Parameters.AddWithValue("DowntimesComment", shiftInfo.DowntimesComment);
                        command.Parameters.AddWithValue("CommonComment", shiftInfo.CommonComment);
                        command.Parameters.AddWithValue("IsChecked", shiftInfo.IsChecked);
                        var result = command.ExecuteScalar();
                        if (AppSettings.Instance.DebugMode) Util.WriteLog($"Смена записана и присвоен ID: {shiftInfo.Id}");
                    }
                    return DbResult.Ok;
                }
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case -1:
                        Util.WriteLog("База данных недоступна.");
                        return DbResult.NoConnection;
                    case 2601 or 2627:
                        Util.WriteLog($"Ошибка №{sqlEx.Number}:\nЗапись в БД уже существует.");
                        return DbResult.Error;
                    case 18456:
                        Util.WriteLog($"Ошибка №{sqlEx.Number}:\nОшибка авторизации.");
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

        public static DbResult ReadShiftInfo(ShiftInfo shiftInfo, out List<ShiftInfo> shifts)
        {
            shifts = new List<ShiftInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM cnc_shifts WHERE ShiftDate = @ShiftDate AND Shift = @Shift AND Machine = @Machine";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("ShiftDate", shiftInfo.ShiftDate);
                        command.Parameters.AddWithValue("Shift", shiftInfo.Shift);
                        command.Parameters.AddWithValue("Machine", shiftInfo.Machine);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                shifts.Add(

                                    new ShiftInfo(
                                        reader.GetInt32(0),                 // Id
                                        reader.GetDateTime(1),              // ShiftDate
                                        reader.GetString(2),                // Shift
                                        reader.GetString(3),                // Machine
                                        reader.GetString(4),                // Master
                                        reader.GetDouble(5),                // UnspecifiedDowntimes
                                        reader.GetString(6),                // DowntimesComment
                                        reader.GetString(7),                // CommonComment
                                        reader.GetBoolean(8),               // IsChecked
                                        reader.GetNullableBoolean(9),       // GiverWorkplaceCleaned
                                        reader.GetNullableBoolean(10),      // GiverFailures
                                        reader.GetNullableBoolean(11),      // GiverExtraneousNoises
                                        reader.GetNullableBoolean(12),      // GiverLiquidLeaks
                                        reader.GetNullableBoolean(13),      // GiverToolBreakage
                                        reader.GetNullableDouble(14),       // GiverCoolantConcentration
                                        reader.GetNullableBoolean(15),      // RecieverWorkplaceCleaned
                                        reader.GetNullableBoolean(16),      // RecieverFailures
                                        reader.GetNullableBoolean(17),      // RecieverExtraneousNoises
                                        reader.GetNullableBoolean(18),      // RecieverLiquidLeaks
                                        reader.GetNullableBoolean(19),      // RecieverToolBreakage
                                        reader.GetNullableDouble(20)        // RecieverCoolantConcentration
                                        )
                                    );
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult GetShiftsByPeriod(ICollection<string> machines, DateTime fromDate, DateTime toDate, Shift shift, out List<ShiftInfo> shifts)
        {
            shifts = new List<ShiftInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    //string machineNums = string.Join(", ", machines.Select((_, i) => $"@machine{i}"));
                    string machinesNames = string.Join(", ", machines.Select(m => $"'{m}'"));

                    string query = $"SELECT * FROM cnc_shifts WHERE ShiftDate BETWEEN @FromDate AND @ToDate AND Machine IN ({machinesNames})";
                    if (shift.Type != Types.ShiftType.All) query += $" AND Shift = '{shift.Name}'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("FromDate", fromDate);
                        command.Parameters.AddWithValue("ToDate", toDate);

                        //for (int i = 0; i < machines.Length; i++)
                        //{
                        //    command.Parameters.AddWithValue($"machine{i}", machines[i]);
                        //}

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                shifts.Add(

                                    new ShiftInfo(
                                        reader.GetInt32(0),                 // Id
                                        reader.GetDateTime(1),              // ShiftDate
                                        reader.GetString(2),                // Shift
                                        reader.GetString(3),                // Machine
                                        reader.GetString(4),                // Master
                                        reader.GetDouble(5),                // UnspecifiedDowntimes
                                        reader.GetString(6),                // DowntimesComment
                                        reader.GetString(7),                // CommonComment
                                        reader.GetBoolean(8),               // IsChecked
                                        reader.GetNullableBoolean(9),       // GiverWorkplaceCleaned
                                        reader.GetNullableBoolean(10),      // GiverFailures
                                        reader.GetNullableBoolean(11),      // GiverExtraneousNoises
                                        reader.GetNullableBoolean(12),      // GiverLiquidLeaks
                                        reader.GetNullableBoolean(13),      // GiverToolBreakage
                                        reader.GetNullableDouble(14),       // GiverCoolantConcentration
                                        reader.GetNullableBoolean(15),      // RecieverWorkplaceCleaned
                                        reader.GetNullableBoolean(16),      // RecieverFailures
                                        reader.GetNullableBoolean(17),      // RecieverExtraneousNoises
                                        reader.GetNullableBoolean(18),      // RecieverLiquidLeaks
                                        reader.GetNullableBoolean(19),      // RecieverToolBreakage
                                        reader.GetNullableDouble(20)        // RecieverCoolantConcentration
                                        )
                                    );
                            }
                        }
                    }
                }
                return DbResult.Ok;
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

        public static DbResult UpdateShiftInfo(ShiftInfo shiftInfo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();

                    string query = $"UPDATE cnc_shifts SET Master = @Master, UnspecifiedDowntimes = @UnspecifiedDowntimes, DowntimesComment = @DowntimesComment, CommonComment = @CommonComment, IsChecked = @IsChecked  " +
                        $"WHERE ShiftDate = @ShiftDate AND Shift = @Shift AND Machine = @Machine";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("ShiftDate", shiftInfo.ShiftDate);
                        command.Parameters.AddWithValue("Shift", shiftInfo.Shift);
                        command.Parameters.AddWithValue("Machine", shiftInfo.Machine);
                        command.Parameters.AddWithValue("Master", shiftInfo.Master);
                        command.Parameters.AddWithValue("UnspecifiedDowntimes", shiftInfo.UnspecifiedDowntimes);
                        command.Parameters.AddWithValue("DowntimesComment", shiftInfo.DowntimesComment);
                        command.Parameters.AddWithValue("CommonComment", shiftInfo.CommonComment);
                        command.Parameters.AddWithValue("IsChecked", shiftInfo.IsChecked);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Util.WriteLog("Смена не найдена, добавение новой.");
                            return WriteShiftInfo(shiftInfo);
                        }
                        else
                        {
                            if (AppSettings.Instance.DebugMode) Util.WriteLog($"Смена обновлена.");
                        }
                    }
                    return DbResult.Ok;
                }
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case -1:
                        Util.WriteLog("База данных недоступна.");
                        return DbResult.NoConnection;
                    case 2601 or 2627:
                        Util.WriteLog($"Ошибка №{sqlEx.Number}:\nЗапись в БД уже существует.");
                        return DbResult.Error;
                    case 18456:
                        Util.WriteLog($"Ошибка №{sqlEx.Number}:\nОшибка авторизации.");
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

        public static DbResult DeletePart(this Part part)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    string query = $"DELETE FROM parts WHERE GUID = @Guid";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("Guid", part.Guid);
                        command.ExecuteNonQuery();
                    }
                }
                return DbResult.Ok;
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

        /// <summary>
        /// Получает лимит наладки для заданного станка, используя строку подключения из настроек приложения.
        /// </summary>
        /// <param name="machine">Имя станка для получения лимита наладки.</param>
        /// <returns>
        /// Кортеж, состоящий из:
        /// - <see cref="DbResult"/>: результат выполнения запроса.
        /// - SetupLimit: лимит наладки для станка (nullable int), может быть null, если данных нет.
        /// - Error: строка с описанием ошибки, если она произошла.
        /// </returns>
        /// <exception cref="InvalidOperationException">Выбрасывается, если строка подключения отсутствует в настройках приложения.</exception>
        public static (DbResult Result, int? SetupLimit, string Error) GetMachineSetupLimit(this string machine)
        {
            if (AppSettings.Instance.ConnectionString == null) throw new InvalidOperationException("Невозомжно получить лимит наладки т.к. отсуствтует строка подключения");
            return machine.GetMachineSetupLimit(AppSettings.Instance.ConnectionString);
        }

        /// <summary>
        /// Получает коэффициент наладки для заданного станка, используя строку подключения из настроек приложения.
        /// </summary>
        /// <param name="machine">Имя станка для получения коэффициента наладки.</param>
        /// <returns>
        /// Кортеж, состоящий из:
        /// - <see cref="DbResult"/>: результат выполнения запроса (например, <c>Ok</c>, <c>NotFound</c>, <c>Error</c>).
        /// - SetupCoefficient: коэффициент наладки для станка (nullable double), может быть null, если данных нет.
        /// - Error: строка с описанием ошибки, если она произошла.
        /// </returns>
        /// <exception cref="InvalidOperationException">Выбрасывается, если строка подключения отсутствует в настройках приложения.</exception>
        public static (DbResult Result, double? SetupCoefficient, string Error) GetMachineSetupCoefficient(this string machine) 
        {
            if (AppSettings.Instance.ConnectionString == null) throw new InvalidOperationException("Невозомжно получить коэффициент лимита наладки т.к. отсуствтует строка подключения");
            return machine.GetMachineSetupCoefficient(AppSettings.Instance.ConnectionString);
        }

        public static DbResult GetWncConfig(out WncConfig wncConfig)
        {
            wncConfig = null!;
            try
            {
                using (var connection = new SqlConnection(AppSettings.Instance.ConnectionString))
                {
                    connection.Open();
                    var query = $"SELECT * FROM cnc_wnc_cfg;";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                wncConfig = new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                                break;
                            }
                        }
                    }
                }
                return DbResult.Ok;
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
