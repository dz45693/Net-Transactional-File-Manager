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
        private readonly string backRoot;
        private readonly string replaceRoot;
        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten, otherwise false.</param>
        /// <param name="backRoot">back file root.</param>
        /// <param name="replaceRoot">old file root.</param>
        /// <param name="deleteBack">if  delete back files.</param>
        public Copy(string sourceFileName, string destFileName, bool overwrite,string backRoot="",string replaceRoot="",bool deleteBack=true)
            : base(destFileName,deleteBack)
        {
            this.sourceFileName = sourceFileName;
            this.overwrite = overwrite;
            this.backRoot = backRoot;
            this.replaceRoot = replaceRoot;
        }

        public override void Execute()
        {
            if (File.Exists(path))
            {
                string temp = string.Empty;
                if (!string.IsNullOrEmpty(backRoot)&& !string.IsNullOrEmpty(replaceRoot)) {                 
                    temp = path.Replace(replaceRoot,backRoot);
                }
                else
                {
                    temp = FileUtils.GetTempFileName(Path.GetExtension(path));
                }
                 
                File.Copy(path, temp);
                backupPath = temp;
            }

            File.Copy(sourceFileName, path, overwrite);
        }
    }
}
