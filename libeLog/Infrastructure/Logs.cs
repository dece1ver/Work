﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace libeLog.Infrastructure
{
    public static class Logs
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Проверяет размер файла логов, если размер превышает допустимый, то он уходит в бэкап.
        /// </summary>
        /// <param name="path"></param>
        public static void CheckLogSize(string path, string copyDir = "")
        {
            if (new FileInfo(path).Length > Constants.MaxLogSize)
            {
                var backupLog = path + $".bk{DateTime.Now:ddMMyy}";
                File.Move(path, backupLog);
                TryCopyBackup(backupLog, copyDir);
            }
        }

        /// <summary>
        /// Записывает информацию об исключении в лог.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="additionMessage"></param>
        public static async Task Write(string path, Exception exception, string additionMessage = "", string copyDir = "")
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (File.Exists(path)) CheckLogSize(path);
                    await File.AppendAllTextAsync(path, $"[{DateTime.Now.ToString(Constants.DateTimeWithSecsFormat)}]: " +
                                                            $"{(string.IsNullOrEmpty(additionMessage) ? string.Empty : $"{additionMessage}\n")}" +
                                                            $"{exception.Message}{(exception.TargetSite is null ? string.Empty : $"\n\tCaller: {exception.TargetSite}")}\n" +
                                                            $"{exception.GetType()}\n" +
                                                            $"{exception.StackTrace}\n\n");
                    if (!string.IsNullOrWhiteSpace(copyDir)) await Task.Run(() => TryCopyLog(path, copyDir));
                    semaphore.Release();
                    return;
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        MessageBox.Show($"Отправь этот текст разработчику:\n{e.GetBaseException()}", $"{e.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
                        semaphore.Release();
                        return;
                    }
                    await Task.Delay(1000);
                }
            }
        }


        /// <summary>
        /// Записывает сообщение в лог.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="copyDir">Папка для копирования логов</param>
        public static async Task Write(string path, string message, string copyDir = "")
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (File.Exists(path)) CheckLogSize(path);
                    await File.AppendAllTextAsync(path, $"[{DateTime.Now.ToString(Constants.DateTimeWithSecsFormat)}]: {message}\n\n");
                    if (!string.IsNullOrWhiteSpace(copyDir)) await Task.Run(() => TryCopyLog(path, copyDir));
                    semaphore.Release();
                    return;
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        MessageBox.Show($"Покажи этот текст разработчику:\n{e.GetBaseException()}", $"{e.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
                        semaphore.Release();
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }
        }


        public static void TryCopyLog(string src, string dest)
        {
            try
            {
                if (!Directory.Exists(dest)) return;
                var logsPath = Path.Combine(dest, "logs");
                if (!Directory.Exists(logsPath)) { Directory.CreateDirectory(logsPath); }
                File.Copy(src, Path.Combine(logsPath, $"{Environment.UserName}.log"), true);
            }
            catch { }
        }

        public static void TryCopyBackup(string src, string dest)
        {
            try
            {
                if (!Directory.Exists(dest)) return;
                var logsPath = Path.Combine(dest, "logs");
                if (!Directory.Exists(logsPath)) { Directory.CreateDirectory(logsPath); }
                File.Copy(src, Path.Combine(logsPath, $"{Environment.UserName}.log.bk{DateTime.Now:ddMMyy}"), true);
            }
            catch { }
        }
    }
}
