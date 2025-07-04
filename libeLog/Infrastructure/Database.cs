using libeLog.Extensions;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Асинхронно получает список наименований серийных деталей из базы данных.
        /// </summary>
        /// <param name="connectionString">
        /// Необязательная строка подключения к базе данных. Если не указана, используется значение из <c>AppSettings.Instance.ConnectionString</c>.
        /// </param>
        /// <param name="progress">
        /// Необязательный объект для отслеживания прогресса выполнения. При наличии отправляются сообщения о ходе подключения, чтения и добавления деталей.
        /// </param>
        /// <returns>
        /// Асинхронная задача, содержащая список имён деталей, отсортированных по возрастанию.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Генерируется, если итоговая строка подключения пуста или содержит только пробелы.
        /// </exception>

        public async static Task<List<SerialPart>> GetSerialPartsAsync(string connectionString, IProgress<string>? progress = null)
        {
            List<SerialPart> parts = new();
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Строка подключения не может быть пустой");
            await Task.Run(async () =>
            {
                progress?.Report("Подключение к БД...");
                using (SqlConnection connection = new(connectionString))
                {
                    await connection.OpenAsync();
                    string query = $"SELECT Id, PartName, YearCount FROM cnc_serial_parts ORDER BY PartName ASC;";
                    using (SqlCommand command = new(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            progress?.Report("Чтение данных о деталях из БД...");
                            while (await reader.ReadAsync())
                            {
                                var part = new SerialPart(await reader.GetFieldValueAsync<int>(0), await reader.GetFieldValueAsync<string>(1), await reader.GetFieldValueAsync<int>(2));
                                parts.Add(part);
                                progress?.Report($"Добавление: {part.PartName}");
                            }
                        }
                    }
                }
                progress?.Report("Чтение завершено");
            });
            return parts;
        }
    }
}
