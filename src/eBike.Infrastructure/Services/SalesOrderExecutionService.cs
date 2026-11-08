using System.Data;
using Dapper;
using eBike.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Services;

public class SalesOrderExecutionService : ISalesOrderExecutionService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<SalesOrderExecutionService> _logger;

    public SalesOrderExecutionService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<SalesOrderExecutionService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<bool> AddSalesOrderAsync(SalesOrderExecutionModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            var dtSalesOrder = BuildSalesOrderDataTable(model);
            var dtSODetails = BuildSODetailDataTable(model);
            var dtBankTrans = BuildBankTransDataTable(model);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.AddSalesOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            var p1 = cmd.Parameters.AddWithValue("@SalesOrder", dtSalesOrder);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertSalesOrderTable";

            var p2 = cmd.Parameters.AddWithValue("@SODetails", dtSODetails);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertSODetailTable";

            if (model.RemindDate.HasValue && model.RemindDate.Value > model.InvoiceDate)
            {
                cmd.Parameters.AddWithValue("@RemindDate", model.RemindDate.Value.Date);
            }
            else
            {
                cmd.Parameters.AddWithValue("@RemindDate", DBNull.Value);
            }

            var p4 = cmd.Parameters.AddWithValue("@BankTrans", dtBankTrans);
            p4.SqlDbType = SqlDbType.Structured;
            p4.TypeName = "dbo.InsertBankTransTable";

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = (int)returnParam.Value;
            _logger.LogInformation("AddSalesOrder SP executed with return code {Result} for InvoiceNo: {InvoiceNo}", result, model.InvoiceNo);

            if (result == 1)
            {
                if (model.SOType != 2)
                {
                    var regItems = model.Details.Where(d => d.RegAmount > 0).ToList();
                    if (regItems.Any())
                    {
                        try
                        {
                            var createdSOrderId = await connection.ExecuteScalarAsync<int?>(
                                "SELECT TOP 1 SOrderID FROM SOrders WHERE InvoiceNo = @InvoiceNo AND ConcernID = @ConcernId ORDER BY SOrderID DESC",
                                new { InvoiceNo = model.InvoiceNo, ConcernId = model.ConcernId });

                            if (createdSOrderId.HasValue)
                            {
                                foreach (var item in regItems)
                                {
                                    var sorderDetailId = await connection.ExecuteScalarAsync<int?>(
                                        "SELECT TOP 1 SOrderDetailID FROM SOrderDetails WHERE SOrderID = @OrderId AND SDetailID = @StockDetailId",
                                        new { OrderId = createdSOrderId.Value, StockDetailId = item.StockDetailId });

                                    await connection.ExecuteAsync(@"
                                        INSERT INTO Registrations (
                                            RegApplyDate, SorderDetailID, SalesDate, CustomerID, ProductID,
                                            Iscompleted, RegFee, RegType, REGMRNo, InsuranceAmt, ConcernID,
                                            CreatedBy, CreateDate, WFStatus, BRTARegFee
                                        ) VALUES (
                                            @RegApplyDate, @SorderDetailID, @SalesDate, @CustomerID, @ProductID,
                                            1, @RegFee, @RegType, @REGMRNo, @InsuranceAmt, @ConcernID,
                                            @CreatedBy, @CreateDate, 2, 0.00
                                        )",
                                        new
                                        {
                                            RegApplyDate = model.InvoiceDate,
                                            SorderDetailID = sorderDetailId,
                                            SalesDate = model.InvoiceDate,
                                            CustomerID = model.CustomerId,
                                            ProductID = item.ProductId,
                                            RegFee = item.RegAmount,
                                            RegType = item.Motor,
                                            REGMRNo = model.REGMRNo ?? string.Empty,
                                            InsuranceAmt = item.InsuranceAmt,
                                            ConcernID = model.ConcernId,
                                            CreatedBy = model.CreatedBy,
                                            CreateDate = model.CreateDate
                                        });
                                }
                            }
                        }
                        catch (Exception regEx)
                        {
                            _logger.LogError(regEx, "Failed to save registration records for InvoiceNo {InvoiceNo}", model.InvoiceNo);
                        }
                    }
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing AddSalesOrder SP for InvoiceNo: {InvoiceNo}, CustomerId: {CustomerId}",
                model.InvoiceNo, model.CustomerId);
            throw;
        }
    }

    public async Task<bool> UpdateSalesOrderAsync(int salesOrderId, int userId, SalesOrderExecutionModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            var dtSalesOrder = BuildSalesOrderDataTable(model);
            var dtSODetails = BuildSODetailDataTable(model);
            var dtBankTrans = BuildBankTransDataTable(model);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.UpdateSalesOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SalesOrderId", salesOrderId);

            var p1 = cmd.Parameters.AddWithValue("@SalesOrder", dtSalesOrder);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertSalesOrderTable";

            var p2 = cmd.Parameters.AddWithValue("@SODetails", dtSODetails);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertSODetailTable";

            var p3 = cmd.Parameters.AddWithValue("@BankTrans", dtBankTrans);
            p3.SqlDbType = SqlDbType.Structured;
            p3.TypeName = "dbo.InsertBankTransTable";

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = (int)returnParam.Value;
            _logger.LogInformation("UpdateSalesOrder SP executed with return code {Result} for SalesOrderId: {SalesOrderId}", result, salesOrderId);

            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UpdateSalesOrder SP for SalesOrderId: {SalesOrderId}", salesOrderId);
            throw;
        }
    }

    public async Task<bool> DeleteSalesOrderAsync(int salesOrderId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.DeleteSalesOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@SalesOrderId", salesOrderId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var result = returnParam.Value is int val ? val : 0;
            _logger.LogInformation("DeleteSalesOrder SP executed with return code {Result} for SalesOrderId: {SalesOrderId}", result, salesOrderId);
            return result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DeleteSalesOrder SP for SalesOrderId: {SalesOrderId}", salesOrderId);
            throw;
        }
    }

    public async Task<string> GetNextInvoiceNoAsync(int concernId, int soType = 1, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        var maxNum = await connection.ExecuteScalarAsync<int?>(@"
            SELECT MAX(TRY_CAST(InvoiceNo AS INT))
            FROM SOrders
            WHERE ConcernID = @ConcernId AND SOType = @SOType",
            new { ConcernId = concernId, SOType = soType });

        var nextNum = (maxNum ?? 0) + 1;
        if (soType == 2)
        {
            return nextNum.ToString();
        }
        return nextNum.ToString("D5");
    }

    public async Task<bool> IsChassisSoldAsync(int stockDetailId, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        var status = await connection.ExecuteScalarAsync<int?>(
            "SELECT Status FROM StockDetails WHERE SDetailID = @StockDetailId",
            new { StockDetailId = stockDetailId });

        return status.HasValue && status.Value != 1; // 1 is Stock, 2 is Sold
    }

    public async Task<bool> IsInvoiceNoExistsAsync(string invoiceNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM SOrders WHERE InvoiceNo = @InvoiceNo AND ConcernID = @ConcernId";
        if (excludeSalesOrderId.HasValue)
        {
            sql += " AND SOrderID != @ExcludeSalesOrderId";
        }

        var count = await connection.ExecuteScalarAsync<int>(sql, new { InvoiceNo = invoiceNo, ConcernId = concernId, ExcludeSalesOrderId = excludeSalesOrderId });
        return count > 0;
    }

    public async Task<bool> IsMRNoExistsAsync(string mrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mrNo)) return false;

        using var connection = _sqlConnectionFactory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM SOrders WHERE MRNo = @MRNo AND ConcernID = @ConcernId";
        if (excludeSalesOrderId.HasValue)
        {
            sql += " AND SOrderID != @ExcludeSalesOrderId";
        }

        var count = await connection.ExecuteScalarAsync<int>(sql, new { MRNo = mrNo, ConcernId = concernId, ExcludeSalesOrderId = excludeSalesOrderId });
        return count > 0;
    }

    public async Task<bool> IsREGMRNoExistsAsync(string regMrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(regMrNo)) return false;

        using var connection = _sqlConnectionFactory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM SOrders WHERE REGMRNo = @REGMRNo AND ConcernID = @ConcernId";
        if (excludeSalesOrderId.HasValue)
        {
            sql += " AND SOrderID != @ExcludeSalesOrderId";
        }

        var count = await connection.ExecuteScalarAsync<int>(sql, new { REGMRNo = regMrNo, ConcernId = concernId, ExcludeSalesOrderId = excludeSalesOrderId });
        return count > 0;
    }

    private static DataTable BuildSalesOrderDataTable(SalesOrderExecutionModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("InvoiceDate", typeof(DateTime));
        dt.Columns.Add("InvoiceNo", typeof(string));
        dt.Columns.Add("VatPercentage", typeof(decimal));
        dt.Columns.Add("VatAmount", typeof(decimal));
        dt.Columns.Add("GrandTotal", typeof(decimal));
        dt.Columns.Add("TDiscountPercentage", typeof(decimal));
        dt.Columns.Add("TDiscountAmount", typeof(decimal));
        dt.Columns.Add("RecAmt", typeof(decimal));
        dt.Columns.Add("PaymentDue", typeof(decimal));
        dt.Columns.Add("TotalAmount", typeof(decimal));
        dt.Columns.Add("TotalDue", typeof(decimal));
        dt.Columns.Add("AdjAmount", typeof(decimal));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("CustomerId", typeof(int));
        dt.Columns.Add("ConcernId", typeof(int));
        dt.Columns.Add("CreatedBy", typeof(int));
        dt.Columns.Add("CreateDate", typeof(DateTime));
        dt.Columns.Add("TotalOffer", typeof(decimal));
        dt.Columns.Add("NetDiscount", typeof(decimal));
        dt.Columns.Add("Remarks", typeof(string));
        dt.Columns.Add("CardPaidAmount", typeof(decimal));
        dt.Columns.Add("CardTypeSetupID", typeof(int));
        dt.Columns.Add("DepositChargePercent", typeof(decimal));
        dt.Columns.Add("IsOffer", typeof(int));
        dt.Columns.Add("TotalMushok", typeof(decimal));
        dt.Columns.Add("TotalInstAmt", typeof(decimal));
        dt.Columns.Add("TOfferAmt", typeof(decimal));
        dt.Columns.Add("MRNo", typeof(string));
        dt.Columns.Add("LedgerNo", typeof(string));
        dt.Columns.Add("REGMRNo", typeof(string));
        dt.Columns.Add("SOType", typeof(int));
        dt.Columns.Add("PenaltyDate", typeof(DateTime));
        dt.Columns.Add("PenaltyAmount", typeof(decimal));
        dt.Columns.Add("PenaltyStatus", typeof(int));

        var row = dt.NewRow();
        row["InvoiceDate"] = model.InvoiceDate;
        row["InvoiceNo"] = model.InvoiceNo;
        row["VatPercentage"] = model.VatPercentage;
        row["VatAmount"] = model.VatAmount;
        row["GrandTotal"] = model.GrandTotal;
        row["TDiscountPercentage"] = model.TDiscountPercentage;
        row["TDiscountAmount"] = model.TDiscountAmount;
        row["RecAmt"] = model.RecAmt;
        row["PaymentDue"] = model.PaymentDue;
        row["TotalAmount"] = model.TotalAmount;
        row["TotalDue"] = model.TotalDue;
        row["AdjAmount"] = model.AdjAmount;
        row["Status"] = model.Status;
        row["CustomerId"] = model.CustomerId;
        row["ConcernId"] = model.ConcernId;
        row["CreatedBy"] = model.CreatedBy;
        row["CreateDate"] = model.CreateDate;
        row["TotalOffer"] = model.TotalOffer;
        row["NetDiscount"] = model.NetDiscount;
        row["Remarks"] = (object?)model.Remarks ?? DBNull.Value;
        row["CardPaidAmount"] = model.CardPaidAmount;
        row["CardTypeSetupID"] = model.CardTypeSetupID;
        row["DepositChargePercent"] = model.DepositChargePercent;
        row["IsOffer"] = model.IsOffer;
        row["TotalMushok"] = model.TotalMushok;
        row["TotalInstAmt"] = model.TotalInstAmt;
        row["TOfferAmt"] = model.TOfferAmt;
        row["MRNo"] = (object?)model.MRNo ?? DBNull.Value;
        row["LedgerNo"] = (object?)model.LedgerNo ?? DBNull.Value;
        row["REGMRNo"] = (object?)model.REGMRNo ?? DBNull.Value;
        row["SOType"] = model.SOType;
        row["PenaltyDate"] = model.PenaltyDate ?? model.InvoiceDate;
        row["PenaltyAmount"] = model.PenaltyAmount;
        row["PenaltyStatus"] = model.PenaltyStatus;

        dt.Rows.Add(row);
        return dt;
    }

    private static DataTable BuildSODetailDataTable(SalesOrderExecutionModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("SOrderDetailID", typeof(int));
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("StockDetailId", typeof(int));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("Status", typeof(int));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("UnitPrice", typeof(decimal));
        dt.Columns.Add("TAmount", typeof(decimal));
        dt.Columns.Add("PPDisPer", typeof(decimal));
        dt.Columns.Add("PPDisAmt", typeof(decimal));
        dt.Columns.Add("MrpRate", typeof(decimal));
        dt.Columns.Add("PPOffer", typeof(decimal));
        dt.Columns.Add("Compressor", typeof(int)); // GodownID
        dt.Columns.Add("Motor", typeof(int));      // RegType
        dt.Columns.Add("Panel", typeof(int));
        dt.Columns.Add("Spareparts", typeof(int));
        dt.Columns.Add("Service", typeof(int));
        dt.Columns.Add("RQuantity", typeof(decimal));
        dt.Columns.Add("ReplaceProductType", typeof(int));
        dt.Columns.Add("PPVat", typeof(decimal));
        dt.Columns.Add("OfferAmt", typeof(decimal));
        dt.Columns.Add("RegAmount", typeof(decimal));
        dt.Columns.Add("OfferID", typeof(int));
        dt.Columns.Add("Mushok", typeof(decimal));
        dt.Columns.Add("SRateWMushok", typeof(decimal));
        dt.Columns.Add("InterestRate", typeof(decimal));
        dt.Columns.Add("InterestAmt", typeof(decimal));
        dt.Columns.Add("InsuranceAmt", typeof(decimal));

        foreach (var item in model.Details)
        {
            var row = dt.NewRow();
            row["SOrderDetailID"] = item.SOrderDetailID;
            row["ProductId"] = item.ProductId;
            row["StockDetailId"] = item.StockDetailId;
            row["ColorId"] = item.ColorId ?? 0;
            row["Status"] = item.Status;
            row["Quantity"] = item.Quantity;
            row["UnitPrice"] = item.UnitPrice;
            row["TAmount"] = item.TAmount;
            row["PPDisPer"] = item.PPDisPer;
            row["PPDisAmt"] = item.PPDisAmt;
            row["MrpRate"] = item.MrpRate;
            row["PPOffer"] = item.PPOffer;
            row["Compressor"] = item.Compressor; // GodownID
            row["Motor"] = item.Motor;           // RegType
            row["Panel"] = item.Panel;
            row["Spareparts"] = item.Spareparts;
            row["Service"] = item.Service;
            row["RQuantity"] = item.RQuantity;
            row["ReplaceProductType"] = item.ReplaceProductType;
            row["PPVat"] = item.PPVat;
            row["OfferAmt"] = item.OfferAmt;
            row["RegAmount"] = item.RegAmount;
            row["OfferID"] = item.OfferID;
            row["Mushok"] = item.Mushok;
            row["SRateWMushok"] = item.SRateWMushok;
            row["InterestRate"] = item.InterestRate;
            row["InterestAmt"] = item.InterestAmt;
            row["InsuranceAmt"] = item.InsuranceAmt;

            dt.Rows.Add(row);
        }

        return dt;
    }

    private static DataTable BuildBankTransDataTable(SalesOrderExecutionModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("TranDate", typeof(DateTime));
        dt.Columns.Add("TransactionNo", typeof(string));
        dt.Columns.Add("TransactionType", typeof(int));
        dt.Columns.Add("Amount", typeof(decimal));
        dt.Columns.Add("BankID", typeof(int));
        dt.Columns.Add("CustomerID", typeof(int));
        dt.Columns.Add("SupplierID", typeof(int));
        dt.Columns.Add("AnotherBankID", typeof(int));
        dt.Columns.Add("ChecqueNo", typeof(string));
        dt.Columns.Add("Remarks", typeof(string));
        dt.Columns.Add("ConcernID", typeof(int));

        if (model.BankTrans != null && model.CardPaidAmount > 0)
        {
            var row = dt.NewRow();
            row["TranDate"] = model.BankTrans.TranDate;
            row["TransactionNo"] = model.BankTrans.TransactionNo;
            row["TransactionType"] = model.BankTrans.TransactionType;
            row["Amount"] = model.BankTrans.Amount;
            row["BankID"] = model.BankTrans.BankID;
            row["CustomerID"] = model.BankTrans.CustomerID;
            row["SupplierID"] = model.BankTrans.SupplierID;
            row["AnotherBankID"] = model.BankTrans.AnotherBankID;
            row["ChecqueNo"] = (object?)model.BankTrans.ChecqueNo ?? DBNull.Value;
            row["Remarks"] = (object?)model.BankTrans.Remarks ?? DBNull.Value;
            row["ConcernID"] = model.BankTrans.ConcernID;
            dt.Rows.Add(row);
        }

        return dt;
    }
}
