using libeLog.Extensions;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Infrastructure
{
    public static class Database
    {
        /// <summary>
        /// Получает лимит наладки для заданного станка из базы данных по переданной строке подключения.
        /// </summary>
        /// <param name="machine">Имя станка для получения лимита наладки.</param>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        /// <returns>
        /// Кортеж, состоящий из:
        /// - Result: результат выполнения запроса <see cref="DbResult"/>.
        /// - SetupCoefficient: коэффициент наладки для машины (nullable double), может быть null, если данных нет.
        /// - Error: строка с описанием ошибки, если она произошла.
        /// </returns>
        /// <exception cref="SqlException">Выбрасывается при ошибках взаимодействия с базой данных, например, ошибки авторизации.</exception>
        /// <exception cref="Exception">Общие ошибки, которые могут возникнуть при выполнении запроса.</exception>
        public static (DbResult Result, int? SetupLimit, string error) GetMachineSetupLimit(this string machine, string connectionString)
        {
            try
            {
                using (SqlConnection connection = new(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SetupLimit FROM cnc_machines WHERE Name = @Name";
                    using (SqlCommand command = new(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", machine);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int setupLimit = reader.GetInt32(0);
                                return (DbResult.Ok, setupLimit, "OK");
                            }
                        }
                    }
                }

                return (DbResult.NotFound, null, "NOT FOUND");
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 18456:
                        return (DbResult.AuthError, null, sqlEx.Number.ToString());
                    default:
                        return (DbResult.Error, null, sqlEx.Number.ToString());
                }
            }
            catch (Exception ex)
            {
                return (DbResult.Error, null, ex.Message);
            }
        }

        /// <summary>
        /// Получает коэффициент наладки для заданного станка из базы данных по переданной строке подключения.
        /// </summary>
        /// <param name="machine">Имя станка для получения коэффициента наладки.</param>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        /// <returns>
        /// Кортеж, состоящий из:
        /// - Result: результат выполнения запроса <see cref="DbResult"/>.
        /// - SetupCoefficient: коэффициент наладки для машины (nullable double), может быть null, если данных нет.
        /// - Error: строка с описанием ошибки, если она произошла.
        /// </returns>
        /// <exception cref="SqlException">Выбрасывается при ошибках взаимодействия с базой данных, например, ошибки авторизации.</exception>
        /// <exception cref="Exception">Общие ошибки, которые могут возникнуть при выполнении запроса.</exception>
        public static (DbResult Result, double? SetupCoefficient, string error) GetMachineSetupCoefficient(this string machine, string connectionString)
        {
            try
            {
                using (SqlConnection connection = new(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SetupCoefficient FROM cnc_machines WHERE Name = @Name";
                    using (SqlCommand command = new(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", machine);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                double setupCoefficient = reader.GetDouble(0);
                                return (DbResult.Ok, setupCoefficient, "OK");
                            }
                        }
                    }
                }

                return (DbResult.NotFound, null, "NOT FOUND");
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 18456:
                        return (DbResult.AuthError, null, sqlEx.Number.ToString());
                    default:
                        return (DbResult.Error, null, sqlEx.Number.ToString());
                }
            }
            catch (Exception ex)
            {
                return (DbResult.Error, null, ex.Message);
            }
        }

        /// <summary>
        /// Асинхронно получает конфигурационные параметры для подключения к API Winnum из таблицы <c>cnc_winnum_cfg</c>.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных SQL Server.</param>
        /// <returns>
        /// Кортеж из трёх строк: <c>BaseUri</c> — базовый адрес API, <c>User</c> — имя пользователя, <c>Pass</c> — пароль.
        /// Если строка не найдена, возвращается кортеж по умолчанию <c>(null, null, null)</c>.
        /// </returns>
        /// <remarks>
        /// Ожидается, что таблица <c>cnc_winnum_cfg</c> содержит не более одной строки с параметрами конфигурации.
        /// Значения, отсутствующие в БД, заменяются на пустую строку.
        /// </remarks>
        public static async Task<(string BaseUri, string User, string Pass, string NcProgramFolder)> GetWinnumConfigAsync(string connectionString)
        {
            using (SqlConnection connection = new(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT [BaseUri], [User], [Pass], [NcProgramFolder] FROM cnc_winnum_cfg";
                using (SqlCommand command = new(query, connection))
                {

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var baseUri = await reader.GetValueOrDefaultAsync(0, "");
                            var user = await reader.GetValueOrDefaultAsync(1, "");
                            var pass = await reader.GetValueOrDefaultAsync(2, "");
                            var ncProgramFolder = await reader.GetValueOrDefaultAsync(3, "");
                            return (baseUri, user, pass, ncProgramFolder);
                        }
                    }
                    return default;
                }
            }
        }

        public static async Task<List<SerialPart>> GetSerialPartsAsync(
            string connectionString,
            IProgress<string>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Строка подключения не может быть пустой", nameof(connectionString));

            progress?.Report("Открываем соединение с БД...");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Словари для быстрого поиска по Id
            var partsById = new Dictionary<int, SerialPart>();
            var opsById = new Dictionary<int, CncOperation>();
            var setupsById = new Dictionary<int, CncSetup>();

            var result = new List<SerialPart>();

            // 1) Загрузка деталей
            progress?.Report("Читаем детали...");
            using (var cmd = new SqlCommand("SELECT Id, PartName, YearCount FROM cnc_serial_parts ORDER BY PartName", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var part = new SerialPart()
                    {
                        Id = reader.GetInt32(0),
                        PartName = reader.GetString(1),
                        YearCount = reader.GetInt32(2),
                        Operations = new ObservableCollection<CncOperation>()
                    };
                    partsById[part.Id] = part;
                    result.Add(part);
                }
            }

            // 2) Загрузка операций
            progress?.Report("Читаем операции...");
            using (var cmd = new SqlCommand("SELECT Id, SerialPartId, Name FROM cnc_operations ORDER BY Name", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var partId = reader.GetInt32(1);
                    var name = reader.GetString(2);

                    var op = new CncOperation(name)
                    {
                        Id = id,
                        Setups = new ObservableCollection<CncSetup>()
                    };
                    opsById[id] = op;

                    if (partsById.TryGetValue(partId, out var part))
                        part.Operations.Add(op);
                }
            }

            // 3) Загрузка установок
            progress?.Report("Читаем установки...");
            using (var cmd = new SqlCommand("SELECT Id, CncOperationId, Number FROM cnc_setups ORDER BY Number", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var opId = reader.GetInt32(1);
                    var number = reader.GetByte(2);

                    var setup = new CncSetup
                    {
                        Id = id,
                        Number = number,
                        Normatives = new ObservableCollection<NormativeEntry>()
                    };
                    setupsById[id] = setup;

                    if (opsById.TryGetValue(opId, out var op))
                        op.Setups.Add(setup);
                }
            }

            // 4) Загрузка нормативов
            progress?.Report("Читаем нормативы...");
            using (var cmd = new SqlCommand(
                "SELECT Id, CncSetupId, NormativeType, Value, EffectiveFrom, IsAproved " +
                "FROM cnc_normatives " +
                "ORDER BY EffectiveFrom", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var setupId = reader.GetInt32(1);
                    var typeRaw = reader.GetByte(2);
                    var value = reader.GetDouble(3);
                    var ef = reader.GetDateTime(4);
                    var apr = reader.GetBoolean(5);

                    var entry = new NormativeEntry
                    {
                        Id = id,
                        Type = (NormativeEntry.NormativeType)typeRaw,
                        Value = value,
                        EffectiveFrom = ef,
                        IsApproved = apr,
                    };

                    if (setupsById.TryGetValue(setupId, out var setup))
                        setup.Normatives.Add(entry);
                }
            }

            progress?.Report($"Загрузка завершена: деталей={result.Count}");
            return result;
        }

        public static async Task SaveSerialPartAsync(
            SerialPart part,
            string connectionString,
            IProgress<string>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Пустая строка подключения", nameof(connectionString));

            if (part == null)
                throw new ArgumentNullException(nameof(part));

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                progress?.Report($"Сохраняем деталь {part.PartName}");

                // 1. INSERT или UPDATE самой детали
                if (part.Id == 0)
                {
                    using var cmd = new SqlCommand(@"
                INSERT INTO cnc_serial_parts(PartName, YearCount)
                VALUES(@name, @year);
                SELECT SCOPE_IDENTITY();", conn, tx);
                    cmd.Parameters.AddWithValue("@name", part.PartName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@year", part.YearCount);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                        throw new InvalidOperationException("Не удалось получить ID новой детали");

                    part.Id = Convert.ToInt32(result);
                }
                else
                {
                    using var cmd = new SqlCommand(@"
                UPDATE cnc_serial_parts
                SET PartName=@name, YearCount=@year
                WHERE Id=@id;", conn, tx);
                    cmd.Parameters.AddWithValue("@id", part.Id);
                    cmd.Parameters.AddWithValue("@name", part.PartName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@year", part.YearCount);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        throw new InvalidOperationException($"Деталь с ID {part.Id} не найдена для обновления");
                }

                // Только для обновления существующей детали — удаляем из БД всё, чего нет в модели
                if (part.Id != 0)
                {
                    // 2.1. Определяем удалённые операции
                    var dbOpIds = new List<int>();
                    using (var cmd = new SqlCommand("SELECT Id FROM cnc_operations WHERE SerialPartId=@pid", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@pid", part.Id);
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                            dbOpIds.Add(rdr.GetInt32(0));
                    }

                    var keepOpIds = part.Operations?.Select(o => o.Id).Where(id => id != 0).ToList() ?? new List<int>();
                    var delOpIds = dbOpIds.Except(keepOpIds).ToList();

                    if (delOpIds.Any())
                    {
                        // 2.1.1. Удаляем нормативы у установок удалённых операций
                        await DeleteByIds(conn, tx, @"
                            DELETE N
                            FROM cnc_normatives N
                            JOIN cnc_setups S ON N.CncSetupId = S.Id
                            WHERE S.CncOperationId IN ({0})", delOpIds);

                        // 2.1.2. Удаляем сами установки
                        await DeleteByIds(conn, tx, @"
                            DELETE FROM cnc_setups
                            WHERE CncOperationId IN ({0})", delOpIds);

                        // 2.1.3. Удаляем операции
                        await DeleteByIds(conn, tx, @"
                            DELETE FROM cnc_operations
                            WHERE Id IN ({0})", delOpIds);
                    }

                    // 2.2. Для каждой оставшейся операции — удалить из БД те установки, которых нет в модели
                    if (part.Operations != null)
                    {
                        foreach (var op in part.Operations.Where(o => o.Id != 0))
                        {
                            var dbSetupIds = new List<int>();
                            using (var cmd = new SqlCommand("SELECT Id FROM cnc_setups WHERE CncOperationId=@oid", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@oid", op.Id);
                                using var rdr = await cmd.ExecuteReaderAsync();
                                while (await rdr.ReadAsync())
                                    dbSetupIds.Add(rdr.GetInt32(0));
                            }

                            var keepSetupIds = op.Setups?.Select(s => s.Id).Where(id => id != 0).ToList() ?? new List<int>();
                            var delSetupIds = dbSetupIds.Except(keepSetupIds).ToList();

                            if (delSetupIds.Any())
                            {
                                // 2.2.1. Удаляем нормативы удалённых установок
                                await DeleteByIds(conn, tx, @"
                            DELETE FROM cnc_normatives
                            WHERE CncSetupId IN ({0})", delSetupIds);

                                // 2.2.2. Удаляем установки
                                await DeleteByIds(conn, tx, @"
                            DELETE FROM cnc_setups
                            WHERE Id IN ({0})", delSetupIds);
                            }

                            // 2.3. Для каждой оставшейся установки — удалить нормативы, которых нет в модели
                            if (op.Setups != null)
                            {
                                foreach (var setup in op.Setups.Where(s => s.Id != 0))
                                {
                                    var dbNormIds = new List<int>();
                                    using (var cmd = new SqlCommand("SELECT Id FROM cnc_normatives WHERE CncSetupId=@sid", conn, tx))
                                    {
                                        cmd.Parameters.AddWithValue("@sid", setup.Id);
                                        using var rdr = await cmd.ExecuteReaderAsync();
                                        while (await rdr.ReadAsync())
                                            dbNormIds.Add(rdr.GetInt32(0));
                                    }

                                    var keepNormIds = setup.Normatives?.Select(n => n.Id).Where(id => id != 0).ToList() ?? new List<int>();
                                    var delNormIds = dbNormIds.Except(keepNormIds).ToList();

                                    if (delNormIds.Any())
                                    {
                                        await DeleteByIds(conn, tx, @"
                                    DELETE FROM cnc_normatives
                                    WHERE Id IN ({0})", delNormIds);
                                    }
                                }
                            }
                        }
                    }
                }

                // 3. INSERT / UPDATE оставшихся операций, установок и нормативов
                if (part.Operations != null)
                {
                    foreach (var op in part.Operations)
                    {
                        progress?.Report($"Сохраняем операцию «{op.Name}»...");

                        if (op.Id == 0)
                        {
                            using var cmd = new SqlCommand(@"
                        INSERT INTO cnc_operations(SerialPartId, Name, OrderIndex)
                        VALUES(@pid, @name, @oid);
                        SELECT SCOPE_IDENTITY();", conn, tx);
                            cmd.Parameters.AddWithValue("@pid", part.Id);
                            cmd.Parameters.AddWithValue("@name", op.Name ?? string.Empty);
                            cmd.Parameters.AddWithValue("@oid", op.OrderIndex);

                            var result = await cmd.ExecuteScalarAsync();
                            if (result == null || result == DBNull.Value)
                                throw new InvalidOperationException($"Не удалось получить ID новой операции {op.Name}");

                            op.Id = Convert.ToInt32(result);
                        }
                        else
                        {
                            using var cmd = new SqlCommand(@"
                        UPDATE cnc_operations
                        SET Name=@name, OrderIndex=@oid 
                        WHERE Id=@id;", conn, tx);
                            cmd.Parameters.AddWithValue("@id", op.Id);
                            cmd.Parameters.AddWithValue("@name", op.Name ?? string.Empty);
                            cmd.Parameters.AddWithValue("@oid", op.OrderIndex);

                            var rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0)
                                throw new InvalidOperationException($"Операция с ID {op.Id} не найдена для обновления");
                        }

                        if (op.Setups != null)
                        {
                            foreach (var setup in op.Setups)
                            {
                                progress?.Report($"Сохраняем установку №{setup.Number}...");

                                if (setup.Id == 0)
                                {
                                    using var cmd = new SqlCommand(@"
                                INSERT INTO cnc_setups(CncOperationId, Number)
                                VALUES(@oid, @num);
                                SELECT SCOPE_IDENTITY();", conn, tx);
                                    cmd.Parameters.AddWithValue("@oid", op.Id);
                                    cmd.Parameters.AddWithValue("@num", setup.Number);

                                    var result = await cmd.ExecuteScalarAsync();
                                    if (result == null || result == DBNull.Value)
                                        throw new InvalidOperationException($"Не удалось получить ID новой установки №{setup.Number}");

                                    setup.Id = Convert.ToInt32(result);
                                }
                                else
                                {
                                    using var cmd = new SqlCommand(@"
                                UPDATE cnc_setups
                                SET Number=@num
                                WHERE Id=@id;", conn, tx);
                                    cmd.Parameters.AddWithValue("@id", setup.Id);
                                    cmd.Parameters.AddWithValue("@num", setup.Number);

                                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                                    if (rowsAffected == 0)
                                        throw new InvalidOperationException($"Установка с ID {setup.Id} не найдена для обновления");
                                }

                                // Добавляем только новые нормативы (история не изменяется)
                                if (setup.Normatives != null)
                                {
                                    foreach (var norm in setup.Normatives.Where(n => n.Id == 0))
                                    {
                                        progress?.Report($"Добавляем норматив {norm.Type}={norm.Value}...");

                                        using var cmd = new SqlCommand(@"
                                    INSERT INTO cnc_normatives(CncSetupId, NormativeType, Value, EffectiveFrom, IsAproved)
                                    VALUES(@sid, @type, @val, @ef, @apr);", conn, tx);
                                        cmd.Parameters.AddWithValue("@sid", setup.Id);
                                        cmd.Parameters.AddWithValue("@type", (byte)norm.Type);
                                        cmd.Parameters.AddWithValue("@val", norm.Value);
                                        cmd.Parameters.AddWithValue("@ef", norm.EffectiveFrom);
                                        cmd.Parameters.AddWithValue("@apr", norm.IsApproved);

                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                        }
                    }
                }

                await tx.CommitAsync();
                progress?.Report("Сохранение завершено");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // Вспомогательный метод для безопасного удаления по списку ID
        private static async Task DeleteByIds(SqlConnection conn, SqlTransaction tx, string sqlTemplate, List<int> ids)
        {
            if (!ids.Any()) return;

            // Проверяем, что все ID действительно числовые (дополнительная защита)
            if (ids.Any(id => id <= 0))
                throw new ArgumentException("Найдены некорректные ID для удаления");

            var parameters = new string[ids.Count];
            using var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.Transaction = tx;

            for (int i = 0; i < ids.Count; i++)
            {
                var paramName = $"@id{i}";
                parameters[i] = paramName;
                cmd.Parameters.AddWithValue(paramName, ids[i]);
            }

            cmd.CommandText = string.Format(sqlTemplate, string.Join(",", parameters));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
