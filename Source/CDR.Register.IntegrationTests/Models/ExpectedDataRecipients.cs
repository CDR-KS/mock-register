using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CDR.Register.IntegrationTests.Models
{
    public class ExpectedDataRecipients
    {
        public ExpectedDataRecipients()
        {
            Data = new List<DataRecipient>();
        }

        [JsonProperty("data")]
        public List<DataRecipient> Data { get; set; }
    }
    public class DataRecipient
    {
        public DataRecipient()
        {
            DataRecipientBrands  = new List<DataRecipientBrand>();
        }

        [JsonProperty("accreditationNumber")]
        public string AccreditationNumber { get; set; }

        [JsonProperty("legalEntityId")]
        public string LegalEntityId { get; set; }

        [JsonProperty("legalEntityName")]
        public string LegalEntityName { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("dataRecipientBrands")]
        public List<DataRecipientBrand> DataRecipientBrands { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }
    }
    public class DataRecipientBrand
    {
        public DataRecipientBrand()
        {
            SoftwareProducts  = new List<SoftwareProduct>();
        }

        [JsonProperty("dataRecipientBrandId")]
        public string DataRecipientBrandId { get; set; }

        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("softwareProducts")]
        public List<SoftwareProduct> SoftwareProducts { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class SoftwareProduct
    {
        [JsonProperty("softwareProductId")]
        public string SoftwareProductId { get; set; }

        [JsonProperty("softwareProductName")]
        public string SoftwareProductName { get; set; }

        [JsonProperty("softwareProductDescription")]
        public string SoftwareProductDescription { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}