using PAM2FASAMS.OutputFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PAM2FASAMS
{
    public class FASAMSValidations
    {
        public static ProviderClientIdentifier ValidateClientIdentifier(string IdString)
        {
            ProviderClientIdentifier clientIdentifier = new ProviderClientIdentifier();
            string sudoSSPattern = "^[A-Z]{3}";
            if (Regex.IsMatch(IdString, sudoSSPattern))
            {
                clientIdentifier.TypeCode = "3";
                clientIdentifier.Identifier = IdString;
            }
            else
            {
                clientIdentifier.TypeCode = "1";
                clientIdentifier.Identifier = IdString;
            }
            return clientIdentifier;
        }
        public static void ProcessProviderClientIdentifiers(ProviderClient client, ProviderClientIdentifier identifier)
        {
            if(client.ProviderClientIdentifiers.Exists(i=> i.TypeCode == identifier.TypeCode))
            {
                var existing = client.ProviderClientIdentifiers.Where(i => i.TypeCode == identifier.TypeCode).Single();
                existing = identifier;
            }
            else
            {
                client.ProviderClientIdentifiers.Add(identifier);
            }
        }
        public static void ProcessAdmission(TreatmentEpisode episode, Admission admission)
        {
            switch (admission.TypeCode)
            {
                case "1":
                    {
                        if (episode.Admissions.Any(a => a.TypeCode == "1"))
                            if(episode.Admissions.Any(a => a.SourceRecordIdentifier == admission.SourceRecordIdentifier))
                            {
                                //same record so just replace it.
                                var existingItem = episode.Admissions.Where(a => a.SourceRecordIdentifier == admission.SourceRecordIdentifier).FirstOrDefault();
                                int id = episode.Admissions.IndexOf(existingItem);
                                episode.Admissions[id] = admission;
                                return;
                            }
                            else
                            {
                                return;
                            }

                        episode.Admissions.Add(admission);
                        return;
                    }
                case "2":
                    {
                        return;
                    }
                default:
                    break;
            }
        }
        public static void ProcessDischarge(Admission admission, Discharge discharge)
        {
            switch (discharge.TypeCode)
            {
                case "1":
                    {
                        if (admission.Discharge != null)
                            if (admission.Discharge.SourceRecordIdentifier == discharge.SourceRecordIdentifier)
                            {
                                //same record so just replace it.
                                admission.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                                admission.Discharge = discharge;
                                return;
                            }
                            else
                            {
                                return;
                            }
                        admission.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                        admission.Discharge = discharge;
                        return;
                    }
                case "2":
                    {
                        if (admission.Discharge != null)
                            if (admission.Discharge.SourceRecordIdentifier == discharge.SourceRecordIdentifier)
                            {
                                //same record so just replace it.    
                                admission.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                                admission.Discharge = discharge;
                                return;
                            }
                            else
                            {
                                return;
                            }
                        admission.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                        admission.Discharge = discharge;
                        return;
                    }
                default:
                    break;
            }
        }
        public static void ProcessImmediateDischarge(TreatmentEpisode episode, ImmediateDischarge discharge)
        {
            if(episode.ImmediateDischarges.Any(i => i.SourceRecordIdentifier == discharge.SourceRecordIdentifier))
            {
                var existingItem = episode.ImmediateDischarges.Where(a => a.SourceRecordIdentifier == discharge.SourceRecordIdentifier).FirstOrDefault();
                int id = episode.ImmediateDischarges.IndexOf(existingItem);
                episode.ImmediateDischarges[id] = discharge;
                return;
            }
            episode.ImmediateDischarges.Add(discharge);
            return;
        }
        public static void ProcessPerformanceOutcomeMeasure(Admission admission, PerformanceOutcomeMeasure outcomeMeasure)
        {
            if(admission.PerformanceOutcomeMeasures.Any(p => p.SourceRecordIdentifier == outcomeMeasure.SourceRecordIdentifier))
            {
                //same record so just replace it.
                var existingPerf = admission.PerformanceOutcomeMeasures.Where(p => p.SourceRecordIdentifier == outcomeMeasure.SourceRecordIdentifier).FirstOrDefault();
                int id = admission.PerformanceOutcomeMeasures.IndexOf(existingPerf);
                outcomeMeasure.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
                admission.PerformanceOutcomeMeasures[id] = outcomeMeasure;
                return;
            }
            if (admission.PerformanceOutcomeMeasures.Any(p => p.PerformanceOutcomeMeasureDate == outcomeMeasure.PerformanceOutcomeMeasureDate))
            {
                //complex data merge here.
                return;
            }
            //last option
            outcomeMeasure.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
            admission.PerformanceOutcomeMeasures.Add(outcomeMeasure);
        }
        public static void ProcessPerformanceOutcomeMeasure(Discharge discharge, PerformanceOutcomeMeasure outcomeMeasure)
        {
            if(discharge.PerformanceOutcomeMeasures == null)
            {
                discharge.PerformanceOutcomeMeasures = outcomeMeasure;
                return;
            }
            if (discharge.PerformanceOutcomeMeasures.SourceRecordIdentifier == outcomeMeasure.SourceRecordIdentifier)
            {
                //same record so just replace it.
                discharge.PerformanceOutcomeMeasures = outcomeMeasure;
                return;
            }
            if (discharge.PerformanceOutcomeMeasures.PerformanceOutcomeMeasureDate != outcomeMeasure.PerformanceOutcomeMeasureDate)
            {
                //complex data merge here.
                return;
            }
            //last option
            discharge.PerformanceOutcomeMeasures=outcomeMeasure;
        }
        public static void ProcessDiagnosis(Admission admission, List<Diagnosis> diagnoses, string evalDate)
        {
            var dxNoLongerPresent = (admission.Diagnoses).Except(diagnoses,new DiagnosisComparer()).ToList();
            var dxToAdd = diagnoses.Except(admission.Diagnoses, new DiagnosisComparer()).ToList();
            foreach(var diag in diagnoses)
            {
                if (admission.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier))
                {
                    //same record so just replace it.
                    var existingDx = admission.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    diag.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
                    admission.Diagnoses[id] = diag;
                }
                if (admission.Diagnoses.Any(d => d.CodeSetIdentifierCode == diag.CodeSetIdentifierCode && d.DiagnosisCode == diag.DiagnosisCode && d.StartDate == diag.StartDate))
                {
                    var existingDx = admission.Diagnoses.Where(d => d.CodeSetIdentifierCode == diag.CodeSetIdentifierCode && d.DiagnosisCode == diag.DiagnosisCode && d.StartDate == diag.StartDate).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    admission.Diagnoses[id].EndDate = diag.EndDate;
                }
            }
            foreach(var diag in dxNoLongerPresent)
            {
                if (admission.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier && d.EndDate == null))
                {
                    //same record so just replace it.
                    var existingDx = admission.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    diag.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
                    diag.EndDate = evalDate;
                    admission.Diagnoses[id] = diag;
                }
            }
            foreach (var diag in dxToAdd)
            {
                if (admission.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier))
                {
                    //same record so just replace it.
                    var existingDx = admission.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    diag.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
                    admission.Diagnoses[id] = diag;
                    continue;
                }
                diag.Admission_SourceRecordIdentifier = admission.SourceRecordIdentifier;
                admission.Diagnoses.Add(diag);
            }

        }
        public static void ProcessDiagnosis(Admission admission, Discharge discharge, List<Diagnosis> diagnoses, string evalDate, string dischargeType)
        {
            var dxNoLongerPresent = (admission.Diagnoses).Except(diagnoses, new DiagnosisComparer()).ToList();
            var dxToAdd = diagnoses.Except(admission.Diagnoses, new DiagnosisComparer()).ToList();
            foreach (var diag in diagnoses)
            {
                if (discharge.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier))
                {
                    //same record so just replace it.
                    var existingDx = discharge.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = discharge.Diagnoses.IndexOf(existingDx);
                    diag.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                    discharge.Diagnoses[id] = diag;
                }
                if (discharge.Diagnoses.Any(d => d.CodeSetIdentifierCode == diag.CodeSetIdentifierCode && d.DiagnosisCode == diag.DiagnosisCode && d.StartDate == diag.StartDate))
                {
                    var existingDx = discharge.Diagnoses.Where(d => d.CodeSetIdentifierCode == diag.CodeSetIdentifierCode && d.DiagnosisCode == diag.DiagnosisCode && d.StartDate == diag.StartDate).FirstOrDefault();
                    int id = discharge.Diagnoses.IndexOf(existingDx);
                    discharge.Diagnoses[id].EndDate = diag.EndDate;
                }
            }
            foreach (var diag in dxNoLongerPresent)
            {
                if (admission.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier && d.EndDate == null))
                {
                    //same record so just replace it.
                    var existingDx = admission.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    diag.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier; //handles linking discharge dx to admission dx record for removal
                    diag.EndDate = evalDate;
                    admission.Diagnoses[id] = diag;
                }
            }
            foreach (var diag in dxToAdd)
            {
                if (discharge.Diagnoses.Any(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier))
                {
                    //same record so just replace it.
                    var existingDx = admission.Diagnoses.Where(d => d.SourceRecordIdentifier == diag.SourceRecordIdentifier).FirstOrDefault();
                    int id = admission.Diagnoses.IndexOf(existingDx);
                    diag.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                    discharge.Diagnoses[id] = diag;
                    continue;
                }
                diag.Discharge_SourceRecordIdentifier = discharge.SourceRecordIdentifier;
                discharge.Diagnoses.Add(diag);
            }
        }
        public static string ValidateFASAMSDate(string dateRaw)
        {
            DateTime result = DateTime.ParseExact(dateRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
            return result.ToShortDateString();
        }
        public static string ValidateFASAMSStaffEduLvlCode(string pamId)
        {
            char delimiter = '-';
            string[] substring = pamId.Split(delimiter);
            return substring[0];
        }
        public static string ValidateFASAMSStaffId(string pamId)
        {
            char delimiter = '-';
            string[] substring = pamId.Split(delimiter);
            return substring[1];
        }
        public static string ValidateIncomeAvailable(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateFamilySizeAvailable(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateSchoolDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateWorkDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateCommunityDaysKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateArrestsKnown(string pamData)
        {
            if (String.IsNullOrWhiteSpace(pamData))
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
        public static string ValidateLegalStatus(string pamData)
        {
            switch (pamData)
            {
                case "1": return "1";
                case "2": return "1";
                case "3": return "0";
                case "4": return "0";
                case "5": return "0";
                case "6": return "0";
                case "97": return "0";
                default: return "";
            }
        }
        public static string ValidateEvalToolScore(FileType type, List<Field> fields)
        {
            switch (type)
            {
                case FileType.CFAR:
                    {
                        int saHist = int.Parse(fields.Where(r => r.Name == "SAHist").Single().Value.Trim());
                        int depress = int.Parse(fields.Where(r => r.Name == "Depress").Single().Value.Trim());
                        int anxiety = int.Parse(fields.Where(r => r.Name == "Anxiety").Single().Value.Trim());
                        int hyperAct = int.Parse(fields.Where(r => r.Name == "HyperAct").Single().Value.Trim());
                        int thought = int.Parse(fields.Where(r => r.Name == "Thought").Single().Value.Trim());
                        int cognitiv = int.Parse(fields.Where(r => r.Name == "Cognitiv").Single().Value.Trim());
                        int medical = int.Parse(fields.Where(r => r.Name == "Medical").Single().Value.Trim());
                        int traumati = int.Parse(fields.Where(r => r.Name == "Traumati").Single().Value.Trim());
                        int substanc = int.Parse(fields.Where(r => r.Name == "Substanc").Single().Value.Trim());
                        int relation = int.Parse(fields.Where(r => r.Name == "Relation").Single().Value.Trim());
                        int behavior = int.Parse(fields.Where(r => r.Name == "Behavior").Single().Value.Trim());
                        int aDLFunct = int.Parse(fields.Where(r => r.Name == "ADLFunct").Single().Value.Trim());
                        int socLegal = int.Parse(fields.Where(r => r.Name == "SocLegal").Single().Value.Trim());
                        int workScho = int.Parse(fields.Where(r => r.Name == "WorkScho").Single().Value.Trim());
                        int dangSelf = int.Parse(fields.Where(r => r.Name == "DangSelf").Single().Value.Trim());
                        int dangOth = int.Parse(fields.Where(r => r.Name == "DangOth").Single().Value.Trim());
                        int security = int.Parse(fields.Where(r => r.Name == "Security").Single().Value.Trim());
                        return (saHist+depress+anxiety+hyperAct+thought+cognitiv+medical+traumati+substanc+relation+behavior+aDLFunct+socLegal+workScho+dangSelf+dangOth+security).ToString();
                    }
                case FileType.FARS:
                    {
                        return null;
                    }
                case FileType.ASAM:
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }
        public static string ValidateDischargeReasonCode(string pamData)
        {
            if (!string.IsNullOrWhiteSpace(pamData))
            {
                //todo discharge reason logic
            }
            return "0";
        }
        public static string ValidateAdmissionCoDependent(string pamData)
        {
            switch (pamData)
            {
                case "1":
                    return "0";
                case "2":
                    return "1";
                case "3":
                    return "0";
                case "4":
                    return "1";
                case "5":
                    return "1";
                case "6":
                    return "1";
                default:
                    return "0";
            }
        }
        public static string ValidateAdmissionProgramCode(string type,string pamData,string evalDate)
        {
            DateTime dob = DateTime.Parse(pamData);
            DateTime date = DateTime.Parse(evalDate);
            int age = CalculateAge(dob, date);
            if (string.Equals("SA",type, StringComparison.InvariantCultureIgnoreCase))
            {
                if (age > 18)
                {
                    return "4";
                }
                else
                {
                    return "2";
                }
            }
            if(string.Equals("MH", type, StringComparison.InvariantCultureIgnoreCase))
            {
                if (age > 18)
                {
                    return "3";
                }
                else
                {
                    return "1";
                }
            }
            switch (type)
            {
                case "1":
                    {
                        if (age > 18)
                        {
                            return "3";
                        }
                        else
                        {
                            return "1";
                        }
                    }
                case "2":
                    {
                        if (age > 18)
                        {
                            return "4";
                        }
                        else
                        {
                            return "2";
                        }
                    }
                default:
                    return "error";
            }
        }
        public static string ValidateTreatmentSettingCodeFromCoveredServiceCode(string pamData)
        {
            switch (pamData)
            {
                case "01": //Assessment
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "02": //Case Management
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "03": //Crisis Stabilization 
                    return "03"; //Rehabilitation/Residential - Hospital (other than Detoxification)

                case "04": //Crisis Support/Emergency
                    return "06"; //Ambulatory – Intensive outpatient

                case "05": //Day Care
                    return "97"; //Non-TEDS Tx Service Settings

                case "06": //Day Treatment
                    return "06"; //Ambulatory – Intensive outpatient

                case "07": //Drop-In/Self-Help Centers
                    return "97"; //Non-TEDS Tx Service Settings

                case "08": //In-Home and On-Site Community based care
                    return "06"; //Ambulatory – Intensive outpatient

                case "09": //Inpatient 
                    return "03"; //Rehabilitation/Residential - Hospital (other than Detoxification)

                case "10": //Intensive Case Management 
                    return "06"; //Ambulatory – Intensive outpatient

                case "11": //Intervention (Individual)
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "12": //Medical Services 
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "13": //Medication Assisted Treatment
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "14": //Outpatient 
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "15": //Outreach  
                    return "97"; //Non-TEDS Tx Service Settings

                case "18": //Residential Level I
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "19": //Residential Level II
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "20": //Residential Level III
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "21": //Residential Level IV
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "22": //Respite Services
                    return "97"; //Non-TEDS Tx Service Settings

                case "24": //Substance Abuse Inpatient Detoxification
                    return "02"; //Detoxification, 24-hour service, Free-Standing Residential

                case "25": //Supportive Employment
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "26": //Supported Housing/Living
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "27": //Treatment Alternative for Safer Community
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "28": //Incidental Expenses 
                    return "97"; //Non-TEDS Tx Service Settings

                case "29": //Aftercare
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "30": //Information and Referral
                    return "97"; //Non-TEDS Tx Service Settings

                case "32": //Substance Abuse Outpatient Detoxification
                    return "08"; //Ambulatory - Detoxification

                case "36": //Room and Board with Supervision Level I
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "37": //Room and Board with Supervision Level II
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "38": //Room and Board with Supervision Level III
                    return "05"; //Rehabilitation/Residential -Long term (more than 30 days)

                case "39": //Short-term Residential Treatment
                    return "04"; //Rehabilitation/Residential -Short term (30 days or fewer)

                case "40": //Mental Health Clubhouse Services
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "44": //Comprehensive Community Service Team
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "46": //Recovery Support
                    return "07"; //Ambulatory – Non-Intensive outpatient

                case "48": //Indicated Prevention
                    return "97"; //Non-TEDS Tx Service Settings

                case "49": //Selective Prevention
                    return "97"; //Non-TEDS Tx Service Settings

                case "50": //Universal Direct Prevention
                    return "97"; //Non-TEDS Tx Service Settings

                case "51": //Universal Indirect Prevention
                    return "97"; //Non-TEDS Tx Service Settings

                default:
                    return "";
            }
        }
        /// <summary>  
        /// For calculating only age  
        /// </summary>  
        /// <param name="dateOfBirth">Date of birth</param>  
        /// <returns> age e.g. 26</returns>  
        private static int CalculateAge(DateTime dateOfBirth)
        {
            int age = 0;
            age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                age = age - 1;

            return age;
        }
        /// <summary>  
        /// For calculating only age  
        /// </summary>  
        /// <param name="dateOfBirth">Date of birth</param>
        /// <param name="asOfDate">As of date</param>
        /// <returns> age e.g. 26</returns>  
        private static int CalculateAge(DateTime dateOfBirth, DateTime asOfDate)
        {
            int age = 0;
            age = asOfDate.Year - dateOfBirth.Year;
            if (asOfDate.DayOfYear < dateOfBirth.DayOfYear)
                age = age - 1;

            return age;
        }
    }
}
