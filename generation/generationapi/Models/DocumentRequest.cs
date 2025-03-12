using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace generationapi.Models;

public class DocumentRequest
{
   [JsonPropertyName("assessor")]
   public Assessor assessor { get; set; } 
   
   [JsonPropertyName("signature_path")]
   public string signaturePath { get; set; }
   
   [JsonPropertyName("adjuster")]
   public string? adjuster { get; set; }
   
   [JsonPropertyName("insurance_company")]
   public string insuranceCompany  { get; set; }
   
   [JsonPropertyName("claim_number")]
   public string claimNumber { get; set; }
   
   [JsonPropertyName("referral_company")]
   public ReferralCompany referralCompany { get; set; }
   
   [JsonPropertyName("date_of_assessment")]
   public DateOnly dateOfAssessment { get; set; }
   
   [JsonPropertyName("claimant")]
   public Claimant claimant { get; set; }
   
   [JsonPropertyName("document")]
   public Document document { get; set; }
   
   [JsonPropertyName("ac")]
   public Ac? ac { get; set; }
   
   [JsonPropertyName("cat")]
   public Cat? cat { get; set; }
   
   [JsonPropertyName("mrb")]
   public Mrb? mrb { get; set; }
   
}

public class Mrb
{
   [JsonPropertyName("date_of_ocf_18")]
   public DateOnly dateOfOcf18 { get; set; }
   
   [JsonPropertyName("assessor")]
   public string assessor { get; set; }
   
   [JsonPropertyName("amount_of_ocf_18")]
   public string amountOfOcf18 { get; set; }
   
}

public class Cat
{
   [JsonPropertyName("date_of_ocf_19")]
   public DateOnly dateOfOcf19 { get; set; }
   
   [JsonPropertyName("assessor")]
   public string assessor { get; set; }
   
}

public class Ac
{
   [JsonPropertyName("first_assessment")]
   public bool firstAssessment { get; set; }
   
   [JsonPropertyName("date_of_last_assessment")]
   public DateOnly? dateOfLastAssessment { get; set; }
   
   [JsonPropertyName("monthly_allowance")]
   public string? monthlyAllowance { get; set; }
   
   [JsonPropertyName("hourly_rates")]
   public List<string>? hourlyRates { get; set; }
   
}

public class Document
{
   [JsonPropertyName("id")]
   public int id { get; set; }
   
   [JsonPropertyName("path")]
   public string path { get; set; }
   
}

public class Claimant
{
   [JsonPropertyName("first_name")]
   public string firstName { get; set; }
   
   [JsonPropertyName("last_name")]
   public string lastName { get; set; }
   
   [JsonPropertyName("gender")]
   public string gender { get; set; }
   
   [JsonPropertyName("age")]
   public int? age { get; set; }
   
   [JsonPropertyName("youth")]
   public bool? youth { get; set; }
   
   [JsonPropertyName("date_of_birth")]
   public DateOnly dateOfBirth { get; set; }
   
   [JsonPropertyName("date_of_loss")]
   public DateOnly dateOfLoss { get; set; }
   
   [JsonPropertyName("address")]
   public Address address { get; set; }
   
}

public class ReferralCompany
{
   [JsonPropertyName("id")]
   public int id { get; set; }
   
   [JsonPropertyName("name")]
   public string name { get; set; }
   
   [JsonPropertyName("common_name")]
   public string common_name { get; set; }
   
   [JsonPropertyName("phone")]
   public string phone { get; set; }
   
   [JsonPropertyName("fax")]
   public string fax { get; set; }
   
   [JsonPropertyName("email")]
   public string email { get; set; }
   
   [JsonPropertyName("address")]
   public Address address { get; set; }
   
}

public class Address
{
   [JsonPropertyName("street_address")]
   public string streetAddress { get; set; }
   
   [JsonPropertyName("unit")]
   public string? unit { get; set; }
   
   [JsonPropertyName("city")]
   public string city { get; set; }
   
   [JsonPropertyName("province")]
   public string province { get; set; }
   
   [JsonPropertyName("postal_code")]
   public string postalCode { get; set; }
   
   [JsonPropertyName("country")]
   public string country { get; set; }
   
}

public class Assessor
{
   [JsonPropertyName("registration_id")]
   public string registrationId { get; set; }
   
   [JsonPropertyName("first_name")]
   public string firstName { get; set; }
   
   [JsonPropertyName("last_name")]
   public string lastName { get; set; }
   
   [JsonPropertyName("gender")]
   public string gender { get; set; }
   
   [JsonPropertyName("email")]
   public string email { get; set; }
   
   [JsonPropertyName("qualifications_paragraph")]
   public string qualificationsParagraph { get; set; }
   
}

public enum Gender
{
   Male,
   Female,
   Other
}