using ChinhDo.Transactions.Utils;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Rollbackable operation which copies a file.
    /// </summary>
    sealed class Copy : SingleFileOperation
    {
        private readonly string sourceFileName;
        private readonly bool overwrite;
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten, otherwise false.</param>
        /// <param name="backupPath">back up file name.</param>
        /// <param name="deleteBack">if true delete back file.</param>
        public Copy(string sourceFileName, string destFileName, bool overwrite,string backupPath = "",bool deleteBack=true)
            : base(destFileName, backupPath,deleteBack)
        {
            this.sourceFileName = sourceFileName;
            this.overwrite = overwrite;
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

            File.Copy(sourceFileName, path, overwrite);
        }
    }
}
