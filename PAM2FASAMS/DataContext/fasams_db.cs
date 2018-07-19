using PAM2FASAMS.OutputFormats;
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
        }
        
        public void Write(object m)
        {
            System.Diagnostics.Debug.Write(m);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
        public virtual DbSet<ProviderClient> ProviderClients { get; set; }
        public virtual DbSet<TreatmentEpisode> TreatmentEpisodes { get; set; }

    }
}
