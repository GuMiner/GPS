using CommandLine;
using GPS.Common;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPS.Console
{
    [Verb("summarizeDictionaries", HelpText = "Summarizes dictionaries, generating an easy-to-search lists of words and frequencies.")]
    public class SummarizeDictionaryOperation
    {
        [Option('i', "input", Required = false, HelpText = "Directories containing wikipedia XML files to scan, uncompressed")]
        public IEnumerable<string> InputFolders { get; set; } = new List<string>();

        /// <summary>
        /// Runs the summarization process.
        /// </summary>
        internal int Summarize()
        {
            Dictionary<string, int> mergedDictionary = new Dictionary<string, int>();

            List<string> files = this.InputFolders.Select(folder => Directory.GetFiles(folder, "dictionary-*.protobin", SearchOption.TopDirectoryOnly)).SelectMany(list => list).ToList();
            System.Console.WriteLine($"Processing {files.Count} dictionary files...");
            foreach (string file in files)
            {
                System.Console.WriteLine($"Reading {Path.GetFileName(file)}...");
                Dictionary<string, int> dictionary;
                using (FileStream stream = File.OpenRead(file))
                {
                    dictionary = Serializer.Deserialize<Dictionary<string, int>>(stream);
                }

                // Due to running a non-compiled 
                System.Console.WriteLine($"Processing {dictionary.Count} unique words in {Path.GetFileName(file)}...");
                foreach (KeyValuePair<string, int> wordFrequency in dictionary)
                {
                    this.MergeWithDictionary(wordFrequency, mergedDictionary);
                }

                System.Console.WriteLine($"Merged dictionary has {mergedDictionary.Count} unique words.");
            }

            System.Console.WriteLine($"Serializing merged dictionary...");
            using (FileStream stream = File.Create(Path.Combine(this.InputFolders.First(), $"wordFrequencies.protobin")))
            {
                Serializer.Serialize(stream, mergedDictionary);
            }

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
    }
}