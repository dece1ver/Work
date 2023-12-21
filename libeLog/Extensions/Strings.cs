using libeLog.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;

namespace libeLog.Extensions;

public static class Strings
{
    /// <summary>
    /// Капитализация строки (для имен, фамилий и тд)
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string Capitalize(this string source)
    {
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        var letters = source.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            if (i == 0)
            {
                letters[i] = char.ToUpper(letters[i]);
            }
            else
            {
                letters[i] = char.ToLower(letters[i]);
            }
        }
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }

    /// <summary>
    /// Парсит строку в TimeSpan
    /// </summary>
    /// <param name="input">Строка с вводом</param>
    /// <param name="time">Спарсеный промежуток времени</param>
    /// <returns></returns>
    public static bool TimeParse(this string input, out TimeSpan time)
    {
        if (string.IsNullOrEmpty(input))
        {
            time = TimeSpan.Zero;
            return false;
        }
        input = input.Replace(",", ".");
        if (input.Length > 0)
        {
            if (input.Count(x => x == ':') == 1)
            {
                var sTime = input.Split(':');
                if (double.TryParse(sTime[0], out var minutes) &&
                    double.TryParse(sTime[1], out var seconds))
                {
                    time = TimeSpan.FromSeconds(minutes * 60 + seconds);
                    return true;
                }
            }
            else if (input.Count(x => x == ':') == 2)
            {
                var sTime = input.Split(':');
                if (double.TryParse(sTime[0], out var hours) &&
                    double.TryParse(sTime[1], out var minutes) &&
                    double.TryParse(sTime[2], out var seconds))
                {
                    time = TimeSpan.FromSeconds(hours * 3600 + minutes * 60 + seconds);
                    return true;
                }
            }
            else
            {
                if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var minutes))
                {
                    time = TimeSpan.FromMinutes(minutes);
                    return true;
                }
                time = TimeSpan.Zero;
                return false;
            }
        }
        time = TimeSpan.Zero;
        return false;
    }

    /// <summary>
    /// Получает число из строки
    /// </summary>
    /// <param name="stringNumber">Строка для получения</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <param name="numberOption">Возвращаемое значение: Any для любого, OnlyPositive для положительных</param>
    /// <returns>Значение Double, при неудаче возвращает значение по умолчанию</returns>
    public static double GetDouble(this string stringNumber, double defaultValue = 0, GetNumberOption numberOption = GetNumberOption.OnlyPositive)
    {
        //if (stringNumber is "-") return double.NegativeInfinity;
        NumberFormatInfo numberFormat = new() { NumberDecimalSeparator = "," };
        if (!double.TryParse(stringNumber, NumberStyles.Any, numberFormat, out var result)) return defaultValue;
        return numberOption switch
        {
            GetNumberOption.OnlyPositive when result >= 0 => result,
            GetNumberOption.Any => result,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Получает число из строки
    /// </summary>
    /// <param name="stringNumber">Строка для получения</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    /// <param name="numberOption">Возвращаемое значение: только положительное или любое</param>
    /// <returns>Значение Int32, при неудаче возвращает значение по умолчанию</returns>
    public static int GetInt(this string stringNumber, int defaultValue = 0, GetNumberOption numberOption = GetNumberOption.OnlyPositive)
    {
        NumberFormatInfo numberFormat = new() { NumberDecimalSeparator = "," };
        if (!int.TryParse(stringNumber, NumberStyles.Any, numberFormat, out var result)) return defaultValue;
        if (numberOption == GetNumberOption.OnlyPositive && result > 0)
        {
            return result;
        }
        return defaultValue;
    }

    /// <summary>
    /// Проверяет наличие директории и права доступа к ней
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="requiredRights"></param>
    /// <returns>CheckDirectoryRightsResult</returns>
    public static CheckDirectoryRightsResult CheckDirectoryRights(this string directoryPath, FileSystemRights requiredRights)
    {
        if (string.IsNullOrEmpty(directoryPath)) return CheckDirectoryRightsResult.NotExists;
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        if (directoryInfo.Exists)
        {
            try
            {
                WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
                WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
                AuthorizationRuleCollection accessRules = directorySecurity.GetAccessRules(true, true, typeof(NTAccount));

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if (rule.AccessControlType == AccessControlType.Allow &&
                        (rule.FileSystemRights & requiredRights) != 0
                        && (currentPrincipal.IsInRole(rule.IdentityReference.Value) 
                        || rule.IdentityReference.Value == currentIdentity.Name))
                    {
                         return CheckDirectoryRightsResult.HasAccess;
                    }
                }

                return CheckDirectoryRightsResult.NoAccess;
            }
            catch
            {
                return CheckDirectoryRightsResult.Error;
            }
        }

        return CheckDirectoryRightsResult.NotExists;
    }

    public static (DbResult result, string message) CheckDbConnection(this string connectionString) 
    {
        if (string.IsNullOrEmpty(connectionString)) return (DbResult.Error, "Строка подключения не инициализирована");

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return (DbResult.Ok, "Ok");
            }
        }
        catch (SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                -1 => (DbResult.Error, Constants.StatusTips.NoConnectionToDb),
                18456 => (DbResult.AuthError, Constants.StatusTips.AuthFailedToDb),
                _ => (DbResult.Error, $"Ошибка БД №{sqlEx.Number}"),
            };
        }
        catch (Exception ex)
        {
            return (DbResult.Error, $"Ошибка: {ex.Message}");
        }
    }

}
