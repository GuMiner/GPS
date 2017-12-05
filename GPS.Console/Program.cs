using CommandLine;

namespace GPS.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ParserResult<object> operation = Parser.Default
                .ParseArguments<ExtractWikiDbOperation, GenerateDictionaryOperation, QueryDictionaryOperation, SummarizeDictionaryOperation, CleanDictionaryOperation>(args);
            return operation.MapResult(
                (ExtractWikiDbOperation op) => op.Extract(),
                (GenerateDictionaryOperation op) => op.Generate(),
                (QueryDictionaryOperation op) => op.Query(),
                (SummarizeDictionaryOperation op) => op.Summarize(),
                (CleanDictionaryOperation op) => op.Clean(),
                _ => 1);
        }
    }
}
