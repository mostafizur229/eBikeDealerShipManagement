namespace eBike.Application.Features.Registrations.DTOs;

public class RegistrationDocDto
{
    public int DocumentType { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public bool IsMultiPage { get; set; }
    public int PageCount { get; set; } = 1;
    public string Language { get; set; } = "en";
    public RegDocCompanyDto Company { get; set; } = new();
    public RegDocCustomerDto Customer { get; set; } = new();
    public RegDocVehicleDto Vehicle { get; set; } = new();
    public RegDocSaleDto Sale { get; set; } = new();
    public RegDocBRTAFeeDto BRTAFee { get; set; } = new();
    public RegDocMushokDto? Mushok { get; set; }
    public RegDocGatePassDto? GatePass { get; set; }
    public RegDocMultiPageDto? MultiPage { get; set; }
    public string? Remarks { get; set; }
    public string? DeclarationText { get; set; }
}

public class RegDocCompanyDto
{
    public int ConcernId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TelephoneNo { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string WebAddress { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string BRTACircle { get; set; } = string.Empty;
    public string BRTACircleAddress { get; set; } = string.Empty;
    public string VATRegistrationNo { get; set; } = string.Empty;
    public string VATBIN { get; set; } = string.Empty;
    public string VATRegPerson { get; set; } = string.Empty;
    public string VATCIssueAddress { get; set; } = string.Empty;
    public string VATChallanCode { get; set; } = string.Empty;
}

public class RegDocCustomerDto
{
    public int CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? HusbandOrWifeName { get; set; }
    public string? Sex { get; set; }
    public string? ContactNo { get; set; }
    public string? NID { get; set; }
    public DateTime? DOB { get; set; }
    public string? FormattedDOB { get; set; }
    public int? Age { get; set; }
    public string? Village { get; set; }
    public string? Post { get; set; }
    public string? Thana { get; set; }
    public string? District { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;
    public string? PermanentAddress { get; set; }
    public string? GuardianName { get; set; }
    public bool IsDocTransfer { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? OriginalCustomerName { get; set; }
}

public class RegDocVehicleDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ClassOfProduct { get; set; } = "Motor Cycle";
    public string ChassisNo { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string CC { get; set; } = string.Empty;
    public decimal ParsedCC { get; set; }
    public string AssembleYear { get; set; } = string.Empty;
    public string NoOfCylinder { get; set; } = string.Empty;
    public string VehicleSize { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string UnLadenWeight { get; set; } = string.Empty;
    public string TireSize { get; set; } = string.Empty;
    public string RingSize { get; set; } = string.Empty;
    public string SeatCapacity { get; set; } = string.Empty;
    public string HorsePower { get; set; } = string.Empty;
    public string RPM { get; set; } = string.Empty;
    public string MakersName { get; set; } = string.Empty;
    public string MadeIn { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal SalesRate { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
}

public class RegDocSaleDto
{
    public int RegistrationId { get; set; }
    public string? RegNo { get; set; }
    public string? REGMRNo { get; set; }
    public DateTime RegApplyDate { get; set; }
    public string SaleType { get; set; } = "Cash";
    public int? OrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string RefNo { get; set; } = string.Empty;
    public string ChallanNo { get; set; } = string.Empty;
    public DateTime? SalesDate { get; set; }
    public string FormattedSalesDate { get; set; } = string.Empty;
    public decimal ReceivedAmount { get; set; }
    public decimal RegistrationFee { get; set; }
    public decimal InsuranceFee { get; set; }
}

public class RegDocBRTAFeeDto
{
    public decimal FeeAmt { get; set; }
    public decimal NewRegMF { get; set; }
    public decimal NewRegIF { get; set; }
    public decimal NewRegTF { get; set; }
    public decimal IssueTaxTokenMF { get; set; }
    public decimal IssueTaxTokenTF { get; set; }
    public decimal RegCertificateMF { get; set; }
    public decimal RegCertificateTF { get; set; }
    public decimal SuppDutyMF { get; set; }
    public decimal SuppDutyTF { get; set; }
    public decimal Contribution { get; set; }
    public decimal NamePlate { get; set; }
    public string RegYears { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string InWordsTotal { get; set; } = string.Empty;
}

public class RegDocMushokDto
{
    public string ChallanNoText { get; set; } = string.Empty;
    public decimal SalesWithoutMushok { get; set; }
    public decimal SalesMushok { get; set; }
    public decimal TotalWithMushok { get; set; }
    public string InWordsTotal { get; set; } = string.Empty;
    public string DescriptionOfGoods { get; set; } = string.Empty;
}

public class RegDocGatePassDto
{
    public string GatePassNo { get; set; } = string.Empty;
    public string Notice { get; set; } = "N.B: This pass is valid for the day of issue only";
    public string DescriptionOfGoods { get; set; } = "Motorcycle";
}

public class RegDocMultiPageDto
{
    public RegDocSection1Dto Page1Section { get; set; } = new();

    public RegDocSection2Dto Page2Section { get; set; } = new();
}

public class RegDocSection1Dto
{
    public string Title { get; set; } = "Section-I (To be filled by the office)";
    public string ClassOfVehicle { get; set; } = "Motor Cycle";
    public string MakersName { get; set; } = string.Empty;
    public string TypeOfBody { get; set; } = "Solo";
    public string YearOfManufacture { get; set; } = string.Empty;
    public string NumberOfCylinders { get; set; } = string.Empty;
    public string ChassisNo { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string HorsePower { get; set; } = string.Empty;
    public string CC { get; set; } = string.Empty;
    public string SeatingCapacity { get; set; } = string.Empty;
    public string UnladenWeight { get; set; } = string.Empty;
    public string LadenWeight { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string FuelUsed { get; set; } = "Petrol / Octane";
    public string TireSize { get; set; } = string.Empty;
}

public class RegDocSection2Dto
{
    public string Title { get; set; } = "Section-II & III (Application Particulars & Specimen Signatures)";
    public string FullName { get; set; } = string.Empty;
    public string FatherOrHusbandName { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string PresentAddress { get; set; } = string.Empty;
    public string PermanentAddress { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Nationality { get; set; } = "Bangladeshi";
    public string NID { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string Declaration { get; set; } = "I hereby declare that the particulars given above are true and that the vehicle has not been registered elsewhere.";
}

