using ChinhDo.Transactions.Utils;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Rollbackable operation which deletes a file. An exception is not thrown if the file does not exist.
    /// </summary>
    sealed class DeleteFile : SingleFileOperation
    {
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to be deleted.</param>
        /// <param name="backupPath">back up file name.</param>
        /// <param name="deleteBack">if true delete back file.</param>
        public DeleteFile(string path,string backupPath="",bool deleteBack=true)
            : base(path, backupPath, deleteBack)
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

            File.Delete(path);
        }
    }
}
