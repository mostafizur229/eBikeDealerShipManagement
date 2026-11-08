using System.Data;
using System.Data.SqlTypes;
using Dapper;
using eBike.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Services;

public class CreditSalesOrderExecutionService : ICreditSalesOrderExecutionService
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<CreditSalesOrderExecutionService> _logger;

    public CreditSalesOrderExecutionService(
        ISqlConnectionFactory sqlConnectionFactory,
        ILogger<CreditSalesOrderExecutionService> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _logger = logger;
    }

    public async Task<bool> AddCreditSalesOrderAsync(CreditSalesOrderExecutionModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            var dtSalesOrder = BuildSalesOrderDataTable(model);
            var dtSODetails = BuildSODetailDataTable(model);
            var dtSchedules = BuildSchedulesDataTable(model.Schedules);
            var dtBankTrans = BuildBankTransDataTable(model.BankTrans);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.AddCreditSalesOrder";
            cmd.CommandType = CommandType.StoredProcedure;

            var p1 = cmd.Parameters.AddWithValue("@CSalesOrder", dtSalesOrder);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertCreditSalesOrderTable";

            var p2 = cmd.Parameters.AddWithValue("@CSODetails", dtSODetails);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertCSODetailTable";

            var p3 = cmd.Parameters.AddWithValue("@CSSchedules", dtSchedules);
            p3.SqlDbType = SqlDbType.Structured;
            p3.TypeName = "dbo.InsertCSSchedulesTable";

            var p4 = cmd.Parameters.AddWithValue("@BankTrans", dtBankTrans);
            p4.SqlDbType = SqlDbType.Structured;
            p4.TypeName = "dbo.InsertBankTransTable";

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var dbResult = returnParam.Value is int val ? val : 0;
            return dbResult == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute AddCreditSalesOrder for InvoiceNo {InvoiceNo}", model.InvoiceNo);
            throw;
        }
    }

    public async Task<bool> InstallmentPaymentAsync(CreditSalesInstallmentPaymentModel model, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            var dtSchedules = BuildSchedulesDataTable(model.Schedules);
            var dtBankTrans = BuildBankTransDataTable(model.BankTrans);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.InstallmentPayment";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@SalesOrderId", model.SalesOrderId);
            cmd.Parameters.AddWithValue("@InstallmentAmount", model.InstallmentAmount);

            var p1 = cmd.Parameters.AddWithValue("@Schedules", dtSchedules);
            p1.SqlDbType = SqlDbType.Structured;
            p1.TypeName = "dbo.InsertCSSchedulesTable";

            cmd.Parameters.AddWithValue("@LastPayAdjustment", model.LastPayAdjustment);

            var p2 = cmd.Parameters.AddWithValue("@BankTrans", dtBankTrans);
            p2.SqlDbType = SqlDbType.Structured;
            p2.TypeName = "dbo.InsertBankTransTable";

            cmd.Parameters.AddWithValue("@CardTypeSetupID", model.CardTypeSetupID);

            var returnParam = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
            returnParam.Direction = ParameterDirection.ReturnValue;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await cmd.ExecuteNonQueryAsync(cancellationToken);

            var dbResult = returnParam.Value is int val ? val : 0;
            return dbResult == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute InstallmentPayment for SalesOrderId {SalesOrderId}", model.SalesOrderId);
            throw;
        }
    }

    public async Task<bool> DeleteCreditSalesOrderAsync(int salesOrderId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)_sqlConnectionFactory.CreateConnection();
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "dbo.DeleteCreditSalesOrder";
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

            var dbResult = returnParam.Value is int val ? val : 0;
            return dbResult == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete credit sales order with ID {SalesOrderId}", salesOrderId);
            throw;
        }
    }

    public async Task<string> GetNextInvoiceNoAsync(int concernId, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT MAX(TRY_CAST(InvoiceNo AS INT))
            FROM dbo.CreditSales
            WHERE ConcernID = @ConcernId;";

        var maxNumber = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { ConcernId = concernId }, cancellationToken: cancellationToken));
        var nextNumber = (maxNumber ?? 0) + 1;
        return nextNumber.ToString("D5");
    }

    public async Task<bool> IsInvoiceNoExistsAsync(string invoiceNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.CreditSales
            WHERE InvoiceNo = @InvoiceNo 
              AND ConcernID = @ConcernId
              AND (@ExcludeId IS NULL OR CreditSalesID <> @ExcludeId);";

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { InvoiceNo = invoiceNo, ConcernId = concernId, ExcludeId = excludeSalesOrderId },
            cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<bool> IsMRNoExistsAsync(string mrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mrNo)) return false;

        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.CreditSales
            WHERE MRNo = @MRNo 
              AND ConcernID = @ConcernId
              AND (@ExcludeId IS NULL OR CreditSalesID <> @ExcludeId);";

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { MRNo = mrNo.Trim(), ConcernId = concernId, ExcludeId = excludeSalesOrderId },
            cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<bool> IsREGMRNoExistsAsync(string regMrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(regMrNo)) return false;

        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1)
            FROM dbo.CreditSales
            WHERE REGMRNo = @REGMRNo 
              AND ConcernID = @ConcernId
              AND (@ExcludeId IS NULL OR CreditSalesID <> @ExcludeId);";

        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { REGMRNo = regMrNo.Trim(), ConcernId = concernId, ExcludeId = excludeSalesOrderId },
            cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<bool> IsChassisSoldAsync(int stockDetailId, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT Status
            FROM dbo.StockDetails
            WHERE SDetailID = @StockDetailId;";

        var status = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { StockDetailId = stockDetailId }, cancellationToken: cancellationToken));
        return status.HasValue && status.Value == 2;
    }

    public async Task<(int CardTypeSetupId, decimal DepositChargePercent)> GetCardSetupAsync(int bankId, int cardTypeId, CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 CardTypeSetupID, Percentage
            FROM dbo.CardTypeSetups
            WHERE BankID = @BankId AND CardTypeID = @CardTypeId;";

        var result = await connection.QueryFirstOrDefaultAsync<CardTypeSetupRow>(new CommandDefinition(
            sql,
            new { BankId = bankId, CardTypeId = cardTypeId },
            cancellationToken: cancellationToken));

        return result != null ? (result.CardTypeSetupID, result.Percentage) : (0, 0m);
    }

    private class CardTypeSetupRow
    {
        public int CardTypeSetupID { get; set; }
        public decimal Percentage { get; set; }
    }

    #region Private TVP Builders

    private static DataTable BuildSalesOrderDataTable(CreditSalesOrderExecutionModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("InvoiceNo", typeof(string));
        dt.Columns.Add("CustomerID", typeof(int));
        dt.Columns.Add("TSalesAmt", typeof(decimal));
        dt.Columns.Add("NoOfInstallment", typeof(int));
        dt.Columns.Add("InstallmentPrinciple", typeof(decimal));
        dt.Columns.Add("IssueDate", typeof(DateTime));
        dt.Columns.Add("UserName", typeof(string));
        dt.Columns.Add("Remaining", typeof(decimal));
        dt.Columns.Add("InterestRate", typeof(decimal));
        dt.Columns.Add("InterestAmount", typeof(decimal));
        dt.Columns.Add("SalesDate", typeof(DateTime));
        dt.Columns.Add("DownPayment", typeof(decimal));
        dt.Columns.Add("WInterestAmt", typeof(decimal));
        dt.Columns.Add("FixedAmount", typeof(decimal));
        dt.Columns.Add("IsStatus", typeof(int));
        dt.Columns.Add("UnExInstallment", typeof(int));
        dt.Columns.Add("Quantity", typeof(int));
        dt.Columns.Add("Discount", typeof(decimal));
        dt.Columns.Add("NetAmount", typeof(decimal));
        dt.Columns.Add("IsUnExpected", typeof(bool));
        dt.Columns.Add("Remarks", typeof(string));
        dt.Columns.Add("Status", typeof(bool));
        dt.Columns.Add("VatPercentage", typeof(decimal));
        dt.Columns.Add("VatAmount", typeof(decimal));
        dt.Columns.Add("TotalDue", typeof(decimal));
        dt.Columns.Add("ConcernId", typeof(int));
        dt.Columns.Add("CreatedBy", typeof(int));
        dt.Columns.Add("CreateDate", typeof(DateTime));
        dt.Columns.Add("TotalOffer", typeof(decimal));
        dt.Columns.Add("InstallmentPeriod", typeof(string));
        dt.Columns.Add("CardPaidAmount", typeof(decimal));
        dt.Columns.Add("CardTypeSetupID", typeof(int));
        dt.Columns.Add("DepositChargePercent", typeof(decimal));
        dt.Columns.Add("MRNo", typeof(string));
        dt.Columns.Add("CBankName", typeof(string));
        dt.Columns.Add("CBBranch", typeof(string));
        dt.Columns.Add("CBChequeNo", typeof(string));
        dt.Columns.Add("LedgerNo", typeof(string));
        dt.Columns.Add("CBNID", typeof(string));
        dt.Columns.Add("REGMRNo", typeof(string));

        var row = dt.NewRow();
        row["InvoiceNo"] = model.InvoiceNo;
        row["CustomerID"] = model.CustomerId;
        row["TSalesAmt"] = model.TSalesAmt;
        row["NoOfInstallment"] = model.NoOfInstallment;
        row["InstallmentPrinciple"] = model.InstallmentPrinciple;
        row["IssueDate"] = model.IssueDate;
        row["UserName"] = model.UserName ?? string.Empty;
        row["Remaining"] = model.Remaining;
        row["InterestRate"] = model.InterestRate;
        row["InterestAmount"] = model.InterestAmount;
        row["SalesDate"] = model.SalesDate;
        row["DownPayment"] = model.DownPayment;
        row["WInterestAmt"] = model.WInterestAmt;
        row["FixedAmount"] = model.FixedAmount;
        row["IsStatus"] = model.IsStatus;
        row["UnExInstallment"] = model.UnExInstallment;
        row["Quantity"] = model.Quantity;
        row["Discount"] = model.Discount;
        row["NetAmount"] = model.NetAmount;
        row["IsUnExpected"] = model.IsUnExpected;
        row["Remarks"] = (object?)model.Remarks ?? DBNull.Value;
        row["Status"] = model.Status;
        row["VatPercentage"] = model.VatPercentage;
        row["VatAmount"] = model.VatAmount;
        row["TotalDue"] = model.TotalDue;
        row["ConcernId"] = model.ConcernId;
        row["CreatedBy"] = model.CreatedBy;
        row["CreateDate"] = model.CreateDate;
        row["TotalOffer"] = model.TotalOffer;
        row["InstallmentPeriod"] = model.InstallmentPeriod ?? $"{model.NoOfInstallment} Months";
        row["CardPaidAmount"] = model.CardPaidAmount;
        row["CardTypeSetupID"] = model.CardTypeSetupID;
        row["DepositChargePercent"] = model.DepositChargePercent;
        row["MRNo"] = (object?)model.MRNo ?? DBNull.Value;
        row["CBankName"] = (object?)model.CBankName ?? DBNull.Value;
        row["CBBranch"] = (object?)model.CBBranch ?? DBNull.Value;
        row["CBChequeNo"] = (object?)model.CBChequeNo ?? DBNull.Value;
        row["LedgerNo"] = (object?)model.LedgerNo ?? DBNull.Value;
        row["CBNID"] = (object?)model.CBNID ?? DBNull.Value;
        row["REGMRNo"] = (object?)model.REGMRNo ?? DBNull.Value;

        dt.Rows.Add(row);
        return dt;
    }

    private static DataTable BuildSODetailDataTable(CreditSalesOrderExecutionModel model)
    {
        var dt = new DataTable();
        dt.Columns.Add("ProductId", typeof(int));
        dt.Columns.Add("StockDetailId", typeof(int));
        dt.Columns.Add("ColorId", typeof(int));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("UnitPrice", typeof(decimal));
        dt.Columns.Add("UTAmount", typeof(decimal));
        dt.Columns.Add("MpRateTotal", typeof(decimal));
        dt.Columns.Add("MrpRate", typeof(decimal));
        dt.Columns.Add("PPOffer", typeof(decimal));
        dt.Columns.Add("IntPercentage", typeof(decimal));
        dt.Columns.Add("IntTotalAmt", typeof(decimal));
        dt.Columns.Add("Compressor", typeof(int));
        dt.Columns.Add("Motor", typeof(int));
        dt.Columns.Add("Panel", typeof(int));
        dt.Columns.Add("Spareparts", typeof(int));
        dt.Columns.Add("Service", typeof(int));
        dt.Columns.Add("CreditSalesDetailsId", typeof(int));
        dt.Columns.Add("Mushok", typeof(decimal));
        dt.Columns.Add("SRateWMushok", typeof(decimal));
        dt.Columns.Add("RegAmt", typeof(decimal));
        dt.Columns.Add("OfferId", typeof(decimal));
        dt.Columns.Add("OfferAmt", typeof(decimal));
        dt.Columns.Add("Field1", typeof(int));
        dt.Columns.Add("Field2", typeof(int));
        dt.Columns.Add("Field3", typeof(decimal));
        dt.Columns.Add("Field4", typeof(decimal));
        dt.Columns.Add("Field5", typeof(string));
        dt.Columns.Add("Field6", typeof(string));

        foreach (var item in model.Details)
        {
            var row = dt.NewRow();
            row["ProductId"] = item.ProductId;
            row["StockDetailId"] = item.StockDetailId;
            row["ColorId"] = item.ColorId;
            row["Quantity"] = item.Quantity;
            row["UnitPrice"] = item.UnitPrice;
            row["UTAmount"] = item.UTAmount;
            row["MpRateTotal"] = item.MpRateTotal;
            row["MrpRate"] = item.MrpRate;
            row["PPOffer"] = item.PPOffer;
            row["IntPercentage"] = item.IntPercentage;
            row["IntTotalAmt"] = item.IntTotalAmt;
            row["Compressor"] = item.Compressor; // GodownId
            row["Motor"] = item.Motor;
            row["Panel"] = item.Panel;
            row["Spareparts"] = item.Spareparts;
            row["Service"] = item.Service;
            row["CreditSalesDetailsId"] = item.CreditSalesDetailsId;
            row["Mushok"] = item.Mushok;
            row["SRateWMushok"] = item.SRateWMushok;
            row["RegAmt"] = item.RegAmt;
            row["OfferId"] = item.OfferId;
            row["OfferAmt"] = item.OfferAmt;
            row["Field1"] = (int)item.InsuranceAmt;
            row["Field2"] = 0;
            row["Field3"] = 0m;
            row["Field4"] = 0m;
            row["Field5"] = string.Empty;
            row["Field6"] = string.Empty;

            dt.Rows.Add(row);
        }

        return dt;
    }

    private static DataTable BuildSchedulesDataTable(IEnumerable<CreditSalesScheduleExecutionItem> schedules)
    {
        var dt = new DataTable();
        dt.Columns.Add("MonthDate", typeof(DateTime));
        dt.Columns.Add("Balance", typeof(decimal));
        dt.Columns.Add("InstallmentAmt", typeof(decimal));
        dt.Columns.Add("PaymentDate", typeof(DateTime));
        dt.Columns.Add("PaymentStatus", typeof(string));
        dt.Columns.Add("InterestAmount", typeof(decimal));
        dt.Columns.Add("ClosingBalance", typeof(decimal));
        dt.Columns.Add("ScheduleNo", typeof(int));
        dt.Columns.Add("Remarks", typeof(string));
        dt.Columns.Add("IsUnExpected", typeof(int));
        dt.Columns.Add("HireValue", typeof(decimal));
        dt.Columns.Add("NetValue", typeof(decimal));
        dt.Columns.Add("ExpectedInstallment", typeof(decimal));
        dt.Columns.Add("CardPaidAmount", typeof(decimal));
        dt.Columns.Add("CardTypeSetupID", typeof(int));
        dt.Columns.Add("DepositChargePercent", typeof(decimal));
        dt.Columns.Add("BankTransID", typeof(int));
        dt.Columns.Add("MRNO", typeof(string));
        dt.Columns.Add("Field1", typeof(string));
        dt.Columns.Add("Field2", typeof(string));
        dt.Columns.Add("Field3", typeof(string));

        var sno = 1;
        foreach (var item in schedules)
        {
            var row = dt.NewRow();
            row["MonthDate"] = item.MonthDate;
            row["Balance"] = item.Balance;
            row["InstallmentAmt"] = item.InstallmentAmt;
            row["PaymentDate"] = item.PaymentDate.HasValue && item.PaymentDate.Value > SqlDateTime.MinValue.Value
                ? item.PaymentDate.Value
                : SqlDateTime.MinValue.Value;
            row["PaymentStatus"] = item.PaymentStatus ?? "Due";
            row["InterestAmount"] = item.InterestAmount;
            row["ClosingBalance"] = item.ClosingBalance;
            row["ScheduleNo"] = item.ScheduleNo > 0 ? item.ScheduleNo : sno;
            row["Remarks"] = (object?)item.Remarks ?? string.Empty;
            row["IsUnExpected"] = item.IsUnExpected;
            row["HireValue"] = item.HireValue;
            row["NetValue"] = item.NetValue;
            row["ExpectedInstallment"] = item.ExpectedInstallment > 0 ? item.ExpectedInstallment : item.InstallmentAmt;
            row["CardPaidAmount"] = item.CardPaidAmount;
            row["CardTypeSetupID"] = item.CardTypeSetupID;
            row["DepositChargePercent"] = item.DepositChargePercent;
            row["BankTransID"] = item.BankTransID;
            row["MRNO"] = (object?)item.MRNO ?? string.Empty;
            row["Field1"] = string.Empty;
            row["Field2"] = string.Empty;
            row["Field3"] = string.Empty;

            dt.Rows.Add(row);
            sno++;
        }

        return dt;
    }

    private static DataTable BuildBankTransDataTable(BankTransExecutionItem? bankTrans)
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

        if (bankTrans != null && bankTrans.Amount > 0 && bankTrans.BankID > 0)
        {
            var row = dt.NewRow();
            row["TranDate"] = bankTrans.TranDate;
            row["TransactionNo"] = bankTrans.TransactionNo ?? string.Empty;
            row["TransactionType"] = bankTrans.TransactionType;
            row["Amount"] = bankTrans.Amount;
            row["BankID"] = bankTrans.BankID;
            row["CustomerID"] = bankTrans.CustomerID;
            row["SupplierID"] = bankTrans.SupplierID;
            row["AnotherBankID"] = bankTrans.AnotherBankID;
            row["ChecqueNo"] = (object?)bankTrans.ChecqueNo ?? string.Empty;
            row["Remarks"] = (object?)bankTrans.Remarks ?? "Card payment deposit transactions during credit sales.";
            row["ConcernID"] = bankTrans.ConcernID;
            dt.Rows.Add(row);
        }

        return dt;
    }

    #endregion
}
