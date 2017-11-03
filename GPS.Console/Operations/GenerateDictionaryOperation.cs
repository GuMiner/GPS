﻿using CommandLine;
using GPS.Common;
using ProtoBuf;
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

        /// <summary>
        /// Runs the generation process.
        /// </summary>
        internal int Generate()
        {
            DictionaryGenerator generator = new DictionaryGenerator();

            List<string> files = this.InputFolders.Select(folder => Directory.GetFiles(folder, "html-*.protobin", SearchOption.AllDirectories)).SelectMany(list => list).ToList();
            System.Console.WriteLine($"Processing {files.Count} uncompressed wiki files...");
            foreach (string file in files)
            {
                List<WikiPage> pages;
                using (FileStream stream = File.OpenRead(file))
                {
                    pages = Serializer.Deserialize<List<WikiPage>>(stream);
                }

                System.Console.WriteLine($"Processing {pages.Count} wiki pages in {file}...");
                int count = 0;
                Parallel.ForEach(pages, (page) =>
                {
                    generator.ProcessPage(page);

                    if (Interlocked.Increment(ref count) % 10 == 0)
                    {
                        System.Console.WriteLine($"  Processed {count} of {pages.Count} pages, for a total of {generator.Dictionary.WordMap.Count} distinct words.");
                    }
                });
            }

            // TODO: Don't duplicate this in this manner.
            files = this.CompressedInputFolders.Select(folder => Directory.GetFiles(folder, "html-*.protobin", SearchOption.AllDirectories)).SelectMany(list => list).ToList();
            System.Console.WriteLine($"Processing {files.Count} compressed wiki files...");
            foreach (string file in files)
            {
                List<CompressedWikiPage> pages;
                using (FileStream stream = File.OpenRead(file))
                {
                    pages = Serializer.Deserialize<List<CompressedWikiPage>>(stream);
                }

                System.Console.WriteLine($"Processing {pages.Count} wiki pages in {file}...");
                int count = 0;
                Parallel.ForEach(pages, (page) =>
                {
                    generator.ProcessPage(page);

                    if (Interlocked.Increment(ref count) % 10 == 0)
                    {
                        System.Console.WriteLine($"  Processed {count} of {pages.Count} pages, for a total of {generator.Dictionary.WordMap.Count} distinct words.");
                    }
                });
            }

            using (FileStream stream = File.Create(Path.Combine(this.OutputFolder, $"quote_dictionary.protobin")))
            {
                Serializer.Serialize(stream, generator.Dictionary);
            }

            return 0;
        }
    }
}