using System.Collections;

namespace LogAnalyzer
{
    class Program
    {

        private readonly static  object obj = new object();
        public static void Main(string[] args)
        {
            try
            {
                string logFolder = @"C:\Logs";

                Console.WriteLine("Log analyzer started. Press any key to exit.");

                /*    
               // Create a file system watcher to monitor the folder for new log files

                using (FileSystemWatcher watcher = new FileSystemWatcher())
                {
                    watcher.Path = logFolder;
                    watcher.Filter = "*.txt";
                    watcher.NotifyFilter = NotifyFilters.FileName;
                    watcher.Created += OnLogFileCreated;
                    watcher.EnableRaisingEvents = true;

                    Console.WriteLine("Log Analyzer is running. Press any key to exit.");
                    Console.ReadKey();
                } 
                */

                string fileExtension = "txt";

                CustomFileSystemWatcher watcher = new CustomFileSystemWatcher(logFolder, fileExtension);
                watcher.FileCreated += OnLogFileCreated;

                watcher.StartWatching();

                 Console.WriteLine("CustomFileSystemWatcher is running. Press any key to exit.");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error in FileWatcher: {ex.Message}");
            }

        }

        private static async void OnLogFileCreated(object? sender,string filePath)
        {
            try
            {
                // Print a message when a new log file is created
                lock (obj)
                {
                    Console.WriteLine($"New log file created: {Path.GetFileName(filePath)}");
                }

                // Process the newly created log file asynchronously
                await ProcessLogFileAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file - OnLogFileCreated {filePath}: {ex.Message}");
            }

        }

        private static async Task ProcessLogFileAsync(string filePath)
        {

            try
            {
                if (IsFileReady(filePath)) // Check if the file is ready (not being written to)
                {

                    string[] keywords = { "warning", "error", "fail" };
                    Dictionary<string, int> keywordCounts = new Dictionary<string, int>();

                    await using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            foreach (string keyword in keywords)
                            {
                                // Split the line into words and check each word for the presence of keywords
                                string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string word in words)
                                {
                                    if (word.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                                    {

                                        if (!keywordCounts.ContainsKey(keyword))
                                        {
                                            keywordCounts.Add(keyword, 1);
                                        }
                                        else
                                        {
                                            keywordCounts[keyword] = keywordCounts[keyword] + 1;
                                        }
                                    }
                                }
                                
                            }
                        }
                    }

                    // Print keyword counts to the console
                    lock (obj)
                    {
                        Console.WriteLine("\n############");
                        Console.WriteLine($"Summary of occurrences in File: {Path.GetFileName(filePath)}");
                        foreach (var kvp in keywordCounts)
                        {
                            Console.WriteLine($"- '{kvp.Key}': {kvp.Value}");
                        }
                        Console.WriteLine("############");

                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing log file {filePath}: {ex.Message}");
            }
        }
   

        // Check if a file is ready (not being written to)
        static bool IsFileReady(string filePath)
        {
            try
            {
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
