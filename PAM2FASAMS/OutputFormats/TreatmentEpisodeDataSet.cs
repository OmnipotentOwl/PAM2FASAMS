using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PAM2FASAMS.OutputFormats
{
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "TreatmentEpisodeDataSet", IsNullable = false)]
    public partial class TreatmentEpisodeDataSet
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("TreatmentEpisode", IsNullable = false)]
        public List<TreatmentEpisode> TreatmentEpisodes { get; set; }
    }

    [Table(name:"TreatmentEpisodes")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TreatmentEpisode
    {
        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string FederalTaxIdentifier { get; set; }
        [Required]
        public string ClientSourceRecordIdentifier { get; set; }
        public List<ImmediateDischarge> ImmediateDischarges { get; set; }
        public List<Admission> Admissions { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }

    [Table(name: "ImmediateDischarges")]
    public partial class ImmediateDischarge
    {
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Required]
        public string StaffEducationLevelCode { get; set; }
        [Required]
        [MaxLength(100)]
        public string StaffIdentifier { get; set; }
        [Required]
        public string EvaluationDate { get; set; }
        public string Note { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalEvaluationDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EvaluationDate))
                {
                    return DateTime.Parse(EvaluationDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Episode"), Column(Order = 0)]
        public string TreatmentSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Episode"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual TreatmentEpisode Episode { get; set; }
    }
    [Table(name: "Admissions")]
    public partial class Admission
    {
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        public string SiteIdentifier { get; set; }
        public string StaffEducationLevelCode { get; set; }
        [MaxLength(100)]
        public string StaffIdentifier { get; set; }
        public string SubcontractNumber { get; set; }
        public string ContractNumber { get; set; }
        public string ProgramAreaCode { get; set; }
        public string AdmissionDate { get; set; }
        public string TreatmentSettingCode { get; set; }
        public string TypeCode { get; set; }
        public string IsCodependentCode { get; set; }
        public string ReferralSourceCode { get; set; }
        public string DaysWaitingToEnterTreatmentKnownCode { get; set; }
        public int DaysWaitingToEnterTreatmentNumber { get; set; }
        public List<PerformanceOutcomeMeasure> PerformanceOutcomeMeasures { get; set; }
        public List<Evaluation> Evaluations { get; set; }
        public List<Diagnosis> Diagnoses { get; set; }
        public Discharge Discharge { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalAdmissionDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AdmissionDate))
                {
                    return DateTime.Parse(AdmissionDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Discharge")]
        public string Discharge_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Episode"),Column(Order = 0)]
        public string TreatmentSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Episode"), Column(Order = 1)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual TreatmentEpisode Episode { get; set; }
    }

    [Table(name: "PerformanceOutcomeMeasures")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class PerformanceOutcomeMeasure
    {
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Required]
        public string StaffEducationLevelCode { get; set; }
        [Required]
        [MaxLength(100)]
        public string StaffIdentifier { get; set; }
        [Required]
        public string PerformanceOutcomeMeasureDate { get; set; }
        public ClientDemographic ClientDemographic { get; set; }
        public FinancialAndHousehold FinancialAndHousehold { get; set; }
        public Health Health { get; set; }
        public EducationAndEmployment EducationAndEmployment { get; set; }
        public StabilityOfHousing StabilityOfHousing { get; set; }
        public Recovery Recovery { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SubstanceUseDisorder", IsNullable = false)]
        public List<SubstanceUseDisorder> SubstanceUseDisorders { get; set; }
        public MentalHealth MentalHealth { get; set; }
        public Medication Medication { get; set; }
        public Legal Legal { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Admission")]
        public string Admission_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Admission Admission { get; set; }
        [XmlIgnore()]
        public DateTime InternalPerformanceOutcomeMeasureDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PerformanceOutcomeMeasureDate))
                {
                    return DateTime.Parse(PerformanceOutcomeMeasureDate);
                }
                return DateTime.Now;
            }
        }
    }

    [Table(name: "Evaluations")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Evaluation
    {
        private string staffEducationLevelCodeField;

        private string staffIdentifierField;

        private string typeCodeField;

        private string toolCodeField;

        private string evaluationDateField;

        private string determinationDateField;

        private string scoreCodeField;

        private string actualLevelCodeField;

        private string recommendedLevelCodeField;

        private string actionField;

        /// <remarks/>
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }

        /// <remarks/>
        [Required]
        public string StaffEducationLevelCode
        {
            get
            {
                return this.staffEducationLevelCodeField;
            }
            set
            {
                this.staffEducationLevelCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        [MaxLength(100)]
        public string StaffIdentifier
        {
            get
            {
                return this.staffIdentifierField;
            }
            set
            {
                this.staffIdentifierField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string ToolCode
        {
            get
            {
                return this.toolCodeField;
            }
            set
            {
                this.toolCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string EvaluationDate
        {
            get
            {
                return this.evaluationDateField;
            }
            set
            {
                this.evaluationDateField = value;
            }
        }

        /// <remarks/>
        public string DeterminationDate
        {
            get
            {
                return this.determinationDateField;
            }
            set
            {
                this.determinationDateField = value;
            }
        }

        /// <remarks/>
        public string ScoreCode
        {
            get
            {
                return this.scoreCodeField;
            }
            set
            {
                this.scoreCodeField = value;
            }
        }

        /// <remarks/>
        public string ActualLevelCode
        {
            get
            {
                return this.actualLevelCodeField;
            }
            set
            {
                this.actualLevelCodeField = value;
            }
        }

        /// <remarks/>
        public string RecommendedLevelCode
        {
            get
            {
                return this.recommendedLevelCodeField;
            }
            set
            {
                this.recommendedLevelCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalEvaluationDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EvaluationDate))
                {
                    return DateTime.Parse(EvaluationDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Admission")]
        public string Admission_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Admission Admission { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Discharge")]
        public string Discharge_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Discharge Discharge { get; set; }
    }

    [Table(name: "Diagnoses")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Diagnosis
    {
        private string sourceRecordIdentifierField;

        private string staffEducationLevelCodeField;

        private string staffIdentifierField;

        private string codeSetIdentifierCodeField;

        private string diagnosisCodeField;

        private string startDateField;

        private string endDateField;

        private string actionField;

        /// <remarks/>
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier
        {
            get
            {
                return this.sourceRecordIdentifierField;
            }
            set
            {
                this.sourceRecordIdentifierField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string StaffEducationLevelCode
        {
            get
            {
                return this.staffEducationLevelCodeField;
            }
            set
            {
                this.staffEducationLevelCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        [MaxLength(100)]
        public string StaffIdentifier
        {
            get
            {
                return this.staffIdentifierField;
            }
            set
            {
                this.staffIdentifierField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string CodeSetIdentifierCode
        {
            get
            {
                return this.codeSetIdentifierCodeField;
            }
            set
            {
                this.codeSetIdentifierCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string DiagnosisCode
        {
            get
            {
                return this.diagnosisCodeField;
            }
            set
            {
                this.diagnosisCodeField = value;
            }
        }

        /// <remarks/>
        [Required]
        public string StartDate
        {
            get
            {
                return this.startDateField;
            }
            set
            {
                this.startDateField = value;
            }
        }

        /// <remarks/>
        public string EndDate
        {
            get
            {
                return this.endDateField;
            }
            set
            {
                this.endDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Admission")]
        public string Admission_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Admission Admission { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Discharge")]
        public string Discharge_SourceRecordIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Discharge Discharge { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Discharge
    {

        /// <remarks/>
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }

        /// <remarks/>
        public string StaffEducationLevelCode { get; set; }

        /// <remarks/>
        [MaxLength(100)]
        public string StaffIdentifier { get; set; }

        /// <remarks/>
        [Required]
        public string TypeCode { get; set; }

        /// <remarks/>
        [Required]
        public string DischargeDate { get; set; }

        /// <remarks/>
        [Required]
        public string LastContactDate { get; set; }

        /// <remarks/>
        [Required]
        public string DischargeReasonCode { get; set; }

        /// <remarks/>
        public string DischargeDestinationCode { get; set; }

        /// <remarks/>
        public string BirthOutcomeCode { get; set; }

        /// <remarks/>
        public string DrugFreeAtDeliveryCode { get; set; }

        /// <remarks/>
        public PerformanceOutcomeMeasure PerformanceOutcomeMeasures { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Evaluation", IsNullable = false)]
        public List<Evaluation> Evaluations { get; set; }

        /// <remarks/>
        public List<Diagnosis> Diagnoses { get; set; }

        [XmlAttribute()]
        public string action { get; set; }

        [XmlIgnore()]
        public DateTime InternalDischargeDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DischargeDate))
                {
                    return DateTime.Parse(DischargeDate);
                }
                return DateTime.Now;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ClientDemographic
    {
        public string VeteranStatusCode { get; set; }
        public string MaritalStatusCode { get; set; }
        public string ResidenceCountyAreaCode { get; set; }
        public string ResidencePostalCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class FinancialAndHousehold
    {
        public string PrimaryIncomeSourceCode { get; set; }
        public string AnnualPersonalIncomeKnownCode { get; set; }
        public decimal AnnualPersonalIncomeAmount { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AnnualPersonalIncomeAmountSpecified { get; set; }
        public string AnnualFamilyIncomeKnownCode { get; set; }
        public decimal AnnualFamilyIncomeAmount { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AnnualFamilyIncomeAmountSpecified { get; set; }
        public string PrimaryPaymentSourceCode { get; set; }
        public string DisabilityIncomeStatusCode { get; set; }
        public string HealthInsuranceCode { get; set; }
        public string TemporaryAssistanceForNeedyFamiliesStatusCode { get; set; }
        public string FamilySizeNumberKnownCode { get; set; }
        public int FamilySizeNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FamilySizeNumberSpecified { get; set; }
        public string DependentsKnownCode { get; set; }
        public int DependentsCount { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DependentsCountSpecified { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Health
    {
        public string AmericansWithDisabilitiesActDisabledStatusCode { get; set; }
        public string PregnantCode { get; set; }
        public string PregnancyTrimesterCode { get; set; }
        public string RecentlyBecomePostpartumCode { get; set; }
        public string UnableToPerformDailyLivingActivitiesCode { get; set; }
        public string IntravenousSubstanceHistoryCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EducationAndEmployment
    {
        public string EducationGradeLevelCode { get; set; }
        public string SchoolAttendanceStatusCode { get; set; }
        public string SchoolDaysAvailableInLast90DaysKnownCode { get; set; }
        public int SchoolDaysAvailableInLast90DaysNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SchoolDaysAvailableInLast90DaysNumberSpecified { get; set; }
        public string SchoolDaysAttendedInLast90DaysKnownCode { get; set; }
        public int SchoolDaysAttendedInLast90DaysNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SchoolDaysAttendedInLast90DaysNumberSpecified { get; set; }
        public string SchoolSuspensionOrExpulsionStatusCode { get; set; }
        public string EmploymentStatusCode { get; set; }
        public string DaysWorkedInLast30DaysKnownCode { get; set; }
        public int DaysWorkedInLast30DaysNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DaysWorkedInLast30DaysNumberSpecified { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StabilityOfHousing
    {
        public string DaysSpentInCommunityInLast30DaysKnownCode { get; set; }
        public int DaysSpentInCommunityInLast30DaysNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DaysSpentInCommunityInLast30DaysNumberSpecified { get; set; }
        public string LivingArrangementCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Recovery
    {
        public string SelfHelpGroupAttendanceFrequencyCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubstanceUseDisorder
    {
        [Key, Column(Order = 1)]
        public string DisorderRankCode { get; set; }
        public string DisorderCode { get; set; }
        public string RouteOfAdministrationCode { get; set; }
        public string FrequencyofUseCode { get; set; }
        public string FirstUseAge { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key,ForeignKey("Perf"), Column(Order = 0)]
        public string PerfSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual PerformanceOutcomeMeasure Perf { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MentalHealth
    {
        public string MentalHealthProblemRiskCode { get; set; }
        public string HasRiskFactorsForEmotionalDisturbanceCode { get; set; }
        public string PrognosisStatusCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Medication
    {
        public string MedicationAssistedOpioidTherapyCode { get; set; }
        public string ReceivedPrescriptionsThroughIndigentDrugProgramCode { get; set; }
        public string ReceivedPrescriptionsThroughPatientAssistanceProgramCode { get; set; }
        public string TakingAntipsychoticMedicationCode { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Legal
    {
        public string ArrestsInLast30DaysKnownCode { get; set; }
        public int ArrestsInLast30DaysNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ArrestsInLast30DaysNumberSpecified { get; set; }
        public string IsVoluntarilyInTreatmentCode { get; set; }
        public string IsLegallyIncompetentCode { get; set; }
        public string LegalStatusCode { get; set; }
        public string LegalGuardianRelationshipCode { get; set; }
        public string ChildrenDependencyOrDelinquencyStatusCode { get; set; }
        public string CompetencyStatusCode { get; set; }
        public string HasBeenCommittedToJuvenileJusticeCode { get; set; }
        public string MeetsCriteriaForMarchmanActCode { get; set; }
        public string MarchmanActTypeCode { get; set; }
        public string MeetsCriteriaForBakerActCode { get; set; }
        public string BakerActRouteCode { get; set; }
        public string DrugCourtOrderedCode { get; set; }
        public string OrderingCountyAreaCode { get; set; }
    }

    public class DiagnosisComparer : IEqualityComparer<Diagnosis>
    {
        public bool Equals(Diagnosis x, Diagnosis y)
        {
            //Check whether the objects are the same object.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check if dx is the same
            return x.CodeSetIdentifierCode == y.CodeSetIdentifierCode
                && x.DiagnosisCode == y.DiagnosisCode && x.EndDate == y.EndDate;
        }
        public int GetHashCode(Diagnosis obj)
        {
            //Get hash code for the StartDate field if it is not null. 
            //int hashStartDate = obj.StartDate == null ? 0 : obj.StartDate.GetHashCode();

            //Get hash code for the CodeSetIdentifierCode field. 
            int hashCodeSetIdentifierCode = obj.CodeSetIdentifierCode == null ? 0 : obj.CodeSetIdentifierCode.GetHashCode();

            //Get hash code for the DiagnosisCode field. 
            int hashDiagnosisCode = obj.DiagnosisCode == null ? 0 : obj.DiagnosisCode.GetHashCode();

            //Get hash code for the EndDate field if it is not null. 
            int hashEndDate = obj.EndDate == null ? 0 : obj.EndDate.GetHashCode();

            //Calculate the hash code for the product. 
            return hashCodeSetIdentifierCode ^ hashDiagnosisCode ^ hashEndDate;

        }
    }
}
