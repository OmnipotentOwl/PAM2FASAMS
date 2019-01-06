using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.Models.FASAMS
{
    public enum PaymentType
    {
        Availibility,
        Utilization
    }
    public enum DefaultUnitOfMeasurement
    {
        Day,
        DirectStaffMinute,
        Dosage,
        NonDirectStaffMinute,
        DollarsSpent
    }
    public enum EventType
    {
        ClientSpecific,
        NonClientSpecific
    }
    [Table("CoveredServices", Schema = "FLReporting_FASAMS")]
    public class CoveredService
    {
        [MaxLength(2)]
        public string TreatmentSettingCode { get; set; }
        [MaxLength(300)]
        public string TreatmentSettingName { get; set; }
        [Key]
        [MaxLength(2)]
        public string CoveredServiceCode { get; set; }
        [MaxLength(300)]
        public string CoveredServiceName { get; set; }
        public bool AdultMH { get; set; }
        public bool AdultSA { get; set; }
        public bool ChildrenMH { get; set; }
        public bool ChildrenSA { get; set; }
        public List<EventType> EventTypes { get; set; }
        public PaymentType PaymentType { get; set; }
        public DefaultUnitOfMeasurement DefaultUnitOfMeasurement { get; set; }

        public virtual ICollection<ExpenditureOcaCode> ExpenditureOcaCodes { get; set; }
    }
}
