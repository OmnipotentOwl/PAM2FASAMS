using CommandLine;
using PAM2FASAMS.DataContext;
using PAM2FASAMS.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PAM2FASAMS
{
    class Program
    {
        
        static int Main(string[] args)
        {
            Init();
            var parser = new Parser(config => config.HelpWriter = Console.Out);
            return parser.ParseArguments<Options, AdminOptions>(args)
                .MapResult(
                    (Options opts) => RunOptionsAndReturnExitCode(opts),
                    (AdminOptions opts) => RunAdminOptionsAndReturnExitCode(opts),
                    errors => 1);
        }

        static int RunOptionsAndReturnExitCode(Options options)
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
            return 0;
        }
        static int RunAdminOptionsAndReturnExitCode(AdminOptions options)
        {
            if (options.Directory == null)
            {
                options.Directory = Environment.CurrentDirectory;
            }
            options.Directory = Path.GetFullPath(options.Directory);
            AdminFunctions functions = new AdminFunctions();
            Console.WriteLine("Running Administrative Tasks based on input options.");
            switch (options.Type)
            {
                case AdminTask.DUMP_DB:
                    functions.ExecuteDumpDatabase(options);
                    break;
                case AdminTask.LOAD_DB:
                    functions.ExecuteLoadDatabase(options);
                    break;
                case AdminTask.LOAD_FILE:
                    functions.ExecuteLoadFile(options);
                    break;
            }
            return 0;
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
