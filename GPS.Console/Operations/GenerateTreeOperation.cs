using CommandLine;
using GPS.Common;
using GPS.Common.Data;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPS.Console
{
    [Verb("generateTree", HelpText = "Generates a tree from the dictionary of Wikipedia words for efficiency in searches.")]
    public class GenerateTreeOperation
    {
        [Option('i', "input", Required = true, HelpText = "The name of the input dictionary file to read, in Dictionary<string, int> format.")]
        public string InputFile { get; set; }

        [Option('m', "minFrequency", Required = false, HelpText = "The minimum number of times")]
        public int MinFrequency { get; set; } = 10;

        internal int Generate()
        {
            System.Console.WriteLine($"Minimum frequency for words: {this.MinFrequency}");

            Dictionary<string, int> dictionary;
            System.Console.WriteLine($"Reading {Path.GetFileName(this.InputFile)}...");
            using (FileStream stream = File.OpenRead(this.InputFile))
            {
                dictionary = Serializer.Deserialize<Dictionary<string, int>>(stream);
            }

            System.Console.WriteLine($"Read in {dictionary.Count} items.");
            TreeDictionary tree = new TreeDictionary() { RootNodes = new List<TreeNode>() };

            System.Console.WriteLine($"Converting to tree structure...");
            int edgeCount = 0;
            while (dictionary.Keys.Count > 0)
            {
                KeyValuePair<string, int> item = dictionary.First();
                edgeCount += TreeGenerator.AddWord(tree, item.Key, item.Value);

                dictionary.Remove(item.Key);

                if (dictionary.Count % 10000 == 0)
                {
                    System.Console.WriteLine($"  {dictionary.Count} items remaining, {edgeCount} edges added to the tree.");
                }
            }

            System.Console.WriteLine($"Pruning tree...");
            TreeGenerator.Prune(this.MinFrequency);

            string outputFilename = Path.Combine(Path.GetDirectoryName(this.InputFile), "tree-dictionary.protobuf");
            System.Console.WriteLine($"Saving {outputFilename}...");
            using (FileStream stream = File.Create(outputFilename))
            {
                Serializer.Serialize(stream, tree);
            }

            return 0;
        }
    }
}
