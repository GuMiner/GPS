using CommandLine;

namespace GPS.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ParserResult<object> operation = Parser.Default.ParseArguments<ExtractOperation, GenerateDictionaryOperation, QueryDictionaryOperation>(args);
            return operation.MapResult(
                (ExtractOperation op) => op.Extract(),
                (GenerateDictionaryOperation op) => op.Generate(),
                (QueryDictionaryOperation op) => op.Query(),
                _ => 1);
        }
    }
}
