using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.WinApi.Windows
{
    public static class FileLockerDetector
    {
        public enum SystemInformationClass
        {
            SystemHandleInformation = 16
        }

        public enum ProcessInformationClass
        {
            ProcessHandleInformation = 20
        }

        public enum ObjectInformationClass
        {
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            QueryInformation = 0x0400,
            DuplicateHandle = 0x0040
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_HANDLE_INFORMATION
        {
            public uint NumberOfHandles;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_HANDLE_ENTRY
        {
            public uint ProcessId;
            public byte ObjectType;
            public byte HandleFlags;
            public ushort HandleValue;
            public IntPtr ObjectPointer;
            public uint GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BY_HANDLE_FILE_INFORMATION
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
    }
}
