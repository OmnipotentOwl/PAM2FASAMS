using PAM2FASAMS.Migrations;
using PAM2FASAMS.Models.FASAMS;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.DataContext
{
    public class SqliteDbContextInitializer : SqliteDropCreateDatabaseWhenModelChanges<fasams_db>
    {
        public SqliteDbContextInitializer(DbModelBuilder modelBuilder)
        : base(modelBuilder) { }

        protected override void Seed(fasams_db context)
        {
            #region FundingSources Seed
            List<FundingSource> fundingSources = new List<FundingSource>
            {
                new FundingSource { FundingSourceCode = "2", FundingSourceName = "SAMH" },
                new FundingSource { FundingSourceCode = "3", FundingSourceName = "TANF" },
                new FundingSource { FundingSourceCode = "5", FundingSourceName = "Local Match" },
                new FundingSource { FundingSourceCode = "B", FundingSourceName = "Title XXI" }
            };
            context.FundingSources.AddRange(fundingSources);
            #endregion
            #region CoveredServices Seed
            List<CoveredService> coveredServices = new List<CoveredService>
            {
                new CoveredService
                {
                    TreatmentSettingCode = "02",
                    TreatmentSettingName = "Detoxification, 24-hour service, Free-Standing Residential",
                    CoveredServiceCode = "24",
                    CoveredServiceName = "Substance Abuse Inpatient Detoxification",
                    AdultMH = false,
                    AdultSA = true,
                    ChildrenMH = false,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Availibility,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "08",
                    TreatmentSettingName = "Ambulatory - Detoxification",
                    CoveredServiceCode = "32",
                    CoveredServiceName = "Substance Abuse Outpatient Detoxification",
                    AdultMH = false,
                    AdultSA = true,
                    ChildrenMH = false,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Availibility,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "03",
                    TreatmentSettingName = "Rehabilitation/Residential - Hospital (other than Detoxification)",
                    CoveredServiceCode = "03",
                    CoveredServiceName = "Crisis Stabilization",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Availibility,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "03",
                    TreatmentSettingName = "Rehabilitation/Residential - Hospital (other than Detoxification)",
                    CoveredServiceCode = "09",
                    CoveredServiceName = "Inpatient",
                    AdultMH = true,
                    AdultSA = false,
                    ChildrenMH = true,
                    ChildrenSA = false,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "04",
                    TreatmentSettingName = "Rehabilitation/Residential -Short term (30 days or fewer)",
                    CoveredServiceCode = "39",
                    CoveredServiceName = "Short-term Residential Treatment",
                    AdultMH = true,
                    AdultSA = false,
                    ChildrenMH = false,
                    ChildrenSA = false,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "18",
                    CoveredServiceName = "Residential Level I",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "19",
                    CoveredServiceName = "Residential Level II",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "20",
                    CoveredServiceName = "Residential Level III",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "21",
                    CoveredServiceName = "Residential Level IV",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "36",
                    CoveredServiceName = "Room and Board with Supervision Level I",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "37",
                    CoveredServiceName = "Room and Board with Supervision Level II",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "05",
                    TreatmentSettingName = "Rehabilitation/Residential -Long term (more than 30 days)",
                    CoveredServiceCode = "38",
                    CoveredServiceName = "Room and Board with Supervision Level III",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Day
                },
                new CoveredService
                {
                    TreatmentSettingCode = "06",
                    TreatmentSettingName = "Ambulatory – Intensive outpatient",
                    CoveredServiceCode = "04",
                    CoveredServiceName = "Crisis Support/Emergency",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific, EventType.NonClientSpecific },
                    PaymentType = PaymentType.Availibility,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "06",
                    TreatmentSettingName = "Ambulatory – Intensive outpatient",
                    CoveredServiceCode = "06",
                    CoveredServiceName = "Day Treatment",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "06",
                    TreatmentSettingName = "Ambulatory – Intensive outpatient",
                    CoveredServiceCode = "08",
                    CoveredServiceName = "In-Home and On-Site Community based care",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "06",
                    TreatmentSettingName = "Ambulatory – Intensive outpatient",
                    CoveredServiceCode = "10",
                    CoveredServiceName = "Intensive Case Management",
                    AdultMH = true,
                    AdultSA = false,
                    ChildrenMH = true,
                    ChildrenSA = false,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "01",
                    CoveredServiceName = "Assessment",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "02",
                    CoveredServiceName = "Case Management",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "11",
                    CoveredServiceName = "Intervention (Individual)",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "12",
                    CoveredServiceName = "Medical Services",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "13",
                    CoveredServiceName = "Medication Assisted Treatment",
                    AdultMH = false,
                    AdultSA = true,
                    ChildrenMH = false,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.Dosage
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "14",
                    CoveredServiceName = "Outpatient",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "25",
                    CoveredServiceName = "Supportive Employment",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "26",
                    CoveredServiceName = "Supported Housing/Living",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "27",
                    CoveredServiceName = "Treatment Alternative for Safer Community",
                    AdultMH = false,
                    AdultSA = true,
                    ChildrenMH = false,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "29",
                    CoveredServiceName = "Aftercare",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "40",
                    CoveredServiceName = "Mental Health Clubhouse Services",
                    AdultMH = true,
                    AdultSA = false,
                    ChildrenMH = false,
                    ChildrenSA = false,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "44",
                    CoveredServiceName = "Comprehensive Community Service Team",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific, EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "07",
                    TreatmentSettingName = "Ambulatory – Non-Intensive outpatient",
                    CoveredServiceCode = "46",
                    CoveredServiceName = "Recovery Support",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "05",
                    CoveredServiceName = "Day Care",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "07",
                    CoveredServiceName = "Drop-In/Self-Help Centers",
                    AdultMH = true,
                    AdultSA = false,
                    ChildrenMH = false,
                    ChildrenSA = false,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.NonDirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "15",
                    CoveredServiceName = "Outreach",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.NonDirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "22",
                    CoveredServiceName = "Respite Services",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "28",
                    CoveredServiceName = "Incidental Expenses",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DollarsSpent
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "30",
                    CoveredServiceName = "Information and Referral",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Availibility,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "48",
                    CoveredServiceName = "Indicated Prevention",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.ClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.DirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "49",
                    CoveredServiceName = "Selective Prevention",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.NonDirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "50",
                    CoveredServiceName = "Universal Direct Prevention",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.NonDirectStaffMinute
                },
                new CoveredService
                {
                    TreatmentSettingCode = "97",
                    TreatmentSettingName = "Non-TEDS Tx Service Settings",
                    CoveredServiceCode = "51",
                    CoveredServiceName = "Universal Indirect Prevention",
                    AdultMH = true,
                    AdultSA = true,
                    ChildrenMH = true,
                    ChildrenSA = true,
                    EventTypes = new List<EventType> { EventType.NonClientSpecific },
                    PaymentType = PaymentType.Utilization,
                    DefaultUnitOfMeasurement = DefaultUnitOfMeasurement.NonDirectStaffMinute
                }
            };
            context.CoveredServices.AddRange(coveredServices);
            context.SaveChanges();
            #endregion
            #region ExpenditureOcaCodes Seed
            List<ExpenditureOcaCode> expenditureOcaCodes = new List<ExpenditureOcaCode>
            {
                new ExpenditureOcaCode
                {
                    Code = "MH000",
                    Name = "ME Services & Supports Provider Activity - Mental Health",
                    EffectiveDate = DateTime.Parse("7/1/2015"),
                    ValidCoveredServices = context.CoveredServices.Where(x => new string[] {
                        "01","02","03","04","05","06","07","08","09","10","11","12","14","15","18","19","20","21","22","25","26","28",
                        "29","30","36","37","38","39","40","44","46"
                    }.Contains(x.CoveredServiceCode)).ToList(),
                    ValidProgram_MH=true,
                    ValidFunds = context.FundingSources.Where(x => new string[] { "2", "5" }.Contains(x.FundingSourceCode)).ToList()
                },
                new ExpenditureOcaCode
                {
                    Code = "MHSFP",
                    Name = "MH for Profit Contracting ",
                    EffectiveDate = DateTime.Parse("10/1/2016"),
                    ValidCoveredServices = context.CoveredServices.Where(x => new string[] {
                        "01","02","04","05","06","07","08","10","11","12","14","15","18","19","20","21","22","25","26","28",
                        "29","30","36","37","38","39","40","44","46"
                    }.Contains(x.CoveredServiceCode)).ToList(),
                    ValidProgram_MH=true,
                    ValidFunds = context.FundingSources.Where(x => new string[] { "2", "5" }.Contains(x.FundingSourceCode)).ToList()
                }
            };
            context.ExpenditureOcaCodes.AddRange(expenditureOcaCodes);
            #endregion
            #region ExpenditureCodeModifiers Seed
            List<ExpenditureCodeModifier> expenditureCodeModifiers = new List<ExpenditureCodeModifier> {
                new ExpenditureCodeModifier { Code = "BD", Description = "Children Non-Residential Services", ExpenditureCode = "MHC09" },
                new ExpenditureCodeModifier { Code = "BC", Description = "Children Mental Health 24hr Residential Services", ExpenditureCode = "MHC01" },
                new ExpenditureCodeModifier { Code = "BE", Description = "Children Crisis Services", ExpenditureCode = "MHC18" },
                new ExpenditureCodeModifier { Code = "BF", Description = "Children Prevention Services", ExpenditureCode = "MHC25" },
                new ExpenditureCodeModifier { Code = "BH", Description = "Residential Treatment for Emotionally Disturbed Children/Youth", ExpenditureCode = "MHC71" },
                new ExpenditureCodeModifier { Code = "BI", Description = "Title XXI Children’s Health Insurance Program (Behavioral Health Network)", ExpenditureCode = "MHCBN" },
                new ExpenditureCodeModifier { Code = "BJ", Description = "Miami Wrap Around Grant", ExpenditureCode = "MHCMD" },
                new ExpenditureCodeModifier { Code = "BK", Description = "FACES Miami", ExpenditureCode = "MHCFA" },
                new ExpenditureCodeModifier { Code = "DB", Description = "Child At Risk Emotionally Disturbed", ExpenditureCode = "MHC77" },
                new ExpenditureCodeModifier { Code = "DU", Description = "Specialized Treatment, Education and Prevention Services (STEPS)", ExpenditureCode = "MH050" },
                new ExpenditureCodeModifier { Code = "EH", Description = "MH For Profit Contracting", ExpenditureCode = "MHSFP" },
                new ExpenditureCodeModifier { Code = "EJ", Description = "MH System of Care", ExpenditureCode = "MHSOC" }
            };
            context.ExpenditureCodeModifiers.AddRange(expenditureCodeModifiers);
            #endregion

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
