using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PAM2FASAMS.Utilities
{
    public class FileMapping
    {
        const string InputMappingFile = @"InputFormats/FileMapping.xml";

        public static List<InputFile> GetFileMapping()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<InputFile>));
            using (FileStream stream = File.OpenRead(InputMappingFile))
            {
                return (List<InputFile>)serializer.Deserialize(stream);
            }
        }
        public static void SaveFileMapping(List<InputFile> files)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<InputFile>));
            using (FileStream stream = File.OpenWrite(InputMappingFile))
            {
                serializer.Serialize(stream, files);
            }
        }
        public static void Seed()
        {
            if(File.Exists(InputMappingFile))
            {
                return;
            }
            else
            {
                List<InputFile> files = new List<InputFile>();
                files.Add(new InputFile { FileName = "IdUpdate.xml", RecordType = "IDUP", Sequence = 2, Notes = "Updates Key Id Records" });
                files.Add(new InputFile { FileName = "12SSN.Txt", RecordType = "SSN", Sequence = 14, Notes = "Updates SSN Records from PAM Format" });
                files.Add(new InputFile { FileName = "13Demo.Txt", RecordType = "DEMO", Sequence = 15, Notes = "New/Updated DEMO Records" });
                files.Add(new InputFile { FileName = "11Demo.Del", RecordType = "DEMO", Sequence = 13, Notes = "Deleted Demo Records" });
                files.Add(new InputFile { FileName = "18CFAR.Txt", RecordType = "CFARS", Sequence = 20, Notes = "New/Updated CFARS Records" });
                files.Add(new InputFile { FileName = "05CFAR.Del", RecordType = "CFARS", Sequence = 7, Notes = "Deleted CFARS Records" });
                files.Add(new InputFile { FileName = "19FARS.Txt", RecordType = "FARS", Sequence = 21, Notes = "New/Updated FARS Records" });
                files.Add(new InputFile { FileName = "04FARS.Del", RecordType = "FARS", Sequence = 6, Notes = "Deleted FARS Records" });
                files.Add(new InputFile { FileName = "17Perf.Txt", RecordType = "PERF", Sequence = 19, Notes = "New/Updated PERF Records" });
                files.Add(new InputFile { FileName = "06Perf.Del", RecordType = "PERF", Sequence = 8, Notes = "Deleted PERF Records" });
                files.Add(new InputFile { FileName = "14OUTI.Txt", RecordType = "SAPERFA", Sequence = 16, Notes = "New/Updated SA Perf Admission Records" });
                files.Add(new InputFile { FileName = "08OUTI.Del", RecordType = "SAPERFA", Sequence = 10, Notes = "Deleted SA Perf Admission Records" });
                files.Add(new InputFile { FileName = "15OUTD.Txt", RecordType = "SAPERFD", Sequence = 17, Notes = "New/Updated SA PERF Discharge Records" });
                files.Add(new InputFile { FileName = "07OUTD.Del", RecordType = "SAPERFD", Sequence = 9, Notes = "Deleted SA PERF Discharge Records" });
                files.Add(new InputFile { FileName = "16OUTX.Txt", RecordType = "SADT", Sequence = 18, Notes = "New/Updated SA DETOX Records" });
                files.Add(new InputFile { FileName = "09OUTX.Del", RecordType = "SADT", Sequence = 11, Notes = "Deleted SA DETOX Records" });
                files.Add(new InputFile { FileName = "20ASAM.Txt", RecordType = "ASAM", Sequence = 22, Notes = "New/Updated ASAM Records" });
                files.Add(new InputFile { FileName = "03ASAM.Del", RecordType = "ASAM", Sequence = 5, Notes = "Deleted ASAM Records" });
                files.Add(new InputFile { FileName = "21Serv.Txt", RecordType = "SERV", Sequence = 23, Notes = "New/Updated Service Records" });
                files.Add(new InputFile { FileName = "01Serv.Del", RecordType = "SERV", Sequence = 3, Notes = "Deleted Service Records" });
                files.Add(new InputFile { FileName = "22Evnt.Txt", RecordType = "EVNT", Sequence = 24, Notes = "New/Updated Event Records" });
                files.Add(new InputFile { FileName = "02Evnt.Del", RecordType = "EVNT", Sequence = 4, Notes = "Deleted Event Records" });
                files.Add(new InputFile { FileName = "23SANDR.Txt", RecordType = "SANDR", Sequence = 25, Notes = "New/Updated SANDR Records" });
                files.Add(new InputFile { FileName = "10SANDR.Del", RecordType = "SANDR", Sequence = 12, Notes = "Deleted SANDR Records" });
                SaveFileMapping(files);
            }
        }
    }
    public class InputFile
    {
        public string FileName { get; set; }
        public string RecordType { get; set; }
        public int Sequence { get; set; }
        public string Notes { get; set; }
    }
}
