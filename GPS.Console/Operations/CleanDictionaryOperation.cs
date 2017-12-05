using CommandLine;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPS.Console
{
    [Verb("cleanDictionary", HelpText = "Generates a tree from the dictionary of Wikipedia words for efficiency in searches.")]
    public class CleanDictionaryOperation
    {
        [Option('i', "input", Required = true, HelpText = "The name of the input dictionary file to read, in Dictionary<string, int> format.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "The name of the cleaned, ordered dictionary file to write, in List<KeyValuePair<string, int>> format.")]
        public string OutputFile { get; set; }

        [Option('m', "minFrequency", Required = false, HelpText = "The minimum number of times a word is allowed to exist in the dictionary. Defaults to 8")]
        public int MinFrequency { get; set; } = 8; // Emperically determined

        [Option('l', "maxLength", Required = false, HelpText = "The maximum word length in the dictionary. Defaults to 67")]
        public int MaxLength { get; set; } = 66; // Length of pneumonoultramicroscopicsilicovolcanoconiosis + 20 (a bit less than 50%)

        internal int Clean()
        {
            System.Console.WriteLine($"Minimum frequency for words: {this.MinFrequency}. Maximum length of words: {this.MaxLength}.");
            
            System.Console.WriteLine($"Reading {Path.GetFileName(this.InputFile)}...");

            Dictionary<string, int> cleanedDictionary = new Dictionary<string, int>();

            long itemCount = 0;
            long excludedWords = 0;
            long includedWords = 0;
            using (FileStream stream = File.OpenRead(this.InputFile))
            {
                // We need to stream in the dictionary file. We barely were able to write it out on 32 GiB of memory...
                object @object;
                while (Serializer.NonGeneric.TryDeserializeWithLengthPrefix(stream, PrefixStyle.Base128, (tag) => typeof(KeyValuePair<string, int>), out @object))
                {
                    KeyValuePair<string, int> item = (KeyValuePair<string, int>)@object;
                    if (item.Key.Length > this.MaxLength || item.Value < this.MinFrequency)
                    {
                        ++excludedWords;
                    }
                    else
                    {
                        ++includedWords;
                        cleanedDictionary.Add(item.Key, item.Value);
                    }

                    ++itemCount;
                    if (itemCount % 100000 == 0)
                    {
                        System.Console.WriteLine($"Read {itemCount} items out of {(float)stream.Position / (float)(1024 * 1024)} MiB of data.");
                    }
                }
            }

            System.Console.WriteLine($"Included {includedWords}, excluded {excludedWords}.");
            System.Console.WriteLine($"Sorting...");
            List<KeyValuePair<string, int>> orderedWordList = cleanedDictionary.OrderBy(kvp => kvp.Key).ToList();
            
            System.Console.WriteLine($"Saving {this.OutputFile}...");
            using (FileStream stream = File.Create(this.OutputFile))
            {
                Serializer.Serialize(stream, orderedWordList);
            }

            return 0;
        }
    }
}
