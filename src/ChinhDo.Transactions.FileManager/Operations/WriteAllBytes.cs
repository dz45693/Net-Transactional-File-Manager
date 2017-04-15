using ChinhDo.Transactions.Utils;
using System.IO;

namespace ChinhDo.Transactions.FileManager.Operations
{
    /// <summary>
    /// Creates a file, and writes the specified contents to it.
    /// </summary>
    sealed class WriteAllBytes : SingleFileOperation
    {
        private readonly byte[] contents;

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="backupPath">back up file name.</param>
        /// <param name="deleteBack">if true delete back file.</param>
        public WriteAllBytes(string path, byte[] contents, string backupPath = "", bool deleteBack = true)
            : base(path)
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

            File.WriteAllBytes(path, contents);
        }
    }
}