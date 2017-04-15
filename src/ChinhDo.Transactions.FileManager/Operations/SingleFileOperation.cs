﻿using System;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Class that contains common code for those rollbackable file operations which need
    /// to backup a single file and restore it when Rollback() is called.
    /// </summary>
    abstract class SingleFileOperation : IRollbackableOperation, IDisposable
    {
        protected readonly string path;
        protected string backupPath;
        // tracks whether Dispose has been called
        private bool disposed;
        private bool deleteBack = true;

        public SingleFileOperation(string path)
        {
            this.path = path;
        }
        public SingleFileOperation(string path,bool deleteBack)
        {
            this.path = path;
            this.deleteBack = deleteBack;
        }

        /// <summary>
        /// Disposes the resources used by this class.
        /// </summary>
        ~SingleFileOperation()
        {
            InnerDispose();
        }

        public abstract void Execute();

        public void Rollback()
        {
            if (backupPath != null)
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(backupPath, path, true);
            }
            else
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources of this class.
        /// </summary>
        private void InnerDispose()
        {
            if (!disposed)
            {
                if (backupPath != null && deleteBack)
                {
                    FileInfo fi = new FileInfo(backupPath);
                    if (fi.IsReadOnly)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }
                    File.Delete(backupPath);
                }

                disposed = true;
            }
        }
    }
}