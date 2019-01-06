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
            var converter = new PAMConvert();
            if (options.BatchMode)
            {
                if(options.Directory == null)
                {
                    options.Directory = Environment.CurrentDirectory;
                }
                options.Directory = Path.GetFullPath(options.Directory);
                IEnumerable<InputFile> inputFiles = FileMapping.GetFileMapping().OrderBy(i => i.Sequence);
                Console.WriteLine("Beginning batch execution");
                converter.RunBatchJobAsync(inputFiles, options).Wait();
                Console.WriteLine("Batch execution completed!");
            }
            else
            {
                switch (options.Type)
                {
                    case FileType.IDUP:
                        break;
                    case FileType.SSN:
                        converter.InvokeSSNConversionAsync(options.InputFile, options.OutputFile).Wait();
                        break;
                    case FileType.DEMO:
                        converter.InvokeDemoConversionAsync(options.InputFile, options.OutputFile).Wait();
                        break;
                    case FileType.PERF:
                        converter.InvokePerfConversionAsync(options.InputFile, options.OutputFile).Wait();
                        break;
                    case FileType.CFAR:
                        converter.InvokeCFARSConversionAsync(options.InputFile, options.OutputFile).Wait();
                        break;
                    case FileType.SERV:
                        converter.InvokeServConversionAsync(options.InputFile, options.OutputFile).Wait();
                        break;
                    case FileType.EVNT:
                        converter.InvokeEvntConversionAsync(options.InputFile, options.OutputFile).Wait();
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
                case AdminTask.DUMP_FILE:
                    functions.ExecuteExportFileAsync(options).Wait();
                    break;
                case AdminTask.LOAD_FILE:
                    functions.ExecuteLoadFileAsync(options).Wait();
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
