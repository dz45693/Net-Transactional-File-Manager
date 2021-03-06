﻿using ChinhDo.Transactions.Utils;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Rollbackable operation which appends a string to an existing file, or creates the file if it doesn't exist.
    /// </summary>
    sealed class AppendAllText : SingleFileOperation
    {
        private readonly string contents;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to append the string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="backupPath">back up file name.</param>
        /// <param name="deleteBack">if true delete back file.</param>
        public AppendAllText(string path, string contents, string backupPath = "", bool deleteBack = true)
            : base(path,backupPath,deleteBack)
        {
            this.contents = contents;
        }

        public override void Execute()
        {
            if (File.Exists(path))
            {
                string temp = FileUtils.GetTempFileName(Path.GetExtension(path));
                if (!string.IsNullOrEmpty(backupPath))
                {
                    temp = backupPath;
                }
                File.Copy(path, temp);
                backupPath = temp;
            }

            File.AppendAllText(path, contents);
        }
    }
}
