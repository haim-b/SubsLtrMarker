using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubsLtrMarker
{
    class Program
    {
        private const char RightToLeftMark = (char)0x200F;
        private const char LeftToRightEmbeddingMark = (char)0x202A;
        private const string SubtitlesSearchPattern = "*.srt";//"*.he*.srt";
        private static readonly char[] PanctuationMarks = new char[] { '.', ',', '!', '?', ':', ';', '(', ')', '"', '\'', '-', ' ' };

        private static Encoding HebEncoding = CodePagesEncodingProvider.Instance.GetEncoding(1255);

        static void Main(string[] args)
        {
            string baseFolder = args.ElementAtOrDefault(0) ?? Environment.CurrentDirectory;

            var watcher = StartMonitoringFolder(baseFolder);

            foreach (string sub in Directory.EnumerateFiles(baseFolder, SubtitlesSearchPattern, SearchOption.AllDirectories))
            {
                FixFile(sub);
            }

            Console.WriteLine("Finished");
            
            Console.WriteLine("Monitoring folder for new files.");

            Console.ReadLine();
        }

        private static void FixFile(string sub)
        {
            try
            {
                Encoding encoding = GetEncoding(sub);

                var lines = File.ReadAllLines(sub, encoding).ToList();

                if (lines.Any(l => l.StartsWith(LeftToRightEmbeddingMark))
                    || !lines.SelectMany(l => l).Any(c => IsHebrew(c))
                    || File.Exists(Path.ChangeExtension(sub, "bak")))
                {
                    return;
                }

                bool isReadOnly = File.GetAttributes(sub).HasFlag(FileAttributes.ReadOnly);

                if (isReadOnly)
                {
                    File.SetAttributes(sub, File.GetAttributes(sub) & ~FileAttributes.ReadOnly);
                }

                File.WriteAllLines(Path.ChangeExtension(sub, "bak"), lines.ToArray());
                File.WriteAllLines(sub, lines.Select(FixLine).ToArray(), encoding?.EncodingName == Encoding.Unicode.EncodingName ? Encoding.Unicode : Encoding.UTF8);

                if (isReadOnly)
                {
                    File.SetAttributes(sub, File.GetAttributes(sub) | FileAttributes.ReadOnly);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Taken from https://stackoverflow.com/questions/46558258/byte-array-read-from-a-file-and-byte-array-converted-from-string-read-from-same/46558425#46558425
        private static Encoding GetEncoding(string sub)
        {
            var encoding = HebEncoding;

            using (var reader = new StreamReader(sub, HebEncoding, true))
            {
                reader.Peek(); // you need this!
                encoding = reader.CurrentEncoding;
            }

            return encoding;
        }

        private static string FixLine(string line)
        {
            if (line == string.Empty || line.Contains("-->") || IsInt(line))
            {
                return line;
            }

            if (line.StartsWith(LeftToRightEmbeddingMark) || line.StartsWith(RightToLeftMark))
            {
                return line;
            }

            if (!TryGetFirstChar(line, out char firstChar)) // Contains only punctualtion
            {                 
                return line;
            }

            if (IsInt(firstChar.ToString()))
            {
                line = line.Replace(firstChar.ToString(), RightToLeftMark + firstChar.ToString());
            }

            return LeftToRightEmbeddingMark + line;
        }

        private static IDisposable StartMonitoringFolder(string baseFolder)
        {
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(baseFolder, SubtitlesSearchPattern);
            fileSystemWatcher.BeginInit();
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += OnFileCreatedOrChanged;
            fileSystemWatcher.Renamed += OnFileCreatedOrChanged;
            fileSystemWatcher.EndInit();
            return fileSystemWatcher;
        }

        private static void OnFileCreatedOrChanged(object sender, FileSystemEventArgs e)
        {
            FixFile(e.FullPath);
        }

        private static bool TryGetFirstChar(string line, out char firstChar)
        {
            firstChar = line.TrimStart(PanctuationMarks).ElementAtOrDefault(0);

            return firstChar != default(char);
        }

        private static bool IsInt(string str)
        {
            return int.TryParse(str, out _);
        }

        private const char FirstHebChar = (char)1488; //א
        private const char LastHebChar = (char)1514; //ת

        private static bool IsHebrew(char c)
        {
            return c >= FirstHebChar && c <= LastHebChar;
        }
    }
}
