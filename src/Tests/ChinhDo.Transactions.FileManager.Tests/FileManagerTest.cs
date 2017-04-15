﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Xml;
using Xunit;

namespace ChinhDo.Transactions.FileManager.Tests
{
    public class FileManagerTest : IDisposable
    {
        private int _numTempFiles;
        private IFileManager _target;

        public FileManagerTest()
        {
            _target = new TxFileManager();
            _numTempFiles = Directory.GetFiles(Path.Combine(Path.GetTempPath(), "CdFileMgr")).Length;
        }

        private byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace("-", "").Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private byte[] GetHelloWorld()
        {
            return StringToByteArray("68-65-6C-6C-6F-20-77-6F-72-6C-64");
        }

        private byte[] GetHelloWorldAgain()
        {
            return StringToByteArray("68-65-6C-6C-6F-20-61-67-61-69-6E-20-77-6F-72-6C-64");
        }

        public void Dispose()
        {
            int numTempFiles = Directory.GetFiles(Path.Combine(Path.GetTempPath(), "CdFileMgr")).Length;
            Assert.Equal(_numTempFiles, numTempFiles);
        }

        #region Operations
        [Fact]
        public void CanAppendText()
        {
            string f1 = _target.GetTempFileName();
            const string contents = "123";

            try
            {
                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.AppendAllText(f1, contents);
                    scope1.Complete();
                }
                Assert.Equal(contents, File.ReadAllText(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void CannotAppendText()
        {
            string f1 = _target.GetTempFileName();
            const string contents = "123";

            Assert.Throws<IOException>(() =>
            {
                try
                {
                    using (TransactionScope scope1 = new TransactionScope())
                    {
                        using (FileStream fs = File.Open(f1, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
                        {
                            _target.AppendAllText(f1, contents);
                        }
                    }
                }
                finally
                {
                    File.Delete(f1);
                }
            });
        }

        [Fact]
        public void CanAppendTextAndRollback()
        {
            string f1 = _target.GetTempFileName();
            const string contents = "qwerty";
            using (TransactionScope sc1 = new TransactionScope())
            {
                _target.AppendAllText(f1, contents);
            }

            Assert.False(File.Exists(f1), f1 + " should not exist.");
        }

        [Fact]
        public void CanCopy()
        {
            string sourceFileName = _target.GetTempFileName();
            string destFileName = _target.GetTempFileName();

            try
            {
                const string expectedText = "Test 123.";
                using (TransactionScope scope1 = new TransactionScope())
                {
                    File.WriteAllText(sourceFileName, expectedText);
                    _target.Copy(sourceFileName, destFileName, false);
                    scope1.Complete();
                }

                Assert.Equal(expectedText, File.ReadAllText(sourceFileName));
                Assert.Equal(expectedText, File.ReadAllText(destFileName));
            }
            finally
            {
                File.Delete(sourceFileName);
                File.Delete(destFileName);
            }
        }

        [Fact]
        public void CanCopyAndRollback()
        {
            string sourceFileName = _target.GetTempFileName();
            const string expectedText = "Hello 123.";
            File.WriteAllText(sourceFileName, expectedText);
            string destFileName = _target.GetTempFileName();

            try
            {
                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.Copy(sourceFileName, destFileName, false);
                    // rollback
                }

                Assert.False(File.Exists(destFileName), destFileName + " should not exist.");
            }
            finally
            {
                File.Delete(sourceFileName);
                File.Delete(destFileName);
            }
        }

        [Fact]
        public void CanCreateDirectory()
        {
            string d1 = _target.GetTempFileName();
            try
            {
                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.CreateDirectory(d1);
                    scope1.Complete();
                }
                Assert.True(Directory.Exists(d1), d1 + " should exist.");
            }
            finally
            {
                Directory.Delete(d1);
            }
        }

        [Fact]
        public void CanRollbackNestedDirectories()
        {
            string baseDir = _target.GetTempFileName(string.Empty);
            string nested1 = Path.Combine(baseDir, "level1");
            using (new TransactionScope())
            {
                _target.CreateDirectory(nested1);
            }
            Assert.False(Directory.Exists(baseDir), baseDir + " should not exist.");
        }

        [Fact]
        public void CanCreateDirectoryAndRollback()
        {
            string d1 = _target.GetTempFileName();
            using (TransactionScope scope1 = new TransactionScope())
            {
                _target.CreateDirectory(d1);
            }
            Assert.False(Directory.Exists(d1), d1 + " should not exist.");
        }

        [Fact]
        public void CanDeleteDirectory()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                Directory.CreateDirectory(f1);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.DeleteDirectory(f1);
                    scope1.Complete();
                }

                Assert.False(Directory.Exists(f1), f1 + " should no longer exist.");
            }
            finally
            {
                if (Directory.Exists(f1))
                {
                    Directory.Delete(f1, true);
                }
            }
        }

        [Fact]
        public void CanDeleteDirectoryAndRollback()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                Directory.CreateDirectory(f1);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.DeleteDirectory(f1);
                }

                Assert.True(Directory.Exists(f1), f1 + " should exist.");
            }
            finally
            {
                if (Directory.Exists(f1))
                {
                    Directory.Delete(f1, true);
                }
            }
        }

        [Fact]
        public void CanDeleteFile()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                const string contents = "abc";
                File.WriteAllText(f1, contents);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.Delete(f1);
                    scope1.Complete();
                }

                Assert.False(File.Exists(f1), f1 + " should no longer exist.");
            }
            finally
            {
                if (Directory.Exists(f1))
                {
                    Directory.Delete(f1, true);
                }
            }
        }

        [Fact]
        public void CanDeleteFileAndRollback()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                const string contents = "abc";
                File.WriteAllText(f1, contents);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.Delete(f1);
                }

                Assert.True(File.Exists(f1), f1 + " should exist.");
                Assert.Equal(contents, File.ReadAllText(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void CanMoveFile()
        {
            const string contents = "abc";
            string f1 = _target.GetTempFileName();
            string f2 = _target.GetTempFileName();
            try
            {
                File.WriteAllText(f1, contents);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    Assert.True(File.Exists(f1));
                    Assert.False(File.Exists(f2));
                    _target.Move(f1, f2);
                    scope1.Complete();
                }
            }
            finally
            {
                File.Delete(f1);
                File.Delete(f2);
            }
        }

        [Fact]
        public void CanMoveFileAndRollback()
        {
            const string contents = "abc";
            string f1 = _target.GetTempFileName();
            string f2 = _target.GetTempFileName();
            try
            {
                File.WriteAllText(f1, contents);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    Assert.True(File.Exists(f1));
                    Assert.False(File.Exists(f2));
                    _target.Move(f1, f2);
                }

                Assert.Equal(contents, File.ReadAllText(f1));
                Assert.False(File.Exists(f2));
            }
            finally
            {
                File.Delete(f1);
                File.Delete(f2);
            }
        }

        [Fact]
        public void CanSnapshot()
        {
            string f1 = _target.GetTempFileName();

            using (TransactionScope scope1 = new TransactionScope())
            {
                _target.Snapshot(f1);

                _target.AppendAllText(f1, "<test></test>");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(f1);
            }

            Assert.False(File.Exists(f1), f1 + " should not exist.");
        }

        [Fact]
        public void CanWriteAllText()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                const string contents = "abcdef";
                File.WriteAllText(f1, "123");

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.WriteAllText(f1, contents);
                    scope1.Complete();
                }

                Assert.Equal(contents, File.ReadAllText(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void CanWriteAllTextAndRollback()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                const string contents1 = "123";
                const string contents2 = "abcdef";
                File.WriteAllText(f1, contents1);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.WriteAllText(f1, contents2);
                }

                Assert.Equal(contents1, File.ReadAllText(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void Scratch()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                Directory.CreateDirectory(f1);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.DeleteDirectory(f1);
                    scope1.Complete();
                }

                Assert.False(Directory.Exists(f1), f1 + " should no longer exist.");
            }
            finally
            {
                if (Directory.Exists(f1))
                {
                    Directory.Delete(f1, true);
                }
            }            
        }

        [Fact]
        public void CanWriteAllBytes()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                byte[] contents = GetHelloWorldAgain();
                File.WriteAllBytes(f1, GetHelloWorld());

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.WriteAllBytes(f1, contents);
                    scope1.Complete();
                }

                Assert.Equal(contents, File.ReadAllBytes(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void CanWriteAllBytesAndRollback()
        {
            string f1 = _target.GetTempFileName();
            try
            {
                byte[] contents1 = GetHelloWorld();
                byte[] contents2 = GetHelloWorldAgain();
                File.WriteAllBytes(f1, contents1);

                using (TransactionScope scope1 = new TransactionScope())
                {
                    _target.WriteAllBytes(f1, contents2);
                }

                Assert.Equal(contents1, File.ReadAllBytes(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        #endregion

        #region Error Handling

        [Fact]
        public void CanHandleCopyErrors()
        {
            string f1 = _target.GetTempFileName();
            string f2 = _target.GetTempFileName();

            var fs = new FileStream(f2, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            try
            {
                const string expectedText = "Test 123.";
                using (TransactionScope scope1 = new TransactionScope())
                {
                    File.WriteAllText(f1, expectedText);

                    try
                    {
                        _target.Copy(f1, f2, false);
                    }
                    catch (System.IO.IOException)
                    {
                        // Ignore IOException
                    }

                    //rollback
                }

            }
            finally
            {
                File.Delete(f1);
                fs.Close();
                File.Delete(f2);
            }
        }

        #endregion

        #region Transaction Support

        [Fact]
        public void CannotRollback()
        {
            string f1 = _target.GetTempFileName(".txt");
            string f2 = _target.GetTempFileName(".txt");

            Assert.Throws<TransactionException>(() =>
            {
                try
                {
                    using (TransactionScope scope1 = new TransactionScope())
                    {
                        _target.WriteAllText(f1, "Test.");
                        _target.WriteAllText(f2, "Test.");

                        FileInfo fi1 = new FileInfo(f1);
                        fi1.Attributes = FileAttributes.ReadOnly;

                        // rollback
                    }
                }
                finally
                {
                    FileInfo fi1 = new FileInfo(f1);
                    fi1.Attributes = FileAttributes.Normal;
                    File.Delete(f1);
                }
            });
        }

        [Fact]
        public void CanReuseManager()
        {
            {
                string sourceFileName = _target.GetTempFileName();
                File.WriteAllText(sourceFileName, "Hello.");
                string destFileName = _target.GetTempFileName();

                try
                {
                    using (TransactionScope scope1 = new TransactionScope())
                    {
                        _target.Copy(sourceFileName, destFileName, false);

                        // rollback
                    }

                    Assert.False(File.Exists(destFileName), destFileName + " should not exist.");
                }
                finally
                {
                    File.Delete(sourceFileName);
                    File.Delete(destFileName);
                }
            }

            {
                string sourceFileName = _target.GetTempFileName();
                File.WriteAllText(sourceFileName, "Hello.");
                string destFileName = _target.GetTempFileName();

                try
                {
                    using (TransactionScope scope1 = new TransactionScope())
                    {
                        _target.Copy(sourceFileName, destFileName, false);

                        // rollback
                    }

                    Assert.False(File.Exists(destFileName), destFileName + " should not exist.");
                }
                finally
                {
                    File.Delete(sourceFileName);
                    File.Delete(destFileName);
                }
            }
        }

        [Fact]
        public void CanSupportTransactionScopeOptionSuppress()
        {
            const string contents = "abc";
            string f1 = _target.GetTempFileName(".txt");
            try
            {
                using (TransactionScope scope1 = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    _target.WriteAllText(f1, contents);
                }

                Assert.Equal(contents, File.ReadAllText(f1));
            }
            finally
            {
                File.Delete(f1);
            }
        }

        [Fact]
        public void CanDoMultiThread()
        {
            const int numThreads = 10;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < numThreads; i++)
            {
                threads.Add(new Thread(CanAppendText));
                threads.Add(new Thread(CanAppendTextAndRollback));
                threads.Add(new Thread(CanCopy));
                threads.Add(new Thread(CanCopyAndRollback));
                threads.Add(new Thread(CanCreateDirectory));
                threads.Add(new Thread(CanCreateDirectoryAndRollback));
                threads.Add(new Thread(CanDeleteFile));
                threads.Add(new Thread(CanDeleteFileAndRollback));
                threads.Add(new Thread(CanMoveFile));
                threads.Add(new Thread(CanMoveFileAndRollback));
                threads.Add(new Thread(CanSnapshot));
                threads.Add(new Thread(CanWriteAllText));
                threads.Add(new Thread(CanWriteAllTextAndRollback));
            }

            foreach (Thread t in threads)
            {
                t.Start();
                t.Join();
            }
        }

        [Fact]
        public void CanNestTransactions()
        {
            string f1 = _target.GetTempFileName(".txt");
            const string f1Contents = "f1";
            string f2 = _target.GetTempFileName(".txt");
            const string f2Contents = "f2";
            string f3 = _target.GetTempFileName(".txt");
            const string f3Contents = "f3";

            try
            {
                using (TransactionScope sc1 = new TransactionScope())
                {
                    _target.WriteAllText(f1, f1Contents);

                    using (TransactionScope sc2 = new TransactionScope())
                    {
                        _target.WriteAllText(f2, f2Contents);
                        sc2.Complete();
                    }

                    using (TransactionScope sc3 = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        _target.WriteAllText(f3, f3Contents);
                        sc3.Complete();
                    }

                    sc1.Dispose();
                }

                Assert.False(File.Exists(f1));
                Assert.False(File.Exists(f2));
                Assert.True(File.Exists(f3));
            }
            finally
            {
                File.Delete(f1);
                File.Delete(f2);
                File.Delete(f3);
            }
        }

        #endregion

    }
}
