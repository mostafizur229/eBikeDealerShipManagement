using System.Data;
using Dapper;
using eBike.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Services;

public class PurchaseOrderExecutionService : IPurchaseOrderExecutionService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<PurchaseOrderExecutionService> _logger;

    public PurchaseOrderExecutionService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<PurchaseOrderExecutionService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<bool> AddPurchaseOrderAsync(PurchaseOrderExecutionModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            // 1. Fetch existing Stocks for matching product/color/godown
            var existingStocks = (await connection.QueryAsync<(int StockID, int ProductID, int? ColorID, int GodownID)>(
                "SELECT StockID, ProductID, ColorID, GodownID FROM Stocks WHERE ConcernID = @ConcernId",
                new { ConcernId = model.ConcernId })).ToList();

            // 2. Prepare DataTables for the 5 TVPs
            var dtPurchaseOrder = BuildPurchaseOrderDataTable(model);
            var dtPODetails = BuildPODetailDataTable(model);
            var dtPOProductDetails = BuildPOProductDetailDataTable(model);
            var dtStocks = BuildStockDataTable(model, existingStocks);
            var dtStockDetails = BuildStockDetailDataTable(model);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.AddPurchaseOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            var p1 = cmd.Parameters.AddWithValue("@PurchaseOrder", dtPurchaseOrder);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertPurchaseOrderTable";

            var p2 = cmd.Parameters.AddWithValue("@PODetails", dtPODetails);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertPODetailTable";

            var p3 = cmd.Parameters.AddWithValue("@POProductDetails", dtPOProductDetails);
            p3.SqlDbType = SqlDbType.Structured;
            p3.TypeName = "dbo.InsertPOProductDetailTable";

            var p4 = cmd.Parameters.AddWithValue("@Stocks", dtStocks);
            p4.SqlDbType = SqlDbType.Structured;
            p4.TypeName = "dbo.InsertStockTable";

            var p5 = cmd.Parameters.AddWithValue("@StockDetails", dtStockDetails);
            p5.SqlDbType = SqlDbType.Structured;
            p5.TypeName = "dbo.InsertStockDetailTable";

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = (int)returnParam.Value;
            _logger.LogInformation("AddPurchaseOrder SP executed with return code {Result} for ChallanNo: {ChallanNo}", result, model.ChallanNo);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing AddPurchaseOrder SP for ChallanNo: {ChallanNo}, SupplierId: {SupplierId}",
                model.ChallanNo, model.SupplierId);
            throw;
        }
    }

    public async Task<bool> UpdatePurchaseOrderAsync(int purchaseOrderId, PurchaseOrderExecutionModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            var existingStocks = (await connection.QueryAsync<(int StockID, int ProductID, int? ColorID, int GodownID)>(
                "SELECT StockID, ProductID, ColorID, GodownID FROM Stocks WHERE ConcernID = @ConcernId",
                new { ConcernId = model.ConcernId })).ToList();

            var dtPurchaseOrder = BuildPurchaseOrderDataTable(model);
            var dtPODetails = BuildPODetailDataTable(model);
            var dtPOProductDetails = BuildPOProductDetailDataTable(model);
            var dtStocks = BuildStockDataTableForUpdate(model, existingStocks);
            var dtStockDetails = BuildStockDetailDataTable(model);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.UpdatePurchaseOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);

            var p1 = cmd.Parameters.AddWithValue("@PurchaseOrder", dtPurchaseOrder);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertPurchaseOrderTable";

            var p2 = cmd.Parameters.AddWithValue("@PODetails", dtPODetails);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertPODetailTable";

            var p3 = cmd.Parameters.AddWithValue("@POProductDetails", dtPOProductDetails);
            p3.SqlDbType = SqlDbType.Structured;
            p3.TypeName = "dbo.InsertPOProductDetailTable";

            var p4 = cmd.Parameters.AddWithValue("@Stocks", dtStocks);
            p4.SqlDbType = SqlDbType.Structured;
            p4.TypeName = "dbo.InsertStockTable";

            var p5 = cmd.Parameters.AddWithValue("@StockDetails", dtStockDetails);
            p5.SqlDbType = SqlDbType.Structured;
            p5.TypeName = "dbo.InsertStockDetailTable";

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = (int)returnParam.Value;
            _logger.LogInformation("UpdatePurchaseOrder SP executed with return code {Result} for POrderID: {POrderID}", result, purchaseOrderId);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UpdatePurchaseOrder SP for POrderID: {POrderID}", purchaseOrderId);
            throw;
        }
    }

    public async Task<bool> ReturnPurchaseOrderAsync(int purchaseOrderId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.ReturnPurchaseOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@PurchaseOrderId", purchaseOrderId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = (int)returnParam.Value;
            _logger.LogInformation("ReturnPurchaseOrder SP executed with return code {Result} for POrderID: {POrderID}", result, purchaseOrderId);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ReturnPurchaseOrder SP for POrderID: {POrderID}, UserId: {UserId}",
                purchaseOrderId, userId);
            throw;
        }
    }

    public async Task<int> CheckProductStatusByPOIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        try
        {
            var soldCount = await connection.ExecuteScalarAsync<int>(
                "SELECT [dbo].[CheckProductStatusByPOId](@PurchaseOrderId)",
                new { PurchaseOrderId = purchaseOrderId });
            return soldCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CheckProductStatusByPOId for POrderID: {POrderID}", purchaseOrderId);
            throw;
        }
    }

    public async Task<int> CheckIMENoDuplicacyByConcernIdAsync(int concernId, string imeNo, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        try
        {
            var existCount = await connection.ExecuteScalarAsync<int>(
                "SELECT [dbo].[CheckIMENoDuplicacyByConcernId](@ConcernId, @IMENo)",
                new { ConcernId = concernId, IMENo = imeNo });
            return existCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CheckIMENoDuplicacyByConcernId for ConcernId: {ConcernId}, IMENo: {IMENo}", concernId, imeNo);
            throw;
        }
    }

    public async Task<string> GetNextChallanNoAsync(int concernId, int poType = 1, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        try
        {
            var maxChallan = poType == 2 ? null : await connection.ExecuteScalarAsync<int?>(
                @"SELECT MAX(TRY_CAST(ChallanNo AS INT)) 
                  FROM POrders 
                  WHERE ConcernID = @ConcernId AND POType = @POType",
                new { ConcernId = concernId, POType = poType });

            if (!maxChallan.HasValue || maxChallan.Value <= 0)
            {
                var globalMax = await connection.ExecuteScalarAsync<int?>(
                    "SELECT MAX(TRY_CAST(ChallanNo AS INT)) FROM POrders WHERE POType = @POType",
                    new { POType = poType });
                maxChallan = globalMax;
            }

            var nextNo = (maxChallan ?? 0) + 1;
            return poType == 2 ? $"RC-{nextNo:D4}" : nextNo.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next ChallanNo for ConcernId: {ConcernId}, POType: {POType}", concernId, poType);
            return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }
    }

    #region DataTable Builders for TVPs

    private static DataTable BuildPurchaseOrderDataTable(PurchaseOrderExecutionModel m)
    {
        var dt = new DataTable();
        dt.Columns.Add("OrderDate", typeof(DateTime));
        dt.Columns.Add("ChallanNo", typeof(string));
        dt.Columns.Add("SupplierId", typeof(int));
        dt.Columns.Add("GrandTotal", typeof(decimal));
        dt.Columns.Add("TDiscount", typeof(decimal));
        dt.Columns.Add("TotalAmt", typeof(decimal));
        dt.Columns.Add("RecAmt", typeof(decimal));
        dt.Columns.Add("PaymentDue", typeof(decimal));
        dt.Columns.Add("TotalDue", typeof(decimal));
        dt.Columns.Add("AdjAmount", typeof(decimal));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("PPTDisAmt", typeof(decimal));
        dt.Columns.Add("NetDiscount", typeof(decimal));
        dt.Columns.Add("LabourCost", typeof(decimal));
        dt.Columns.Add("ConcernId", typeof(int));
        dt.Columns.Add("CreateDate", typeof(DateTime));
        dt.Columns.Add("CreatedBy", typeof(int));
        dt.Columns.Add("IsDamageOrder", typeof(int));
        dt.Columns.Add("Remarks", typeof(string));
        dt.Columns.Add("TotalMushok", typeof(decimal));
        dt.Columns.Add("TotalTax", typeof(decimal));
        dt.Columns.Add("POType", typeof(int));

        var row = dt.NewRow();
        row["OrderDate"] = m.OrderDate;
        row["ChallanNo"] = m.ChallanNo;
        row["SupplierId"] = m.SupplierId;
        row["GrandTotal"] = m.GrandTotal;
        row["TDiscount"] = m.TDiscount;
        row["TotalAmt"] = m.TotalAmt;
        row["RecAmt"] = m.RecAmt;
        row["PaymentDue"] = m.PaymentDue;
        row["TotalDue"] = m.TotalDue;
        row["AdjAmount"] = m.AdjAmount;
        row["Status"] = m.Status;
        row["PPTDisAmt"] = m.PPTDisAmt;
        row["NetDiscount"] = m.NetDiscount;
        row["LabourCost"] = m.LabourCost;
        row["ConcernId"] = m.ConcernId;
        row["CreateDate"] = m.CreateDate;
        row["CreatedBy"] = m.CreatedBy;
        row["IsDamageOrder"] = m.IsDamageOrder;
        row["Remarks"] = (object?)m.Remarks ?? DBNull.Value;
        row["TotalMushok"] = m.TotalMushok;
        row["TotalTax"] = m.TotalTax;
        row["POType"] = m.POType;

        dt.Rows.Add(row);
        return dt;
    }

    private static DataTable BuildPODetailDataTable(PurchaseOrderExecutionModel m)
    {
        var dt = new DataTable();
        dt.Columns.Add("POrderDetailId", typeof(int));
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("UnitPrice", typeof(decimal));
        dt.Columns.Add("TAmount", typeof(decimal));
        dt.Columns.Add("PPDisPer", typeof(decimal));
        dt.Columns.Add("PPDisAmt", typeof(decimal));
        dt.Columns.Add("MrpRate", typeof(decimal));
        dt.Columns.Add("SalesRate", typeof(decimal));
        dt.Columns.Add("ExtraPPDISPer", typeof(decimal));
        dt.Columns.Add("ExtraPPDISAmt", typeof(decimal));
        dt.Columns.Add("PPOffer", typeof(decimal));
        dt.Columns.Add("CreditSalesRate", typeof(decimal));
        dt.Columns.Add("CRSalesRate12Month", typeof(decimal));
        dt.Columns.Add("CRSalesRate3Month", typeof(decimal));
        dt.Columns.Add("GodownId", typeof(int));
        dt.Columns.Add("Mushok", typeof(decimal));
        dt.Columns.Add("MRPWMushok", typeof(decimal));
        dt.Columns.Add("DPRate", typeof(decimal));
        dt.Columns.Add("RecQuantity", typeof(decimal));
        dt.Columns.Add("DOrderDetailId", typeof(int));
        dt.Columns.Add("PWTaxAmt", typeof(decimal));

        foreach (var item in m.Details)
        {
            var row = dt.NewRow();
            if (item.POrderDetailId.HasValue && item.POrderDetailId.Value > 0)
                row["POrderDetailId"] = item.POrderDetailId.Value;
            else
                row["POrderDetailId"] = DBNull.Value;

            row["ProductId"] = item.ProductId;
            row["ColorId"] = (object?)item.ColorId ?? DBNull.Value;
            row["Status"] = item.Status;
            row["Quantity"] = item.Quantity;
            row["UnitPrice"] = item.UnitPrice;
            row["TAmount"] = item.TAmount;
            row["PPDisPer"] = item.PPDisPer;
            row["PPDisAmt"] = item.PPDisAmt;
            row["MrpRate"] = item.MrpRate;
            row["SalesRate"] = item.SalesRate;
            row["ExtraPPDISPer"] = item.ExtraPPDISPer;
            row["ExtraPPDISAmt"] = item.ExtraPPDISAmt;
            row["PPOffer"] = item.PPOffer;
            row["CreditSalesRate"] = item.CreditSalesRate;
            row["CRSalesRate12Month"] = item.CRSalesRate12Month;
            row["CRSalesRate3Month"] = item.CRSalesRate3Month;
            row["GodownId"] = item.GodownId;
            row["Mushok"] = item.Mushok;
            row["MRPWMushok"] = item.MRPWMushok;
            row["DPRate"] = item.DPRate > 0 ? item.DPRate : item.UnitPrice;
            row["RecQuantity"] = item.RecQuantity;
            row["DOrderDetailId"] = item.DOrderDetailId;
            row["PWTaxAmt"] = item.PWTaxAmt;

            dt.Rows.Add(row);
        }

        return dt;
    }

    private static DataTable BuildPOProductDetailDataTable(PurchaseOrderExecutionModel m)
    {
        var dt = new DataTable();
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("IMENO", typeof(string));
        dt.Columns.Add("DamagePOPDID", typeof(int));
        dt.Columns.Add("EngineNo", typeof(string));
        dt.Columns.Add("KeyNo", typeof(string));

        foreach (var detail in m.Details)
        {
            if (detail.Serials == null || detail.Serials.Count == 0)
            {
                var row = dt.NewRow();
                row["ProductId"] = detail.ProductId;
                row["ColorId"] = (object?)detail.ColorId ?? DBNull.Value;
                row["IMENO"] = "No Barcode";
                row["DamagePOPDID"] = 0;
                row["EngineNo"] = "0";
                row["KeyNo"] = "0";
                dt.Rows.Add(row);
            }
            else
            {
                foreach (var serial in detail.Serials)
                {
                    var row = dt.NewRow();
                    row["ProductId"] = serial.ProductId;
                    row["ColorId"] = (object?)serial.ColorId ?? (object?)detail.ColorId ?? DBNull.Value;
                    row["IMENO"] = string.IsNullOrWhiteSpace(serial.IMENO) ? "No Barcode" : serial.IMENO.Trim();
                    row["DamagePOPDID"] = serial.DamagePOPDID ?? 0;
                    row["EngineNo"] = string.IsNullOrWhiteSpace(serial.EngineNo) ? "0" : serial.EngineNo.Trim();
                    row["KeyNo"] = string.IsNullOrWhiteSpace(serial.KeyNo) ? "0" : serial.KeyNo.Trim();
                    dt.Rows.Add(row);
                }
            }
        }

        return dt;
    }

    private static DataTable BuildStockDataTable(
        PurchaseOrderExecutionModel m,
        List<(int StockID, int ProductID, int? ColorID, int GodownID)> existingStocks)
    {
        var dt = new DataTable();
        dt.Columns.Add("StockId", typeof(int));
        dt.Columns.Add("StockCode", typeof(string));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("EntryDate", typeof(DateTime));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("MRPPrice", typeof(decimal));
        dt.Columns.Add("LPPrice", typeof(decimal));
        dt.Columns.Add("ConcernId", typeof(int));
        dt.Columns.Add("CreateDate", typeof(DateTime));
        dt.Columns.Add("CreatedBy", typeof(int));
        dt.Columns.Add("GodownId", typeof(int));

        // Group by (ProductId, ColorId, GodownId) to prevent multiple stock records per grouping
        var groupedDetails = m.Details
            .GroupBy(d => new { d.ProductId, d.ColorId, d.GodownId, d.ProductCode });

        foreach (var grp in groupedDetails)
        {
            var matchedStock = existingStocks.FirstOrDefault(s =>
                s.ProductID == grp.Key.ProductId &&
                s.ColorID == grp.Key.ColorId &&
                s.GodownID == grp.Key.GodownId);

            var firstItem = grp.First();
            var totalQty = grp.Sum(x => x.Quantity);

            var row = dt.NewRow();
            if (matchedStock.StockID > 0)
                row["StockId"] = matchedStock.StockID;
            else
                row["StockId"] = DBNull.Value;

            row["StockCode"] = grp.Key.ProductCode;
            row["ColorId"] = (object?)grp.Key.ColorId ?? DBNull.Value;
            row["Status"] = firstItem.Status;
            row["EntryDate"] = m.CreateDate;
            row["Quantity"] = totalQty;
            row["ProductId"] = grp.Key.ProductId;
            row["MRPPrice"] = firstItem.MrpRate;
            row["LPPrice"] = firstItem.UnitPrice;
            row["ConcernId"] = m.ConcernId;
            row["CreateDate"] = m.CreateDate;
            row["CreatedBy"] = m.CreatedBy;
            row["GodownId"] = grp.Key.GodownId;

            dt.Rows.Add(row);
        }

        return dt;
    }

    private static DataTable BuildStockDataTableForUpdate(
        PurchaseOrderExecutionModel m,
        List<(int StockID, int ProductID, int? ColorID, int GodownID)> existingStocks)
    {
        var dt = new DataTable();
        dt.Columns.Add("StockId", typeof(int));
        dt.Columns.Add("StockCode", typeof(string));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("EntryDate", typeof(DateTime));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("MRPPrice", typeof(decimal));
        dt.Columns.Add("LPPrice", typeof(decimal));
        dt.Columns.Add("ConcernId", typeof(int));
        dt.Columns.Add("CreateDate", typeof(DateTime));
        dt.Columns.Add("CreatedBy", typeof(int));
        dt.Columns.Add("GodownId", typeof(int));

        foreach (var item in m.Details)
        {
            var matchedStock = existingStocks.FirstOrDefault(s =>
                s.ProductID == item.ProductId &&
                s.ColorID == item.ColorId &&
                s.GodownID == item.GodownId);

            var row = dt.NewRow();
            if (matchedStock.StockID > 0)
                row["StockId"] = matchedStock.StockID;
            else
                row["StockId"] = DBNull.Value;

            row["StockCode"] = item.ProductCode;
            row["ColorId"] = (object?)item.ColorId ?? DBNull.Value;
            row["Status"] = item.Status;
            row["EntryDate"] = m.CreateDate;
            row["Quantity"] = item.Status == 4 ? item.StockQtyDiff : item.Quantity;
            row["ProductId"] = item.ProductId;
            row["MRPPrice"] = item.MrpRate;
            row["LPPrice"] = item.UnitPrice;
            row["ConcernId"] = m.ConcernId;
            row["CreateDate"] = m.CreateDate;
            row["CreatedBy"] = m.CreatedBy;
            row["GodownId"] = item.GodownId;

            dt.Rows.Add(row);
        }

        return dt;
    }

    private static DataTable BuildStockDetailDataTable(PurchaseOrderExecutionModel m)
    {
        var dt = new DataTable();
        dt.Columns.Add("StockCode", typeof(string));
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("IMENO", typeof(string));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("SalesRate", typeof(decimal));
        dt.Columns.Add("CreditSRate", typeof(decimal));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("GodownId", typeof(int));
        dt.Columns.Add("EngineNo", typeof(string));
        dt.Columns.Add("KeyNo", typeof(string));

        foreach (var detail in m.Details)
        {
            if (detail.Serials == null || detail.Serials.Count == 0)
            {
                var row = dt.NewRow();
                row["StockCode"] = detail.ProductCode;
                row["ProductId"] = detail.ProductId;
                row["ColorId"] = (object?)detail.ColorId ?? DBNull.Value;
                row["IMENO"] = "No Barcode";
                row["Status"] = 1; // EnumStockStatus.Stock = 1
                row["SalesRate"] = detail.SalesRate;
                row["CreditSRate"] = detail.CreditSalesRate;
                row["Quantity"] = detail.Quantity;
                row["GodownId"] = detail.GodownId;
                row["EngineNo"] = "0";
                row["KeyNo"] = "0";
                dt.Rows.Add(row);
            }
            else
            {
                foreach (var serial in detail.Serials)
                {
                    var row = dt.NewRow();
                    row["StockCode"] = detail.ProductCode;
                    row["ProductId"] = detail.ProductId;
                    row["ColorId"] = (object?)serial.ColorId ?? (object?)detail.ColorId ?? DBNull.Value;
                    row["IMENO"] = string.IsNullOrWhiteSpace(serial.IMENO) ? "No Barcode" : serial.IMENO.Trim();
                    row["Status"] = 1; // EnumStockStatus.Stock = 1
                    row["SalesRate"] = detail.SalesRate;
                    row["CreditSRate"] = detail.CreditSalesRate;
                    row["Quantity"] = 1; // Each serialized bike is 1 unit
                    row["GodownId"] = detail.GodownId;
                    row["EngineNo"] = string.IsNullOrWhiteSpace(serial.EngineNo) ? "0" : serial.EngineNo.Trim();
                    row["KeyNo"] = string.IsNullOrWhiteSpace(serial.KeyNo) ? "0" : serial.KeyNo.Trim();
                    dt.Rows.Add(row);
                }
            }
        }

        return dt;
    }

    #endregion
}
