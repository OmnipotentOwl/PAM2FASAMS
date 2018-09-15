using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.Utilities
{
    public class IdHistory
    {
        [Key, Column(Order = 0)]
        public string RecordType { get; set; }
        [Key, Column(Order = 1)]
        public string OrigionalSourceRecordId { get; set; }
        public string ONewSourceRecordId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
