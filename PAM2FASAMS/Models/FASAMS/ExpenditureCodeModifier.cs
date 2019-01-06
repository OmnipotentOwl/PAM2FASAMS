using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.Models.FASAMS
{
    [Table("ExpenditureCodeModifiers", Schema = "FLReporting_FASAMS")]
    public class ExpenditureCodeModifier
    {
        [Key]
        [MaxLength(2)]
        public string Code { get; set; }
        [MaxLength(80)]
        public string Description { get; set; }
        [MaxLength(5)]
        public string ExpenditureCode { get; set; }
    }
}
