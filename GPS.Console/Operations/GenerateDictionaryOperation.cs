using CommandLine;
using GPS.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPS.Console
{
    [Verb("generateDictionary", HelpText = "Scans wikipedia pages, generating the hyperlinked dictionary.")]
    public class GenerateDictionaryOperation
    {
        [Option('i', "input", Required = false, HelpText = "Directories containing wikipedia XML files to scan, uncompressed")]
        public IEnumerable<string> InputFolders { get; set; } = new List<string>();

        [Option('c', "cinput", Required = false, HelpText = "Directories containing wikipedia XML files to scan, compressed")]
        public IEnumerable<string> CompressedInputFolders { get; set; } = new List<string>();

        [Option('o', "output", Required = true, HelpText = "Directory that will contain the dictionary files for later operations")]
        public string OutputFolder { get; set; }

        [Option('w', "wordsOnly", Required = false, HelpText = "If set, only summarize words, not all word linkages.")]
        public bool WordsOnly { get; set; } = false;

        /// <summary>
        /// Runs the generation process.
        /// </summary>
        internal int Generate()
        {
            System.Console.WriteLine($"Word-only summarization: {this.WordsOnly}");
            Dictionary<string, int> maxWordsFlat = GeneratePerFileDictionaries<WikiPage>(this.InputFolders, "flat", (page, generator) => generator.ProcessPage(page));
            Dictionary<string, int> maxWordsCompressed = GeneratePerFileDictionaries<CompressedWikiPage>(this.CompressedInputFolders, "compressed", (page, generator) => generator.ProcessPage(page));

            // Combine results so we can see how our outputs were.
            foreach (KeyValuePair<string, int> word in maxWordsFlat)
            {
                this.MergeWithDictionary(word, maxWordsCompressed);
            }
            
            System.Console.WriteLine("Summary (Top 200)...");
            System.Console.Write(string.Join(Environment.NewLine, maxWordsCompressed.OrderByDescending(item => item.Value).Take(200).Select(item => $"{item.Key}: {item.Value}")));
            System.Console.Write("Done");

            return 0;
        }

        private void MergeWithDictionary(KeyValuePair<string, int> word, Dictionary<string, int> summaryDictionary)
        {
            int dictionaryCount;
            if (!summaryDictionary.TryGetValue(word.Key, out dictionaryCount))
            {
                dictionaryCount = 0;
                summaryDictionary.Add(word.Key, dictionaryCount);
            }

            summaryDictionary[word.Key] += word.Value;
        }

        private Dictionary<string, int> GeneratePerFileDictionaries<T>(IEnumerable<string> folders, string type, Action<T, DictionaryGenerator> pageProcessAction)
        {
            Dictionary<string, int> maxWords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            List<string> files = folders.Select(folder => Directory.GetFiles(folder, "html-*.protobin", SearchOption.AllDirectories)).SelectMany(list => list).ToList();
            System.Console.WriteLine($"Processing {files.Count} {type} wiki files...");
            int fileCount = 0;
            foreach (string file in files)
            {
                // Use a new generator for each file so we don't run out of memory
                DictionaryGenerator generator = new DictionaryGenerator(this.WordsOnly);

                List<T> pages;
                using (FileStream stream = File.OpenRead(file))
                {
                    pages = Serializer.Deserialize<List<T>>(stream);
                }

                System.Console.WriteLine($"Processing {pages.Count} wiki pages in {file}...");
                int count = 0;
                Parallel.ForEach(pages, new ParallelOptions() { MaxDegreeOfParallelism = 12 }, (page) =>
                {
                    try
                    {
                        pageProcessAction(page, generator);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(Path.Combine(this.OutputFolder, "errors.txt"), $"{ex.Message}: {ex.StackTrace}");
                    }

                    if (Interlocked.Increment(ref count) % 1000 == 0)
                    {
                        System.Console.WriteLine($"  Processed {count} of {pages.Count} pages, for a total of {generator.Dictionary.WordFrequencies.Count} distinct words.");
                    }
                });
                
                System.Console.WriteLine("Saving...");
                using (FileStream stream = File.Create(Path.Combine(this.OutputFolder, $"dictionary-{fileCount}-{type}.protobin")))
                {
                    if (this.WordsOnly)
                    {
                        Serializer.Serialize(stream, generator.Dictionary.WordFrequencies);
                    }
                    else
                    {
                        Serializer.Serialize(stream, generator.Dictionary);
                    }
                }
                ++fileCount;

                // Keep a running total of the maximum word count found.
                System.Console.WriteLine("Aggregating top 1000...");
                foreach (KeyValuePair<string, int> word in generator.Dictionary.WordFrequencies.OrderByDescending(item => item.Value).Take(1000))
                {
                    MergeWithDictionary(word, maxWords);
                }
            }

            return maxWords;
        }
    }
}