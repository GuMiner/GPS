using CommandLine;
using GPS.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GPS.Console
{
    [Verb("extract", HelpText = "Extracts XOWA wikipedia pages into protobuf WikiPage format.")]
    public class ExtractOperation
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing XOWA files to convert")]
        public string InputFolder { get; set; }

        [Option('n', "index", HelpText = "File linking page IDs to primary and redirected titles. If not provided, this will be computed on startup and saved in the working directory.")]
        public string IndexFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Directory that will contain the output of the conversion")]
        public string OutputFolder { get; set; }

        [Option('c', "compress", Required = true, HelpText = "If set, the default compression is mirrored to the output (gzip)")]
        public bool Compress { get; set; } = false;

        internal int Extract()
        {
            if (this.Compress)
            {
                System.Console.WriteLine("Skipping compression...");
                Thread.Sleep(1000);
            }

            XowaParser parser = new XowaParser(this.InputFolder);
            Dictionary<Int64, PageIndex> pageIndices = this.GetPageIndices(parser);

            int fileIdx = 0;
            List<string> htmlFiles = parser.GetHtmlDbFileNames();
            foreach (string file in htmlFiles)
            {
                // We output a series of files to avoid (a) too many small files (b) too large of a single file.
                if (this.Compress)
                {
                    CompressedWikiPageList pages = parser.ReadCompressedHtmlDbFile(file, pageIndices);

                    using (FileStream stream = File.Create(Path.Combine(this.OutputFolder, $"html-{fileIdx}.protobin")))
                    {
                        Serializer.Serialize(stream, pages.ValidPages);
                    }

                    string invalidFolder = Path.Combine(this.OutputFolder, "invalid");
                    if (!Directory.Exists(invalidFolder) && pages.InvalidPages.Any())
                    {
                        Directory.CreateDirectory(invalidFolder);
                    }
                    else if (pages.InvalidPages.Any())
                    {
                        using (FileStream stream = File.Create(Path.Combine(invalidFolder, $"html-invalid-{fileIdx}.protobin")))
                        {
                            Serializer.Serialize(stream, pages.InvalidPages);
                        }
                    }
                }
                else
                {
                    WikiPageList pages = parser.ReadHtmlDbFile(file, pageIndices);

                    using (FileStream stream = File.Create(Path.Combine(this.OutputFolder, $"html-{fileIdx}.protobin")))
                    {
                        Serializer.Serialize(stream, pages.ValidPages);
                    }

                    string invalidFolder = Path.Combine(this.OutputFolder, "invalid");
                    if (!Directory.Exists(invalidFolder) && pages.InvalidPages.Any())
                    {
                        Directory.CreateDirectory(invalidFolder);
                    }
                    else if (pages.InvalidPages.Any())
                    {
                        using (FileStream stream = File.Create(Path.Combine(invalidFolder, $"html-invalid-{fileIdx}.protobin")))
                        {
                            Serializer.Serialize(stream, pages.InvalidPages);
                        }
                    }
                }

                ++fileIdx;
            }

            return 0;
        }

        private Dictionary<Int64, PageIndex> GetPageIndices(XowaParser parser)
        {
            Dictionary<Int64, PageIndex> pageIndices;
            if (!string.IsNullOrWhiteSpace(this.IndexFile))
            {
                using (FileStream stream = File.OpenRead(this.IndexFile))
                {
                    pageIndices = Serializer.Deserialize<Dictionary<Int64, PageIndex>>(stream);
                }
            }
            else
            {
                string workingDir = Directory.GetCurrentDirectory();
                pageIndices = parser.ReadPageIndices();

                string saveFile = Path.Combine(workingDir, "pageIndices.protobin");
                using (FileStream stream = File.Create(saveFile))
                {
                    Serializer.Serialize(stream, pageIndices);
                }
                System.Console.WriteLine($"Saved the page indices to {saveFile}.");
            }

            return pageIndices;
        }
    }
}