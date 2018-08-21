using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.OutputFormats
{
    public class JobLog
    {
        [Key,Column(Order = 0)]
        public int JobNumber { get; set; }
        [Key,Column(Order = 1)]
        public string RecordType { get; set; }
        [Key,Column(Order = 2)]
        public string SourceRecordId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
