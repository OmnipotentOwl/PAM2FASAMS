using PAM2FASAMS.OutputFormats;
using PAM2FASAMS.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PAM2FASAMS
{
    public class AdminFunctions
    {
        public void ExecuteDumpDatabase(AdminOptions options)
        {
            Console.WriteLine("NOT YET IMPLIMENTED");
        }
        public void ExecuteLoadDatabase(AdminOptions options)
        {
            Console.WriteLine("NOT YET IMPLIMENTED");
        }
        public void ExecuteLoadFile(AdminOptions options)
        {
            DirectoryInfo d = new DirectoryInfo(options.Directory);
            FileInfo[] files = d.GetFiles();
            foreach (FileInfo file in files)
            {
                Console.WriteLine("Loading file: {0}", file.Name);
                switch (file.Name)
                {
                    case "Contract.xml":
                        LoadContractFile(file);
                        break;
                }
            }
        }

        private void LoadContractFile(FileInfo file)
        {
            Console.WriteLine("Loading Contract file into database:");
            Subcontracts Subcontracts = new Subcontracts();
            try
            {
                Subcontracts = (Subcontracts)ReadXml(Subcontracts, file);
                foreach (var subcontract in Subcontracts.subcontracts)
                {
                    if(subcontract.TypeCode=="1")
                    {
                        subcontract.AmendmentNumber = "";
                    }
                    if (subcontract.SubcontractServices != null)
                    {
                        foreach (var row in subcontract.SubcontractServices)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                        }
                    } //fixes relationships
                    if (subcontract.SubcontractOutputMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutputMeasures)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                        }
                    } //fixes relationships
                    if (subcontract.SubcontractOutcomeMeasures != null)
                    {
                        foreach (var row in subcontract.SubcontractOutcomeMeasures)
                        {
                            row.ContractNumber = subcontract.ContractNumber;
                            row.SubcontractNumber = subcontract.SubcontractNumber;
                            row.AmendmentNumber = subcontract.AmendmentNumber;
                        }
                    } //fixes relationships

                    try
                    {
                        DataTools.UpsertSubContract(subcontract);
                        Console.WriteLine("Added Contract: {0}, {1}, {2}", subcontract.ContractNumber, subcontract.SubcontractNumber, subcontract.AmendmentNumber);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        // Retrieve the error messages as a list of strings.
                        var errorMessages = ex.EntityValidationErrors
                                .SelectMany(x => x.ValidationErrors)
                                .Select(x => x.ErrorMessage);

                        // Join the list to a single string.
                        var fullErrorMessage = string.Join(";", errorMessages);

                        // Combine the original exception message with the new one.
                        var exceptionMessage = string.Concat(ex.Message, "The validation errors are: ", fullErrorMessage);

                        // Throw a new DbEntityValidationException with the improved exception message.
                        throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                    }
                }
            }
            catch(Exception ex)
            {
                WriteErrorLog(ex, "ContractFile", file.DirectoryName);
            }
            Console.WriteLine("Completed loading contract file into database!");
        }

        private object ReadXml(object dataStructure, FileInfo file)
        {
            Type t = dataStructure.GetType();
            XmlSerializer serializer = new XmlSerializer(t);
            StreamReader reader = new StreamReader(file.FullName);
            dataStructure = serializer.Deserialize(reader);
            reader.Close();
            return dataStructure;
        }
        private static void WriteErrorLog(Exception ex, string outputFileName, string outputPath, string inputFile, int rowNum)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += string.Format("InputFile: {0}, Row Number: {1}", inputFile, rowNum);
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = outputPath + "\\" + outputFileName + "-ErrorLog.txt";
            WriteErrorLog(message, path);
        }
        private static void WriteErrorLog(Exception ex, string outputFileName, string outputPath)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = outputPath + "\\" + outputFileName + "-ErrorLog.txt";
            WriteErrorLog(message, path);
        }
        private static void WriteErrorLog(string message, string path)
        {
            Console.WriteLine("Logged Error to {0}", path);
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }
}
