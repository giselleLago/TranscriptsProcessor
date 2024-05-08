using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TranscriptsProcessor.TranscriptionService;
using Microsoft.Extensions.Logging;

namespace TranscriptsProcessor.Services
{
    public class Service
    {
        public Service(ILogger logger)
        {
            Logger = logger;
        }

        public void Run(string filePath)
        {
            FilePath = filePath;
            var files = GetPendingFiles(FilePath);
            SendFiles(files);
        }

        private IEnumerable<string> GetPendingFiles(string path)
        {
            Logger.LogInformation("Get pending MP3 files");
            // Get all mp3 files in the directory
            string[] mp3Files = Directory.GetFiles(path, "*.mp3");
            // Get all txt files in the directory
            string[] txtFiles = Directory.GetFiles(path, "*.txt");

            // Extract filenames without extension
            HashSet<string> mp3Names = new HashSet<string>(mp3Files.Select(Path.GetFileNameWithoutExtension));
            HashSet<string> txtNames = new HashSet<string>(txtFiles.Select(Path.GetFileNameWithoutExtension));

            var result = mp3Names.Where(name => !txtNames.Contains(name)).Select(x => $"{x}.mp3");
            return result;
        }

        private void SendFiles(IEnumerable<string> filePaths)
        {
            var splitFilePaths = SplitFilePaths(filePaths.ToList());
            foreach (var splitFilePath in splitFilePaths)
            {
                foreach (var filePath in splitFilePath)
                {
                    try
                    {
                        if (ValidateFiles($"{FilePath}\\{filePath}"))
                        {
                            // Read all bytes from the file
                            byte[] fileContent = File.ReadAllBytes($"{FilePath}\\{filePath}");
                            ProcessBatch(filePath, fileContent);
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"An IO exception was caught: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"An exception was caught: {e.Message}");
                    }
                }
            }

            if (ErrorFiles.Any())
            {
                Logger.LogWarning($"Fail to transcript the following documents: {ErrorFiles}");
            }
        }

        private void ProcessBatch(string fileName, byte[] fileContent)

        {
            var fts = new MockTranscriptionService();
            var retryAttempt = 0;

            SendToTranscript(retryAttempt, fileName, fileContent, fts);
        }

        private void SendToTranscript(int retryAttempt, string fileName, byte[] fileContents, MockTranscriptionService fts)
        {
            try
            {
                Logger.LogInformation("Sending MP3 files to FTS INVOX to transcript");

                var text = fts.Transcribe(fileName);

                var filePath = fileName.Substring(0, fileName.Length - 3) + "txt";

                Logger.LogInformation("Adding TXT file with the transcription");
                File.WriteAllText($"{FilePath}\\{filePath}", text);
            }
            catch (Exception)
            {
                if (retryAttempt <= 3)
                {
                    Logger.LogInformation($"Retry send to transcript. Attempt {retryAttempt}");
                    retryAttempt++;
                    FilesToSendRetry[fileName] = retryAttempt;
                    SendToTranscript(retryAttempt, fileName, fileContents, fts);
                }
                else
                {
                    ErrorFiles.Add(fileName);
                }
            }
        }

        private List<List<string>> SplitFilePaths(IList<string> files)
        {
            List<string> part1 = new List<string>();
            List<string> part2 = new List<string>();
            List<string> part3 = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        part1.Add(files[i]);
                        break;
                    case 1:
                        part2.Add(files[i]);
                        break;
                    case 2:
                        part3.Add(files[i]);
                        break;
                }
            }

            return new List<List<string>> { part1, part2, part3 };
        }

        private bool ValidateFiles(string filePath)
        {
            return true;
           // return IsValidSize(filePath) && IsValidBasicMP3(filePath) && ContainsMP3Headers(filePath);
        }

        private bool IsValidSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long sizeInBytes = fileInfo.Length;

            return (51200 < sizeInBytes && sizeInBytes < 3145728);
        }

        public bool IsValidBasicMP3(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Logger.LogError("File doesn't exists.");
                return false;
            }


            // Check the file extension
            if (Path.GetExtension(filePath).ToLower() != ".mp3")
            {
                Logger.LogError("File doesn't exists.");
                return false;
            }

            return true;
        }

        //ToCheck
        public bool ContainsMP3Headers(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[10];  // Read first 10 bytes from the file
                while (fs.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    if ((buffer[0] == 0xFF) && ((buffer[1] & 0xE0) == 0xE0))
                    {
                        return true;  // Found an MP3 frame header
                    }
                }
            }
            return false;
        }

        private string FilePath;
        private Dictionary<string, int> FilesToSendRetry = new Dictionary<string, int>();
        private readonly List<string> ErrorFiles = new List<string>();
        private readonly ILogger Logger;
    }
}
