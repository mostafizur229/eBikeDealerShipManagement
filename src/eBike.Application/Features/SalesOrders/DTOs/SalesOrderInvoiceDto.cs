namespace eBike.Application.Features.SalesOrders.DTOs;

public class SalesOrderInvoiceDto
{
    public InvoiceCompanyDto Company { get; set; } = new();
    public InvoiceCustomerDto Customer { get; set; } = new();
    public InvoiceMetadataDto Order { get; set; } = new();
    public InvoiceSalesPersonDto SalesPerson { get; set; } = new();
    public InvoiceFinancialsDto Financials { get; set; } = new();
    public List<SalesOrderInvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceCompanyDto
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

public class InvoiceCustomerDto
{
    public int CustomerID { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? DistrictName { get; set; }
    public string? ThanaName { get; set; }
    public string? PostOffice { get; set; }
    public string? RefName { get; set; }
    public string? RefFather { get; set; }
    public string? RefContact { get; set; }
    public string? RefAddress { get; set; }
    public decimal PreviousDue { get; set; }
    public decimal TotalDue { get; set; }
    public DateTime? RemindDate { get; set; }
}

public class InvoiceMetadataDto
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int SOType { get; set; }
    public string SOTypeName { get; set; } = "Vehicle"; // "Vehicle" or "Auto Parts"
    public int Status { get; set; }
    public string StatusName { get; set; } = "Sales";
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;

    public string TemplateName { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public bool HasInvoice2 { get; set; }
    public string? Invoice2TemplateName { get; set; }
    public string? Invoice2TemplatePath { get; set; }
}

public class InvoiceSalesPersonDto
{
    public int? EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeContact { get; set; }
    public int? CreatedByID { get; set; }
    public string? CreatedByName { get; set; }
}

public class InvoiceFinancialsDto
{
    public decimal GrandTotal { get; set; }
    public decimal ItemDiscountTotal { get; set; }
    public decimal ItemOfferTotal { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TDPercentage { get; set; }
    public decimal TDAmount { get; set; }
    public decimal LabourCost { get; set; }
    public decimal AdjAmount { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RecAmount { get; set; }
    public decimal CardPaidAmount { get; set; }
    public decimal CashPaidAmount { get; set; }
    public string? BankName { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal PreviousDue { get; set; }
    public decimal TotalDue { get; set; }
    public string InWordsTotal { get; set; } = string.Empty;
    public string InWordsPaid { get; set; } = string.Empty;
}

public class SalesOrderInvoiceItemDto
{
    public int Sl { get; set; }
    public int SOrderDetailID { get; set; }
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
    public string? HorsePower { get; set; }
    public string? SeatingCapacity { get; set; }
    public string? Weight { get; set; }
    public string? VehicleSize { get; set; }
    public string? MakersName { get; set; }
    public string? MFGYear { get; set; }
    public string? ApplicableModel { get; set; }
    public int RegType { get; set; }
    public string? RegTypeName { get; set; }
    public decimal RegAmount { get; set; }


    public string? PartNo { get; set; }
    public string? LocationName { get; set; }

    public decimal Quantity { get; set; }
    public string UnitType { get; set; } = "Pcs";
    public decimal UnitPrice { get; set; }
    public decimal PPDPercentage { get; set; }
    public decimal PPDAmount { get; set; }
    public decimal PPOffer { get; set; }
    public decimal TotalAmount { get; set; }


    public string? CompressorWarranty { get; set; }
    public string? MotorWarranty { get; set; }
    public string? ServiceWarranty { get; set; }
    public string? SparePartsWarranty { get; set; }
}
