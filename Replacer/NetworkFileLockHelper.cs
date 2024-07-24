using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Replacer
{
    public static class NetworkFileLockHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct FILE_INFO_3
        {
            public uint fi3_id;
            public uint fi3_permissions;
            public uint fi3_num_locks;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_pathname;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_username;
        }

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetFileEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            [MarshalAs(UnmanagedType.LPWStr)] string basepath,
            [MarshalAs(UnmanagedType.LPWStr)] string username,
            int level,
            out IntPtr bufptr,
            uint prefmaxlen,
            out uint entriesread,
            out uint totalentries,
            IntPtr resume_handle);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private static extern int NetApiBufferFree(IntPtr Buffer);

        public static List<string> GetLockingUsers(string serverName, string filePath)
        {
            IntPtr buffer = IntPtr.Zero;
            uint entriesRead = 0;
            uint totalEntries = 0;

            List<string> lockingUsers = new List<string>();

            try
            {
                int result = NetFileEnum(serverName, filePath, null, 3, out buffer, uint.MaxValue, out entriesRead, out totalEntries, IntPtr.Zero);
                if (result == 0 && entriesRead > 0)
                {
                    FILE_INFO_3[] fileInfos = new FILE_INFO_3[entriesRead];
                    IntPtr iter = buffer;

                    for (uint i = 0; i < entriesRead; i++)
                    {
                        fileInfos[i] = (FILE_INFO_3)Marshal.PtrToStructure(iter, typeof(FILE_INFO_3));
                        iter = (IntPtr)((long)iter + Marshal.SizeOf(typeof(FILE_INFO_3)));
                        lockingUsers.Add(fileInfos[i].fi3_username);
                    }
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    NetApiBufferFree(buffer);
                }
            }

            return lockingUsers;
        }
    }
}
