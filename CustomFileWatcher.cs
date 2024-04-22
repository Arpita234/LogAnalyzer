using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace LogAnalyzer
{
    public class CustomFileSystemWatcher
    {
        private readonly string _folderPath;
        private readonly string _fileExtension;
        private DateTime? lastProcessedTime;
        string lastProcessedFileTime = "Last_Processed_Time.txt";
        string lastProcessedFilePath = string.Empty;

        public event EventHandler<string>? FileCreated;
        public CustomFileSystemWatcher(string folderPath, string fileExtension)
        {
            _folderPath = folderPath;
            _fileExtension = fileExtension;

            lastProcessedFilePath = Path.Combine(Directory.GetCurrentDirectory(), lastProcessedFileTime);


            lastProcessedTime = LoadLastProcessedTime(lastProcessedFilePath);


            if (!Directory.Exists(_folderPath))
            {
                throw new DirectoryNotFoundException($"Directory {_folderPath} does not exist.");
            }
        }

        public void StartWatching()
        {
            try
            {
                while (!Console.KeyAvailable)
                {
                    string[] files = Directory.GetFiles(_folderPath, $"*.{_fileExtension}");
                    
                    // Filter out files that were created before the last processed time
                    files = files.Where(file => File.GetLastWriteTime(file) > lastProcessedTime).ToArray();


                    foreach (string filePath in files)
                    {                                            
                       OnFileCreated(filePath);                                          
                    }

                    if (files.Length > 0)
                    {
                        DateTime latestFileTime = files.Max(file => File.GetLastWriteTime(file));

                        // update last procesed time
                        lastProcessedTime = latestFileTime;
                        File.WriteAllText(lastProcessedFilePath, latestFileTime.ToString());
                    }
                    // Wait for 1 second before checking for new files again
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartWatching: {ex.Message}");
            }

        }

        public DateTime? LoadLastProcessedTime(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string lastProcessedTimeString = File.ReadAllText(filePath);
                    if (!string.IsNullOrEmpty(lastProcessedTimeString))
                         return DateTime.Parse(lastProcessedTimeString);
                }
                        
                 return DateTime.MinValue;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadLastProcessedTime: {ex.Message}");
                return null;          
            }
        }

        protected virtual void OnFileCreated(string filePath)
        {
            try
            {
                if (FileCreated != null)
                {
                    FileCreated?.Invoke(this, filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileCreated: {ex.Message}");
            }

        }

    }

}
