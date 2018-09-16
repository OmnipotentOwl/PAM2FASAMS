using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.OutputFormats
{
    public class SubcontractDataSet
    {

    }
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Subcontracts
    {
        [System.Xml.Serialization.XmlElementAttribute("Subcontract")]
        public List<Subcontract> subcontracts { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Subcontract
    {
        [Key, Column(Order = 0)]
        public string ContractNumber { get; set; }
        [Key, Column(Order = 1)]
        [MaxLength(50)]
        public string SubcontractNumber { get; set; }
        [Required]
        public string FederalTaxIdentifier { get; set; }
        [Required]
        public string TypeCode { get; set; }
        [Required]
        public string EffectiveDate { get; set; }
        [Required]
        public string ExpirationDate { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(50)]
        [DefaultValue("")]
        public string AmendmentNumber { get; set; }
        public string AmendmentDate { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SubcontractService", IsNullable = false)]
        public List<SubcontractService> SubcontractServices { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SubcontractOutputMeasure", IsNullable = false)]
        public List<SubcontractOutputMeasure> SubcontractOutputMeasures { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("SubcontractOutcomeMeasure", IsNullable = false)]
        public List<SubcontractOutcomeMeasure> SubcontractOutcomeMeasures { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalEffectiveDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EffectiveDate))
                {
                    return DateTime.Parse(EffectiveDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalExpirationDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ExpirationDate))
                {
                    return DateTime.Parse(ExpirationDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalAmendmentDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AmendmentDate))
                {
                    return DateTime.Parse(AmendmentDate);
                }
                return DateTime.Now;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubcontractService
    {
        [Key]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Required]
        public string CoveredServiceCode { get; set; }
        [Required]
        public string ProgramAreaCode { get; set; }
        [Required]
        public string ExpenditureOcaCode { get; set; }
        [Required]
        public string EffectiveDate { get; set; }
        [Required]
        public string ExpirationDate { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal ContractedAmount { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal PaymentRatePerUnitAmount { get; set; }
        public string PaymentMethodCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitOfMeasureCode { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalEffectiveDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EffectiveDate))
                {
                    return DateTime.Parse(EffectiveDate);
                }
                return DateTime.Now;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime InternalExpirationDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ExpirationDate))
                {
                    return DateTime.Parse(ExpirationDate);
                }
                return DateTime.Now;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Subcontract"), Column(Order = 1)]
        public string ContractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Subcontract"), Column(Order = 2)]
        public string SubcontractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Subcontract"), Column(Order = 3)]
        public string AmendmentNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Subcontract Subcontract { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubcontractOutputMeasure
    {
        [Key, Column(Order = 0)]
        public string ProgramAreaCode { get; set; }
        [Key, Column(Order = 1)]
        public string ServiceCategoryCode { get; set; }
        [Required]
        [DefaultValue(0)]
        public int TargetPersonsServedCount { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key,ForeignKey("Subcontract"), Column(Order = 2)]
        public string ContractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key,ForeignKey("Subcontract"), Column(Order = 3)]
        public string SubcontractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key,ForeignKey("Subcontract"), Column(Order = 4)]
        public string AmendmentNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Subcontract Subcontract { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SubcontractOutcomeMeasure
    {
        [Key, Column(Order = 0)]
        public string ProgramAreaCode { get; set; }
        [Key, Column(Order = 1)]
        public string OutcomeMeasureCode { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal TargetValue { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Subcontract"), Column(Order = 2)]
        public string ContractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Subcontract"), Column(Order = 3)]
        public string SubcontractNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Subcontract"), Column(Order = 4)]
        public string AmendmentNumber { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Subcontract Subcontract { get; set; }
    }
}
