using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class FileLockDetector
{
    #region Windows API

    [DllImport("ntdll.dll")]
    private static extern uint NtQuerySystemInformation(
        SystemInformationClass SystemInformationClass,
        IntPtr SystemInformation,
        uint SystemInformationLength,
        out uint ReturnLength);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(
        ProcessAccessFlags processAccess,
        bool bInheritHandle,
        int processId);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("ntdll.dll")]
    private static extern uint NtQueryInformationProcess(
        IntPtr processHandle,
        ProcessInformationClass processInformationClass,
        IntPtr processInformation,
        uint processInformationLength,
        out uint returnLength);

    [DllImport("ntdll.dll")]
    private static extern uint NtQueryObject(
        IntPtr Handle,
        ObjectInformationClass ObjectInformationClass,
        IntPtr ObjectInformation,
        uint ObjectInformationLength,
        out uint ReturnLength);

    [DllImport("kernel32.dll")]
    private static extern bool DuplicateHandle(
        IntPtr hSourceProcessHandle,
        IntPtr hSourceHandle,
        IntPtr hTargetProcessHandle,
        out IntPtr lpTargetHandle,
        uint dwDesiredAccess,
        bool bInheritHandle,
        uint dwOptions);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetFileInformationByHandle(
        IntPtr hFile,
        out BY_HANDLE_FILE_INFORMATION lpFileInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    #endregion

    #region Enums and Structures

    private enum SystemInformationClass
    {
        SystemHandleInformation = 16
    }

    private enum ProcessInformationClass
    {
        ProcessHandleInformation = 20
    }

    private enum ObjectInformationClass
    {
        ObjectNameInformation = 1,
        ObjectTypeInformation = 2
    }

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        QueryInformation = 0x0400,
        DuplicateHandle = 0x0040
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_HANDLE_INFORMATION
    {
        public uint NumberOfHandles;
        // Followed by NumberOfHandles * SYSTEM_HANDLE_ENTRY
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_HANDLE_ENTRY
    {
        public uint ProcessId;
        public byte ObjectType;
        public byte HandleFlags;
        public ushort HandleValue;
        public IntPtr ObjectPointer;
        public uint GrantedAccess;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    #endregion

    /// <summary>
    /// Определяет процесс, блокирующий указанный файл
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Процесс, блокирующий файл, или null если не найден</returns>
    public static Process GetProcessLockingFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            // Получаем информацию о файле для сравнения
            var targetFileInfo = GetFileInformation(filePath);
            if (targetFileInfo == null)
                return null;

            // Получаем все хэндлы в системе
            var handleInfos = GetSystemHandles();
            if (handleInfos == null)
                return null;

            var processIds = new HashSet<int>();

            // Ищем хэндлы, которые могут указывать на наш файл
            foreach (var handleInfo in handleInfos)
            {
                if (handleInfo.ObjectType != 0x1C) // FILE_TYPE (примерно)
                    continue;

                try
                {
                    // Открываем процесс
                    var processHandle = OpenProcess(
                        ProcessAccessFlags.QueryInformation | ProcessAccessFlags.DuplicateHandle,
                        false,
                        (int)handleInfo.ProcessId);

                    if (processHandle == IntPtr.Zero)
                        continue;

                    try
                    {
                        // Дублируем хэндл
                        if (DuplicateHandle(
                            processHandle,
                            new IntPtr(handleInfo.HandleValue),
                            GetCurrentProcess(),
                            out IntPtr duplicatedHandle,
                            0,
                            false,
                            0x2)) // DUPLICATE_SAME_ACCESS
                        {
                            try
                            {
                                // Получаем информацию о файле по хэндлу
                                if (GetFileInformationByHandle(duplicatedHandle, out BY_HANDLE_FILE_INFORMATION fileInfo))
                                {
                                    // Сравниваем файлы по индексу и серийному номеру тома
                                    if (targetFileInfo.Value.FileIndexHigh == fileInfo.FileIndexHigh &&
                                        targetFileInfo.Value.FileIndexLow == fileInfo.FileIndexLow &&
                                        targetFileInfo.Value.VolumeSerialNumber == fileInfo.VolumeSerialNumber)
                                    {
                                        processIds.Add((int)handleInfo.ProcessId);
                                    }
                                }
                            }
                            finally
                            {
                                CloseHandle(duplicatedHandle);
                            }
                        }
                    }
                    finally
                    {
                        CloseHandle(processHandle);
                    }
                }
                catch
                {
                    // Пропускаем процессы, к которым нет доступа
                    continue;
                }
            }

            // Возвращаем первый найденный процесс (исключая текущий)
            var currentProcessId = Process.GetCurrentProcess().Id;
            var lockingProcessId = processIds.FirstOrDefault(id => id != currentProcessId);

            if (lockingProcessId != 0)
            {
                try
                {
                    return Process.GetProcessById(lockingProcessId);
                }
                catch
                {
                    // Процесс мог завершиться
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при определении блокирующего процесса: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Альтернативный способ через попытку получить эксклюзивный доступ
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Список возможных процессов</returns>
    public static List<Process> GetPossibleLockingProcesses(string filePath)
    {
        var result = new List<Process>();

        try
        {
            if (!File.Exists(filePath))
                return result;

            var fileName = Path.GetFileName(filePath);
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // Проверяем имя процесса
                    if (process.ProcessName.Equals(Path.GetFileNameWithoutExtension(fileName),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(process);
                        continue;
                    }

                    // Проверяем главный модуль (если доступен)
                    try
                    {
                        if (process.MainModule?.FileName != null &&
                            process.MainModule.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(process);
                        }
                    }
                    catch
                    {
                        // Нет доступа к MainModule
                    }
                }
                catch
                {
                    // Пропускаем процессы, к которым нет доступа
                    continue;
                }
            }

            return result;
        }
        catch
        {
            return result;
        }
    }

    private static BY_HANDLE_FILE_INFORMATION? GetFileInformation(string filePath)
    {
        try
        {
            var handle = CreateFile(
                filePath,
                0, // No access needed
                0x7, // FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE
                IntPtr.Zero,
                3, // OPEN_EXISTING
                0,
                IntPtr.Zero);

            if (handle == new IntPtr(-1))
                return null;

            try
            {
                if (GetFileInformationByHandle(handle, out BY_HANDLE_FILE_INFORMATION fileInfo))
                {
                    return fileInfo;
                }
            }
            finally
            {
                CloseHandle(handle);
            }
        }
        catch
        {
            // Ignore
        }

        return null;
    }

    private static SYSTEM_HANDLE_ENTRY[] GetSystemHandles()
    {
        try
        {
            uint bufferSize = 0x10000;
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal((int)bufferSize);
                uint status;

                while ((status = NtQuerySystemInformation(
                    SystemInformationClass.SystemHandleInformation,
                    buffer,
                    bufferSize,
                    out uint returnLength)) == 0xC0000004) // STATUS_INFO_LENGTH_MISMATCH
                {
                    Marshal.FreeHGlobal(buffer);
                    bufferSize = returnLength;
                    buffer = Marshal.AllocHGlobal((int)bufferSize);
                }

                if (status != 0)
                    return null;

                var handleCount = (uint)Marshal.ReadInt32(buffer);
                var handleEntries = new SYSTEM_HANDLE_ENTRY[handleCount];

                var entrySize = Marshal.SizeOf<SYSTEM_HANDLE_ENTRY>();
                var currentPtr = buffer + 4;

                for (int i = 0; i < handleCount; i++)
                {
                    handleEntries[i] = Marshal.PtrToStructure<SYSTEM_HANDLE_ENTRY>(currentPtr);
                    currentPtr += entrySize;
                }

                return handleEntries;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }
        catch
        {
            return null;
        }
    }
}