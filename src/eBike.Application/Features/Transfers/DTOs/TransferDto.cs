namespace eBike.Application.Features.Transfers.DTOs;

public class TransferListDto
{
    public int TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public int FromConcernId { get; set; }
    public string FromConcernName { get; set; } = string.Empty;
    public int ToConcernId { get; set; }
    public string ToConcernName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public int Status { get; set; }
    public string StatusName => Status == 1 ? "Transferred" : "Pending";
    public int ItemCount { get; set; }
}

public class TransferDto
{
    public int TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public int FromConcernId { get; set; }
    public string FromConcernName { get; set; } = string.Empty;
    public int ToConcernId { get; set; }
    public string ToConcernName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public int Status { get; set; }
    public DateTime? CreateDate { get; set; }
    public List<TransferDetailDto> Details { get; set; } = new();
}

public class TransferDetailDto
{
    public int TDetailId { get; set; }
    public int TransferId { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int SDetailId { get; set; }
    public string? IMEI { get; set; }
    public string? EngineNo { get; set; }
    public int? FromGodownId { get; set; }
    public string? FromGodownName { get; set; }
    public int ToGodownId { get; set; }
    public string? ToGodownName { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public decimal PRate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UTAmount { get; set; }
}

public class TransferEligibleStockDto
{
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int StockDetailId { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public int GodownId { get; set; }
    public string? GodownName { get; set; }
    public decimal PRate { get; set; }
    public decimal SRate { get; set; }
    public decimal Quantity { get; set; }
}

public class InternalTransferHistoryDto
{
    public int TransferHID { get; set; }
    public DateTime TransferDate { get; set; }
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int FromGodownId { get; set; }
    public string? FromGodownName { get; set; }
    public int ToGodownId { get; set; }
    public string? ToGodownName { get; set; }
    public decimal Qty { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
}


public class BranchConcernDto
{
    public int ConcernId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
}