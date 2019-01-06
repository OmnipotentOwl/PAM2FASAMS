using PAM2FASAMS.Models.Utils;
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
        #region Globals
        public static int JobNumber;
        #endregion
        #region Conversion Functions
        public async Task RunBatchJobAsync(IEnumerable<InputFile> inputFiles, Options options)
        {
            var dt = new DataTools();
            JobNumber = dt.GetMaxJobNumber()+1;
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>() };
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            await ProcessInputFiles(inputFiles, options, clientDataSet, treatmentEpisodeDataSet, serviceEventsDataSet);
            await InvokeChronologicalReorganizationValidation();
            clientDataSet.clients.Clear();
            treatmentEpisodeDataSet.TreatmentEpisodes.Clear();
            serviceEventsDataSet.serviceEvents.Clear();
            await CreateOutputDataSet(clientDataSet, DataSetTypes.Client);
            await CreateOutputDataSet(treatmentEpisodeDataSet, DataSetTypes.TreatmentEpisode);
            await CreateOutputDataSet(serviceEventsDataSet, DataSetTypes.ServiceEvent);
            WriteXml(clientDataSet, null, "ClientDataSet", options.Directory);
            WriteXml(treatmentEpisodeDataSet, null, "TreatmentEpisodeDataSet", options.Directory);
            WriteXml(serviceEventsDataSet, null, "ServiceEventDataSet", options.Directory);
            await dt.MarkJobBatchComplete(JobNumber);
        }
        public async Task InvokeSSNConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SSN update file..");
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>() };
            await InvokeSSNConversionAsync(clientDataSet, inputFile);
            WriteXml(clientDataSet, outputFile, "ClientDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SSN update file.");
        }
        public async Task InvokeSSNConversionAsync(ProviderClients clientDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SSN update file..");
            PAMMappingFile = @"InputFormats/PAM-SSN.xml";
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                ProviderClient client = new ProviderClient
                {
                    ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                };
                var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "OldSSN").Single().Value));
                var newClientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "NewSSN").Single().Value));
                client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                fValidations.ProcessProviderClientIdentifiers(client, newClientId);
                try
                {
                    await dt.UpsertProviderClient(client);
                    clientDataSet.clients.Add(client);
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "ClientDataSet", Path.GetDirectoryName(inputFile));
                    pamErrors.Add(pamRow);
                }
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM SSN update file.");
        }
        public async Task InvokeDemoConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM DEMO file..");
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>()};
            await InvokeDemoConversionAsync(clientDataSet, inputFile);
            WriteXml(clientDataSet, outputFile, "ClientDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM DEMO file.");
        }
        public async Task InvokeDemoConversionAsync(ProviderClients clientDataSet, string inputFile)
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
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
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
                        var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                        client = await dt.OpportuniticlyLoadProviderClient(clientDataSet, clientId, fedTaxId);
                        client.action = "delete";
                    }
                    else
                    {
                        var sourceRecordId = (pamRow.Where(r => r.Name == "ClientID").Single().Value);
                        var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                        client = await dt.OpportuniticlyLoadProviderClient(clientDataSet, sourceRecordId, fedTaxId);
                        if(client.SourceRecordIdentifier == null)
                        {
                            client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                        }
                        if(client.SourceRecordIdentifier == null)
                        {
                            client.SourceRecordIdentifier = sourceRecordId;
                        }
                        client.FederalTaxIdentifier = fedTaxId;
                        client.BirthDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "DOB").Single().Value));
                        client.FirstName = (pamRow.Where(r => r.Name == "First").Single().Value).Trim();
                        client.MiddleName = (pamRow.Where(r => r.Name == "Middle").Single().Value).Trim();
                        client.LastName = (pamRow.Where(r => r.Name == "Last").Single().Value).Trim();
                        client.SuffixName = (pamRow.Where(r => r.Name == "Suffix").Single().Value).Trim();
                        client.GenderCode = (pamRow.Where(r => r.Name == "Gender").Single().Value);
                        client.RaceCode = (pamRow.Where(r => r.Name == "Race").Single().Value);
                        client.EthnicityCode = (pamRow.Where(r => r.Name == "Ethnic").Single().Value);
                        if(client?.action == "delete")
                        {
                            client.action = "undo-delete";
                        }
                        
                        fValidations.ProcessProviderClientIdentifiers(client, clientId);
                    }
                    try
                    {
                        await dt.UpsertProviderClient(client);
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
                    pamErrors.Add(pamRow);
                }
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM DEMO file.");
        }
        public async Task InvokeSAAdmitConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SA ADMSN file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokeSAAdmitConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SA ADMSN file.");
        }
        public async Task InvokeSAAdmitConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SA ADMSN file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-SAADMSN-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-SAADMSN.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    var type = pValidations.ValidateEvalPurpose(FileType.SAPERFA, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                    string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    string programCode = fValidations.ValidateAdmissionProgramCode("SA", client.BirthDate, evalDate);
                                    string subContNum = (pamRow.Where(r => r.Name == "ContNum1").Single().Value);
                                    var contract = await dt.OpportuniticlyLoadSubcontract(subContNum, evalDate, fedTaxId);
                                    if (treatmentEpisode.Admissions != null && treatmentEpisode.Admissions.Count > 0)
                                    {
                                        Admission initialAdmit = treatmentEpisode.Admissions.Where(a => a.TypeCode == "1").Single();
                                        if (initialAdmit.InternalAdmissionDate < DateTime.Parse(evalDate) && initialAdmit.ProgramAreaCode == programCode)
                                        {
                                            treatmentEpisode = new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = client.SourceRecordIdentifier, FederalTaxIdentifier = fedTaxId, Admissions = new List<Admission>() };
                                        }
                                    }
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    if (admission.AdmissionDate == null)
                                    {
                                        admission.SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                                        admission.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.ContractNumber = contract?.ContractNumber; //subject to change based on ME feedback.
                                        admission.SubcontractNumber = contract?.SubcontractNumber; //subject to change based on ME feedback.
                                        admission.AdmissionDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                                        admission.ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value);
                                        admission.TypeCode = "1";
                                        admission.ProgramAreaCode = fValidations.ValidateAdmissionProgramCode("SA", client.BirthDate, evalDate);
                                        admission.TreatmentSettingCode = "0"; //updated by ServiceEvent Data.
                                        admission.IsCodependentCode = (pamRow.Where(r => r.Name == "Collateral").Single().Value);
                                        admission.DaysWaitingToEnterTreatmentKnownCode = fValidations.ValidateWaitingDaysAvailable((pamRow.Where(r => r.Name == "WaitDays").Single().Value));
                                        admission.DaysWaitingToEnterTreatmentNumber = ((fValidations.ValidateWaitingDaysAvailable((pamRow.Where(r => r.Name == "WaitDays").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "WaitDays").Single().Value) : 0);
                                        admission.PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>();
                                        admission.Diagnoses = new List<Diagnosis>();
                                    }
                                    Admission newAdmission = new Admission
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        ContractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value),
                                        AdmissionDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
                                        ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value),
                                        TypeCode = "1",
                                        ProgramAreaCode = fValidations.ValidateAdmissionProgramCode("SA", client.BirthDate, evalDate),
                                        TreatmentSettingCode = "0", //updated by ServiceEvent Data.
                                        IsCodependentCode = (pamRow.Where(r => r.Name == "Collateral").Single().Value),
                                        DaysWaitingToEnterTreatmentKnownCode = fValidations.ValidateWaitingDaysAvailable((pamRow.Where(r => r.Name == "WaitDays").Single().Value)),
                                        DaysWaitingToEnterTreatmentNumber = ((fValidations.ValidateWaitingDaysAvailable((pamRow.Where(r => r.Name == "WaitDays").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "WaitDays").Single().Value) : 0),
                                        PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>(),
                                        Diagnoses = new List<Diagnosis>(),
                                    };

                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
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
                                            AnnualPersonalIncomeKnownCode = fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "IncoPers").Single().Value)),
                                            AnnualPersonalIncomeAmount = ((fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "IncoPers").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "IncoPers").Single().Value) : 0),
                                            AnnualPersonalIncomeAmountSpecified = (fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "IncoPers").Single().Value)) == "1") ? true : false,
                                            AnnualFamilyIncomeKnownCode = fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
                                            DependentsKnownCode = fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "Depend").Single().Value)),
                                            DependentsCount = ((fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "Depend").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Depend").Single().Value) : 0),
                                            DependentsCountSpecified = (fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "Depend").Single().Value)) == "1") ? true : false
                                        },
                                        Health = new Health
                                        {
                                            PregnancyTrimesterCode = (pamRow.Where(r => r.Name == "PregTrim").Single().Value),
                                            RecentlyBecomePostpartumCode = (pamRow.Where(r => r.Name == "PostPart").Single().Value),
                                            IntravenousSubstanceHistoryCode = (pamRow.Where(r => r.Name == "IVHist").Single().Value)
                                        },
                                        EducationAndEmployment = new EducationAndEmployment
                                        {
                                            EducationGradeLevelCode = (pamRow.Where(r => r.Name == "Grade").Single().Value),
                                            SchoolAttendanceStatusCode = "",
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                        },
                                        Recovery = new Recovery
                                        {
                                            SelfHelpGroupAttendanceFrequencyCode = (pamRow.Where(r => r.Name == "Social").Single().Value)
                                        },
                                        SubstanceUseDisorders = new List<SubstanceUseDisorder>(),
                                        Medication = new Medication
                                        {
                                            MedicationAssistedOpioidTherapyCode = (pamRow.Where(r => r.Name == "OpioidReplac").Single().Value)
                                        },
                                        Legal = new Legal
                                        {
                                            ArrestsInLast30DaysKnownCode = fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            LegalGuardianRelationshipCode = (pamRow.Where(r => r.Name == "LegGuard").Single().Value),
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            MeetsCriteriaForMarchmanActCode = (pamRow.Where(r => r.Name == "Marchman").Single().Value),
                                            MarchmanActTypeCode = (pamRow.Where(r => r.Name == "Marchman").Single().Value),
                                            DrugCourtOrderedCode = (pamRow.Where(r => r.Name == "DrugCrt").Single().Value)
                                        }
                                    };
                                    List<SubstanceUseDisorder> updatedSUD = new List<SubstanceUseDisorder>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbPrim").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "1",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbPrim").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutPrim").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqPrim").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgePrim").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbSec").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "2",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbSec").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutSec").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqSec").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgeSec").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbTer").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "3",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbTer").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutTer").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqTer").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgeTer").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    performanceOutcomeMeasure.SubstanceUseDisorders = updatedSUD;
                                    fValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    fValidations.ProcessDiagnosis(admission, updatedDx, evalDate);
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    
                                    break;
                                }
                            case PAMValidations.UpdateType.ImDischarge:
                                {
                                    ImmediateDischarge immediateDischarge = await dt.OpportuniticlyLoadImmediateDischarge(treatmentEpisode, type, evalDate);
                                    immediateDischarge.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.EvaluationDate = evalDate;
                                    fValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    break;
                                }
                        }
                    }
                    try
                    {
                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM SA ADMSN file.");
        }
        public async Task InvokeSADischargeConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SA DCHRG file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokeSADischargeConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SA DCHRG file.");
        }
        public async Task InvokeSADischargeConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SA DCHRG file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-SADCHRG-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-SADCHRG.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        var type = pValidations.ValidateEvalPurpose(FileType.SAPERFD, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                        string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    var dischargeType = pamRow.Where(r => r.Name == "Purpose").Single().Value;
                                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    discharge.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.TypeCode = "2";
                                    discharge.DischargeDate = evalDate;
                                    discharge.LastContactDate = evalDate;
                                    discharge.DischargeReasonCode = fValidations.ValidateDischargeReasonCode((pamRow.Where(r => r.Name == "DReason").Single().Value).Trim());
                                    discharge.BirthOutcomeCode = (pamRow.Where(r => r.Name == "DOutcome").Single().Value);
                                    discharge.DrugFreeAtDeliveryCode = (pamRow.Where(r => r.Name == "DrugFree").Single().Value);
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
                                        ClientDemographic = new ClientDemographic
                                        {
                                            //VeteranStatusCode = (pamRow.Where(r => r.Name == "VetStatus").Single().Value),
                                            MaritalStatusCode = (pamRow.Where(r => r.Name == "Marital").Single().Value),
                                            ResidenceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyResid").Single().Value),
                                            //ResidencePostalCode = (pamRow.Where(r => r.Name == "Zip").Single().Value),
                                        },
                                        Health = new Health
                                        {
                                            PregnancyTrimesterCode = (pamRow.Where(r => r.Name == "PregTrim").Single().Value),
                                            //RecentlyBecomePostpartumCode = (pamRow.Where(r => r.Name == "PostPart").Single().Value),
                                            //IntravenousSubstanceHistoryCode = (pamRow.Where(r => r.Name == "IVHist").Single().Value)
                                        },
                                        EducationAndEmployment = new EducationAndEmployment
                                        {
                                            EducationGradeLevelCode = (pamRow.Where(r => r.Name == "Grade").Single().Value),
                                            SchoolAttendanceStatusCode = "",
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                        },
                                        Recovery = new Recovery
                                        {
                                            SelfHelpGroupAttendanceFrequencyCode = (pamRow.Where(r => r.Name == "Social").Single().Value)
                                        },
                                        SubstanceUseDisorders = new List<SubstanceUseDisorder>(),
                                        Legal = new Legal
                                        {
                                            ArrestsInLast30DaysKnownCode = fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            //LegalGuardianRelationshipCode = (pamRow.Where(r => r.Name == "LegGuard").Single().Value),
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            //MeetsCriteriaForMarchmanActCode = (pamRow.Where(r => r.Name == "Marchman").Single().Value),
                                            //MarchmanActTypeCode = (pamRow.Where(r => r.Name == "Marchman").Single().Value),
                                            DrugCourtOrderedCode = (pamRow.Where(r => r.Name == "DrugCrt").Single().Value)
                                        }
                                    };
                                    List<SubstanceUseDisorder> updatedSUD = new List<SubstanceUseDisorder>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbPrim").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "1",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbPrim").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutPrim").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqPrim").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgePrim").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbSec").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "2",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbSec").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutSec").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqSec").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgeSec").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "ProbTer").Single().Value))
                                    {
                                        SubstanceUseDisorder sud = new SubstanceUseDisorder
                                        {
                                            DisorderRankCode = "3",
                                            DisorderCode = (pamRow.Where(r => r.Name == "ProbTer").Single().Value),
                                            RouteOfAdministrationCode = (pamRow.Where(r => r.Name == "RoutTer").Single().Value),
                                            FrequencyofUseCode = (pamRow.Where(r => r.Name == "FreqTer").Single().Value),
                                            FirstUseAge = pamRow.Where(r => r.Name == "AgeTer").Single().Value.Trim(),
                                            PerfSourceId = performanceOutcomeMeasure.SourceRecordIdentifier
                                        };
                                        updatedSUD.Add(sud);
                                    }
                                    performanceOutcomeMeasure.SubstanceUseDisorders = updatedSUD;
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    fValidations.ProcessPerformanceOutcomeMeasure(discharge, performanceOutcomeMeasure);
                                    fValidations.ProcessDiagnosis(admission, discharge, updatedDx, evalDate, dischargeType);
                                    fValidations.ProcessDischarge(admission, discharge);
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    try
                                    {
                                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM SA DCHRG file.");
        }
        public async Task InvokePerfConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM PERF file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokePerfConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM PERF file.");
        }
        public async Task InvokePerfConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
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
            string typeCode = "2"; //for use with CGAS Eval
            string toolCode = "9"; //for use with CGAS Eval
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    var type = pValidations.ValidateEvalPurpose(FileType.PERF, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                    string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                    if (IsDelete)
                    {
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    if(admission.ProgramAreaCode == "1" || admission.ProgramAreaCode == "3")
                                    {
                                        admission.action = "delete";
                                    }
                                    
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = admission.PerformanceOutcomeMeasures.Where(p => p.InternalPerformanceOutcomeMeasureDate == DateTime.Parse(evalDate)).LastOrDefault();
                                    performanceOutcomeMeasure.action = "delete";
                                    fValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    discharge.action = "delete";
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(discharge, evaluation);
                                    fValidations.ProcessDischarge(admission, discharge);
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    break;
                                }
                            case PAMValidations.UpdateType.ImDischarge:
                                {
                                    ImmediateDischarge immediateDischarge = await dt.OpportuniticlyLoadImmediateDischarge(treatmentEpisode, type, evalDate);
                                    immediateDischarge.action = "delete";
                                    fValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    break;
                                }
                        }
                    }
                    else
                    {                      
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    
                                    string programCode = fValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate);
                                    string subContNum = (pamRow.Where(r => r.Name == "ContNum1").Single().Value);
                                    var contract = await dt.OpportuniticlyLoadSubcontract(subContNum, evalDate, fedTaxId);
                                    if (treatmentEpisode.Admissions != null && treatmentEpisode.Admissions.Count > 0)
                                    {
                                        Admission initialAdmit = treatmentEpisode.Admissions.Where(a => a.TypeCode == "1").Single();
                                        if (initialAdmit.InternalAdmissionDate < DateTime.Parse(evalDate) && initialAdmit.ProgramAreaCode == programCode)
                                        {
                                            treatmentEpisode = new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), ClientSourceRecordIdentifier = client.SourceRecordIdentifier, FederalTaxIdentifier = fedTaxId, Admissions = new List<Admission>() };
                                        }
                                    }
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    if (admission.AdmissionDate == null)
                                    {
                                        admission.SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                                        admission.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                        admission.ContractNumber = contract?.ContractNumber; //subject to change based on ME feedback.
                                        admission.SubcontractNumber = contract?.SubcontractNumber; //subject to change based on ME feedback.
                                        admission.AdmissionDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value));
                                        admission.ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value);
                                        admission.TypeCode = "1";
                                        admission.ProgramAreaCode = fValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate);
                                        admission.TreatmentSettingCode = "0"; //updated by ServiceEvent Data.
                                        admission.IsCodependentCode = fValidations.ValidateAdmissionCoDependent(fValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate));
                                        admission.DaysWaitingToEnterTreatmentKnownCode = "0";
                                        admission.PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>();
                                        admission.Diagnoses = new List<Diagnosis>();
                                    }
                                    Admission newAdmission = new Admission
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        ContractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value),
                                        AdmissionDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value)),
                                        ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value),
                                        TypeCode = "1",
                                        ProgramAreaCode = fValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate),
                                        TreatmentSettingCode = "0", //updated by ServiceEvent Data.
                                        IsCodependentCode = fValidations.ValidateAdmissionCoDependent(fValidations.ValidateAdmissionProgramCode("MH", client.BirthDate, evalDate)),
                                        DaysWaitingToEnterTreatmentKnownCode = "0",
                                        PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>(),
                                        Diagnoses = new List<Diagnosis>(),
                                    };

                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
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
                                            AnnualFamilyIncomeKnownCode = fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
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
                                            SchoolDaysAvailableInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
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
                                            ArrestsInLast30DaysKnownCode = fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            HasBeenCommittedToJuvenileJusticeCode = (pamRow.Where(r => r.Name == "DJJCommit").Single().Value),
                                            MeetsCriteriaForBakerActCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value),
                                            BakerActRouteCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value)
                                        }
                                    };
                                    fValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CGAS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        if (!string.IsNullOrWhiteSpace(newEvaluation.ScoreCode))
                                        {
                                            admission.Evaluations.Add(newEvaluation);
                                        }
                                    }

                                    fValidations.ProcessDiagnosis(admission, updatedDx, evalDate);
                                    if (admission.action == "delete")
                                    {
                                        admission.action = "undo-delete";
                                    }
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);                                  
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
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
                                            AnnualFamilyIncomeKnownCode = fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
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
                                            SchoolDaysAvailableInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
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
                                            ArrestsInLast30DaysKnownCode = fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
                                            IsVoluntarilyInTreatmentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            IsLegallyIncompetentCode = (pamRow.Where(r => r.Name == "AdmiType").Single().Value),
                                            LegalStatusCode = "",
                                            ChildrenDependencyOrDelinquencyStatusCode = (pamRow.Where(r => r.Name == "DepCrimS").Single().Value),
                                            HasBeenCommittedToJuvenileJusticeCode = (pamRow.Where(r => r.Name == "DJJCommit").Single().Value),
                                            MeetsCriteriaForBakerActCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value),
                                            BakerActRouteCode = (pamRow.Where(r => r.Name == "BakerAct").Single().Value)
                                        }
                                    };
                                    fValidations.ProcessPerformanceOutcomeMeasure(admission, performanceOutcomeMeasure);
                                    List<Diagnosis> updatedDx = new List<Diagnosis>();
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CGAS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        if (!string.IsNullOrWhiteSpace(newEvaluation.ScoreCode))
                                        {
                                            fValidations.ProcessEvaluation(admission, newEvaluation);
                                        }
                                    }
                                    fValidations.ProcessDiagnosis(admission, updatedDx, evalDate);
                                    
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    var dischargeType = pamRow.Where(r => r.Name == "Purpose").Single().Value;
                                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    discharge.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    discharge.TypeCode = "2";
                                    discharge.DischargeDate = evalDate;
                                    discharge.LastContactDate = evalDate;
                                    discharge.DischargeReasonCode = fValidations.ValidateDischargeReasonCode((pamRow.Where(r => r.Name == "DReason").Single().Value).Trim());
                                    PerformanceOutcomeMeasure performanceOutcomeMeasure = new PerformanceOutcomeMeasure
                                    {
                                        SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                        StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                        PerformanceOutcomeMeasureDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value)),
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
                                            AnnualFamilyIncomeKnownCode = fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)),
                                            AnnualFamilyIncomeAmount = ((fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? decimal.Parse(pamRow.Where(r => r.Name == "FamInc").Single().Value) : 0),
                                            AnnualFamilyIncomeAmountSpecified = (fValidations.ValidateIncomeAvailable((pamRow.Where(r => r.Name == "FamInc").Single().Value)) == "1") ? true : false,
                                            PrimaryPaymentSourceCode = "",
                                            DisabilityIncomeStatusCode = (pamRow.Where(r => r.Name == "DisIncom").Single().Value),
                                            HealthInsuranceCode = "",
                                            TemporaryAssistanceForNeedyFamiliesStatusCode = (pamRow.Where(r => r.Name == "TStat").Single().Value),
                                            FamilySizeNumberKnownCode = fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)),
                                            FamilySizeNumber = ((fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "FamSize").Single().Value) : 0),
                                            FamilySizeNumberSpecified = (fValidations.ValidateFamilySizeAvailable((pamRow.Where(r => r.Name == "FamSize").Single().Value)) == "1") ? true : false,
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
                                            SchoolDaysAvailableInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)),
                                            SchoolDaysAvailableInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
                                            SchoolDaysAvailableInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value)) == "1") ? true : false,
                                            SchoolDaysAttendedInLast90DaysKnownCode = fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)),
                                            SchoolDaysAttendedInLast90DaysNumber = ((fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAtte").Single().Value) : 0),
                                            SchoolDaysAttendedInLast90DaysNumberSpecified = (fValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAtte").Single().Value)) == "1") ? true : false,
                                            SchoolSuspensionOrExpulsionStatusCode = (pamRow.Where(r => r.Name == "School").Single().Value),
                                            EmploymentStatusCode = (pamRow.Where(r => r.Name == "Empl").Single().Value),
                                            DaysWorkedInLast30DaysKnownCode = fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)),
                                            DaysWorkedInLast30DaysNumber = ((fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysWork").Single().Value) : 0),
                                            DaysWorkedInLast30DaysNumberSpecified = (fValidations.ValidateWorkDaysKnown((pamRow.Where(r => r.Name == "DaysWork").Single().Value)) == "1") ? true : false
                                        },
                                        StabilityOfHousing = new StabilityOfHousing
                                        {
                                            DaysSpentInCommunityInLast30DaysKnownCode = fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)),
                                            DaysSpentInCommunityInLast30DaysNumber = ((fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "DaysCom").Single().Value) : 0),
                                            DaysSpentInCommunityInLast30DaysNumberSpecified = (fValidations.ValidateCommunityDaysKnown((pamRow.Where(r => r.Name == "DaysCom").Single().Value)) == "1") ? true : false
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
                                            ArrestsInLast30DaysKnownCode = fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)),
                                            ArrestsInLast30DaysNumber = ((fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? int.Parse(pamRow.Where(r => r.Name == "Arrest").Single().Value) : 0),
                                            ArrestsInLast30DaysNumberSpecified = (fValidations.ValidateArrestsKnown((pamRow.Where(r => r.Name == "Arrest").Single().Value)) == "1") ? true : false,
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
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "2",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                                    {
                                        Diagnosis dx = new Diagnosis
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            CodeSetIdentifierCode = "3",
                                            DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                            StartDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value))
                                        };
                                        updatedDx.Add(dx);
                                    }
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CGAS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier
                                        };
                                        if (!string.IsNullOrWhiteSpace(newEvaluation.ScoreCode))
                                        {
                                            discharge.Evaluations.Add(newEvaluation);
                                        }
                                    }

                                    fValidations.ProcessPerformanceOutcomeMeasure(discharge, performanceOutcomeMeasure);
                                    fValidations.ProcessDiagnosis(admission, discharge, updatedDx, evalDate, dischargeType);
                                    if(discharge.action == "delete")
                                    {
                                        discharge.action = "undo-delete";
                                    }
                                    fValidations.ProcessDischarge(admission, discharge);
                                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    break;
                                }
                            case PAMValidations.UpdateType.ImDischarge:
                                {
                                    ImmediateDischarge immediateDischarge = await dt.OpportuniticlyLoadImmediateDischarge(treatmentEpisode, type, evalDate);
                                    immediateDischarge.StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value));
                                    immediateDischarge.EvaluationDate = evalDate;
                                    if(immediateDischarge.action == "delete")
                                    {
                                        immediateDischarge.action = "undo-delete";
                                    }
                                    fValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    break;
                                }
                        }
                    }
                    try
                    {
                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM PERF file.");
        }
        public async Task InvokeCFARSConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM CFARS file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokeCFARSConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Writing of FASAMS CFARS data.");
        }
        public async Task InvokeCFARSConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
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
            string toolCode = "6";
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            var pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    var type = pValidations.ValidateEvalPurpose(FileType.CFAR, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                    if (IsDelete)
                    {
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.CFAR, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier
                                        };
                                        discharge.Evaluations.Add(newEvaluation);
                                    }
                                    fValidations.ProcessDischarge(admission, discharge);
                                    break;
                                }

                        }
                    }
                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                    try
                    {
                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM CFARS file.");
        }
        public async Task InvokeFARSConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM FARS file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokeFARSConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Writing of FASAMS FARS data.");
        }
        public async Task InvokeFARSConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM FARS file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-FARS-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-FARS.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            string toolCode = "5";
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    var type = pValidations.ValidateEvalPurpose(FileType.FARS, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                    if (IsDelete)
                    {
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    evaluation.action = "delete";
                                    fValidations.ProcessEvaluation(admission, evaluation);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.FARS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }                                   
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.FARS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }                                   
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.FARS, pamRow);
                                    if (evaluation == null || evaluation.ScoreCode != score)
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = (pamRow.Where(r => r.Name == "EduLevel").Single().Value).Trim(),
                                            StaffIdentifier = (pamRow.Where(r => r.Name == "FMHINum").Single().Value).Trim(),
                                            TypeCode = "2",
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            ScoreCode = score,
                                            Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier
                                        };
                                        discharge.Evaluations.Add(newEvaluation);
                                    }
                                    fValidations.ProcessDischarge(admission, discharge);
                                    break;
                                }
                        }
                    }
                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                    try
                    {
                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM FARS file.");
        }
        public async Task InvokeASAMConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM ASAM file..");
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            await InvokeASAMConversionAsync(treatmentEpisodeDataSet, inputFile);
            WriteXml(treatmentEpisodeDataSet, outputFile, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Writing of FASAMS ASAM data.");
        }
        public async Task InvokeASAMConversionAsync(TreatmentEpisodeDataSet treatmentEpisodeDataSet, string inputFile)
        {
            Console.WriteLine("Starting Conversion of PAM ASAM file..");
            bool IsDelete = string.Equals(Path.GetExtension(inputFile), ".del", StringComparison.OrdinalIgnoreCase);
            if (IsDelete)
            {
                PAMMappingFile = @"InputFormats/PAM-ASAM-D.xml";
            }
            else
            {
                PAMMappingFile = @"InputFormats/PAM-ASAM.xml";
            }
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            int rowNum = 1;
            string typeCode = "1";
            string toolCode = "4";
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
            foreach (var pamRow in pamFile)
            {
                try
                {
                    var fedTaxId = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    string evalDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "EvalDate").Single().Value));
                    var type = pValidations.ValidateEvalPurpose(FileType.ASAM, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, client.SourceRecordIdentifier, fedTaxId);
                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        
                        string recommendedLvl = "todo"; //FASAMSValidations.ValidateEvalToolRLvl(FileType.ASAM,pamRow);
                        string actualLvl = "todo"; //FASAMSValidations.ValidateEvalToolALvl(FileType.ASAM, pamRow);
                        switch (type)
                        {
                            case PAMValidations.UpdateType.Admission:
                                {
                                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, PAMValidations.UpdateType.Update, evalDate, client.SourceRecordIdentifier, fedTaxId);
                                    admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, type, evalDate);
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    if (evaluation == null || (evaluation.RecommendedLevelCode != recommendedLvl || evaluation.ActualLevelCode != actualLvl))
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            DeterminationDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "ASAMDate").Single().Value)),
                                            RecommendedLevelCode = recommendedLvl,
                                            ActualLevelCode = actualLvl,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    break;
                                }
                            case PAMValidations.UpdateType.Update:
                                {
                                    Evaluation evaluation = admission.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    var score = fValidations.ValidateEvalToolScore(EvaluationToolTypes.ASAM, pamRow);
                                    if (evaluation == null || (evaluation.RecommendedLevelCode != recommendedLvl || evaluation.ActualLevelCode != actualLvl))
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            DeterminationDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "ASAMDate").Single().Value)),
                                            RecommendedLevelCode = recommendedLvl,
                                            ActualLevelCode = actualLvl,
                                            Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier
                                        };
                                        admission.Evaluations.Add(newEvaluation);
                                    }
                                    break;
                                }
                            case PAMValidations.UpdateType.Discharge:
                                {
                                    Discharge discharge = await dt.OpportuniticlyLoadDischarge(admission, evalDate);
                                    Evaluation evaluation = discharge.Evaluations.Where(e => e.EvaluationDate == evalDate && e.ToolCode == toolCode).SingleOrDefault();
                                    if (evaluation == null || (evaluation.RecommendedLevelCode != recommendedLvl || evaluation.ActualLevelCode != actualLvl))
                                    {
                                        Evaluation newEvaluation = new Evaluation
                                        {
                                            SourceRecordIdentifier = Guid.NewGuid().ToString(),
                                            StaffEducationLevelCode = fValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            StaffIdentifier = fValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                            TypeCode = typeCode,
                                            ToolCode = toolCode,
                                            EvaluationDate = evalDate,
                                            DeterminationDate = fValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "ASAMDate").Single().Value)),
                                            RecommendedLevelCode = recommendedLvl,
                                            ActualLevelCode = actualLvl,
                                            Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier
                                        };
                                        discharge.Evaluations.Add(newEvaluation);
                                    }
                                    fValidations.ProcessDischarge(admission, discharge);
                                    break;
                                }
                        }
                    }
                    fValidations.ProcessAdmission(treatmentEpisode, admission);
                    try
                    {
                        await dt.UpsertTreatmentSession(treatmentEpisode);
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
                }
                catch (Exception ex)
                {
                    WriteErrorLog(ex, "TreatmentEpisodeDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum);
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM ASAM file.");
        }
        public async Task InvokeServConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM SERV file..");
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            await InvokeServConversionAsync(serviceEventsDataSet, inputFile);
            WriteXml(serviceEventsDataSet, outputFile, "ServiceEventDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM SERV file.");
        }
        public async Task InvokeServConversionAsync(ServiceEvents serviceEventsDataSet, string inputFile)
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
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
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
                    var clientId = fValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value));
                    var recordDate = fValidations.ValidateFASAMSDate(pamRow.Where(r => r.Name == "ServDate").Single().Value);
                    client = await dt.OpportuniticlyLoadProviderClient(clientId, fedTaxId);
                    TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(PAMValidations.TreatmentEpisodeType.Admission, recordDate, client.SourceRecordIdentifier, fedTaxId);
                    string treatmentSetting = fValidations.ValidateTreatmentSettingCodeFromCoveredServiceCode((pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value).Trim());
                    var setting = pamRow.Where(r => r.Name == "Setting").Single().Value;
                    Admission admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, recordDate, treatmentSetting);
                    if (IsDelete)
                    {
                        string progCode = fValidations.ValidateAdmissionProgramCode((pamRow.Where(r => r.Name == "ProgType").Single().Value), client.BirthDate, recordDate); //this may change depending on feedback from ME
                        string covrdSvc = (pamRow.Where(r => r.Name == "CovrdSvcs").Single().Value);
                        ServiceEvent serviceEventDel = new ServiceEvent
                        {
                            EpisodeSourceRecordIdentifier = treatmentEpisode.SourceRecordIdentifier,
                            AdmissionSourceRecordIdentifier = admission.SourceRecordIdentifier,
                            TreatmentSettingCode = treatmentSetting,
                            ProgramAreaCode = progCode,
                            CoveredServiceCode = covrdSvc,
                            HcpcsProcedureCode = (pamRow.Where(r => r.Name == "ProcCode").Single().Value),
                            ServiceDate = recordDate,
                            StartTime = (pamRow.Where(r => r.Name == "BeginTime").Single().Value).Trim(),
                            ServiceCountyAreaCode = (pamRow.Where(r => r.Name == "CntyServ").Single().Value)
                        };
                    }
                    else
                    {
                        string contNum = null; //this may change depending on feedback from ME
                        string subcontNum = (pamRow.Where(r => r.Name == "ContNum1").Single().Value); //this may change depending on feedback from ME
                        var contract = await dt.OpportuniticlyLoadSubcontract(subcontNum, recordDate, fedTaxId);
                        if(admission.TreatmentSettingCode == "0")
                        {
                            await SetInitialTreatmentLevel(treatmentEpisode, admission, treatmentSetting);
                        }
                        if(admission.TreatmentSettingCode != treatmentSetting && admission.TreatmentSettingCode != "0")
                        {
                            string siteId = (pamRow.Where(r => r.Name == "SiteId").Single().Value);
                            await TransferTreatmentLevel(treatmentEpisode, admission, contract, treatmentSetting, recordDate, siteId);
                            admission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, recordDate, treatmentSetting);
                        }

                        var newService = fValidations.CreateServiceEvent(PAMValidations.ServiceEventType.Service, pamRow,recordDate,contract,treatmentEpisode,admission,client);
                        service = await dt.OpportuniticlyLoadServiceEvent(PAMValidations.ServiceEventType.Service, newService);
                        if (service.SourceRecordIdentifier == null)
                        {
                            service = newService;
                        }

                    }
                    try
                    {
                        await dt.UpsertServiceEvent(service);
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
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM SERV file.");
        }
        public async Task InvokeEvntConversionAsync(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM EVNT file..");           
            ServiceEvents serviceEventsDataSet = new ServiceEvents { serviceEvents = new List<ServiceEvent>() };
            await InvokeEvntConversionAsync(serviceEventsDataSet, inputFile);
            WriteXml(serviceEventsDataSet, outputFile, "ServiceEventDataSet", Path.GetDirectoryName(inputFile));
            Console.WriteLine("Completed Conversion of PAM EVNT file.");
        }
        public async Task InvokeEvntConversionAsync(ServiceEvents serviceEventsDataSet, string inputFile)
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
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            List<List<Field>> pamErrors = new List<List<Field>>();
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
                    var recordDate = fValidations.ValidateFASAMSDate(pamRow.Where(r => r.Name == "ServDate").Single().Value);
                    if (IsDelete)
                    {

                    }
                    else
                    {
                        string contNum = null; //this may change depending on feedback from ME
                        string subcontNum = (pamRow.Where(r => r.Name == "ContNum1").Single().Value); //this may change depending on feedback from ME
                        var contract = await dt.OpportuniticlyLoadSubcontract(subcontNum, recordDate, fedTaxId);
                        var newService = fValidations.CreateServiceEvent(PAMValidations.ServiceEventType.Event, pamRow, recordDate,contract);
                        service = await dt.OpportuniticlyLoadServiceEvent(PAMValidations.ServiceEventType.Event, newService);
                        if(service.SourceRecordIdentifier == null)
                        {
                            service = newService;
                        }
                    }
                    try
                    {
                        await dt.UpsertServiceEvent(service);
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
                    pamErrors.Add(pamRow);
                }
                rowNum++;
            }
            if (pamErrors.Count > 0)
            {
                WritePAMErrorFile(inputFile, pamErrors);
            }
            Console.WriteLine("Completed Conversion of PAM EVNT file.");
        }

        #endregion
        #region internal functions
        private async Task ProcessInputFiles(IEnumerable<InputFile> inputFiles, Options options, ProviderClients clientDataSet, 
            TreatmentEpisodeDataSet treatmentEpisodeDataSet, ServiceEvents serviceEventsDataSet)
        {
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
                        await InvokeSSNConversionAsync(clientDataSet, options.InputFile);
                        break;
                    case "DEMO":
                        await InvokeDemoConversionAsync(clientDataSet, options.InputFile);
                        break;
                    case "SAPERFA":
                        await InvokeSAAdmitConversionAsync(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "SAPERFD":
                        await InvokeSADischargeConversionAsync(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "SADT":
                        break;
                    case "PERF":
                        await InvokePerfConversionAsync(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "CFARS":
                        await InvokeCFARSConversionAsync(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "FARS":
                        await InvokeFARSConversionAsync(treatmentEpisodeDataSet, options.InputFile);
                        break;
                    case "ASAM":
                        break;
                    case "SERV":
                        await InvokeServConversionAsync(serviceEventsDataSet, options.InputFile);
                        break;
                    case "EVNT":
                        await InvokeEvntConversionAsync(serviceEventsDataSet, options.InputFile);
                        break;
                    case "SANDR":
                        break;
                    default:
                        break;
                }
            }
        }
        private async Task TransferTreatmentLevel(TreatmentEpisode episode, Admission admission, Subcontract contract, string newLevel, string recordDate, string siteId)
        {
            string existingLevel = admission.TreatmentSettingCode;
            Console.WriteLine("Automatic Transfer Invoked: {0} => {1}",existingLevel, newLevel);
            var pValidations = new PAMValidations();
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            Discharge transferDischarge = fValidations.CreateTransferDischarge(recordDate);
            Admission transferAdmission = fValidations.CreateTransferAdmission(recordDate, contract, episode, newLevel, admission, siteId);
            // need new function to handle transfer as the admission may already be final discharged or transfered and we are inserting out of order records.
            fValidations.ProcessDischarge(admission, transferDischarge);
            fValidations.ProcessAdmission(episode, admission);
            fValidations.ProcessAdmission(episode, transferAdmission);
            try
            {
                await dt.UpsertTreatmentSession(episode);
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
        private async Task SetInitialTreatmentLevel(TreatmentEpisode episode, Admission admission, string newLevel)
        {
            string existingLevel = admission.TreatmentSettingCode;
            var fValidations = new FASAMSValidations();
            var dt = new DataTools();
            admission.TreatmentSettingCode = newLevel;
            fValidations.ProcessAdmission(episode, admission);
            try
            {
                await dt.UpsertTreatmentSession(episode);
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
        private async Task InvokeChronologicalReorganizationValidation()
        {
            Console.WriteLine("Starting Chronological Reorganization Validation Process");
            var dt = new DataTools();
            List<JobLog> pendingJobs = new List<JobLog>();
            pendingJobs = await dt.LoadPendingJobs();

            foreach(var job in pendingJobs.Where(j => j.RecordType == "PM").OrderBy(j=> j.CreatedAt))
            {
                try
                {
                    PerformanceOutcomeMeasure perf = await dt.OpportuniticlyLoadPerformanceOutcomeMeasure(job.SourceRecordId);
                    Admission currentAdmission;
                    Admission checkedAdmission;
                    TreatmentEpisode treatmentEpisode;
                    if (perf.Admission_SourceRecordIdentifier != null)
                    {
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(perf.Admission_SourceRecordIdentifier);
                    }
                    else
                    {
                        Discharge discharge = await dt.OpportuniticlyLoadDischarge(perf);
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(discharge);
                    }
                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(currentAdmission.TreatmentSourceId);
                    checkedAdmission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, perf.PerformanceOutcomeMeasureDate);
                    if (currentAdmission.SourceRecordIdentifier != checkedAdmission.SourceRecordIdentifier)
                    {
                        perf.Admission_SourceRecordIdentifier = checkedAdmission.SourceRecordIdentifier;
                        Console.WriteLine("Updating Perf Id:{0} from Admision Id:{1} to Id:{2}",perf.SourceRecordIdentifier, currentAdmission.SourceRecordIdentifier,checkedAdmission.SourceRecordIdentifier);
                        try
                        {
                            await dt.UpsertPerformanceOutcomeMeasure(perf);
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
                    //WriteErrorLog(ex, "ServiceEventDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum); //Handle Later
                }
            }
            foreach (var job in pendingJobs.Where(j => j.RecordType == "EV").OrderBy(j => j.CreatedAt))
            {
                try
                {
                    Evaluation evaluation = await dt.OpportuniticlyLoadEvaluation(job.SourceRecordId);
                    Admission currentAdmission;
                    Admission checkedAdmission;
                    TreatmentEpisode treatmentEpisode;
                    if (evaluation.Admission_SourceRecordIdentifier != null)
                    {
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(evaluation.Admission_SourceRecordIdentifier);
                    }
                    else
                    {
                        Discharge discharge = await dt.OpportuniticlyLoadDischarge(evaluation.Discharge_SourceRecordIdentifier);
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(discharge);
                    }
                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(currentAdmission.TreatmentSourceId);
                    checkedAdmission = await dt.OpportuniticlyLoadAdmission(treatmentEpisode, evaluation.EvaluationDate);
                    if (currentAdmission.SourceRecordIdentifier != checkedAdmission.SourceRecordIdentifier)
                    {
                        evaluation.Admission_SourceRecordIdentifier = checkedAdmission.SourceRecordIdentifier;
                        Console.WriteLine("Updating Eval Id:{0} from Admision Id:{1} to Id:{2}", evaluation.SourceRecordIdentifier, currentAdmission.SourceRecordIdentifier, checkedAdmission.SourceRecordIdentifier);
                        try
                        {
                            await dt.UpsertEvaluation(evaluation);
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
                    //WriteErrorLog(ex, "ServiceEventDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum); //Handle Later
                }
            }
            foreach (var job in pendingJobs.Where(j => j.RecordType == "DX").OrderBy(j => j.CreatedAt))
            {
                try
                {
                    Diagnosis diagnosis = await dt.OpportuniticlyLoadDiagnosis(job.SourceRecordId);
                    Admission currentAdmission;
                    Admission checkedAdmission;
                    TreatmentEpisode treatmentEpisode;
                    if (diagnosis.Admission_SourceRecordIdentifier != null)
                    {
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(diagnosis.Admission_SourceRecordIdentifier);
                    }
                    else
                    {
                        Discharge discharge = await dt.OpportuniticlyLoadDischarge(diagnosis.Discharge_SourceRecordIdentifier);
                        currentAdmission = await dt.OpportuniticlyLoadAdmission(discharge);
                    }
                    treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(currentAdmission.TreatmentSourceId);
                    //checkedAdmission = dt.OpportuniticlyLoadAdmission(treatmentEpisode, diagnosis.EvaluationDate);
                    //if (currentAdmission.SourceRecordIdentifier != checkedAdmission.SourceRecordIdentifier)
                    //{
                    //    diagnosis.Admission_SourceRecordIdentifier = checkedAdmission.SourceRecordIdentifier;
                    //    Console.WriteLine("Updating Diagnosis Id:{0} from Admision Id:{1} to Id:{2}", diagnosis.SourceRecordIdentifier, currentAdmission.SourceRecordIdentifier, checkedAdmission.SourceRecordIdentifier);
                    //    try
                    //    {
                    //        dt.UpsertDiagnosis(diagnosis);
                    //    }
                    //    catch (DbEntityValidationException ex)
                    //    {
                    //        // Retrieve the error messages as a list of strings.
                    //        var errorMessages = ex.EntityValidationErrors
                    //                .SelectMany(x => x.ValidationErrors)
                    //                .Select(x => x.ErrorMessage);

                    //        // Join the list to a single string.
                    //        var fullErrorMessage = string.Join(";", errorMessages);

                    //        // Combine the original exception message with the new one.
                    //        var exceptionMessage = string.Concat(ex.Message, "The validation errors are: ", fullErrorMessage);

                    //        // Throw a new DbEntityValidationException with the improved exception message.
                    //        throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    //WriteErrorLog(ex, "ServiceEventDataSet", Path.GetDirectoryName(inputFile), inputFile, rowNum); //Handle Later
                }
            }
            foreach (var job in pendingJobs.Where(j => j.RecordType == "AD").OrderBy(j => j.CreatedAt))
            {

            }
            foreach (var job in pendingJobs.Where(j => j.RecordType == "DC").OrderBy(j => j.CreatedAt))
            {

            }

            Console.WriteLine("Completed Chronological Reorganization Validation Process");
        }
        private async Task CreateOutputDataSet(object dataStructure, DataSetTypes dataSet)
        {
            var dt = new DataTools();
            var fValidations = new FASAMSValidations();
            List<JobLog> pendingJobs = new List<JobLog>();
            pendingJobs = await dt.LoadPendingJobs();
            switch (dataSet)
            {
                case DataSetTypes.Client:
                    {
                        ProviderClients clientDataSet = (ProviderClients)dataStructure;
                        foreach (var job in pendingJobs.Where(j => j.RecordType == "CL").OrderBy(j => j.CreatedAt))
                        {
                            ProviderClient client = await dt.OpportuniticlyLoadProviderClient(job.SourceRecordId);
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        clientDataSet.clients.Add(client);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        clientDataSet.clients.Add(client);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case DataSetTypes.TreatmentEpisode:
                    {
                        TreatmentEpisodeDataSet episodeDataSet = (TreatmentEpisodeDataSet)dataStructure;
                        foreach (var job in pendingJobs.Where(j => j.RecordType == "TE").OrderBy(j => j.CreatedAt))
                        {
                            TreatmentEpisode treatmentEpisode = await dt.OpportuniticlyLoadTreatmentSession(job.SourceRecordId);
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        episodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        episodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                                    }
                                    break;
                            }
                        }
                        foreach (var job in pendingJobs.Where(j => j.RecordType == "AD").OrderBy(j => j.CreatedAt))
                        {
                            Admission admission = await dt.OpportuniticlyLoadAdmission(job.SourceRecordId);
                            TreatmentEpisode treatmentEpisode = episodeDataSet.TreatmentEpisodes.Where(e => e.SourceRecordIdentifier == admission.TreatmentSourceId 
                            && e.FederalTaxIdentifier == admission.FederalTaxIdentifier).SingleOrDefault();
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    }
                                    break;
                            }
                        }
                        foreach (var job in pendingJobs.Where(j => j.RecordType == "DC").OrderBy(j => j.CreatedAt))
                        {
                            Discharge discharge = await dt.OpportuniticlyLoadDischarge(job.SourceRecordId);
                            Admission dbadmission = await dt.OpportuniticlyLoadAdmission(discharge);
                            TreatmentEpisode treatmentEpisode = episodeDataSet.TreatmentEpisodes.Where(e => e.SourceRecordIdentifier == dbadmission.TreatmentSourceId
                            && e.FederalTaxIdentifier == dbadmission.FederalTaxIdentifier).SingleOrDefault();
                            Admission admission = treatmentEpisode.Admissions.Where(a => a.SourceRecordIdentifier == dbadmission.SourceRecordIdentifier).SingleOrDefault();
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        fValidations.ProcessDischarge(admission, discharge);
                                        fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        fValidations.ProcessDischarge(admission, discharge);
                                        fValidations.ProcessAdmission(treatmentEpisode, admission);
                                    }
                                    break;
                            }
                        }


                        foreach (var job in pendingJobs.Where(j => j.RecordType == "ID").OrderBy(j => j.CreatedAt))
                        {
                            ImmediateDischarge immediateDischarge = await dt.OpportuniticlyLoadImmediateDischarge(job.SourceRecordId);
                            TreatmentEpisode treatmentEpisode = episodeDataSet.TreatmentEpisodes.Where(e => e.SourceRecordIdentifier == immediateDischarge.TreatmentSourceId
                            && e.FederalTaxIdentifier == immediateDischarge.FederalTaxIdentifier).SingleOrDefault();
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        fValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        fValidations.ProcessImmediateDischarge(treatmentEpisode, immediateDischarge);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case DataSetTypes.ServiceEvent:
                    {
                        ServiceEvents serviceEventsDataSet = (ServiceEvents)dataStructure;
                        foreach (var job in pendingJobs.Where(j => j.RecordType == "SE").OrderBy(j => j.CreatedAt))
                        {
                            ServiceEvent serviceEvent = await dt.OpportuniticlyLoadServiceEvent(job.SourceRecordId);
                            switch (job.Status)
                            {
                                case "Update":
                                    {
                                        serviceEventsDataSet.serviceEvents.Add(serviceEvent);
                                    }
                                    break;
                                case "Delete":
                                    {

                                    }
                                    break;
                                case "UnDelete":
                                    {

                                    }
                                    break;
                                default:
                                    {
                                        serviceEventsDataSet.serviceEvents.Add(serviceEvent);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                default: return;
            }
        }
        private static string PAMMappingFile;
        private List<Field> GetFields(string mappingFile)
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
        private List<List<Field>> ParseFile(string inputFile, string mappingFile)
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
        private void WritePAMErrorFile(string inputFileName, List<List<Field>> errors)
        {
            foreach(var error in errors)
            {
                WritePAMErrorFile(inputFileName, error);
            }
        }
        private void WritePAMErrorFile(string inputFileName, List<Field> error)
        {
            string fileName = inputFileName + "-Errors.txt";
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                
                var sortedList = error.OrderBy(e => e.Start);
                int lineLength = sortedList.Last().Start + sortedList.Last().Length;
                StringBuilder line = new StringBuilder(lineLength);
                foreach (var item in sortedList)
                {
                    if (string.IsNullOrWhiteSpace(item.Value))
                    {
                        line.Insert(item.Start, "".PadRight(item.Length, ' '));
                    }
                    else
                    {
                        line.Insert(item.Start, item.Value);
                    }
                }
                writer.WriteLine(line.ToString());
                writer.Close();

            }
        }
        private void WriteXml(object dataStructure, string outputFile, string outputFileName, string outputPath)
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
        private void WriteErrorLog(Exception ex, string outputFileName,string outputPath, string inputFile, int rowNum)
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
        private void WriteErrorLog(Exception ex, string outputFileName, string outputPath)
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
        private void WriteErrorLog(string message, string path)
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
