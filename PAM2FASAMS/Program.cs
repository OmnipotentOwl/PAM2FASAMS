using CommandLine;
using PAM2FASAMS.DataContext;
using PAM2FASAMS.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (options.BatchMode)
            {
                if(options.Directory == null)
                {
                    options.Directory = Environment.CurrentDirectory;
                }
                options.Directory = Path.GetFullPath(options.Directory);
                IEnumerable<InputFile> inputFiles = FileMapping.GetFileMapping().OrderBy(i => i.Sequence);
                Console.WriteLine("Beginning batch execution");
                PAMConvert.RunBatchJob(inputFiles, options);
                Console.WriteLine("Batch execution completed!");
            }
            else
            {
                switch (options.Type)
                {
                    case FileType.IDUP:
                        break;
                    case FileType.SSN:
                        PAMConvert.InvokeSSNConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.DEMO:
                        PAMConvert.InvokeDemoConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.PERF:
                        PAMConvert.InvokePerfConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.CFAR:
                        PAMConvert.InvokeCFARSConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.SERV:
                        PAMConvert.InvokeServConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.EVNT:
                        PAMConvert.InvokeEvntConversion(options.InputFile, options.OutputFile);
                        break;
                    default:
                        break;
                }
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
            FileMapping.Seed();
        }
    }
}
