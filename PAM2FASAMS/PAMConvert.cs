﻿using PAM2FASAMS.OutputFormats;
using PAM2FASAMS.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
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
                        catch { }

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
        public static void InvokeDemoConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM DEMO file..");
            const string PAMMappingFile = @"InputFormats/PAM-DEMO.xml";
            var pamFile = ParseFile(inputFile,PAMMappingFile);
            ProviderClients clientDataSet = new ProviderClients { clients = new List<ProviderClient>()};
            foreach (var pamRow in pamFile)
            {
                ProviderClient client = new ProviderClient
                {
                    ProviderClientIdentifiers = new List<ProviderClientIdentifier>()
                };
                client.FederalTaxIdentifier = (pamRow.Where(r => r.Name == "ProvId").Single().Value);
                client.SourceRecordIdentifier = (pamRow.Where(r => r.Name == "ClientID").Single().Value);
                client.BirthDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name=="DOB").Single().Value));
                client.FirstName = (pamRow.Where(r => r.Name == "First").Single().Value).Trim();
                client.MiddleName = (pamRow.Where(r => r.Name == "Middle").Single().Value).Trim();
                client.LastName = (pamRow.Where(r => r.Name == "Last").Single().Value).Trim();
                client.SuffixName = (pamRow.Where(r => r.Name == "Suffix").Single().Value).Trim();
                client.GenderCode = (pamRow.Where(r => r.Name == "Gender").Single().Value);
                client.RaceCode = (pamRow.Where(r => r.Name == "Race").Single().Value);
                client.EthnicityCode = (pamRow.Where(r => r.Name == "Ethnic").Single().Value);
                client.ProviderClientIdentifiers.Add(FASAMSValidations.ValidateClientIdentifier((pamRow.Where(r => r.Name == "SSN").Single().Value)));
                clientDataSet.clients.Add(client);
            }
            if(outputFile == null)
            {
                outputFile = Environment.CurrentDirectory + "\\ClientDataSet_"+(DateTime.Now.ToString("yyyyMMddHHmmss"))+".xml";
            }
            WriteXml(clientDataSet, outputFile);
            Console.WriteLine("Completed Conversion of PAM DEMO file.");
        }
        public static void InvokePerfConversion(string inputFile, string outputFile)
        {
            Console.WriteLine("Starting Conversion of PAM PERF file..");
            const string PAMMappingFile = @"InputFormats/PAM-PERF.xml";
            var pamFile = ParseFile(inputFile, PAMMappingFile);
            TreatmentEpisodeDataSet treatmentEpisodeDataSet = new TreatmentEpisodeDataSet { TreatmentEpisodes = new List<TreatmentEpisode>() };
            foreach(var pamRow in pamFile)
            {
                var type = PAMValidations.ValidateEvalPurpose(FileType.PERF, (pamRow.Where(r => r.Name == "Purpose").Single().Value));
                switch (type)
                {
                    case PAMValidations.UpdateType.Admission:
                        {
                            TreatmentEpisode treatmentEpisode = new TreatmentEpisode { SourceRecordIdentifier = Guid.NewGuid().ToString(), FederalTaxIdentifier = (pamRow.Where(r => r.Name == "ProviderId").Single().Value) };
                            treatmentEpisode.Admissions = new List<Admission>();
                            treatmentEpisode.ClientSourceRecordIdentifier = (pamRow.Where(r => r.Name == "ClientID").Single().Value);
                            Admission admission = new Admission
                            {
                                SourceRecordIdentifier = "todo",
                                SiteIdentifier = (pamRow.Where(r => r.Name == "SiteId").Single().Value),
                                StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                ContractNumber = (pamRow.Where(r => r.Name == "ContNum1").Single().Value),
                                AdmissionDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value)),
                                ReferralSourceCode = (pamRow.Where(r => r.Name == "Referral").Single().Value),
                                TypeCode = "1",
                                ProgramAreaCode = "Needs to be manually assigned, might be able to calculate",
                                TreatmentSettingCode = "todo",
                                IsCodependentCode = "1",
                                DaysWaitingToEnterTreatmentKnownCode = "0",
                                PerformanceOutcomeMeasures = new List<PerformanceOutcomeMeasure>(),
                                Diagnoses = new List<Diagnosis>()
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
                                    SchoolDaysAvailableInLast90DaysNumber = ((FASAMSValidations.ValidateSchoolDaysKnown((pamRow.Where(r => r.Name == "DaysAvai").Single().Value))=="1") ? int.Parse(pamRow.Where(r => r.Name == "DaysAvai").Single().Value) : 0),
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
                            admission.PerformanceOutcomeMeasures.Add(performanceOutcomeMeasure);
                            if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag").Single().Value))
                            {
                                admission.Diagnoses.Add(new Diagnosis
                                {
                                    SourceRecordIdentifier = "todo",
                                    StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    CodeSetIdentifierCode = "2",
                                    DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag").Single().Value.Trim(),
                                    StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "SaDiag10").Single().Value))
                            {
                                admission.Diagnoses.Add(new Diagnosis
                                {
                                    SourceRecordIdentifier = "todo",
                                    StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    CodeSetIdentifierCode = "3",
                                    DiagnosisCode = pamRow.Where(r => r.Name == "SaDiag10").Single().Value.Trim(),
                                    StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag").Single().Value))
                            {
                                admission.Diagnoses.Add(new Diagnosis
                                {
                                    SourceRecordIdentifier = "todo",
                                    StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    CodeSetIdentifierCode = "2",
                                    DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag").Single().Value.Trim(),
                                    StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(pamRow.Where(r => r.Name == "MhDiag10").Single().Value))
                            {
                                admission.Diagnoses.Add(new Diagnosis
                                {
                                    SourceRecordIdentifier = "todo",
                                    StaffEducationLevelCode = FASAMSValidations.ValidateFASAMSStaffEduLvlCode((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    StaffIdentifier = FASAMSValidations.ValidateFASAMSStaffId((pamRow.Where(r => r.Name == "StaffId").Single().Value)),
                                    CodeSetIdentifierCode = "3",
                                    DiagnosisCode = pamRow.Where(r => r.Name == "MhDiag10").Single().Value.Trim(),
                                    StartDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value))
                                });
                            }                           
                            treatmentEpisode.Admissions.Add(admission);
                            treatmentEpisodeDataSet.TreatmentEpisodes.Add(treatmentEpisode);
                            break;
                        }
                    case PAMValidations.UpdateType.Update:
                        {
                            string evalDate = FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value));
                            string clientSourceRecordIdentifier = pamRow.Where(r => r.Name == "ClientID").Single().Value;
                            TreatmentEpisode treatmentEpisode = DataTools.OpportuniticlyLoadTreatmentSession(treatmentEpisodeDataSet, type, evalDate, clientSourceRecordIdentifier);
                            Admission admission = treatmentEpisode.Admissions.Where(a => a.AdmissionDate == FASAMSValidations.ValidateFASAMSDate((pamRow.Where(r => r.Name == "InitEvada").Single().Value)) && a.Discharge == null).Single();
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
                            admission.PerformanceOutcomeMeasures.Add(performanceOutcomeMeasure);
                            break;
                        }
                    case PAMValidations.UpdateType.Discharge:
                        {

                            break;
                        }
                    case PAMValidations.UpdateType.ImDischarge:
                        {

                            break;
                        }
                }
            }
            if (outputFile == null)
            {
                outputFile = Environment.CurrentDirectory + "\\TreatmentEpisodeDataSet_" + (DateTime.Now.ToString("yyyyMMddHHmmss")) + ".xml";
            }
            WriteXml(treatmentEpisodeDataSet, outputFile);
            Console.WriteLine("Completed Conversion of PAM PERF file.");
        }


        private static void WriteXml(object dataStructure, string outputFile)
        {
            Console.WriteLine("Writing Output File {0}",outputFile);
            Type t = dataStructure.GetType();
            XmlSerializer writer = new XmlSerializer(t);
            FileStream file = File.Create(outputFile);
            writer.Serialize(file, dataStructure);
            file.Close();
        }
    }

    internal class Field
    {
        public string Name { get; set; }
        public int Length { get; set; }
        public int Start { get; set; }
        public string Value { get; set; }
    }
}