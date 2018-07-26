using PAM2FASAMS.OutputFormats;
using PAM2FASAMS.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PAM2FASAMS
{
    public class PAMConvert
    {
        #region Conversion Functions
        public static void RunBatchJob(IEnumerable<InputFile> inputFiles, Options options)
        {
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>() };
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            foreach (InputFile file in inputFiles)
            {
                options.InputFile = options.Directory + '/' + file.FileName;
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
                        InvokeSSNConversion(clientDataSet, options.InputFile);
                        break;
                    case "DEMO":
                        InvokeDemoConversion(clientDataSet, options.InputFile);
                        break;
                    case "SAPERFA":
                        break;
                    case "SAPERFD":
                        break;
                    case "SADT":
                        break;
                    case "PERF":
                        InvokePerfConversion(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "CFARS":
                        InvokeCFARSConversion(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "FARS":
                        break;
                    case "ASAM":
                        break;
                    case "SERV":
                        InvokeServConversion(serviceEventsDataSet, options.InputFile);
                        break;
                    case "EVNT":
                        InvokeEvntConversion(serviceEventsDataSet, options.InputFile);
                        break;
                    case "SANDR":
                        break;
                    default:
                        break;
                }
            }
            WriteXml(clientDataSet, null, "ClientDataSet", options.Directory);
            WriteXml(treatmentEpisodeDataSet, null, "TreatmentEpisodeDataSet", options.Directory);
            WriteXml(serviceEventsDataSet, null, "ServiceEventDataSet", options.Directory);
        }
        public static void InvokeSSNConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SSN update file..");
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>() };
            InvokeSSNConversion(clientDataSet, inputFile);
            WriteXml(clientDataSet, outputFile, "ClientDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SSN update file.");
        }
        public static void InvokeSSNConversion(ProviderClients clientDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SSN update file..");
            PAMMappingFile = @"InputFormats/PAM-SSN.xml";
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            foreach (var pamRow in pamFile)
            {
                ProviderClient client = new ProviderClient
                {
                    ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                };
                var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "OldSSN").Single().Value));
                var newClientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "NewSSN").Single().Value));
                client = DataTools.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                FASAMSValidations.ProcessProviderClientIdentifiers(client, newClientId);
                try
                {
                    DataTools.UpsertProviderClient(client);
                    clientDataSet.clients.Add(client);
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "ClientDataSet", Path.GetDirectoryName(inputFile));
                }
            }
            Console.WriteLine("Completed Conversion of PAM SSN update file.");
        }
        public static void InvokeDemoConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM DEMO file..");
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>()};
            InvokeDemoConversion(clientDataSet, inputFile);
            WriteXml(clientDataSet, outputFile, "ClientDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM DEMO file.");
        }
        public static void InvokeDemoConversion(ProviderClients clientDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM DEMO file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-DEMO-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-DEMO.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            foreach (var pamRow in pamFile)
            {
                try
                {
                    ProviderClient client = new ProviderClient
                    {
                        ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                    };
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    if (IsDelete)
                    {
                        var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                        client = DataTools.OpportuniticlyLoadProviderClient(clientDataSet, clientId, fedTaxId);
                        client.action = "delete";
                    }
                    else
                    {
                        var sourceRecordId = (pamRow.Where(r => r.Name == "ClientID").Single().Value);
                        client = DataTools.OpportuniticlyLoadProviderClient(clientDataSet, sourceRecordId, fedTaxId);
                        client.FederalTaxIdentifier = fedTaxId;
                        client.SourceRecordIdentifier = sourceRecordId;
                        client.BirthDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "DOB").Single().Value));
                        client.FirstName = (pamRow.Where(r => r.Name == "First").Single().Value).Trim();
                        client.MiddleName = (pamRow.Where(r => r.Name == "Middle").Single().Value).Trim();
                        client.LastName = (pamRow.Where(r => r.Name == "Last").Single().Value).Trim();
                        client.SuffixName = (pamRow.Where(r => r.Name == "Suffix").Single().Value).Trim();
                        client.GenderCode = (pamRow.Where(r => r.Name == "Gender").Single().Value);
                        client.RaceCode = (pamRow.Where(r => r.Name == "Race").Single().Value);
                        client.EthnicityCode = (pamRow.Where(r => r.Name == "Ethnic").Single().Value);
                        var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                        FASAMSValidations.ProcessProviderClientIdentifiers(client, clientId);
                    }
                    try
                    {
                        DataTools.UpsertProviderClient(client);
                        clientDataSet.clients.Add(client);
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
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "ClientDataSet", Path.GetDirectoryName(inputFile));
                }
            }
            Console.WriteLine("Completed Conversion of PAM DEMO file.");
        }
        public static void InvokePerfConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM PERF file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            InvokePerfConversion(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM PERF file.");
        }
        public static void InvokePerfConversion(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM PERF file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-PERF-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-PERF.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = DataTools.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        var type = PAMValidations.ValidateEvalPurpose(FileType.PERF, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                        string evalDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    string programCode = FASAMSValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate);
                                    if (treatmentEpisode.Admissions != null && treatmentEpisode.Admissions.Count > 0)
                                    {
                                        Admission initialAdmit = treatmentEpisode.Admissions.Where(a => a.TypeCode == "1").Single();
                                        if (initialAdmit.InternalAdmissionDate < DateTime.Parse(evalDate) && initialAdmit.ProgramAreaCode == programCode)
                                        {
                                            treatmentEpisode = new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = client.SourceRecordIdentifier, FederalTaxIdentifier = fedTaxId, Admissions = new List<Admission>() };
                                        }
                                    }
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    if (admission.AdmissionDate == null)
                                    {
                                        admission.SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                                        admission.StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.ContractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value);
                                        admission.AdmissionDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value));
                                        admission.ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value);
                                        admission.TypeCode = "1";
                                        admission.ProgramAreaCode = FASAMSValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate);
                                        admission.TreatmentSettingCode = "todo"; //not sure how to calculate this based on existing data.
                                        admission.IsCodependentCode = FASAMSValidations.ValidateAdmissionCoDependent(FASAMSValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate));
                                        admission.DaysWaitingToEnterTreatmentKnownCode = "0";
                                        admission.PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>();
                                        admission.Diagnoses = new List<Diagnosis>();
                                    }
                                    Admission newAdmission = new Admission
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value),
                                        StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        ContractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value),
                                        AdmissionDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value)),
                                        ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value),
                                        TypeCode = "1",
                                        ProgramAreaCode = FASAMSValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate),
                                        TreatmentSettingCode = "todo", //not sure how to calculate this based on existing data.
                                        IsCodependentCode = FASAMSValidations.ValidateAdmissionCoDependent(FASAMSValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate)),
                                        DaysWaitingToEnterTreatmentKnownCode = "0",
                                        PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>(),
                                        Diagnoses = new List<Diagnosis>(),
                                    };

                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
                                        ClientDemographic = new ClientDemographic
                                        {
                                            VeteranStatusCode = (pamRow.Where(r => r.Name == "VetStatus").Single().Value),
                                            MaritalStatusCode = (pamRow.Where(r => r.Name == "Marital").Single().Value),
                                            ResidenceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyResid").Single().Value),
                                            ResidencePostalCode = (pamRow.Where(r => r.Name == "Zip").Single().Value),
                                        },
                                        FinancialAndHousehold = new FinancialAndHousehold
                                        {
                                            PrimaryIncomeSourceCode = (pamRow.Where(r => r.Name == "PIncoSrc").Single().Value),
                                            AnnualPersonalIncomeKnownCode = "0",
                                            AnnualPersonalIncomeAmount = 0,
                                            AnnualPersonalIncomeAmountSpecified = false,
                                            AnnualFamilyIncomeKnownCode = FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
                                            DependentsKnownCode = "0",
                                            DependentsCount = 0,
                                            DependentsCountSpecified = false
                                        },
                                        Health = new Health
                                        {
                                            UnableToPerformDailyLivingActivitiesCode = (pamRow.Where(r => r.Name == "ADLFc").Single().Value)
                                        },
                                        EducationAndEmployment = new EducationAndEmployment
                                        {
                                            EducationGradeLevelCode = (pamRow.Where(r => r.Name == "Grade").Single().Value),
                                            SchoolAttendanceStatusCode = "",
                                            SchoolDaysAvailableInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
                                        },
                                        Recovery = new Recovery
                                        {
                                            SelfHelpGroupAttendanceFrequencyCode = (pamRow.Where(r => r.Name == "Social").Single().Value)
                                        },
                                        MentalHealth = new MentalHealth
                                        {
                                            MentalHealthProblemRiskCode = (pamRow.Where(r => r.Name == "MhProb").Single().Value),
                                            HasRiskFactorsForEmotionalDisturbanceCode = (pamRow.Where(r => r.Name == "RiskFact").Single().Value),
                                            PrognosisStatusCode = (pamRow.Where(r => r.Name == "Prognosis").Single().Value)
                                        },
                                        Medication = new Medication
                                        {
                                            ReceivedPrescriptionsThroughIndigentDrugProgramCode = (pamRow.Where(r => r.Name == "RxIDP").Single().Value),
                                            ReceivedPrescriptionsThroughPatientAssistanceProgramCode = (pamRow.Where(r => r.Name == "RxPAP").Single().Value),
                                            TakingAntipsychoticMedicationCode = (pamRow.Where(r => r.Name == "Rx").Single().Value)
                                        },
                                        Legal = new Legal
                                        {
                                            ArrestsInLast30DaysKnownCode = FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            HasBeenCommittedToJuvenileJusticeCode = (pamRow.Where(r => r.Name == "DJJCommit").Single().Value),
                                            MeetsCriteriaForBakerActCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value),
                                            BakerActRouteCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value)
                                        }
                                    };
                                    FASAMSValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    FASAMSValidations.ProcessDiagnosis(admission, updatedDx, evalDate);
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
                                        ClientDemographic = new ClientDemographic
                                        {
                                            VeteranStatusCode = (pamRow.Where(r => r.Name == "VetStatus").Single().Value),
                                            MaritalStatusCode = (pamRow.Where(r => r.Name == "Marital").Single().Value),
                                            ResidenceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyResid").Single().Value),
                                            ResidencePostalCode = (pamRow.Where(r => r.Name == "Zip").Single().Value),
                                        },
                                        FinancialAndHousehold = new FinancialAndHousehold
                                        {
                                            PrimaryIncomeSourceCode = (pamRow.Where(r => r.Name == "PIncoSrc").Single().Value),
                                            AnnualPersonalIncomeKnownCode = "0",
                                            AnnualPersonalIncomeAmount = 0,
                                            AnnualPersonalIncomeAmountSpecified = false,
                                            AnnualFamilyIncomeKnownCode = FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
                                            DependentsKnownCode = "0",
                                            DependentsCount = 0,
                                            DependentsCountSpecified = false
                                        },
                                        Health = new Health
                                        {
                                            UnableToPerformDailyLivingActivitiesCode = (pamRow.Where(r => r.Name == "ADLFc").Single().Value)
                                        },
                                        EducationAndEmployment = new EducationAndEmployment
                                        {
                                            EducationGradeLevelCode = (pamRow.Where(r => r.Name == "Grade").Single().Value),
                                            SchoolAttendanceStatusCode = "",
                                            SchoolDaysAvailableInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
                                        },
                                        Recovery = new Recovery
                                        {
                                            SelfHelpGroupAttendanceFrequencyCode = (pamRow.Where(r => r.Name == "Social").Single().Value)
                                        },
                                        MentalHealth = new MentalHealth
                                        {
                                            MentalHealthProblemRiskCode = (pamRow.Where(r => r.Name == "MhProb").Single().Value),
                                            HasRiskFactorsForEmotionalDisturbanceCode = (pamRow.Where(r => r.Name == "RiskFact").Single().Value),
                                            PrognosisStatusCode = (pamRow.Where(r => r.Name == "Prognosis").Single().Value)
                                        },
                                        Medication = new Medication
                                        {
                                            ReceivedPrescriptionsThroughIndigentDrugProgramCode = (pamRow.Where(r => r.Name == "RxIDP").Single().Value),
                                            ReceivedPrescriptionsThroughPatientAssistanceProgramCode = (pamRow.Where(r => r.Name == "RxPAP").Single().Value),
                                            TakingAntipsychoticMedicationCode = (pamRow.Where(r => r.Name == "Rx").Single().Value)
                                        },
                                        Legal = new Legal
                                        {
                                            ArrestsInLast30DaysKnownCode = FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            HasBeenCommittedToJuvenileJusticeCode = (pamRow.Where(r => r.Name == "DJJCommit").Single().Value),
                                            MeetsCriteriaForBakerActCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value),
                                            BakerActRouteCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value)
                                        }
                                    };
                                    FASAMSValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    FASAMSValidations.ProcessDiagnosis(admission, updatedDx, evalDate);
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    var dischargeType = pamRow.Where(r => r.Name == "Purpose").Single().Value;
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Discharge discharge = DataTools.OpportuniticlyLoadDischarge(admission, evalDate);
                                    discharge.StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.TypeCode = "2";
                                    discharge.DischargeDate = evalDate;
                                    discharge.LastContactDate = evalDate;
                                    discharge.DischargeReasonCode = FASAMSValidations.ValidateDischargeReasonCode((pamRow.Where(r => r.Name == "DReason").Single().Value).Trim());
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
                                        ClientDemographic = new ClientDemographic
                                        {
                                            VeteranStatusCode = (pamRow.Where(r => r.Name == "VetStatus").Single().Value),
                                            MaritalStatusCode = (pamRow.Where(r => r.Name == "Marital").Single().Value),
                                            ResidenceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyResid").Single().Value),
                                            ResidencePostalCode = (pamRow.Where(r => r.Name == "Zip").Single().Value),
                                        },
                                        FinancialAndHousehold = new FinancialAndHousehold
                                        {
                                            PrimaryIncomeSourceCode = (pamRow.Where(r => r.Name == "PIncoSrc").Single().Value),
                                            AnnualPersonalIncomeKnownCode = "0",
                                            AnnualPersonalIncomeAmount = 0,
                                            AnnualPersonalIncomeAmountSpecified = false,
                                            AnnualFamilyIncomeKnownCode = FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (FASAMSValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (FASAMSValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
                                            DependentsKnownCode = "0",
                                            DependentsCount = 0,
                                            DependentsCountSpecified = false
                                        },
                                        Health = new Health
                                        {
                                            UnableToPerformDailyLivingActivitiesCode = (pamRow.Where(r => r.Name == "ADLFc").Single().Value)
                                        },
                                        EducationAndEmployment = new EducationAndEmployment
                                        {
                                            EducationGradeLevelCode = (pamRow.Where(r => r.Name == "Grade").Single().Value),
                                            SchoolAttendanceStatusCode = "",
                                            SchoolDaysAvailableInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (FASAMSValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (FASAMSValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
                                        },
                                        Recovery = new Recovery
                                        {
                                            SelfHelpGroupAttendanceFrequencyCode = (pamRow.Where(r => r.Name == "Social").Single().Value)
                                        },
                                        MentalHealth = new MentalHealth
                                        {
                                            MentalHealthProblemRiskCode = (pamRow.Where(r => r.Name == "MhProb").Single().Value),
                                            HasRiskFactorsForEmotionalDisturbanceCode = (pamRow.Where(r => r.Name == "RiskFact").Single().Value),
                                            PrognosisStatusCode = (pamRow.Where(r => r.Name == "Prognosis").Single().Value)
                                        },
                                        Medication = new Medication
                                        {
                                            ReceivedPrescriptionsThroughIndigentDrugProgramCode = (pamRow.Where(r => r.Name == "RxIDP").Single().Value),
                                            ReceivedPrescriptionsThroughPatientAssistanceProgramCode = (pamRow.Where(r => r.Name == "RxPAP").Single().Value),
                                            TakingAntipsychoticMedicationCode = (pamRow.Where(r => r.Name == "Rx").Single().Value)
                                        },
                                        Legal = new Legal
                                        {
                                            ArrestsInLast30DaysKnownCode = FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (FASAMSValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            HasBeenCommittedToJuvenileJusticeCode = (pamRow.Where(r => r.Name == "DJJCommit").Single().Value),
                                            MeetsCriteriaForBakerActCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value),
                                            BakerActRouteCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value)
                                        }
                                    };
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    FASAMSValidations.ProcessPerformanceOutcomeMeasure(discharge, performanceOutcomeMeasure);
                                    FASAMSValidations.ProcessDiagnosis(admission, discharge, updatedDx, evalDate, dischargeType);
                                    FASAMSValidations.ProcessDischarge(admission, discharge);
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                            case PAMValidations.UpdateType.ImDischarge:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    ImmediateDischarge immediateDischarge = DataTools.OpportuniticlyLoadImmediateDischarge(treatmentEpisode, type, evalDate);
                                    immediateDischarge.StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.EvaluationDate = evalDate;
                                    FASAMSValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                }
                rowNum++;
            }
            Console.WriteLine("Completed Conversion of PAM PERF file.");
        }
        public static void InvokeCFARSConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM CFARS file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            InvokeCFARSConversion(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Writing of FASAMS CFARS data.");
        }
        public static void InvokeCFARSConversion(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM CFARS file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-CFARS-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-CFARS.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = DataTools.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    string evalDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        var type = PAMValidations.ValidateEvalPurpose(FileType.CFAR, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == "6").SingleOrDefault();
                                    var score = FASAMSValidations.ValidateEvalToolScore(FileType.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = "6",
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == "6").SingleOrDefault();
                                    var score = FASAMSValidations.ValidateEvalToolScore(FileType.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = "6",
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Discharge discharge = DataTools.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == "6").SingleOrDefault();
                                    var score = FASAMSValidations.ValidateEvalToolScore(FileType.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = "6",
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier
                                        };
                                        discharge.Evaluations.Add(newEvaluation);
                                    }
                                    FASAMSValidations.ProcessDischarge(admission, discharge);
                                    FASAMSValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        DataTools.UpsertTreatmentSession(treatmentEpisode);
                                        if (!treatmentEpisodeDataSet.TreatmentEpisodes.Any(t => t.SourceRecordIdentifier == treatmentEpisode.SourceRecordIdentifier))
                                        {
                                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                        }
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
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                }
                rowNum++;
            }
            Console.WriteLine("Completed Conversion of PAM CFARS file.");
        }
        public static void InvokeServConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SERV file..");
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            InvokeServConversion(serviceEventsDataSet, inputFile);
            WriteXml(serviceEventsDataSet, outputFile, "ServiceEventDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SERV file.");
        }
        public static void InvokeServConversion(ServiceEvents serviceEventsDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SERV file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-SERV-D.xml";
                return;
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-SERV.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            foreach (var pamRow in pamFile)
            {
                try
                {
                    ProviderClient client = new ProviderClient
                    {
                        ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                    };
                    ServiceEvent service = new ServiceEvent
                    {
                        ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                        ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                        ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>()
                    };
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var recordDate = FASAMSValidations.ValidateFASAMSDate(pamRow.Where(r => r.Name == "ServDate").Single().Value);
                    client = DataTools.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    var treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(PAMValidations.TreatmentEpisodeType.Admission, recordDate, client.SourceRecordIdentifier, fedTaxId);
                    var admission = DataTools.OpportuniticlyLoadAdmission(treatmentEpisode, recordDate);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        service.SourceRecordIdentifier = Guid.NewGuid().ToString();
                        service.TypeCode = "1";
                        service.FederalTaxIdentifier = fedTaxId;
                        service.SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                        service.ContractNumber = null; //this may change depending on feedback from ME
                        service.SubcontractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value); //this may change depending on feedback from ME
                        service.EpisodeSourceRecordIdentifier = treatmentEpisode.SourceRecordIdentifier;
                        service.AdmissionSourceRecordIdentifier = admission.SourceRecordIdentifier;
                        service.ProgramAreaCode = FASAMSValidations.ValidateAdmissionProgramCode((pamRow.Where(r => r.Name == "ProgType").Single().Value), client.BirthDate, recordDate); //this may change depending on feedback from ME
                        service.TreatmentSettingCode = FASAMSValidations.ValidateTreatmentSettingCodeFromCoveredServiceCode((pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value).Trim());
                        service.StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)); //not in spec but ME has advised it must be for State to meet data requirements.
                        service.StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)); //not in spec but ME has advised it must be for State to meet data requirements.
                        service.CoveredServiceCode = (pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value);
                        service.HcpcsProcedureCode = (pamRow.Where(r => r.Name == "ProcCode").Single().Value);
                        service.ServiceDate = recordDate;
                        service.StartTime = (pamRow.Where(r => r.Name == "BeginTime").Single().Value).Trim();
                        service.ExpenditureOcaCode = "todo"; //todo
                        service.ServiceUnitCount = uint.Parse(pamRow.Where(r => r.Name == "Unit").Single().Value);
                        service.FundCode = (pamRow.Where(r => r.Name == "Fund").Single().Value);
                        service.ServiceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyServ").Single().Value);
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier1").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier1").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier2").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier2").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier3").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier3").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier4").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventCoveredServiceModifier modifier = new ServiceEventCoveredServiceModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier4").Single().Value.Trim()
                            };
                            service.ServiceEventCoveredServiceModifiers.Add(modifier);
                        }
                    }
                    try
                    {
                        DataTools.UpsertServiceEvent(service);
                        serviceEventsDataSet.serviceEvents.Add(service);
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
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "ServiceEventDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                }
                rowNum++;
            }
            Console.WriteLine("Completed Conversion of PAM SERV file.");
        }
        public static void InvokeEvntConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM EVNT file..");           
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            InvokeEvntConversion(serviceEventsDataSet, inputFile);
            WriteXml(serviceEventsDataSet, outputFile, "ServiceEventDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM EVNT file.");
        }
        public static void InvokeEvntConversion(ServiceEvents serviceEventsDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM EVNT file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-EVNT-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-EVNT.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            foreach (var pamRow in pamFile)
            {
                try
                {
                    ServiceEvent service = new ServiceEvent
                    {
                        ServiceEventCoveredServiceModifiers = new List<ServiceEventCoveredServiceModifier>(),
                        ServiceEventHcpcsProcedureModifiers = new List<ServiceEventHcpcsProcedureModifier>(),
                        ServiceEventExpenditureModifiers = new List<ServiceEventExpenditureModifier>()
                    };
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var recordDate = FASAMSValidations.ValidateFASAMSDate(pamRow.Where(r => r.Name == "ServDate").Single().Value);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        service.SourceRecordIdentifier = Guid.NewGuid().ToString();
                        service.TypeCode = "2";
                        service.FederalTaxIdentifier = fedTaxId;
                        service.SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                        service.ContractNumber = null; //this may change depending on feedback from ME
                        service.SubcontractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value); //this may change depending on feedback from ME
                        service.ProgramAreaCode = (pamRow.Where(r => r.Name == "ProgType").Single().Value); //this may change depending on feedback from ME
                        service.TreatmentSettingCode = FASAMSValidations.ValidateTreatmentSettingCodeFromCoveredServiceCode((pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value).Trim());
                        service.StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)); //not in spec but ME has advised it must be for State to meet data requirements.
                        service.StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)); //not in spec but ME has advised it must be for State to meet data requirements.
                        service.CoveredServiceCode = (pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value);
                        service.HcpcsProcedureCode = (pamRow.Where(r => r.Name == "ProcCode").Single().Value);
                        service.ServiceDate = recordDate;
                        service.ServiceUnitCount = uint.Parse(pamRow.Where(r => r.Name == "Unit").Single().Value);
                        service.FundCode = (pamRow.Where(r => r.Name == "Fund").Single().Value);
                        service.ServiceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyServ").Single().Value);
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier1").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier1").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier2").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier2").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier3").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventHcpcsProcedureModifier modifier = new ServiceEventHcpcsProcedureModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier3").Single().Value.Trim()
                            };
                            service.ServiceEventHcpcsProcedureModifiers.Add(modifier);
                        }
                        if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "Modifier4").Single().Value)) // the spec is wrong based on the data that the PAM defines here according to ME CFCHS.
                        {
                            ServiceEventCoveredServiceModifier modifier = new ServiceEventCoveredServiceModifier
                            {
                                ModifierCode = pamRow.Where(r => r.Name == "Modifier4").Single().Value.Trim()
                            };
                            service.ServiceEventCoveredServiceModifiers.Add(modifier);
                        }
                    }
                    try
                    {
                        DataTools.UpsertServiceEvent(service);
                        serviceEventsDataSet.serviceEvents.Add(service);
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
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "ServiceEventDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                }
                rowNum++;
            }
            Console.WriteLine("Completed Conversion of PAM EVNT file.");
        }
        #endregion
        #region internal functions
        private static string PAMMappingFile;
        private static List<Field> GetFields(string mappingFile)
        {
            List<Field> fields = new List<Field>();
            XmlDocument map = new XmlDocument();
            //Load the mapping file into the XmlDocument
            map.Load(mappingFile);
            //Load the field nodes.
            XmlNodeList fieldNodes = map.SelectNodes("/FileMap/Field");
            //Loop through the nodes and create a field object
            // for each one.
            foreach (XmlNode fieldNode in fieldNodes)
            {
                Field field = new Field
                {

                    //Set the field's name
                    Name = fieldNode.Attributes["Name"].Value,

                    //Set the field's length
                    Length =
                     Convert.ToInt32(fieldNode.Attributes["Length"].Value),

                    //Set the field's starting position
                    Start =
                      Convert.ToInt32(fieldNode.Attributes["Start"].Value)
                };

                //Add the field to the Field list.
                fields.Add(field);
            }

            return fields;
        }
        private static List<List<Field>> ParseFile(string inputFile, string mappingFile)
        {
            //Get the field mapping.
            List<Field> fields = GetFields(mappingFile);
            //Create a List<List<Field>> collection of collections.
            // The main collection contains our records, and the
            // sub collection contains the fields each one of our
            // records contains.
            List<List<Field>> records = new List<List<Field>>();
            //Open the flat file using a StreamReader.
            using (StreamReader reader = new StreamReader(inputFile))
            {
                //Load the first line of the file.
                string line = reader.ReadLine();

                //Loop through the file until there are no lines
                // left.
                while (line != null)
                {
                    //Create out record (field collection)
                    List<Field> record = new List<Field>();

                    //Loop through the mapped fields
                    foreach (Field field in fields)
                    {
                        Field fileField = new Field();

                        //Use the mapped field's start and length
                        // properties to determine where in the
                        // line to pull our data from.
                        try
                        {
                            fileField.Value =
                            line.Substring(field.Start, field.Length);
                        }
                        catch {
                            fileField.Value = "";
                        }

                        //Set the name of the field.
                        fileField.Name = field.Name;
                        fileField.Start = field.Start;
                        fileField.Length = field.Length;
                        //Add the field to our record.
                        record.Add(fileField);
                    }

                    //Add the record to our record collection
                    records.Add(record);

                    //Read the next line.
                    line = reader.ReadLine();
                }
            }

            //Return all of our records.
            return records;
        }
        private static void WriteXml(object dataStructure, string outputFile, string outputFileName, string outputPath)
        {
            if (outputFile == null)
            {
                outputFile = outputPath + "\\" + outputFileName + "_" + (DateTime.Now.ToString("yyyyMMddHHmmss")) + ".xml";
            }
            Console.WriteLine("Writing Output File {0}",outputFile);
            Type t = dataStructure.GetType();
            XmlSerializer writer = new XmlSerializer(t);
            FileStream file = File.Create(outputFile);
            writer.Serialize(file, dataStructure);
            file.Close();
        }
        private static void WriteErrorLog(Exception ex, string outputFileName,string outputPath, string inputFile, int rowNum)
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
        #endregion
    }

    public class Field
    {
        public string Name { get; set; }
        public int Length { get; set; }
        public int Start { get; set; }
        public string Value { get; set; }
    }
}
