using PAM2FASAMS.Models.FASAMS;
using PAM2FASAMS.Models.Utils;
using PAM2FASAMS.OutputFormats;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.DataContext
{
    public partial class fasams_db : DbContext
    {
        private const string connectionStringName = "Name=fasams_dbEntities";

        public fasams_db()
            : base(connectionStringName)
        {
            this.Database.Log = Write;
            this.Configuration.ProxyCreationEnabled = false;
        }
        
        public void Write(object m)
        {
            System.Diagnostics.Debug.Write(m);

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            string DbType = ConfigurationManager.AppSettings["DBType"];
            if(DbType == "Local")
            {
                var sqliteConnectionInitializer = new SqliteDbContextInitializer(modelBuilder);           
                Database.SetInitializer(sqliteConnectionInitializer);
            }
            if(DbType == "SQLServer")
            {

            }
        }
        public virtual DbSet<ProviderClient> ProviderClients { get; set; }
        public virtual DbSet<ProviderClientIdentifier> ProviderClientIdentifiers { get; set; }
        public virtual DbSet<ProviderClientPhone> ProviderClientPhones { get; set; }
        public virtual DbSet<ProviderClientEmailAddress> ProviderClientEmailAddresses { get; set; }
        public virtual DbSet<ProviderClientPhysicalAddress> ProviderClientPhysicalAddresses { get; set; }
        public virtual DbSet<TreatmentEpisode> TreatmentEpisodes { get; set; }
        public virtual DbSet<Admission> Admissions { get; set; }
        public virtual DbSet<ImmediateDischarge> ImmediateDischarges { get; set; }
        public virtual DbSet<PerformanceOutcomeMeasure> PerformanceOutcomeMeasures { get; set; }
        public virtual DbSet<SubstanceUseDisorder> SubstanceUseDisorders { get; set; }
        public virtual DbSet<Evaluation> Evaluations { get; set; }
        public virtual DbSet<Diagnosis> Diagnoses { get; set; }
        public virtual DbSet<Discharge> Discharges { get; set; }
        public virtual DbSet<ServiceEvent> ServiceEvents { get; set; }
        public virtual DbSet<ServiceEventCoveredServiceModifier> ServiceEventCoveredServiceModifiers { get; set; }
        public virtual DbSet<ServiceEventHcpcsProcedureModifier> ServiceEventHcpcsProcedureModifiers { get; set; }
        public virtual DbSet<ServiceEventExpenditureModifier> ServiceEventExpenditureModifiers { get; set; }
        public virtual DbSet<Subcontract> Subcontracts { get; set; }
        public virtual DbSet<SubcontractService> SubcontractServices { get; set; }
        public virtual DbSet<SubcontractOutputMeasure> SubcontractOutputMeasures { get; set; }
        public virtual DbSet<SubcontractOutcomeMeasure> SubcontractOutcomeMeasures { get; set; }

        public virtual DbSet<JobLog> JobLogs { get; set; }
        public virtual DbSet<IdHistory> IdHistorys { get; set; }
        public virtual DbSet<FundingSource> FundingSources { get; set; }
        public virtual DbSet<CoveredService> CoveredServices { get; set; }
        public virtual DbSet<ExpenditureOcaCode> ExpenditureOcaCodes { get; set; }
        public virtual DbSet<ExpenditureCodeModifier> ExpenditureCodeModifiers { get; set; }

    }
}
