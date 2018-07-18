using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClient
    {
        public string UniqueClientIdentifier { get; set; }
        public string FederalTaxIdentifier { get; set; }
        public string SourceRecordIdentifier { get; set; }
        public string BirthDate { get; set; }
        public string FirstName { get; set; }
        [DefaultValue("")]
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        [DefaultValue("")]
        public string SuffixName { get; set; }
        public string GenderCode { get; set; }
        public string RaceCode { get; set; }
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
        public string TypeCode { get; set; }
        public string Identifier { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhone
    {
        public string TypeCode { get; set; }
        public string PhoneNumber { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientEmailAddress
    {
        public string EmailAddress { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProviderClientPhysicalAddress
    {
        public string TypeCode { get; set; }
        public string StreetAddress { get; set; }
        public string CityName { get; set; }
        public string StateCode { get; set; }
        public string PostalCode { get; set; }
        public string CountyAreaCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string action { get; set; }
    }
}
