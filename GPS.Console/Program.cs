﻿using CommandLine;

namespace GPS.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ParserResult<object> operation = Parser.Default.ParseArguments<ExtractOperation, GenerateDictionaryOperation>(args);
            return operation.MapResult(
                (ExtractOperation op) => op.Extract(),
                (GenerateDictionaryOperation op) => op.Generate(),
                _ => 1);
        }
    }
}
