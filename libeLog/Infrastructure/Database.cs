﻿using libeLog.Models;
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
    }
}
