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

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(baseFolder, SubtitlesSearchPattern);
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += OnFileCreatedOrChanged;
            fileSystemWatcher.Renamed += OnFileCreatedOrChanged;

            foreach (string sub in Directory.EnumerateFiles(baseFolder, SubtitlesSearchPattern, SearchOption.AllDirectories))
            {
                FixFile(sub);
            }

            Console.WriteLine("Finished");

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

        private static void OnFileCreatedOrChanged(object sender, FileSystemEventArgs e)
        {
            FixFile(e.FullPath);
        }

        private static string FixLine(string line)
        {
            if (line == string.Empty || line.Contains("-->") || int.TryParse(line, out _))
            {
                return line;
            }

            if (line.StartsWith(LeftToRightEmbeddingMark) || line.StartsWith(RightToLeftMark))
            {
                return line;
            }

            char firstChar = line.TrimStart(PanctuationMarks).ElementAtOrDefault(0);

            if (firstChar == default(char))
            {
                return line;
            }

            if (int.TryParse(firstChar.ToString(), out _))
            {
                line = line.Replace(firstChar.ToString(), RightToLeftMark + firstChar.ToString());
            }

            return LeftToRightEmbeddingMark + line;
        }


        private const char FirstHebChar = (char)1488; //א
        private const char LastHebChar = (char)1514; //ת
        private static bool IsHebrew(char c)
        {
            return c >= FirstHebChar && c <= LastHebChar;
        }
    }
}
