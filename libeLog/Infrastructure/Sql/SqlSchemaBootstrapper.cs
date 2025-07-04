using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace libeLog.Infrastructure.Sql
{
    public static class SqlSchemaBootstrapper
    {

        /// <summary>
        /// Применяет определение всех таблиц в базу данных. Создаёт отсутствующие таблицы, добавляет недостающие столбцы и ограничения.
        /// Безопасно вызывается повторно: если структура уже соответствует, ничего не делает.
        /// </summary>
        /// <param name="connection">Открытое соединение с SQL Server.</param>
        /// <param name="progress">Опциональный вывод прогресса.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public static async Task ApplyAllAsync(SqlConnection connection, IProgress<(string, Status?)>? progress = null, CancellationToken cancellationToken = default)
        {
            var tables = GetAllTableDefinitions();
            foreach (var table in tables)
            {
                await ApplyAsync(connection, table, progress, cancellationToken);
            }
        }

        public static async Task ApplyAsync(SqlConnection connection, TableDefinition table, IProgress<(string, Status?)>? progress = null, CancellationToken cancellationToken = default)
        {
            var helper = new SqlSchemaDiffHelper(connection.ConnectionString);
            progress?.Report(($"Таблица: {table.Name}", Status.Sync));
            var updated = await helper.ApplyMissingColumnsAndConstraintsAsync(table, progress, cancellationToken);
            if (updated)
                progress?.Report(($"Таблица: {table.Name}", Status.Warning));
            else
                progress?.Report(($"Таблица: {table.Name}", Status.Ok));
        }

        /// <summary>
        /// Возвращает все таблицы приложения, описанные через TableBuilder.
        /// </summary>
        public static List<TableDefinition> GetAllTableDefinitions() => new()
        {
            new TableBuilder("cnc_deviation_reasons")
                .AddIdColumn()
                .AddStringColumn("Reason", -1, false)
                .AddNCharColumn("Type", 9)
                .AddBoolColumn("RequireComment", false)
                .Build(),

            new TableBuilder("cnc_downtime_reasons")
                .AddIdColumn()
                .AddStringColumn("Reason", 50, false)
                .Build(),

            new TableBuilder("cnc_elog_config")
                .AddIdColumn()
                .AddStringColumn("SearchToolTypes", 50)
                .AddStringColumn("UpdatePath", -1)
                .AddStringColumn("LogPath", -1)
                .AddStringColumn("OrderPrefixes", -1)
                .AddStringColumn("AssignedPartsGsId")
                .Build(),

            new TableBuilder("cnc_machines")
                .AddIdColumn()
                .AddStringColumn("Name", 50, false)
                .AddBoolColumn("IsActive", false)
                .AddStringColumn("Type", 50, false)
                .AddIntColumn("SetupLimit", false)
                .AddDoubleColumn("SetupCoefficient", false)
                .AddIntColumn("WnId", false)
                .AddGuidColumn("WnUuid", false, false)
                .AddStringColumn("WnCounterSignal", 8, true)
                .AddStringColumn("WnNcNameSignal", 8, true)
                .AddStringColumn("WnNcPartNameSignal", 8, true)
                .Build(),

            new TableBuilder("cnc_operators")
                .AddIdColumn()
                .AddStringColumn("FirstName", 50, false)
                .AddStringColumn("LastName", 50, false)
                .AddStringColumn("Patronymic", 50)
                .AddIntColumn("Qualification", false)
                .AddBoolColumn("IsActive", false)
                .Build(),

            new TableBuilder("cnc_remelog_config")
                .AddIdColumn()
                .AddDoubleColumn("max_setup_limit", false)
                .AddDoubleColumn("long_setup_limit", false)
                .AddStringColumn("NcArchivePath")
                .AddStringColumn("NcIntermediatePath")
                .Build(),

            new TableBuilder("cnc_serial_parts")
                .AddIdColumn()
                .AddStringColumn("PartName", 255, false)
                .AddCompositeUnique("PartName")
                .AddIntColumn("YearCount", false)
                .Build(),

            new TableBuilder("cnc_shifts")
                .AddIdColumn()
                .AddSmallDateTimeColumn("ShiftDate", false)
                .AddNCharColumn("Shift", 4, false)
                .AddStringColumn("Machine", -1, false)
                .AddStringColumn("Master", -1, false)
                .AddDoubleColumn("UnspecifiedDowntimes", false)
                .AddStringColumn("DowntimesComment", -1, false)
                .AddStringColumn("CommonComment", -1, false)
                .AddBoolColumn("IsChecked", false)
                .AddBoolColumn("GiverWorkplaceCleaned")
                .AddBoolColumn("GiverFailures")
                .AddBoolColumn("GiverExtraneousNoises")
                .AddBoolColumn("GiverLiquidLeaks")
                .AddBoolColumn("GiverToolBreakage")
                .AddDoubleColumn("GiverCoolantConcentration")
                .AddBoolColumn("RecieverWorkplaceCleaned")
                .AddBoolColumn("RecieverFailures")
                .AddBoolColumn("RecieverExtraneousNoises")
                .AddBoolColumn("RecieverLiquidLeaks")
                .AddBoolColumn("RecieverToolBreakage")
                .AddDoubleColumn("RecieverCoolantConcentration")
                .Build(),

            new TableBuilder("cnc_tool_search_cases")
                .AddIdColumn()
                .AddGuidColumn("PartGuid", nullable: false)
                .AddStringColumn("ToolType", 50, false)
                .AddStringColumn("Value", -1, false)
                .AddSmallDateTimeColumn("StartTime", false)
                .AddSmallDateTimeColumn("EndTime", false)
                .AddBoolColumn("IsSuccess")
                .AddForeignKey("PartGuid", "parts", "Guid", ForeignKeyAction.Cascade, ForeignKeyAction.Cascade)
                .Build(),

            new TableBuilder("cnc_wnc_cfg")
                .AddStringColumn("Server", 50, false)
                .AddStringColumn("User", 50, false)
                .AddStringColumn("Pass", 50, false)
                .AddStringColumn("LocalType", 50, false)
                .Build(),

            new TableBuilder("cnc_winnum_cfg")
                .AddIdColumn()
                .AddStringColumn("BaseUri", 255, false)
                .AddStringColumn("User", 50, false)
                .AddStringColumn("Pass", 50, false)
                .AddStringColumn("NcProgramFolder")
                .Build(),

            new TableBuilder("masters")
                .AddIdColumn()
                .AddStringColumn("FullName", -1, false)
                .AddBoolColumn("IsActive", false)
                .Build(),

            new TableBuilder("parts")
                .AddGuidColumn("Guid", isPrimaryKey: true)
                .AddStringColumn("Machine", -1, false)
                .AddNCharColumn("Shift", 4, false)
                .AddSmallDateTimeColumn("ShiftDate", false)
                .AddStringColumn("Operator", -1, false)
                .AddStringColumn("PartName", -1, false)
                .AddStringColumn("Order", 50, false)
                .AddIntColumn("Setup", false)
                .AddDoubleColumn("FinishedCount", false)
                .AddIntColumn("TotalCount", false)
                .AddSmallDateTimeColumn("StartSetupTime", false)
                .AddSmallDateTimeColumn("StartMachiningTime", false)
                .AddDoubleColumn("SetupTimeFact", false)
                .AddSmallDateTimeColumn("EndMachiningTime", false)
                .AddDoubleColumn("SetupTimePlan", false)
                .AddDoubleColumn("SetupTimePlanForReport", false)
                .AddDoubleColumn("SingleProductionTimePlan", false)
                .AddDoubleColumn("ProductionTimeFact", false)
                .AddSqlServerColumn("MachiningTime", "BIGINT", o => o.Nullable(false))
                .AddDoubleColumn("SetupDowntimes", false)
                .AddDoubleColumn("MachiningDowntimes", false)
                .AddDoubleColumn("PartialSetupTime", false)
                .AddDoubleColumn("CreateNcProgramTime", false)
                .AddDoubleColumn("MaintenanceTime", false)
                .AddDoubleColumn("ToolSearchingTime", false)
                .AddDoubleColumn("ToolChangingTime", false)
                .AddDoubleColumn("MentoringTime", false)
                .AddDoubleColumn("ContactingDepartmentsTime", false)
                .AddDoubleColumn("FixtureMakingTime", false)
                .AddDoubleColumn("HardwareFailureTime", false)
                .AddStringColumn("OperatorComment", -1, false)
                .AddStringColumn("MasterSetupComment", -1)
                .AddStringColumn("MasterMachiningComment", -1)
                .AddStringColumn("SpecifiedDowntimesComment", -1)
                .AddStringColumn("UnspecifiedDowntimeComment", -1)
                .AddStringColumn("MasterComment", -1)
                .AddDoubleColumn("FixedSetupTimePlan")
                .AddDoubleColumn("FixedProductionTimePlan")
                .AddStringColumn("EngineerComment", -1)
                .AddBoolColumn("ExcludeFromReports")
                .AddStringColumn("LongSetupReasonComment", -1)
                .AddStringColumn("LongSetupFixComment", -1)
                .AddStringColumn("LongSetupEngeneerComment", -1)
                .AddDoubleColumn("ExcludedOperationsTime")
                .AddStringColumn("IncreaseReason", -1)
                .Build()
        };
    }
}
