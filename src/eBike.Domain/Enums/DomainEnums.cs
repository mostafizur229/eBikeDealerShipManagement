namespace eBike.Domain.Enums;

public enum EBikeStatus
{
    Available = 1,
    InService = 2,
    UnderMaintenance = 3,
    Rented = 4,
    Sold = 5,
    Scrapped = 6
}

public enum ServiceStatus
{
    Pending = 1,
    Diagnosing = 2,
    InProgress = 3,
    WaitingForParts = 4,
    Completed = 5,
    Delivered = 6,
    Cancelled = 7
}

public enum ServicePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum StockTransactionType
{
    PurchaseIn = 1,
    SaleOut = 2,
    ServiceUsage = 3,
    TransferIn = 4,
    TransferOut = 5,
    Adjustment = 6,
    Scrap = 7
}

public enum PermissionAction
{
    View = 1,
    Create = 2,
    Update = 3,
    Delete = 4,
    Approve = 5,
    Export = 6
}

public enum EnumUnitType
{
    Piece = 1,
    Dozen = 2
}

public enum EnumProductType
{
    ExistingBC = 1,
    NoBarcode = 2,
    AutoBC = 3
}

public enum EnumCustomerType
{
    Retail = 1,
    Dealer = 2,
    Credit = 3,
    Branch = 4
}

public enum EnumGenderType
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum EnumTransactionType
{
    Deposit = 1,
    Withdraw = 2,
    CashCollection = 3,
    CashDelivery = 4,
    FundTransfer = 5,
    BankExpense = 6,
    BankIncome = 7,
    LiaPay = 8,
    LiaRec = 9
}

public enum EnumWFStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum EnumCompanyTransaction
{
    Expense = 1,
    Income = 2
}

public enum EnumTranType
{
    FromCustomer = 1,
    ToCompany = 2,
    CollectionPending = 3,
    DeliveryPending = 4,
    CollectionReturn = 5,
    AdvanceCollection = 6,
    CollectionReturnPending = 7,
    AdvanceCollectionPending = 8
}

public enum EnumPayType
{
    Cash = 1,
    Cheque = 2,
    bKash = 3,
    MBanking = 4,
    TT = 5,
    Online = 6
}

public enum EnumPurchaseType
{
    Purchase = 1,
    Return = 2,
    DeliveryOrder = 3,
    DamageReturn = 4,
    ProductReturn = 5
}

public enum EnumStockStatus
{
    Stock = 1,
    Sold = 2,
    Return = 3,
    Damage = 4
}

public enum EnumPOType
{
    Vehicle = 1,
    AutoParts = 2
}

public enum EnumSalesType
{
    Sales = 1,
    Return = 2,
    ProductReturn = 4,
    Pending = 5
}

public enum EnumRegType
{
    None = 0,
    Half = 1,
    Full = 2
}

public enum EnumRegStatus
{
    Initiate = 1,
    Observation = 2,
    FileProcessing = 3,
    Completed = 4,
    Deliver = 5
}

public enum EnumSOType
{
    Vehicle = 1,
    AutoParts = 2
}

public enum EnumProjectType
{
    Motorcycle = 1,
    AutoParts = 2
}

public enum EnumRegistrationDocType
{
    AssessmentForm = 1,
    ApplicationForm = 2,
    OwnerParticulars = 3,
    AgreementLetter = 4,
    ReferenceInfo = 5,
    SalesCertificate = 6,
    SalesInvoice = 7,
    Mushok63 = 8,
    GatePass = 9
}

