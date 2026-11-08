namespace eBike.Application.Features.CashCollections.DTOs;

public class CashCollectionMoneyReceiptDto
{
    public CashCollectionCompanyDto Company { get; set; } = new();
    public CashCollectionCustomerDto Customer { get; set; } = new();
    public CashCollectionReceiptMetadataDto Receipt { get; set; } = new();
    public CashCollectionPaymentDto Payment { get; set; } = new();
}

public class CashCollectionCompanyDto
{
    public int ConcernID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TelephoneNo { get; set; }
    public string? EmailAddress { get; set; }
    public string? WebAddress { get; set; }
    public string? VATRegistrationNo { get; set; }
}

public class CashCollectionCustomerDto
{
    public int CustomerID { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? FatherName { get; set; }
    public decimal TotalDue { get; set; }
}

public class CashCollectionReceiptMetadataDto
{
    public int CashCollectionID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public string? MRNo { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public DateTime? LastSalesDate { get; set; }
    public int ProjectType { get; set; } = 1;
    public string ProjectTypeName { get; set; } = "Motorcycle";
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;
    public int? CreatedByID { get; set; }
    public string? CreatedByName { get; set; }
}

public class CashCollectionPaymentDto
{
    public decimal Amount { get; set; }
    public decimal AdjustAmt { get; set; }
    public decimal InterestAmt { get; set; }
    public decimal NetCollectionAmount { get; set; }
    public string InWordsAmount { get; set; } = string.Empty;
    public int PaymentType { get; set; }
    public string PaymentTypeName { get; set; } = "Cash";
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = "FromCustomer";
    public string? AccountNo { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? MBAccountNo { get; set; }
    public string? BKashNo { get; set; }
    public decimal BalanceDue { get; set; }
}

