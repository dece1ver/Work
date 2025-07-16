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
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SetupLimit FROM cnc_machines WHERE Name = @Name";
                    using (SqlCommand command = new SqlCommand(query, connection))
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
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SetupCoefficient FROM cnc_machines WHERE Name = @Name";
                    using (SqlCommand command = new SqlCommand(query, connection))
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
            using (SqlConnection connection = new SqlConnection(connectionString))
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
                    var part = new SerialPart
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
                "SELECT CncSetupId, NormativeType, Value, EffectiveFrom " +
                "FROM cnc_normatives " +
                "ORDER BY EffectiveFrom", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var setupId = reader.GetInt32(0);
                    var typeRaw = reader.GetByte(1);
                    var value = reader.GetDouble(2);
                    DateTime ef = reader.GetDateTime(3);

                    var entry = new NormativeEntry
                    {
                        Type = (NormativeEntry.NormativeType)typeRaw,
                        Value = value,
                        EffectiveFrom = ef
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

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                progress?.Report("Сохраняем деталь...");
                if (part.Id == 0)
                {
                    using var cmd = new SqlCommand(
                        @"INSERT INTO cnc_serial_parts(PartName, YearCount)
                          VALUES(@name,@year);
                          SELECT SCOPE_IDENTITY();", conn, tx);
                    cmd.Parameters.AddWithValue("@name", part.PartName);
                    cmd.Parameters.AddWithValue("@year", part.YearCount);
                    part.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                else
                {
                    using var cmd = new SqlCommand(
                        @"UPDATE cnc_serial_parts
                          SET PartName=@name, YearCount=@year
                          WHERE Id=@id;", conn, tx);
                    cmd.Parameters.AddWithValue("@id", part.Id);
                    cmd.Parameters.AddWithValue("@name", part.PartName);
                    cmd.Parameters.AddWithValue("@year", part.YearCount);
                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var op in part.Operations)
                {
                    progress?.Report($"Сохраняем операцию «{op.Name}»...");
                    if (op.Id == 0)
                    {
                        using var cmd = new SqlCommand(
                            @"INSERT INTO cnc_operations(SerialPartId, Name)
                              VALUES(@pid,@name);
                              SELECT SCOPE_IDENTITY();", conn, tx);
                        cmd.Parameters.AddWithValue("@pid", part.Id);
                        cmd.Parameters.AddWithValue("@name", op.Name);
                        op.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }
                    else
                    {
                        using var cmd = new SqlCommand(
                            @"UPDATE cnc_operations
                              SET Name=@name
                              WHERE Id=@id;", conn, tx);
                        cmd.Parameters.AddWithValue("@id", op.Id);
                        cmd.Parameters.AddWithValue("@name", op.Name);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    foreach (var setup in op.Setups)
                    {
                        progress?.Report($"Сохраняем установку №{setup.Number}...");
                        if (setup.Id == 0)
                        {
                            using var cmd = new SqlCommand(
                                @"INSERT INTO cnc_setups(CncOperationId, Number)
                                  VALUES(@oid,@num);
                                  SELECT SCOPE_IDENTITY();", conn, tx);
                            cmd.Parameters.AddWithValue("@oid", op.Id);
                            cmd.Parameters.AddWithValue("@num", setup.Number);
                            setup.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                        else
                        {
                            using var cmd = new SqlCommand(
                                @"UPDATE cnc_setups
                                SET Number=@num
                                WHERE Id=@id;", conn, tx);
                            cmd.Parameters.AddWithValue("@id", setup.Id);
                            cmd.Parameters.AddWithValue("@num", setup.Number);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        foreach (var norm in setup.Normatives)
                        {
                            if (norm.Id == 0)
                            {
                                progress?.Report($"Добавляем норматив {norm.Type}={norm.Value}...");
                                using var cmd = new SqlCommand(
                                    @"INSERT INTO cnc_normatives(CncSetupId, NormativeType, Value, EffectiveFrom)
                                    VALUES(@sid,@type,@val,@ef);", conn, tx);
                                cmd.Parameters.AddWithValue("@sid", setup.Id);
                                cmd.Parameters.AddWithValue("@type", (byte)norm.Type);
                                cmd.Parameters.AddWithValue("@val", norm.Value);
                                cmd.Parameters.AddWithValue("@ef", norm.EffectiveFrom);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                tx.Commit();
                progress?.Report("Сохранение завершено");
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


    }
}
