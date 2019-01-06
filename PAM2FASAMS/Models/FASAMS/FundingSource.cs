using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.Models.FASAMS
{
    [Table("FundingSources", Schema = "FLReporting_FASAMS")]
    public class FundingSource
    {
        [Key]
        [MaxLength(2)]
        public string FundingSourceCode { get; set; }
        [MaxLength(150)]
        public string FundingSourceName { get; set; }

        public virtual ICollection<ExpenditureOcaCode> ExpenditureOcaCodes { get; set; }
    }
}
