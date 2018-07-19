using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    public enum FileType
    {
        IDUP,
        SSN,
        DEMO,
        SAPERFA,
        SAPERFD,
        SADT,
        PERF,
        CFAR,
        FARS,
        ASAM,
        SERV,
        EVNT,
        SANDR
    }

    public class Options
    {
        [Option('b', "batch", Default = false, Required = false, HelpText = "Run utility in batch mode for a directory")]
        public bool BatchMode { get; set; }

        [Option('d', "directory", Required = false, HelpText = "Directory for batch mode")]
        public string Directory { get; set; }

        [Option('t', "type", Required = false, HelpText = "PAM file type")]
        public FileType Type { get; set;} 

        [Option('i', "input", Required = false, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file to be generated.")]
        public string OutputFile { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

    }
}
