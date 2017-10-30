using CommandLine;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GPS.Console
{
    [Verb("extract", HelpText = "Extracts XOWA wikipedia pages into protobuf WikiPage format.")]
    public class ExtractOperation
    {
        private Dictionary<Int64, PageIndex> pageIndices;

        [Option('i', "input", Required = true, HelpText = "Directory containing XOWA files to convert")]
        public string InputFolder { get; set; }

        [Option('n', "index", HelpText = "File linking page IDs to primary and redirected titles. If not provided, this will be computed on startup and saved in the working directory.")]
        public string IndexFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Directory that will contain the output of the conversion")]
        public string OutputFolder { get; set; }
        
        internal int Extract()
        {
            XowaParser parser = new XowaParser(this.InputFolder);
            this.PopulatePageIndices(parser);



            return 0;
        }

        private void PopulatePageIndices(XowaParser parser)
        {
            if (!string.IsNullOrWhiteSpace(this.IndexFile))
            {
                using (FileStream stream = File.OpenRead(this.IndexFile))
                {
                    this.pageIndices = Serializer.Deserialize<Dictionary<Int64, PageIndex>>(stream);
                }
            }
            else
            {
                string workingDir = Directory.GetCurrentDirectory();
                this.pageIndices = parser.ReadPageIndices();

                string saveFile = Path.Combine(workingDir, "pageIndices.protobin");
                using (FileStream stream = File.Create(saveFile))
                {
                    Serializer.Serialize(stream, this.pageIndices);
                }
                System.Console.WriteLine($"Saved the page indices to {saveFile}.");
            }
        }
    }
}