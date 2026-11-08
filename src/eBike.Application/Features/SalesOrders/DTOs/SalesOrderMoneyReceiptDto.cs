namespace eBike.Application.Features.SalesOrders.DTOs;

public class SalesOrderMoneyReceiptDto
{
    public InvoiceCompanyDto Company { get; set; } = new();
    public MoneyReceiptCustomerDto Customer { get; set; } = new();
    public MoneyReceiptMetadataDto Receipt { get; set; } = new();
    public MoneyReceiptVehicleInfoDto Vehicle { get; set; } = new();
    public MoneyReceiptPaymentDto Payment { get; set; } = new();
    public InvoiceSalesPersonDto SalesPerson { get; set; } = new();
}

public class MoneyReceiptCustomerDto
{
    public int CustomerID { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? FatherName { get; set; }
}

public class MoneyReceiptMetadataDto
{
    public int SOrderID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? LastSalesDate { get; set; }
    public int SOType { get; set; }
    public string SOTypeName { get; set; } = "Vehicle";
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;
}

public class MoneyReceiptVehicleInfoDto
{
    public string? ProductNames { get; set; }
    public string? ChassisNumbers { get; set; }
    public string? EngineNumbers { get; set; }
}

public class MoneyReceiptPaymentDto
{
    public decimal ReceivedAmount { get; set; }
    public string InWordsAmount { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = "Cash";
    public decimal CashPaidAmount { get; set; }
    public decimal CardPaidAmount { get; set; }
    public string? BankName { get; set; }
    public decimal CurrentDue { get; set; }
    public decimal PreviousDue { get; set; }
    public decimal TotalDue { get; set; }
}
