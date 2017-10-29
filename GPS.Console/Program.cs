using CommandLine;

namespace GPS.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            ParserResult<ExtractOperation> operation = Parser.Default.ParseArguments<ExtractOperation>(args);
            return operation.MapResult(
                (ExtractOperation op) => op.Extract(),
                _ => 1);
        }
    }
}
