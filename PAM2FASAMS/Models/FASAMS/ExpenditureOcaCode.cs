using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.Models.FASAMS
{
    
    [Table("ExpenditureOcaCodes", Schema = "FLReporting_FASAMS")]
    public class ExpenditureOcaCode
    {
        [Key]
        [MaxLength(5)]
        public string Code { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public List<CoveredService> ValidCoveredServices { get; set; }
        public bool ValidProgram_MH { get; set; }
        public bool ValidProgram_SA { get; set; }
        public List<FundingSource> ValidFunds { get; set; }
    }
}
