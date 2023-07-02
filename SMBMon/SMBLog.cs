using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SMBLibrary;

namespace SMBMon
{
    

    public enum NTFileOperation
    {
        Any = -1,
        CreateFile,
        CloseFile,
        ReadFile,
        WriteFile,
        FlushFileBuffers,
        LockFile,
        UnlockFile,
        QueryDirectory,
        GetFileInformation,
        SetFileInformation,
        GetFileSystemInformation,
        SetFileSystemInformation,
        GetSecurityInformation,
        SetSecurityInformation,
        NotifyChange,
        Cancel,
        DeviceIOControl
    }

    public struct SMBLogEntry
    {
        public DateTime Time;
        public IntPtr Handle;
        public NTFileOperation Operation;
        public string Path;
        public NTStatus Result;
        public string Detail;
    }

    class SMBLog
    {
        public static List<SMBLogEntry> LogEntries;

        public static void InitLog()
        {
            LogEntries = new List<SMBLogEntry>();
        }

        public static void AddEntry(SMBLogEntry entry)
        {
            lock (LogEntries)
            {
                LogEntries.Add(entry);
                new Thread(() =>
               {
                   Program.MainForm.AddLogEntry(entry);
               }).Start();
            }
        }
    }
}
