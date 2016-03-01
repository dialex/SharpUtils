using System.IO;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Represents a file location (name, folder, filepath).
    /// </summary>
    public class FileLocation
    {
        public string Folder
        {
            get { return _folder; }
            set { _folder = value; _filepath = _folder + _name; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; _filepath = _folder + _name; }
        }
        public string Filepath
        {
            get { return _filepath; }
        }
        private string _folder, _name, _filepath;

        public FileLocation(string name, string folder)
        {
            this.Name = name;
            this.Folder = folder;
        }
    }

    /// <summary>
    /// Contains file system (folders, files) related methods.
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// Cria pasta se não existir.
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns>True, se a pasta já existia.</returns>
        public static bool CreateDirectory(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                return false;
            }
            else return true;
        }
    }
}
