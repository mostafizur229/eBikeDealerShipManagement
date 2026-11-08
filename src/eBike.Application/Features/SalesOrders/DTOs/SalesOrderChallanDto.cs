namespace eBike.Application.Features.SalesOrders.DTOs;

public class SalesOrderChallanDto
{
    public InvoiceCompanyDto Company { get; set; } = new();
    public InvoiceCustomerDto Customer { get; set; } = new();
    public ChallanMetadataDto Challan { get; set; } = new();
    public InvoiceSalesPersonDto SalesPerson { get; set; } = new();
    public List<SalesOrderChallanItemDto> Items { get; set; } = new();
    public decimal TotalQuantity { get; set; }
}

public class ChallanMetadataDto
{
    public int SOrderID { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public DateTime ChallanDate { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int SOType { get; set; }
    public string SOTypeName { get; set; } = "Vehicle";
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;
}

public class SalesOrderChallanItemDto
{
    public int Sl { get; set; }
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

    public string? PartNo { get; set; }
    public string? LocationName { get; set; }

    public decimal Quantity { get; set; }
    public string UnitType { get; set; } = "Pcs";
    public string? Remarks { get; set; }
}
