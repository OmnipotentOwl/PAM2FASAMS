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
        public async Task ExecuteExportFileAsync(AdminOptions options)
        {
            DirectoryInfo d = new DirectoryInfo(options.Directory);
            switch (options.FileType)
            {
                case DBFileType.Client:
                    break;
                case DBFileType.TreatmentEpisode:
                    break;
                case DBFileType.ServiceEvent:
                    break;
                case DBFileType.Subcontract:
                    await ExportContractDataAsync(d);
                    break;
                default:
                    break;
            }
        }
        public async Task ExecuteLoadFileAsync(AdminOptions options)
        {
            DirectoryInfo d = new DirectoryInfo(options.Directory);
            FileInfo[] files = d.GetFiles();
            DataTools dt = new DataTools();
            PAMConvert.JobNumber = dt.GetMaxJobNumber() + 1;
            foreach (FileInfo file in files)
            {
                
                switch (file.Name)
                {
                    case string a when a.Contains("Contract"):
                        Console.WriteLine("Loading file: {0}", file.Name);
                        await LoadContractFileAsync(file);
                        break;

                    case string a when a.Contains("ClientDataSet"):
                        Console.WriteLine("Loading file: {0}", file.Name);
                        await LoadProviderClientFileAsync(file);
                        break;

                    case string a when a.Contains("Treatment Episode"):
                        Console.WriteLine("Loading file: {0}", file.Name);
                        await LoadTreatmentEpisodeFileAsync(file);
                        break;

                    case string a when a.Contains("Service Event"):
                        Console.WriteLine("Loading file: {0}", file.Name);
                        await LoadServiceEventFileAsync(file);
                        break;
                }
            }
            await dt.MarkJobBatchComplete(PAMConvert.JobNumber);
        }

        private async Task LoadContractFileAsync(FileInfo file)
        {
            Console.WriteLine("Loading Contract file into database:");
            Subcontracts Subcontracts = new Subcontracts();
            var dt = new DataTools();
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
                        await dt.UpsertSubContract(subcontract);
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
        private async Task LoadProviderClientFileAsync(FileInfo file)
        {
            Console.WriteLine("Loading ProviderClient file into database:");
            ProviderClients ProviderClients = new ProviderClients();
            var dt = new DataTools();
            try
            {
                ProviderClients = (ProviderClients)ReadXml(ProviderClients, file);
                foreach (var item in ProviderClients.clients)
                {
                    if (item.ProviderClientIdentifiers != null)
                    {
                        foreach (var row in item.ProviderClientIdentifiers)
                        {
                            row.ClientSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                        }
                    } //fixes relationships
                    if (item.ProviderClientEmailAddresses != null)
                    {
                        foreach (var row in item.ProviderClientEmailAddresses)
                        {
                            row.ClientSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                        }
                    } //fixes relationships
                    if (item.ProviderClientPhones != null)
                    {
                        foreach (var row in item.ProviderClientPhones)
                        {
                            row.ClientSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                        }
                    } //fixes relationships
                    if (item.ProviderClientPhysicalAddresses != null)
                    {
                        foreach (var row in item.ProviderClientPhysicalAddresses)
                        {
                            row.ClientSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                        }
                    } //fixes relationships

                    try
                    {
                        await dt.UpsertProviderClient(item);
                        Console.WriteLine("Added ProviderClient: {0}, {1}", item.SourceRecordIdentifier, item.FederalTaxIdentifier);
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
            catch (Exception ex)
            {
                WriteErrorLog(ex, "ProviderClient", file.DirectoryName);
            }
            Console.WriteLine("Completed loading ProviderClient file into database!");
        }
        private async Task LoadTreatmentEpisodeFileAsync(FileInfo file)
        {
            Console.WriteLine("Loading TreatmentEpisode file into database:");
            TreatmentEpisodeDataSet TreatmentEpisodeDataSet = new TreatmentEpisodeDataSet();
            var dt = new DataTools();
            try
            {
                TreatmentEpisodeDataSet = (TreatmentEpisodeDataSet)ReadXml(TreatmentEpisodeDataSet, file);
                foreach (var item in TreatmentEpisodeDataSet.TreatmentEpisodes)
                {
                    if (item.Admissions != null)
                    {
                        foreach (var row in item.Admissions)
                        {
                            row.TreatmentSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;

                            if (row.PerformanceOutcomeMeasures != null)
                            {
                                foreach (var subrow in row.PerformanceOutcomeMeasures)
                                {
                                    subrow.Admission_SourceRecordIdentifier = row.SourceRecordIdentifier;
                                }
                            } //fixes relationships
                            if (row.Evaluations != null)
                            {
                                foreach (var subrow in row.Evaluations)
                                {
                                    subrow.Admission_SourceRecordIdentifier = row.SourceRecordIdentifier;
                                }
                            } //fixes relationships
                            if (row.Diagnoses != null)
                            {
                                foreach (var subrow in row.Diagnoses)
                                {
                                    subrow.Admission_SourceRecordIdentifier = row.SourceRecordIdentifier;
                                }
                            } //fixes relationships
                            if (row.Discharge != null)
                            {
                                if (row.Discharge.Evaluations != null)
                                {
                                    foreach (var subrow in row.Discharge.Evaluations)
                                    {
                                        subrow.Discharge_SourceRecordIdentifier = row.Discharge.SourceRecordIdentifier;
                                    }
                                } //fixes relationships
                                if (row.Discharge.Diagnoses != null)
                                {
                                    foreach (var subrow in row.Discharge.Diagnoses)
                                    {
                                        subrow.Discharge_SourceRecordIdentifier = row.Discharge.SourceRecordIdentifier;
                                    }
                                } //fixes relationships

                            } //fixes relationships
                        }

                    } //fixes relationships
                    if (item.ImmediateDischarges != null)
                    {
                        foreach (var row in item.ImmediateDischarges)
                        {
                            row.TreatmentSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                        }
                    } //fixes relationships

                    try
                    {
                        await dt.UpsertTreatmentSession(item);
                        Console.WriteLine("Added TreatmentEpisode : {0}, {1}", item.SourceRecordIdentifier, item.FederalTaxIdentifier);
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
            catch (Exception ex)
            {
                WriteErrorLog(ex, "TreatmentEpisode", file.DirectoryName);
            }
            Console.WriteLine("Completed loading TreatmentEpisode file into database!");
        }
        private async Task LoadServiceEventFileAsync(FileInfo file)
        {
            Console.WriteLine("Loading ServiceEvent file into database:");
            ServiceEvents ServiceEvents = new ServiceEvents();
            var dt = new DataTools();
            try
            {
                ServiceEvents = (ServiceEvents)ReadXml(ServiceEvents, file);
                foreach (var item in ServiceEvents.serviceEvents)
                {
                    if (item.ServiceEventCoveredServiceModifiers != null)
                    {
                        foreach (var row in item.ServiceEventCoveredServiceModifiers)
                        {
                            row.EventSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                            row.TypeCode = item.TypeCode;
                        }
                    } //fixes relationships
                    if (item.ServiceEventHcpcsProcedureModifiers != null)
                    {
                        foreach (var row in item.ServiceEventHcpcsProcedureModifiers)
                        {
                            row.EventSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                            row.TypeCode = item.TypeCode;
                        }
                    } //fixes relationships
                    if (item.ServiceEventExpenditureModifiers != null)
                    {
                        foreach (var row in item.ServiceEventExpenditureModifiers)
                        {
                            row.EventSourceId = item.SourceRecordIdentifier;
                            row.FederalTaxIdentifier = item.FederalTaxIdentifier;
                            row.TypeCode = item.TypeCode;
                        }
                    } //fixes relationships
                    
                    try
                    {
                        await dt.UpsertServiceEvent(item);
                        Console.WriteLine("Added ServiceEvent: {0}, {1}", item.SourceRecordIdentifier, item.FederalTaxIdentifier);
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
            catch (Exception ex)
            {
                WriteErrorLog(ex, "ServiceEvent", file.DirectoryName);
            }
            Console.WriteLine("Completed loading ServiceEvent file into database!");
        }
        private async Task ExportContractDataAsync(DirectoryInfo directory)
        {
            //Console.WriteLine("Loading Contract file into database:");
            Subcontracts Subcontracts = new Subcontracts();
            var dt = new DataTools();
            try
            {
                Subcontracts.subcontracts = await dt.GetAllSubcontracts();
                WriteXml(Subcontracts, null, "SubcontractDataSet",directory.FullName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "ContractFile", directory.Name);
            }
            Console.WriteLine("Completed exporting contracts into file!");
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
        private void WriteXml(object dataStructure, string outputFile, string outputFileName, string outputPath)
        {
            if (outputFile == null)
            {
                outputFile = outputPath + "\\" + outputFileName + "_" + (DateTime.Now.ToString("yyyyMMddHHmmss")) + ".xml";
            }
            Console.WriteLine("Writing Output File {0}", outputFile);
            Type t = dataStructure.GetType();
            XmlSerializer writer = new XmlSerializer(t);
            FileStream file = File.Create(outputFile);
            writer.Serialize(file, dataStructure);
            file.Close();
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
