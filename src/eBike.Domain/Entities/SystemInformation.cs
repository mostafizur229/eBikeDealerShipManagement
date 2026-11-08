namespace eBike.Domain.Entities;

public class SystemInformation
{
    public int SystemInfoID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TelephoneNo { get; set; }
    public string? EmailAddress { get; set; }
    public string? WebAddress { get; set; }
    public DateTime? SystemStartDate { get; set; }
    public string? ProductPhotoPath { get; set; }
    public string? SupplierPhotoPath { get; set; }
    public string? CustomerPhotoPath { get; set; }
    public string? CustomerNIDPatht { get; set; }
    public string? SupplierDocPath { get; set; }
    public string? EmployeePhotoPath { get; set; }
    public int ConcernID { get; set; }
    public string? VATRegistrationNo { get; set; }
    public string? VATChallanBank { get; set; }
    public string? BRTAName { get; set; }
    public string? BRTAAddress { get; set; }
    public string? VATRegPerson { get; set; }
    public string? VATBIN { get; set; }
    public string? VATCIssueAddress { get; set; }
    public string? VATChallanCode { get; set; }
    public string? DeviceIP { get; set; }
    public string? DeviceSerialNO { get; set; }
    public string? APIKey { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? ExpireMessage { get; set; }
    public string? WarningMsg { get; set; }

    // SMS & Dealership Policies from Old System Information
    public int SMSServiceEnable { get; set; }
    public string? InsuranceContactNo { get; set; }
    public int DaysBeforeSendSMS { get; set; }
    public int SMSProviderID { get; set; }
    public int SMSSendToOwner { get; set; }
    public string? SenderId { get; set; }
    public string? CompanyURL { get; set; }
    public int IsNewSInvoiceEnable { get; set; }
    public int IsAutoCreditInterestPer { get; set; }
    public decimal smsCharge { get; set; }
    public int? IsRetailSMSEnable { get; set; }
    public int CustomerDueLimitApply { get; set; }
    public int EmployeeDueLimitApply { get; set; }
    public int ApprovalSystemEnable { get; set; }
    public int MRNoRequired { get; set; }
    public int REGMRNoRequired { get; set; }
    public int LedgerNoRequired { get; set; }

    public byte[]? CompanyLogo { get; set; }
    public string? LogoMimeType { get; set; }
    public byte[]? BrandLogo { get; set; }
    public string? BLogoMimeType { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
