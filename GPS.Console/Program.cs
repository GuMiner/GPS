using CommandLine;

namespace GPS.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ParserResult<object> operation = Parser.Default
                .ParseArguments<ExtractWikiDbOperation, GenerateDictionaryOperation, QueryDictionaryOperation, SummarizeDictionaryOperation, GenerateTreeOperation>(args);
            return operation.MapResult(
                (ExtractWikiDbOperation op) => op.Extract(),
                (GenerateDictionaryOperation op) => op.Generate(),
                (QueryDictionaryOperation op) => op.Query(),
                (SummarizeDictionaryOperation op) => op.Summarize(),
                (GenerateTreeOperation op) => op.Generate(),
                _ => 1);
        }
    }
}
