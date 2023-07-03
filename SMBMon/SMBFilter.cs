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
        Contains,
        DoesNotContain,
        StartsWith,
        EndsWith,
        And,
        Or,
        Xor,
        True
    }

    public class SMBFilterClause
    {
        FilterField field;
        FilterOperand operand;
        string valueString;
        ulong valueInt;

        private bool EvaluateString(string val)
        {
            bool result = false;

            switch (operand)
            {
                case FilterOperand.Equals:
                    result = (val == valueString);
                    break;
                case FilterOperand.Contains:
                    result = val.Contains(valueString);
                    break;
                case FilterOperand.DoesNotContain:
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

        public bool Evaluate(ref NTFilteredFileSystem.CreateFileParams createFileParams)
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

        public bool Evaluate(ref NTFilteredFileSystem.CloseFileParams closeFileParams)
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

        public bool Evaluate(ref NTFilteredFileSystem.ReadFileParams readFileParams)
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

        public bool Evaluate(ref NTFilteredFileSystem.WriteFileParams writeFileParams)
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
        public List<SMBFilterClause> Clauses;
        public NTFileOperation Operation;
        public FilterAction Action;

        public bool Match(ref NTFilteredFileSystem.CreateFileParams createFileParams)
        {
            bool result = false;

            foreach (SMBFilterClause clause in Clauses)
            {
                result |= clause.Evaluate(ref createFileParams);
            }

            return result;
        }

        public bool Match(ref NTFilteredFileSystem.CloseFileParams closeFileParams)
        {
            bool result = false;

            foreach (SMBFilterClause clause in Clauses)
            {
                result |= clause.Evaluate(ref closeFileParams);
            }

            return result;
        }

        public bool Match(ref NTFilteredFileSystem.ReadFileParams readFileParams)
        {
            bool result = false;

            foreach (SMBFilterClause clause in Clauses)
            {
                result |= clause.Evaluate(ref readFileParams);
            }

            return result;
        }

        public bool Match(ref NTFilteredFileSystem.WriteFileParams writeFileParams)
        {
            bool result = false;

            foreach (SMBFilterClause clause in Clauses)
            {
                result |= clause.Evaluate(ref writeFileParams);
            }

            return result;
        }

        public void Apply(ref NTFilteredFileSystem.CreateFileParams createFileParams)
        {
            if (!Match(ref createFileParams))
            {
                return;
            }

            switch (Action)
            {
                case FilterAction.Log:
                    createFileParams.Log = true;
                    break;
                default:
                    break;
            }
        }

        public void Apply(ref NTFilteredFileSystem.CloseFileParams closeFileParams)
        {
            if (!Match(ref closeFileParams))
            {
                return;
            }

            switch (Action)
            {
                case FilterAction.Log:
                    closeFileParams.Log = true;
                    break;
                default:
                    break;
            }
        }

        public void Apply(ref NTFilteredFileSystem.ReadFileParams readFileParams)
        {
            if (!Match(ref readFileParams))
            {
                return;
            }

            switch (Action)
            {
                case FilterAction.Log:
                    readFileParams.Log = true;
                    break;
                default:
                    break;
            }
        }

        public void Apply(ref NTFilteredFileSystem.WriteFileParams writeFileParams)
        {
            if (!Match(ref writeFileParams))
            {
                return;
            }

            switch (Action)
            {
                case FilterAction.Log:
                    writeFileParams.Log = true;
                    break;
                default:
                    break;
            }
        }

        public void AddClause(SMBFilterClause clause)
        {
            Clauses.Add(clause);
        }

        public SMBFilter(NTFileOperation Operation, FilterAction Action)
        {
            this.Operation = Operation;
            this.Action = Action;

            Clauses = new List<SMBFilterClause>();
        }

    }
}
