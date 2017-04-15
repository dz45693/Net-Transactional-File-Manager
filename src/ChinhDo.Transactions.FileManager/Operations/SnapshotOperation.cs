using ChinhDo.Transactions.Utils;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Rollbackable operation which takes a snapshot of a file. The snapshot is used to rollback the file later if needed.
    /// </summary>
    sealed class Snapshot: SingleFileOperation
    {
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to take a snapshot for.</param>
        /// <param name="backupPath">back up file name.</param>
        /// <param name="deleteBack">if true delete back file.</param>
        public Snapshot(string path, string backupPath = "", bool deleteBack = true) : base(path,backupPath,deleteBack)
        {
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
        }
    }
}
