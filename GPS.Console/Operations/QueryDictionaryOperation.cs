using CommandLine;
using GPS.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPS.Console
{
    [Verb("queryDictionary", HelpText = "Querys the hyperlinked dictionary.")]
    public class QueryDictionaryOperation
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing the WikiDictionary files to summarize")]
        public string Input { get; set; }

        /// <summary>
        /// Runs the query process.
        /// </summary>
        internal int Query()
        {
            Dictionary<string, int> maxWords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<string> files = Directory.GetFiles(this.Input, "*.protobin", SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in files)
            {
                System.Console.WriteLine($"Reading {Path.GetFileNameWithoutExtension(file)}...");

                WikiDictionary dictionary;
                using (FileStream stream = File.OpenRead(file))
                {
                    dictionary = Serializer.Deserialize<WikiDictionary>(stream);
                }

                System.Console.WriteLine($"Merging top 800 words...");
                foreach (KeyValuePair<string, int> word in dictionary.WordFrequencies.OrderByDescending(item => item.Value).Take(800))
                {
                    this.MergeWithDictionary(word, maxWords);
                }
            }
            

            System.Console.WriteLine("Top 200 words in the dictionary...");
            System.Console.Write(string.Join(Environment.NewLine, maxWords.OrderByDescending(item => item.Value).Take(200).Select(item => $"{item.Key}: {item.Value}")));
            System.Console.Write("Done");

            return 0;
        }

        private void MergeWithDictionary(KeyValuePair<string, int> word, Dictionary<string, int> maxWordsCompressed)
        {
            int dictionaryCount;
            if (!maxWordsCompressed.TryGetValue(word.Key, out dictionaryCount))
            {
                dictionaryCount = 0;
                maxWordsCompressed.Add(word.Key, dictionaryCount);
            }

            maxWordsCompressed[word.Key] += word.Value;
        }
    }
}