﻿using System;
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
        [MaxLength(100)]
        public string SourceRecordIdentifier { get; set; }
        [Required]
        public string BirthDate { get; set; }
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [DefaultValue("")]
        [MaxLength(100)]
        public string MiddleName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [DefaultValue("")]
        [MaxLength(100)]
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
        [Key, Column(Order = 1)]
        public string TypeCode { get; set; }
        [Required]
        public string Identifier { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key,ForeignKey("Client"),Column(Order = 0)]
        public string ClientSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Client"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhone
    {
        [Key, Column(Order = 1)]
        public string TypeCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Client"), Column(Order = 0)]
        public string ClientSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Client"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientEmailAddress
    {
        [Key, Column(Order = 1)]
        public string EmailAddress { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Client"), Column(Order = 0)]
        public string ClientSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Client"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhysicalAddress
    {
        [Key, Column(Order = 1)]
        public string TypeCode { get; set; }
        [Required]
        [MaxLength(100)]
        public string StreetAddress { get; set; }
        [Required]
        [MaxLength(100)]
        public string CityName { get; set; }
        [Required]
        public string StateCode { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string CountyAreaCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [Key, ForeignKey("Client"), Column(Order = 0)]
        public string ClientSourceId { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ForeignKey("Client"), Column(Order = 2)]
        public string FederalTaxIdentifier { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ProviderClient Client { get; set; }
    }
}
