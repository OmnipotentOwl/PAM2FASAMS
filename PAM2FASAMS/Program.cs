using CommandLine;
using PAM2FASAMS.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Init();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void RunOptionsAndReturnExitCode(Options options)
        {
            Console.WriteLine("PAM to FASAMS Execution Starting");
            switch (options.Type)
            {
                case FileType.DEMO: PAMConvert.InvokeDemoConversion(options.InputFile, options.OutputFile);
                    break;
                case FileType.PERF: PAMConvert.InvokePerfConversion(options.InputFile, options.OutputFile);
                    break;
                default:
                    break;
            }


            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {

        }

        static void Init()
        {
            using (var db = new fasams_db())
            {
                db.Database.CreateIfNotExists();
            }
        }
    }
}
