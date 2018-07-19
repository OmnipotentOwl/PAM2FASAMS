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
                IEnumerable<InputFile> inputFiles = FileMapping.GetFileMapping().OrderBy(i => i.Sequence);
                Console.WriteLine("Beginning batch execution");
                foreach(InputFile file in inputFiles)
                {
                    options.InputFile = options.Directory +'/'+ file.FileName;
                    Console.WriteLine("File: {0}, Type: {1}", file.FileName, file.RecordType);
                    if (!File.Exists(options.InputFile))
                    {
                        Console.WriteLine("File: {0} not found, skipping file.", file.FileName);
                        continue;
                    }
                    switch (file.RecordType)
                    {
                        case "IDUP":
                            break;
                        case "SSN":
                            break;
                        case "DEMO":
                            PAMConvert.InvokeDemoConversion(options.InputFile, options.OutputFile);
                            break;
                        case "SAPERFA":
                            break;
                        case "SAPERFD":
                            break;
                        case "SADT":
                            break;
                        case "PERF":
                            PAMConvert.InvokePerfConversion(options.InputFile, options.OutputFile);
                            break;
                        case "CFAR":
                            break;
                        case "FARS":
                            break;
                        case "ASAM":
                            break;
                        case "SERV":
                            break;
                        case "EVNT":
                            break;
                        case "SANDR":
                            break;
                    }
                }
                Console.WriteLine("Batch execution completed!");
            }
            else
            {
                switch (options.Type)
                {
                    case FileType.DEMO:
                        PAMConvert.InvokeDemoConversion(options.InputFile, options.OutputFile);
                        break;
                    case FileType.PERF:
                        PAMConvert.InvokePerfConversion(options.InputFile, options.OutputFile);
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
