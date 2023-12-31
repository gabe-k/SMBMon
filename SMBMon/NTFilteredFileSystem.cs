﻿// This is forked from SMBLibrary.Win32.NTDirectoryFileSystem
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMBLibrary;
using SMBLibrary.Win32;
using Utilities;
using System.Threading;

namespace SMBMon
{
    public interface ICallInfo
    {
        NTFileOperation Operation();
        bool Log { get; set; }
    }

    internal class PendingRequest
    {
        public IntPtr FileHandle;
        public uint ThreadID;
        public IO_STATUS_BLOCK IOStatusBlock;
        public bool Cleanup;
    }

    public class NTFilteredFileSystem : INTFileStore
    {
        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtCreateFile(out IntPtr handle, uint desiredAccess, ref OBJECT_ATTRIBUTES objectAttributes, out IO_STATUS_BLOCK ioStatusBlock, ref long allocationSize, SMBLibrary.FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, IntPtr eaBuffer, uint eaLength);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtClose(IntPtr handle);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtReadFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, byte[] buffer, uint length, ref long byteOffset, IntPtr key);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtWriteFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, byte[] buffer, uint length, ref long byteOffset, IntPtr key);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtFlushBuffersFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtLockFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, ref long byteOffset, ref long length, uint key, bool failImmediately, bool exclusiveLock);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtUnlockFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock, ref long byteOffset, ref long length, uint key);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtQueryDirectoryFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, byte[] fileInformation, uint length, uint fileInformationClass, bool returnSingleEntry, ref UNICODE_STRING fileName, bool restartScan);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtQueryInformationFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock, byte[] fileInformation, uint length, uint fileInformationClass);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtSetInformationFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock, byte[] fileInformation, uint length, uint fileInformationClass);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtQueryVolumeInformationFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock, byte[] fsInformation, uint length, uint fsInformationClass);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtSetVolumeInformationFile(IntPtr handle, out IO_STATUS_BLOCK ioStatusBlock, byte[] fsInformation, uint length, uint fsInformationClass);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtQuerySecurityObject(IntPtr handle, SecurityInformation securityInformation, byte[] securityDescriptor, uint length, out uint lengthNeeded);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtSetSecurityObject(IntPtr handle, SecurityInformation securityInformation, byte[] securityDescriptor);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtNotifyChangeDirectoryFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, byte[] buffer, uint bufferSize, NotifyChangeFilter completionFilter, bool watchTree);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtFsControlFile(IntPtr handle, IntPtr evt, IntPtr apcRoutine, IntPtr apcContext, out IO_STATUS_BLOCK ioStatusBlock, uint ioControlCode, byte[] inputBuffer, uint inputBufferLength, byte[] outputBuffer, uint outputBufferLength);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtAlertThread(IntPtr threadHandle);

        // Available starting from Windows Vista.
        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        private static extern NTStatus NtCancelSynchronousIoFile(IntPtr threadHandle, IntPtr ioRequestToCancel, out IO_STATUS_BLOCK ioStatusBlock);

        private static readonly int QueryDirectoryBufferSize = 4096;
        private static readonly int FileInformationBufferSize = 8192;
        private static readonly int FileSystemInformationBufferSize = 4096;

        private DirectoryInfo m_directory;
        private PendingRequestCollection m_pendingRequests = new PendingRequestCollection();
        private Dictionary<IntPtr, string> m_handlePathDict = new Dictionary<IntPtr, string>();
        private List<SMBFilter> m_filters = new List<SMBFilter>();


        public NTFilteredFileSystem(string path) : this(new DirectoryInfo(path))
        {
        }

        public NTFilteredFileSystem(DirectoryInfo directory)
        {
            m_directory = directory;
        }

        public void AddFilter(SMBFilter filter)
        {
            m_filters.Add(filter);
        }

        public List<SMBFilter> GetFilters()
        {
            return m_filters;
        }

        private OBJECT_ATTRIBUTES InitializeObjectAttributes(UNICODE_STRING objectName)
        {
            OBJECT_ATTRIBUTES objectAttributes = new OBJECT_ATTRIBUTES();
            objectAttributes.RootDirectory = IntPtr.Zero;
            objectAttributes.ObjectName = Marshal.AllocHGlobal(Marshal.SizeOf(objectName));
            Marshal.StructureToPtr(objectName, objectAttributes.ObjectName, false);
            objectAttributes.SecurityDescriptor = IntPtr.Zero;
            objectAttributes.SecurityQualityOfService = IntPtr.Zero;

            objectAttributes.Length = Marshal.SizeOf(objectAttributes);
            return objectAttributes;
        }

        public class CreateFileInfo : ICallInfo
        {
            public string Path;
            public AccessMask DesiredAccess;
            public SMBLibrary.FileAttributes FileAttributes;
            public ShareAccess ShareAccess;
            public CreateDisposition CreateDisposition;
            public CreateOptions CreateOptions;

            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.CreateFile;
            }
        }
        private NTStatus CreateFile(out IntPtr handle, out FileStatus fileStatus, string nativePath, AccessMask desiredAccess, long allocationSize, SMBLibrary.FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions)
        {
            //bool writeLog = false;
            CreateFileInfo curParams = new CreateFileInfo() { Path = nativePath, DesiredAccess = desiredAccess, FileAttributes = fileAttributes, ShareAccess = shareAccess, CreateDisposition = createDisposition, CreateOptions = createOptions };
            // apply filters
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.CreateFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            UNICODE_STRING objectName = new UNICODE_STRING(nativePath);
            OBJECT_ATTRIBUTES objectAttributes = InitializeObjectAttributes(objectName);
            IO_STATUS_BLOCK ioStatusBlock;
            NTStatus status = NtCreateFile(out handle, (uint)desiredAccess, ref objectAttributes, out ioStatusBlock, ref allocationSize, fileAttributes, shareAccess, createDisposition, createOptions, IntPtr.Zero, 0);
            fileStatus = (FileStatus)ioStatusBlock.Information;


            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = handle,
                    Operation = NTFileOperation.CreateFile,
                    Path = nativePath,
                    Result = status,
                    Detail = string.Format("Desired Access: {0:X} File Attributes {1:X} Share Access: {2:X} Create Disposition: {3:X} Create Options: {4:X}", desiredAccess, fileAttributes, shareAccess, createDisposition, createOptions)
                };
                SMBLog.AddEntry(entry);
            }
            m_handlePathDict[handle] = nativePath;
            return status;
        }

        private string ToNativePath(string path)
        {
            if (!path.StartsWith(@"\"))
            {
                path = @"\" + path;
            }
            return @"\??\" + m_directory.FullName + path;
        }

        public NTStatus CreateFile(out object handle, out FileStatus fileStatus, string path, AccessMask desiredAccess, SMBLibrary.FileAttributes fileAttributes, ShareAccess shareAccess, CreateDisposition createDisposition, CreateOptions createOptions, SecurityContext securityContext)
        {
            IntPtr fileHandle;
            string nativePath = ToNativePath(path);
            // NtQueryDirectoryFile will return STATUS_PENDING if the directory handle was not opened with SYNCHRONIZE and FILE_SYNCHRONOUS_IO_ALERT or FILE_SYNCHRONOUS_IO_NONALERT.
            // Our usage of NtNotifyChangeDirectoryFile assumes the directory handle is opened with SYNCHRONIZE and FILE_SYNCHRONOUS_IO_ALERT (or FILE_SYNCHRONOUS_IO_NONALERT starting from Windows Vista).
            // Note: Sometimes a directory will be opened without specifying FILE_DIRECTORY_FILE.
            desiredAccess |= AccessMask.SYNCHRONIZE;
            createOptions &= ~CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT;
            createOptions |= CreateOptions.FILE_SYNCHRONOUS_IO_ALERT;

            if ((createOptions & CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING) > 0 &&
                ((FileAccessMask)desiredAccess & FileAccessMask.FILE_APPEND_DATA) > 0)
            {
                // FILE_NO_INTERMEDIATE_BUFFERING is incompatible with FILE_APPEND_DATA
                // [MS-SMB2] 3.3.5.9 suggests setting FILE_APPEND_DATA to zero in this case.
                desiredAccess = (AccessMask)((uint)desiredAccess & (uint)~FileAccessMask.FILE_APPEND_DATA);
            }

            NTStatus status = CreateFile(out fileHandle, out fileStatus, nativePath, desiredAccess, 0, fileAttributes, shareAccess, createDisposition, createOptions);
            handle = fileHandle;
            return status;
        }

        public class CloseFileInfo : ICallInfo
        {
            public string Path;

            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.CloseFile;
            }
        }
        public NTStatus CloseFile(object handle)
        {
            // [MS-FSA] 2.1.5.4 The close operation has to complete any pending ChangeNotify request with STATUS_NOTIFY_CLEANUP.
            // - When closing a synchronous handle we must explicitly cancel any pending ChangeNotify request, otherwise the call to NtClose will hang.
            //   We use request.Cleanup to tell that we should complete such ChangeNotify request with STATUS_NOTIFY_CLEANUP.
            // - When closing an asynchronous handle Windows will implicitly complete any pending ChangeNotify request with STATUS_NOTIFY_CLEANUP as required.
            List<PendingRequest> pendingRequests = m_pendingRequests.GetRequestsByHandle((IntPtr)handle);
            foreach (PendingRequest request in pendingRequests)
            {
                request.Cleanup = true;
                Cancel(request);
            }

            CloseFileInfo curParams = new CloseFileInfo() { Path = m_handlePathDict[(IntPtr)handle], Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.CloseFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }
            NTStatus status = NtClose((IntPtr)handle);

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.CloseFile,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = "",
                };
                SMBLog.AddEntry(entry);
            }
            return status;
        }

        public class ReadFileInfo : ICallInfo
        {
            public string Path;
            public long Offset;
            public int Length;

            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.ReadFile;
            }
        }
        public NTStatus ReadFile(out byte[] data, object handle, long offset, int maxCount)
        {
            IO_STATUS_BLOCK ioStatusBlock;

            ReadFileInfo curParams = new ReadFileInfo() { Path = m_handlePathDict[(IntPtr)handle], Offset = offset, Length = maxCount, Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.ReadFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            data = new byte[maxCount];
            NTStatus status = NtReadFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ioStatusBlock, data, (uint)maxCount, ref offset, IntPtr.Zero);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                int bytesRead = (int)ioStatusBlock.Information;
                if (bytesRead < maxCount)
                {
                    data = ByteReader.ReadBytes(data, 0, bytesRead);
                }
            }

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.ReadFile,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = string.Format("Offset: {0:X} Length: {1:X}", offset, maxCount)
                };
                SMBLog.AddEntry(entry);
            }
            return status;
        }

        public class WriteFileInfo : ICallInfo
        {
            public string Path;
            public long Offset;
            public int Length;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.WriteFile;
            }
        }
        public NTStatus WriteFile(out int numberOfBytesWritten, object handle, long offset, byte[] data)
        {
            WriteFileInfo curParams = new WriteFileInfo() { Path = m_handlePathDict[(IntPtr)handle], Offset = offset, Length = data.Length, Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.WriteFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }


            IO_STATUS_BLOCK ioStatusBlock;
            NTStatus status = NtWriteFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ioStatusBlock, data, (uint)data.Length, ref offset, IntPtr.Zero);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                numberOfBytesWritten = (int)ioStatusBlock.Information;
            }
            else
            {
                numberOfBytesWritten = 0;
            }

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.WriteFile,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = string.Format("Offset: {0:X} Length: {1:X}", offset, numberOfBytesWritten)
                };
                SMBLog.AddEntry(entry);
            }

            return status;
        }

        public class FlushFileBuffersInfo : ICallInfo
        {
            public string Path;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.FlushFileBuffers;
            }
        }
        public NTStatus FlushFileBuffers(object handle)
        {
            FlushFileBuffersInfo curParams = new FlushFileBuffersInfo() { Path = m_handlePathDict[(IntPtr)handle], Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.FlushFileBuffers || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            IO_STATUS_BLOCK ioStatusBlock;
            NTStatus status = NtFlushBuffersFile((IntPtr)handle, out ioStatusBlock);

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.FlushFileBuffers,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = ""
                };
                SMBLog.AddEntry(entry);
            }
            return status;
        }

        public class LockFileInfo : ICallInfo
        {
            public string Path;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.LockFile;
            }
        }
        public NTStatus LockFile(object handle, long byteOffset, long length, bool exclusiveLock)
        {
            LockFileInfo curParams = new LockFileInfo() { Path = m_handlePathDict[(IntPtr)handle], Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.LockFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            IO_STATUS_BLOCK ioStatusBlock;
            NTStatus status = NtLockFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ioStatusBlock, ref byteOffset, ref length, 0, true, exclusiveLock);

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.LockFile,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = ""
                };
                SMBLog.AddEntry(entry);
            }

            return status;
        }

        public class UnlockFileInfo : ICallInfo
        {
            public string Path;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.UnlockFile;
            }
        }
        public NTStatus UnlockFile(object handle, long byteOffset, long length)
        {
            UnlockFileInfo curParams = new UnlockFileInfo() { Path = m_handlePathDict[(IntPtr)handle], Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.UnlockFile || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            IO_STATUS_BLOCK ioStatusBlock;
            NTStatus status = NtUnlockFile((IntPtr)handle, out ioStatusBlock, ref byteOffset, ref length, 0);

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.UnlockFile,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = ""
                };
                SMBLog.AddEntry(entry);
            }

            return status;
        }

        public NTStatus QueryDirectory(out List<QueryDirectoryFileInformation> result, object handle, string fileName, FileInformationClass informationClass)
        {
            IO_STATUS_BLOCK ioStatusBlock;
            byte[] buffer = new byte[QueryDirectoryBufferSize];
            UNICODE_STRING fileNameStructure = new UNICODE_STRING(fileName);
            result = new List<QueryDirectoryFileInformation>();
            bool restartScan = true;
            while (true)
            {
                NTStatus status = NtQueryDirectoryFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ioStatusBlock, buffer, (uint)buffer.Length, (byte)informationClass, false, ref fileNameStructure, restartScan);
                if (status == NTStatus.STATUS_NO_MORE_FILES)
                {
                    break;
                }
                else if (status != NTStatus.STATUS_SUCCESS)
                {
                    return status;
                }
                int numberOfBytesWritten = (int)ioStatusBlock.Information;
                List<QueryDirectoryFileInformation> page = QueryDirectoryFileInformation.ReadFileInformationList(buffer, 0, informationClass);
                result.AddRange(page);
                restartScan = false;
            }
            fileNameStructure.Dispose();
            return NTStatus.STATUS_SUCCESS;
        }

        public class GetFileInformationInfo : ICallInfo
        {
            public string Path;
            public FileInformationClass InformationClass;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.GetFileInformation;
            }
        }
        public NTStatus GetFileInformation(out FileInformation result, object handle, FileInformationClass informationClass)
        {
            GetFileInformationInfo curParams = new GetFileInformationInfo() { Path = m_handlePathDict[(IntPtr)handle], InformationClass = informationClass, Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.GetFileInformation || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }

            IO_STATUS_BLOCK ioStatusBlock;
            byte[] buffer = new byte[FileInformationBufferSize];
            NTStatus status = NtQueryInformationFile((IntPtr)handle, out ioStatusBlock, buffer, (uint)buffer.Length, (uint)informationClass);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                int numberOfBytesWritten = (int)ioStatusBlock.Information;
                buffer = ByteReader.ReadBytes(buffer, 0, numberOfBytesWritten);
                result = FileInformation.GetFileInformation(buffer, 0, informationClass);
            }
            else
            {
                result = null;
            }

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.GetFileInformation,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = string.Format("Information Class: 0x{0:X}", informationClass)
                };
                SMBLog.AddEntry(entry);
            }

            return status;
        }

        public class SetFileInformationInfo : ICallInfo
        {
            public string Path;
            public FileInformationClass InformationClass;
            public bool Log { get; set; }

            NTFileOperation ICallInfo.Operation()
            {
                return NTFileOperation.SetFileInformation;
            }
        }
        public NTStatus SetFileInformation(object handle, FileInformation information)
        {
            IO_STATUS_BLOCK ioStatusBlock;
            if (information is FileRenameInformationType2)
            {
                FileRenameInformationType2 fileRenameInformationRemote = (FileRenameInformationType2)information;
                if (ProcessHelper.Is64BitProcess)
                {
                    // We should not modify the FileRenameInformationType2 instance we received - the caller may use it later.
                    FileRenameInformationType2 fileRenameInformationLocal = new FileRenameInformationType2();
                    fileRenameInformationLocal.ReplaceIfExists = fileRenameInformationRemote.ReplaceIfExists;
                    fileRenameInformationLocal.FileName = ToNativePath(fileRenameInformationRemote.FileName);
                    information = fileRenameInformationLocal;
                }
                else
                {
                    // Note: WOW64 process should use FILE_RENAME_INFORMATION_TYPE_1.
                    // Note: Server 2003 x64 has issues with using FILE_RENAME_INFORMATION under WOW64.
                    FileRenameInformationType1 fileRenameInformationLocal = new FileRenameInformationType1();
                    fileRenameInformationLocal.ReplaceIfExists = fileRenameInformationRemote.ReplaceIfExists;
                    fileRenameInformationLocal.FileName = ToNativePath(fileRenameInformationRemote.FileName);
                    information = fileRenameInformationLocal;
                }
            }
            else if (information is FileLinkInformationType2)
            {
                FileLinkInformationType2 fileLinkInformationRemote = (FileLinkInformationType2)information;
                if (ProcessHelper.Is64BitProcess)
                {
                    FileRenameInformationType2 fileLinkInformationLocal = new FileRenameInformationType2();
                    fileLinkInformationLocal.ReplaceIfExists = fileLinkInformationRemote.ReplaceIfExists;
                    fileLinkInformationLocal.FileName = ToNativePath(fileLinkInformationRemote.FileName);
                    information = fileLinkInformationRemote;
                }
                else
                {
                    FileLinkInformationType1 fileLinkInformationLocal = new FileLinkInformationType1();
                    fileLinkInformationLocal.ReplaceIfExists = fileLinkInformationRemote.ReplaceIfExists;
                    fileLinkInformationLocal.FileName = ToNativePath(fileLinkInformationRemote.FileName);
                    information = fileLinkInformationRemote;
                }
            }
            SetFileInformationInfo curParams = new SetFileInformationInfo() { Path = m_handlePathDict[(IntPtr)handle], InformationClass = information.FileInformationClass, Log = false };
            foreach (SMBFilter filter in m_filters)
            {
                if (filter.Operation == NTFileOperation.SetFileInformation || filter.Operation == NTFileOperation.Any)
                {
                    filter.Apply(curParams);
                }
            }
            byte[] buffer = information.GetBytes();
            NTStatus status = NtSetInformationFile((IntPtr)handle, out ioStatusBlock, buffer, (uint)buffer.Length, (uint)information.FileInformationClass);

            if (curParams.Log)
            {
                SMBLogEntry entry = new SMBLogEntry()
                {
                    Time = DateTime.Now,
                    Handle = (IntPtr)handle,
                    Operation = NTFileOperation.SetFileInformation,
                    Path = m_handlePathDict[(IntPtr)handle],
                    Result = status,
                    Detail = string.Format("Information Class: 0x{0:X}", information.FileInformationClass)
                };
                SMBLog.AddEntry(entry);
            }

            return status;
        }

        public NTStatus GetFileSystemInformation(out FileSystemInformation result, FileSystemInformationClass informationClass)
        {
            IO_STATUS_BLOCK ioStatusBlock;
            byte[] buffer = new byte[FileSystemInformationBufferSize];
            IntPtr volumeHandle;
            FileStatus fileStatus;
            string nativePath = @"\??\" + m_directory.FullName.Substring(0, 3);
            NTStatus status = CreateFile(out volumeHandle, out fileStatus, nativePath, AccessMask.GENERIC_READ, 0, (SMBLibrary.FileAttributes)0, ShareAccess.Read, CreateDisposition.FILE_OPEN, (CreateOptions)0);
            result = null;
            if (status != NTStatus.STATUS_SUCCESS)
            {
                return status;
            }
            status = NtQueryVolumeInformationFile((IntPtr)volumeHandle, out ioStatusBlock, buffer, (uint)buffer.Length, (uint)informationClass);
            CloseFile(volumeHandle);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                int numberOfBytesWritten = (int)ioStatusBlock.Information;
                buffer = ByteReader.ReadBytes(buffer, 0, numberOfBytesWritten);
                result = FileSystemInformation.GetFileSystemInformation(buffer, 0, informationClass);
            }
            return status;
        }

        public NTStatus SetFileSystemInformation(FileSystemInformation information)
        {
            return NTStatus.STATUS_NOT_SUPPORTED;
        }

        public NTStatus GetSecurityInformation(out SecurityDescriptor result, object handle, SecurityInformation securityInformation)
        {
            result = null;
            return NTStatus.STATUS_INVALID_DEVICE_REQUEST;
        }

        public NTStatus SetSecurityInformation(object handle, SecurityInformation securityInformation, SecurityDescriptor securityDescriptor)
        {
            // [MS-FSA] If the object store does not implement security, the operation MUST be failed with STATUS_INVALID_DEVICE_REQUEST.
            return NTStatus.STATUS_INVALID_DEVICE_REQUEST;
        }

        public NTStatus NotifyChange(out object ioRequest, object handle, NotifyChangeFilter completionFilter, bool watchTree, int outputBufferSize, OnNotifyChangeCompleted onNotifyChangeCompleted, object context)
        {
            byte[] buffer = new byte[outputBufferSize];
            ManualResetEvent requestAddedEvent = new ManualResetEvent(false);
            PendingRequest request = new PendingRequest();
            Thread m_thread = new Thread(delegate ()
            {
                request.FileHandle = (IntPtr)handle;
                request.ThreadID = ThreadingHelper.GetCurrentThreadId();
                m_pendingRequests.Add(request);
                // The request has been added, we can now return STATUS_PENDING.
                requestAddedEvent.Set();
                // There is a possibility of race condition if the caller will wait for STATUS_PENDING and then immediate call Cancel, but this scenario is very unlikely.
                NTStatus status = NtNotifyChangeDirectoryFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out request.IOStatusBlock, buffer, (uint)buffer.Length, completionFilter, watchTree);
                if (status == NTStatus.STATUS_SUCCESS)
                {
                    int length = (int)request.IOStatusBlock.Information;
                    buffer = ByteReader.ReadBytes(buffer, 0, length);
                }
                else
                {
                    const NTStatus STATUS_ALERTED = (NTStatus)0x00000101;
                    const NTStatus STATUS_OBJECT_TYPE_MISMATCH = (NTStatus)0xC0000024;

                    buffer = new byte[0];
                    if (status == STATUS_OBJECT_TYPE_MISMATCH)
                    {
                        status = NTStatus.STATUS_INVALID_HANDLE;
                    }
                    else if (status == STATUS_ALERTED)
                    {
                        status = NTStatus.STATUS_CANCELLED;
                    }

                    // If the handle is closing and we had to cancel a ChangeNotify request as part of a cleanup,
                    // we return STATUS_NOTIFY_CLEANUP as specified in [MS-FSA] 2.1.5.4.
                    if (status == NTStatus.STATUS_CANCELLED && request.Cleanup)
                    {
                        status = NTStatus.STATUS_NOTIFY_CLEANUP;
                    }
                }
                onNotifyChangeCompleted(status, buffer, context);
                m_pendingRequests.Remove((IntPtr)handle, request.ThreadID);
            });
            m_thread.Start();

            // We must wait for the request to be added in order for Cancel to function properly.
            requestAddedEvent.WaitOne();
            ioRequest = request;
            return NTStatus.STATUS_PENDING;
        }

        public NTStatus Cancel(object ioRequest)
        {
            PendingRequest request = (PendingRequest)ioRequest;
            const uint THREAD_TERMINATE = 0x00000001;
            const uint THREAD_ALERT = 0x00000004;
            uint threadID = request.ThreadID;
            IntPtr threadHandle = ThreadingHelper.OpenThread(THREAD_TERMINATE | THREAD_ALERT, false, threadID);
            if (threadHandle == IntPtr.Zero)
            {
                Win32Error error = (Win32Error)Marshal.GetLastWin32Error();
                if (error == Win32Error.ERROR_INVALID_PARAMETER)
                {
                    return NTStatus.STATUS_INVALID_HANDLE;
                }
                else
                {
                    throw new Exception("OpenThread failed, Win32 error: " + error.ToString("D"));
                }
            }

            NTStatus status;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                IO_STATUS_BLOCK ioStatusBlock;
                status = NtCancelSynchronousIoFile(threadHandle, IntPtr.Zero, out ioStatusBlock);
            }
            else
            {
                // The handle was opened for synchronous operation so NtNotifyChangeDirectoryFile is blocking.
                // We MUST use NtAlertThread to send a signal to stop the wait. The handle cannot be closed otherwise.
                // Note: The handle was opened with CreateOptions.FILE_SYNCHRONOUS_IO_ALERT as required.
                status = NtAlertThread(threadHandle);
            }

            ThreadingHelper.CloseHandle(threadHandle);
            m_pendingRequests.Remove(request.FileHandle, request.ThreadID);
            return status;
        }

        public NTStatus DeviceIOControl(object handle, uint ctlCode, byte[] input, out byte[] output, int maxOutputLength)
        {
            switch ((IoControlCode)ctlCode)
            {
                case IoControlCode.FSCTL_IS_PATHNAME_VALID:
                case IoControlCode.FSCTL_GET_COMPRESSION:
                case IoControlCode.FSCTL_GET_RETRIEVAL_POINTERS:
                case IoControlCode.FSCTL_SET_OBJECT_ID:
                case IoControlCode.FSCTL_GET_OBJECT_ID:
                case IoControlCode.FSCTL_DELETE_OBJECT_ID:
                case IoControlCode.FSCTL_SET_OBJECT_ID_EXTENDED:
                case IoControlCode.FSCTL_CREATE_OR_GET_OBJECT_ID:
                case IoControlCode.FSCTL_SET_SPARSE:
                case IoControlCode.FSCTL_READ_FILE_USN_DATA:
                case IoControlCode.FSCTL_SET_DEFECT_MANAGEMENT:
                case IoControlCode.FSCTL_SET_COMPRESSION:
                case IoControlCode.FSCTL_QUERY_SPARING_INFO:
                case IoControlCode.FSCTL_QUERY_ON_DISK_VOLUME_INFO:
                case IoControlCode.FSCTL_SET_ZERO_ON_DEALLOCATION:
                case IoControlCode.FSCTL_QUERY_FILE_REGIONS:
                case IoControlCode.FSCTL_QUERY_ALLOCATED_RANGES:
                case IoControlCode.FSCTL_SET_ZERO_DATA:
                    {
                        IO_STATUS_BLOCK ioStatusBlock;
                        output = new byte[maxOutputLength];
                        NTStatus status = NtFsControlFile((IntPtr)handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ioStatusBlock, ctlCode, input, (uint)input.Length, output, (uint)maxOutputLength);
                        if (status == NTStatus.STATUS_SUCCESS)
                        {
                            int numberOfBytesWritten = (int)ioStatusBlock.Information;
                            output = ByteReader.ReadBytes(output, 0, numberOfBytesWritten);
                        }
                        return status;
                    }
                default:
                    {
                        output = null;
                        return NTStatus.STATUS_NOT_SUPPORTED;
                    }
            }
        }
    }
}
