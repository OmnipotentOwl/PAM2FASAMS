using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    public enum FileType
    {
        DEMO,
        PERF,
        CFAR,
        SERV
    }

    public class Options
    {
        [Option('t', "type", Required = true, HelpText = "PAM file type")]
        public FileType Type { get; set;} 

        [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file to be generated.")]
        public string OutputFile { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

    }
}
