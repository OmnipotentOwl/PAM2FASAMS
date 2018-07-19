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
    public class ClientDataSet
    {

    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ProviderClients
    {
        [System.Xml.Serialization.XmlElementAttribute("ProviderClient")]
        public List<ProviderClient> clients { get; set; }
    }
    [Table(name: "ProviderClient")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClient
    {
        
        public string UniqueClientIdentifier { get; set; }
        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string FederalTaxIdentifier { get; set; }
        [Key]
        [Column(Order =1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string SourceRecordIdentifier { get; set; }
        [Required]
        public string BirthDate { get; set; }
        [Required]
        public string FirstName { get; set; }
        [DefaultValue("")]
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [DefaultValue("")]
        public string SuffixName { get; set; }
        [Required]
        public string GenderCode { get; set; }
        [Required]
        public string RaceCode { get; set; }
        [Required]
        public string EthnicityCode { get; set; }
        public List<ProviderClientIdentifier> ProviderClientIdentifiers { get; set; }
        public List<ProviderClientPhone> ProviderClientPhones { get; set; }
        public List<ProviderClientEmailAddress> ProviderClientEmailAddresses { get; set; }
        [System.Xml.Serialization.XmlArrayItemAttribute("ProviderClientPhysicalAddress", IsNullable = false)]
        public List<ProviderClientPhysicalAddress> ProviderClientPhysicalAddresses { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientIdentifier
    {
        [Key]
        public string TypeCode { get; set; }
        public string Identifier { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhone
    {
        [Key]
        public string TypeCode { get; set; }
        public string PhoneNumber { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientEmailAddress
    {
        [Key]
        public string EmailAddress { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhysicalAddress
    {
        [Key]
        public string TypeCode { get; set; }
        public string StreetAddress { get; set; }
        public string CityName { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }
        public string CountyAreaCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
}
