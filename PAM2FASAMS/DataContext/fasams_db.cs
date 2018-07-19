using PAM2FASAMS.OutputFormats;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
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
            var sqliteConnectionInitializer = new SqliteDropCreateDatabaseWhenModelChanges<fasams_db>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);

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

    }
}
