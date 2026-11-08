namespace eBike.Application.Features.Registrations.DTOs;

public class RegistrationListDto
{
    public int RegistrationID { get; set; }
    public DateTime RegApplyDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerPhone { get; set; }

    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public string? ChassisNo { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public string? ColorName { get; set; }

    public string? SaleType { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? SalesDate { get; set; }

    public int RegType { get; set; }
    public string? RegTypeName { get; set; }
    public decimal RegFee { get; set; }
    public decimal InsuranceAmt { get; set; }
    public string? REGMRNo { get; set; }

    public int? Iscompleted { get; set; }
    public string? StatusName { get; set; }

    public DateTime? ObservationDate { get; set; }
    public decimal BRTARegFee { get; set; }
    public decimal? BRTATIPS { get; set; }
    public DateTime? FileReceivedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? RegNo { get; set; }

    public int? IsDocTransfer { get; set; }
    public int? NewCusID { get; set; }
    public string? NewCustomerName { get; set; }
    public DateTime? TransferDate { get; set; }
    public int ConcernID { get; set; }
}

