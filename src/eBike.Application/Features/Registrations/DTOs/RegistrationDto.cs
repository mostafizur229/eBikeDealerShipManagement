namespace eBike.Application.Features.Registrations.DTOs;

public class RegistrationDto
{
    public int RegistrationID { get; set; }
    public DateTime RegApplyDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }

    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? CC { get; set; }
    public decimal? ParsedCC { get; set; }

    public int? StockDetailID { get; set; }
    public string? ChassisNo { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public string? ColorName { get; set; }

    public int? SorderDetailID { get; set; }
    public int? CreditSaleDetailsID { get; set; }
    public string? SaleType { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? SalesDate { get; set; }

    public int RegType { get; set; }
    public string? RegTypeName { get; set; }
    public decimal RegFee { get; set; }
    public decimal InsuranceAmt { get; set; }
    public string? REGMRNo { get; set; }

    // Step 1 - Initiate (1)
    public int? Iscompleted { get; set; }
    public string? StatusName { get; set; }

    // Step 2 - Observation (2)
    public DateTime? ObservationDate { get; set; }
    public decimal BRTARegFee { get; set; }
    public decimal? BRTATIPS { get; set; }

    // Step 3 - File Processing (3)
    public DateTime? FileReceivedDate { get; set; }

    // Step 4 - Completed (4)
    public DateTime? CompletionDate { get; set; }
    public string? RegNo { get; set; }

    // Document Transfer
    public int? IsDocTransfer { get; set; }
    public int? NewCusID { get; set; }
    public string? NewCustomerName { get; set; }
    public string? NewCustomerCode { get; set; }
    public DateTime? TransferDate { get; set; }

    public int ConcernID { get; set; }
    public int WFStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? ModifiedBy { get; set; }
}

