namespace eBike.Domain.Entities;

public class Registration
{
    public int RegistrationID { get; set; }
    public string? RegNo { get; set; }
    public DateTime RegApplyDate { get; set; }
    public int? SorderDetailID { get; set; }
    public int? CreditSaleDetailsID { get; set; }
    public DateTime? SalesDate { get; set; }
    public int CustomerID { get; set; }
    public int ProductID { get; set; }
    public int? Iscompleted { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int? AssembleYear { get; set; }
    public decimal RegFee { get; set; }
    public DateTime? ObservationDate { get; set; }
    public DateTime? FileReceivedDate { get; set; }
    public int RegType { get; set; }
    public int? IsDocTransfer { get; set; }
    public int? NewCusID { get; set; }
    public DateTime? TransferDate { get; set; }
    public decimal InsuranceAmt { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public decimal? BRTATIPS { get; set; }
    public decimal BRTARegFee { get; set; }
    public string? REGMRNo { get; set; }
    public int WFStatus { get; set; }

    public Customer? Customer { get; set; }
    public Customer? NewCustomer { get; set; }
    public Product? Product { get; set; }
    public SOrderDetail? SOrderDetail { get; set; }
    public CreditSaleDetail? CreditSaleDetail { get; set; }
}
