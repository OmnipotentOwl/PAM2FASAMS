using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.OutputFormats
{
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TreatmentEpisodeDataSet
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("TreatmentEpisode", IsNullable = false)]
        public List<TreatmentEpisode> TreatmentEpisodes { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class TreatmentEpisode
    {
        public string SourceRecordIdentifier { get; set; }
        public string FederalTaxIdentifier { get; set; }
        public string ClientSourceRecordIdentifier { get; set; }
        public List<ImmediateDischarge> ImmediateDischarges { get; set; }
        public List<Admission> Admissions { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }

    public partial class ImmediateDischarge
    {
        public string SourceRecordIdentifier { get; set; }
        public string EvaluationDate { get; set; }
        public string Note { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
    public partial class Admission
    {
        public string SourceRecordIdentifier { get; set; }
        public string SiteIdentifier { get; set; }
        public string StaffEducationLevelCode { get; set; }
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
        public string PriorityPopulationCode { get; set; }
        public List<PerformanceOutcomeMeasure> PerformanceOutcomeMeasures { get; set; }
        public List<Evaluation> Evaluations { get; set; }
        public List<Diagnosis> Diagnoses { get; set; }
        public List<Discharge> Discharge { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class PerformanceOutcomeMeasure
    {
        public string SourceRecordIdentifier { get; set; }
        public string StaffEducationLevelCode { get; set; }
        public string StaffIdentifier { get; set; }
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
    }

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
        public string SourceRecordIdentifier { get; set; }

        /// <remarks/>
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
    }

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
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Discharge
    {
        private string sourceRecordIdentifierField;

        private string staffEducationLevelCodeField;

        private string staffIdentifierField;

        private string typeCodeField;

        private string dischargeDateField;

        private string lastContactDateField;

        private string dischargeReasonCodeField;

        private string dischargeDestinationCodeField;

        private string birthOutcomeCodeField;

        private string drugFreeAtDeliveryCodeField;

        private string futureDrugUseIntendedCodeField;

        private string friendsUseDrugsCodeField;

        private PerformanceOutcomeMeasure performanceOutcomeMeasuresField;

        private List<Evaluation> evaluationsField;

        private Diagnosis diagnosesField;

        /// <remarks/>
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
        public string DischargeDate
        {
            get
            {
                return this.dischargeDateField;
            }
            set
            {
                this.dischargeDateField = value;
            }
        }

        /// <remarks/>
        public string LastContactDate
        {
            get
            {
                return this.lastContactDateField;
            }
            set
            {
                this.lastContactDateField = value;
            }
        }

        /// <remarks/>
        public string DischargeReasonCode
        {
            get
            {
                return this.dischargeReasonCodeField;
            }
            set
            {
                this.dischargeReasonCodeField = value;
            }
        }

        /// <remarks/>
        public string DischargeDestinationCode
        {
            get
            {
                return this.dischargeDestinationCodeField;
            }
            set
            {
                this.dischargeDestinationCodeField = value;
            }
        }

        /// <remarks/>
        public string BirthOutcomeCode
        {
            get
            {
                return this.birthOutcomeCodeField;
            }
            set
            {
                this.birthOutcomeCodeField = value;
            }
        }

        /// <remarks/>
        public string DrugFreeAtDeliveryCode
        {
            get
            {
                return this.drugFreeAtDeliveryCodeField;
            }
            set
            {
                this.drugFreeAtDeliveryCodeField = value;
            }
        }

        /// <remarks/>
        public string FutureDrugUseIntendedCode
        {
            get
            {
                return this.futureDrugUseIntendedCodeField;
            }
            set
            {
                this.futureDrugUseIntendedCodeField = value;
            }
        }

        /// <remarks/>
        public string FriendsUseDrugsCode
        {
            get
            {
                return this.friendsUseDrugsCodeField;
            }
            set
            {
                this.friendsUseDrugsCodeField = value;
            }
        }

        /// <remarks/>
        public PerformanceOutcomeMeasure PerformanceOutcomeMeasures
        {
            get
            {
                return this.performanceOutcomeMeasuresField;
            }
            set
            {
                this.performanceOutcomeMeasuresField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Evaluation", IsNullable = false)]
        public List<Evaluation> Evaluations
        {
            get
            {
                return this.evaluationsField;
            }
            set
            {
                this.evaluationsField = value;
            }
        }

        /// <remarks/>
        public Diagnosis Diagnoses
        {
            get
            {
                return this.diagnosesField;
            }
            set
            {
                this.diagnosesField = value;
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
        public string DisorderRankCode { get; set; }
        public string DisorderCode { get; set; }
        public string RouteOfAdministrationCode { get; set; }
        public string FrequencyofUseCode { get; set; }
        public string FirstUseAge { get; set; }
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
}
