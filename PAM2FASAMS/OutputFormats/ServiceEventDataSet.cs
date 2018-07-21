using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAM2FASAMS.OutputFormats
{
    class ServiceEventDataSet
    {
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ServiceEvents
    {
        [System.Xml.Serialization.XmlElementAttribute("ServiceEvent")]
        public List<ServiceEvent> serviceEvents { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServiceEvent
    {
        [Key,Column(Order = 0)]
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Key, Column(Order = 1)]
        public string TypeCode { get; set; }
        [Key, Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [Required]
        public string SiteIdentifier { get; set; }
        public string ContractNumber { get; set; }
        public string SubcontractNumber { get; set; }
        public string EpisodeSourceRecordIdentifier { get; set; }
        public string AdmissionSourceRecordIdentifier { get; set; }
        [Required]
        public string ProgramAreaCode { get; set; }
        [Required]
        public string TreatmentSettingCode { get; set; }
        [Required]
        public string CoveredServiceCode { get; set; }
        [Required]
        public string HcpcsProcedureCode { get; set; }
        [Required]
        public string ServiceDate { get; set; }
        public string StartTime { get; set; }
        [Required]
        public object ExpenditureOcaCode { get; set; }
        [Required]
        public uint ServiceUnitCount { get; set; }
        [Required]
        public string FundCode { get; set; }
        public decimal ActualPaymentRateAmount { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ActualPaymentRateAmountSpecified { get; set; }
        public string InvoicedDate { get; set; }
        [MaxLength(100)]
        public string InvoiceNumber { get; set; }
        public string PaidDate { get; set; }
        [MaxLength(100)]
        public string PaymentReferenceNumber { get; set; }
        [Required]
        public string ServiceCountyAreaCode { get; set; }
        public List<ServiceEventCoveredServiceModifier> ServiceEventCoveredServiceModifiers { get; set; }
        public List<ServiceEventHcpcsProcedureModifier> ServiceEventHcpcsProcedureModifiers { get; set; }
        public List<ServiceEventExpenditureModifier> ServiceEventExpenditureModifiers { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServiceEventCoveredServiceModifier
    {
        [Key, Column(Order = 3)]
        public string ModifierCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Event"), Column(Order = 0)]
        public string EventSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 1)]
        public string TypeCode { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ServiceEvent Event { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServiceEventHcpcsProcedureModifier
    {
        [Key, Column(Order = 3)]
        public string ModifierCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Event"), Column(Order = 0)]
        public string EventSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 1)]
        public string TypeCode { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ServiceEvent Event { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ServiceEventExpenditureModifier
    {
        [Key, Column(Order = 3)]
        public string ModifierCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Event"), Column(Order = 0)]
        public string EventSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Event"), Column(Order = 1)]
        public string TypeCode { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ServiceEvent Event { get; set; }
    }
}
