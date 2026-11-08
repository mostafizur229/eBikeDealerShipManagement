namespace eBike.Application.Features.CreditSalesOrders.DTOs;

public class CreditSalesOrderInvoiceDto
{
    public CreditSalesCompanyDto Company { get; set; } = new();
    public CreditSalesCustomerDto Customer { get; set; } = new();
    public CreditSalesMetadataDto Order { get; set; } = new();
    public CreditSalesFinancialsDto Financials { get; set; } = new();
    public List<CreditSalesInvoiceScheduleDto> Schedules { get; set; } = new();
    public List<CreditSalesInvoiceItemDto> Items { get; set; } = new();
}

public class CreditSalesCompanyDto
{
    public int ConcernID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TelephoneNo { get; set; }
    public string? EmailAddress { get; set; }
    public string? WebAddress { get; set; }
    public string? VATRegistrationNo { get; set; }
    public int? IsNewSInvoiceEnable { get; set; }
}

public class CreditSalesCustomerDto
{
    public int CustomerID { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? NID { get; set; }
    public string? DistrictName { get; set; }
    public string? ThanaName { get; set; }
    public string? PostOffice { get; set; }
    public string? RefName { get; set; }
    public string? RefFather { get; set; }
    public string? RefAddress { get; set; }
    public string? RefContact { get; set; }
    public decimal CreditDue { get; set; }
    public decimal TotalDue { get; set; }
}

public class CreditSalesMetadataDto
{
    public int CreditSalesID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime SalesDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string ReportHeader { get; set; } = "Credit Invoice";
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "Sales";
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;
    public int? CreatedByID { get; set; }
    public string? CreatedByName { get; set; }
    public string? EmployeeName { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? ChequeNo { get; set; }

    public string TemplateName { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public bool HasInvoice2 { get; set; }
    public string? Invoice2TemplateName { get; set; }
    public string? Invoice2TemplatePath { get; set; }
}

public class CreditSalesFinancialsDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal Discount { get; set; }
    public decimal DownPayment { get; set; }
    public decimal Remaining { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal NetAmount { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal InstallmentPrinciple { get; set; }
    public string InWordsTotal { get; set; } = string.Empty;
    public string InWordsDownpayment { get; set; } = string.Empty;
}

public class CreditSalesInvoiceScheduleDto
{
    public int ScheduleNo { get; set; }
    public DateTime ScheduleDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public string Status { get; set; } = "Due";
}

public class CreditSalesInvoiceItemDto
{
    public int Sl { get; set; }
    public int CreditSaleDetailsID { get; set; }
    public int ProductID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? CompanyName { get; set; }
    public string? ColorName { get; set; }
    public string? GodownName { get; set; }

    public string? ChassisNo { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public string? CC { get; set; }
    public string? NoOfCylinder { get; set; }
    public string? SeatingCapacity { get; set; }
    public string? VehicleSize { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal IntAmt { get; set; } // IntTotalAmt
    public decimal VatAmt { get; set; }
    public decimal TotalAmt { get; set; }

    public string? CompressorWarranty { get; set; }
    public string? MotorWarranty { get; set; }
    public string? ServiceWarranty { get; set; }
    public string? SparePartsWarranty { get; set; }
}

