using CommandLine;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPS.Console
{
    [Verb("extract", HelpText = "Extracts XOWA wikipedia pages into protobuf WikiPage format.")]
    public class ExtractOperation
    {
        private Dictionary<Int32, PageIndex> pageIndices;

        [Option('i', "input", HelpText = "Directory containing XOWA files to convert")]
        public string InputFolder { get; set; }

        [Option('n', "index", HelpText = "File linking page IDs to primary and redirected titles. If not provided, this will be computed on startup.")]
        public string IndexFile { get; set; }

        [Option('o', "output", HelpText = "Directory that will contain the output of the conversion")]
        public string OutputFolder { get; set; }
        
        internal int Extract()
        {
            if (!string.IsNullOrWhiteSpace(this.IndexFile))
            {
                using (FileStream stream = File.OpenRead(this.IndexFile))
                {
                    this.pageIndices = Serializer.Deserialize<Dictionary<Int32, PageIndex>>(stream);
                }
            }
            else
            {
                
            }

            return 0;
        }
    }
}