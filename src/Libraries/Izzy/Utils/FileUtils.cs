using System.Collections.Generic;
using System.IO;

namespace TT2026.libraries.Izzy.Utils
{
    public static class FileUtils
    {
        
        public delegate void FileLoaderMethod(FileInfo file, params string[] parameters);
        /// <summary>
        /// Runs <paramref name="method"/> on every file in the directory tree with the parameters <paramref name="parameters"/>.
        /// </summary>
        public static void RunMethodOnFilesWithExtensionsFromDirectoryTree(DirectoryInfo directory, string[] extensions, FileLoaderMethod method, params string[] parameters)
        {
            if (!directory.Exists) { throw new DirectoryNotFoundException($"directory {directory.FullName} does not exist"); }
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                RunMethodOnFilesWithExtensionsFromDirectoryTree(subDirectory, extensions, method);
            }
            RunMethodOnFilesInDirectory(directory, extensions, method, parameters);
        }
        /// <summary>
        /// Runs <paramref name="method"/> on every file in the directory with the parameters <paramref name="parameters"/>.
        /// </summary>
        public static void RunMethodOnFilesInDirectory(DirectoryInfo directory, string[] extensions, FileLoaderMethod method, params string[] parameters)
		{
            if (extensions == null) { extensions = new string[0]; }
            FileInfo[] filesInFolder = directory.GetFiles();
            foreach (FileInfo file in GetAllFilesInDirectory(directory, extensions))
            {
                method.Invoke(file, parameters);
            }
        }
        /// <param name="extensions">Only files with these extensions will be returned (will return all files if left empty)</param>
        public static FileInfo[] GetAllFilesInDirectory (DirectoryInfo directory, params string[] extensions)
		{
            if (extensions == null || extensions.Length == 0) { return directory.GetFiles(); }

            List<FileInfo> output = new List<FileInfo>();
            foreach (FileInfo file in directory.GetFiles())
            {
                if (System.Array.IndexOf(extensions, Path.GetExtension(file.FullName)) != -1) // If the file extension is in [extensions]
                {
                    output.Add(file);
                }
            }
            return output.ToArray();
        }
        public static DirectoryInfo GetSubdirectory (this DirectoryInfo directory, string name)
		{
            foreach(DirectoryInfo subdirectory in directory.GetDirectories())
			{
                if (subdirectory.Name == name) { return subdirectory; }
			}
            return null;
		}
        public static FileInfo GetFileWithName(this DirectoryInfo directory, string name)
		{
            foreach(FileInfo file in directory.GetFiles())
			{
                if (file.Name == name) { return file; }
			}
            return null;
		}

        /// <param name="commentChars">Any of these characters will cause the rest of the line to be commented out</param>
        /// <returns>An array where each entry is a line in the file</returns>
        public static string[] GetFileLinesAsStringArray (FileInfo textFile, params char[] commentChars)
		{
            if (textFile == null) return new string[0];
            StreamReader stream = textFile.OpenText();
            List<string> lines = new List<string>();
            while(!stream.EndOfStream)
			{
                string line = stream.ReadLine().Split(commentChars)[0];
                lines.Add(line);
			}
            return lines.ToArray();
		}

        /// <summary>
        /// Each line in the file formatted as key [keyValueSeperator] value is added as an entry into the dictionary.
        /// So if keyValueSeperator is '=', myKey = myValue would get parsed as such.
        /// Lines without a [keyValueSeperator] will be ignored.
        /// </summary>
        /// <param name="commentChars">These characters will cause the rest of the line to be commented out</param>
        public static Dictionary<string, string> ParseSimpleConfigurationFile(FileInfo file, char keyValueSeperator, params char[] commentChars)
		{
            string[] lines = GetFileLinesAsStringArray(file, commentChars);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < lines.Length; i++)
			{
                string line = lines[i];
                string[] keyValue = line.Split('=');
                if(keyValue.Length < 2) { continue; }
                dictionary.Add(keyValue[0].Trim(), keyValue[1].Trim());
            }
            return dictionary;
        }
    }
}