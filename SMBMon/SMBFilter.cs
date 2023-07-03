using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMBMon
{
    public enum FilterField
    {
        Path, // CreateFile, QueryDirectory - fileName
        DesiredAccess, // CreateFile
        FileAttributes, // CreateFile
        ShareAccess, // CreateFile
        CreateDisposition, // CreateFile
        // TODO: account for SecurityContext...
        // TODO: maybe filter close file?
        Offset, // ReadFile, WriteFile, LockFile - byteOffset, UnlockFile - byteOffset
        Length, // ReadFile - maxCount, WriteFile - data.Length, LockFile, UnlockFile
        InformationClass, // QueryDirectory, GetFileInformation, SetFileInformation, GetFileSystemInformation, SetFileSystemInformation
        SecurityInformation, // GetSecurityInformation, SetSecurityInformation
        IOCTL, // DeviceIOControl
    }

    public enum FilterAction
    {
        Log,
        Redirect
    }

    public enum FilterOperand
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        And,
        Or,
        Xor,
        True
    }

    public class SMBFilterClause
    {
        public FilterField field;
        public FilterOperand operand;
        public string valueString;
        public ulong valueInt;

        private bool EvaluateString(string val)
        {
            bool result = false;

            switch (operand)
            {
                case FilterOperand.Equals:
                    result = (val == valueString);
                    break;
                case FilterOperand.NotEquals:
                    result = (val != valueString);
                    break;
                case FilterOperand.Contains:
                    result = val.Contains(valueString);
                    break;
                case FilterOperand.NotContains:
                    result = !val.Contains(valueString);
                    break;
                case FilterOperand.StartsWith:
                    result = val.StartsWith(valueString);
                    break;
                case FilterOperand.EndsWith:
                    result = val.EndsWith(valueString);
                    break;
                default:
                    break;
            }

            return result;
        }

        private bool EvaluateInt(ulong val)
        {
            bool result = false;

            switch (operand)
            {
                case FilterOperand.Equals:
                    result = (val == valueInt);
                    break;
                case FilterOperand.NotEquals:
                    result = (val != valueInt);
                    break;
                case FilterOperand.And:
                    result = ((val & valueInt) != 0);
                    break;
                case FilterOperand.Xor:
                    result = ((val ^ valueInt) != 0);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.CreateFileInfo createFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch(field)
            {
                case FilterField.Path:
                    result = EvaluateString(createFileParams.Path);
                    break;
                case FilterField.DesiredAccess:
                    result = EvaluateInt((ulong)createFileParams.DesiredAccess);
                    break;
                case FilterField.FileAttributes:
                    result = EvaluateInt((ulong)createFileParams.FileAttributes);
                    break;
                case FilterField.ShareAccess:
                    result = EvaluateInt((ulong)createFileParams.ShareAccess);
                    break;
                case FilterField.CreateDisposition:
                    result = EvaluateInt((ulong)createFileParams.CreateDisposition);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.CloseFileInfo closeFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(closeFileParams.Path);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.ReadFileInfo readFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(readFileParams.Path);
                    break;
                case FilterField.Offset:
                    result = EvaluateInt((ulong)readFileParams.Offset);
                    break;
                case FilterField.Length:
                    result = EvaluateInt((ulong)readFileParams.Length);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.WriteFileInfo writeFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(writeFileParams.Path);
                    break;
                case FilterField.Offset:
                    result = EvaluateInt((ulong)writeFileParams.Offset);
                    break;
                case FilterField.Length:
                    result = EvaluateInt((ulong)writeFileParams.Length);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.FlushFileBuffersInfo flushFileBuffersParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(flushFileBuffersParams.Path);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.LockFileInfo lockFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(lockFileParams.Path);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.UnlockFileInfo unlockFileParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(unlockFileParams.Path);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.GetFileInformationInfo getFileInfoParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(getFileInfoParams.Path);
                    break;
                case FilterField.InformationClass:
                    result = EvaluateInt((ulong)getFileInfoParams.InformationClass);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool Evaluate(NTFilteredFileSystem.SetFileInformationInfo setFileInfoParams)
        {
            bool result = false;

            if (operand == FilterOperand.True)
            {
                return true;
            }

            switch (field)
            {
                case FilterField.Path:
                    result = EvaluateString(setFileInfoParams.Path);
                    break;
                case FilterField.InformationClass:
                    result = EvaluateInt((ulong)setFileInfoParams.InformationClass);
                    break;
                default:
                    break;
            }

            return result;
        }

        public SMBFilterClause(FilterField field, FilterOperand operand, string value)
        {
            this.field = field;
            this.operand = operand;
            valueString = value;
        }

        public SMBFilterClause(FilterField field, FilterOperand operand, ulong value)
        {
            this.field = field;
            this.operand = operand;
            valueInt = value;
        }
    }

    public class SMBFilter
    {
        public SMBFilterClause Clause;
        public NTFileOperation Operation;
        public FilterAction Action;
        public bool Enabled;

        public bool Match(ICallInfo callInfo)
        {
            bool result = false;

            switch (callInfo.Operation())
            {
                case NTFileOperation.CreateFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.CreateFileInfo)callInfo);
                    break;
                case NTFileOperation.CloseFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.CloseFileInfo)callInfo);
                    break;
                case NTFileOperation.ReadFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.ReadFileInfo)callInfo);
                    break;
                case NTFileOperation.WriteFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.WriteFileInfo)callInfo);
                    break;
                case NTFileOperation.FlushFileBuffers:
                    result |= Clause.Evaluate((NTFilteredFileSystem.FlushFileBuffersInfo)callInfo);
                    break;
                case NTFileOperation.LockFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.LockFileInfo)callInfo);
                    break;
                case NTFileOperation.UnlockFile:
                    result |= Clause.Evaluate((NTFilteredFileSystem.UnlockFileInfo)callInfo);
                    break;
                case NTFileOperation.GetFileInformation:
                    result |= Clause.Evaluate((NTFilteredFileSystem.GetFileInformationInfo)callInfo);
                    break;
                case NTFileOperation.SetFileInformation:
                    result |= Clause.Evaluate((NTFilteredFileSystem.SetFileInformationInfo)callInfo);
                    break;
                default:
                    break;
            }

            return result;
        }

        public void Apply(ICallInfo callInfo)
        {

            if (!Match(callInfo))
            {
                return;
            }

            switch (Action)
            {
                case FilterAction.Log:
                    callInfo.Log = true;
                    break;
                default:
                    break;
            }
        }

        public void AddClause(SMBFilterClause clause)
        {
            Clause = clause;
        }

        public SMBFilter(NTFileOperation Operation, FilterAction Action, bool Enabled)
        {
            this.Operation = Operation;
            this.Action = Action;
            this.Enabled = Enabled;
        }

    }
}
