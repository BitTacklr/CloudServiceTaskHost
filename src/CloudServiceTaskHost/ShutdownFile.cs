using System;
using System.IO;

namespace CloudServiceTaskHost
{
    public class ShutdownFile : IDisposable
    {
        private readonly FileInfo _file;

        public static ShutdownFile CreateRandom(DirectoryInfo directory)
        {
            if (directory == null) throw new ArgumentNullException("directory");
            return new ShutdownFile(new FileInfo(Path.Combine(directory.FullName, Path.GetRandomFileName())));
        }

        private ShutdownFile(FileInfo file)
        {
            _file = file;
        }

        public string FullName { get { return _file.FullName; } }

        public void Notify()
        {
            using (var writer = _file.CreateText())
            {
                writer.Write(DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                writer.Flush();
            }
        }

        public void Dispose()
        {
            if (_file.Exists)
            {
                _file.Delete();
            }
        }
    }
}