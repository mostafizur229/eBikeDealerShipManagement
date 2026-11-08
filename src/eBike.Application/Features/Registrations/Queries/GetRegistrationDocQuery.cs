using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Domain.Entities;
using MediatR;

namespace eBike.Application.Features.Registrations.Queries;

public record GetRegistrationDocQuery(int RegistrationId, int DocumentType) : IRequest<RegistrationDocDto>;

public class GetRegistrationDocQueryHandler : IRequestHandler<GetRegistrationDocQuery, RegistrationDocDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetRegistrationDocQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<RegistrationDocDto> Handle(GetRegistrationDocQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var regSql = @"
            SELECT 
                r.RegistrationID,
                r.RegNo,
                r.RegApplyDate,
                r.SorderDetailID,
                r.CreditSaleDetailsID,
                r.SalesDate,
                r.CustomerID,
                r.ProductID,
                r.IsDocTransfer,
                r.NewCusID,
                r.TransferDate,
                r.RegType,
                r.RegFee,
                r.InsuranceAmt,
                r.REGMRNo,
                r.BRTATIPS,
                r.BRTARegFee,
                r.ObservationDate,
                r.FileReceivedDate,
                r.CompletionDate,
                r.Iscompleted,
                r.ConcernID
            FROM Registrations r
            WHERE r.RegistrationID = @Id";

        var reg = await connection.QueryFirstOrDefaultAsync(regSql, new { Id = request.RegistrationId });
        if (reg == null)
        {
            throw new NotFoundException(nameof(Registration), request.RegistrationId);
        }

        int concernId = reg.ConcernID;
        int originalCustId = reg.CustomerID;
        bool isTransferred = (reg.IsDocTransfer == 1 && reg.NewCusID != null && (int)reg.NewCusID > 0);
        int effectiveCustId = isTransferred ? (int)reg.NewCusID : originalCustId;

        var custSql = @"
            SELECT 
                c.CustomerID, c.Code, c.Name, c.FName, c.MName, c.HusbandOrWifeName,
                c.ContactNo, c.NID, c.Village, c.Post, c.SexType, c.DOB,
                c.PermanentAddress, c.RefName,
                COALESCE(th.Name, c.Thana) AS ThanaName,
                COALESCE(dt.Name, c.Dist) AS DistrictName
            FROM Customers c
            LEFT JOIN Thanas th ON c.ThanaID = th.ThanaID
            LEFT JOIN Districts dt ON c.DistrictID = dt.DistrictID
            WHERE c.CustomerID = @CustId";

        var cust = await connection.QueryFirstOrDefaultAsync(custSql, new { CustId = effectiveCustId });
        if (cust == null)
        {
            throw new NotFoundException(nameof(Customer), effectiveCustId);
        }

        string originalCustName = string.Empty;
        if (isTransferred)
        {
            originalCustName = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Name FROM Customers WHERE CustomerID = @Id", new { Id = originalCustId }) ?? string.Empty;
        }

        var vehSql = @"
            SELECT TOP 1
                p.ProductID, p.ProductName, p.Code AS ProductCode, p.CC, p.Weight, p.SeatingCapacity,
                p.TiyerSize, p.RingSize, p.NoofCylinder, p.VehicleSize, p.HorsePower, p.RPM,
                p.MakersName, p.MFGYear, p.UnLadenWeight, p.MadeIn, p.MRP,
                p.COMPANYNAME AS CompanyName, p.CATEGORYNAME AS CategoryName,
                COALESCE(sd1.IMENO, sd2.IMENO, '') AS ChassisNo,
                COALESCE(sd1.EngineNo, sd2.EngineNo, '') AS EngineNo,
                COALESCE(sd1.KeyNo, sd2.KeyNo, '') AS KeyNo,
                COALESCE(clr1.Name, clr2.Name, '') AS ColorName,
                COALESCE(so.SOrderID, cs.CreditSalesID) AS OrderId,
                COALESCE(so.InvoiceNo, cs.InvoiceNo, '') AS InvoiceNo,
                COALESCE(so.InvoiceDate, cs.SalesDate) AS SalesDate,
                COALESCE(so.VATChallanPotroNo, cs.VATChallanPotroNo, '') AS ChallanNo,
                COALESCE(so.RecAmount, cs.DownPayment, 0) AS ReceivedAmount,
                COALESCE(sod.Quantity, csd.Quantity, 1) AS Quantity,
                COALESCE(sod.MPRate, csd.UnitPrice, p.MRP, 0) AS UnitPrice,
                COALESCE(sod.UTAmount / NULLIF(sod.Quantity, 0), csd.UTAmount / NULLIF(csd.Quantity, 0), p.MRP, 0) AS SalesRate,
                COALESCE(sod.UTAmount, csd.UTAmount, p.MRP, 0) AS TotalAmount,
                COALESCE(sod.PPVat, 0) AS VatAmount,
                COALESCE(sod.Mushok, csd.Mushok, 0) AS Mushok,
                COALESCE(sod.SRateWMushok, csd.SRateWMushok, 0) AS SRateWMushok,
                CASE 
                    WHEN reg.SorderDetailID IS NOT NULL AND reg.SorderDetailID > 0 THEN 'Cash'
                    WHEN reg.CreditSaleDetailsID IS NOT NULL AND reg.CreditSaleDetailsID > 0 THEN 'Credit'
                    ELSE 'Cash'
                END AS SaleType
            FROM Registrations reg
            INNER JOIN Products p ON reg.ProductID = p.ProductID
            LEFT JOIN SOrderDetails sod ON reg.SorderDetailID = sod.SOrderDetailID
            LEFT JOIN SOrders so ON sod.SOrderID = so.SOrderID
            LEFT JOIN StockDetails sd1 ON COALESCE(sod.RStockDetailId, sod.SDetailID) = sd1.SDetailID
            LEFT JOIN Colors clr1 ON sd1.ColorID = clr1.ColorID
            LEFT JOIN CreditSaleDetails csd ON reg.CreditSaleDetailsID = csd.CreditSaleDetailsID
            LEFT JOIN CreditSales cs ON csd.CreditSalesID = cs.CreditSalesID
            LEFT JOIN StockDetails sd2 ON csd.StockDetailID = sd2.SDetailID
            LEFT JOIN Colors clr2 ON sd2.ColorID = clr2.ColorID
            WHERE reg.RegistrationID = @RegId";

        var veh = await connection.QueryFirstOrDefaultAsync(vehSql, new { RegId = request.RegistrationId });

        var sysSql = @"
            SELECT TOP 1
                ConcernID, Name, Address, TelephoneNo, EmailAddress, WebAddress,
                BRTAName, BRTAAddress, VATRegistrationNo, VATBIN, VATRegPerson,
                VATCIssueAddress, VATChallanCode, IsNewSInvoiceEnable
            FROM SystemInformations
            WHERE ConcernID = @ConcernId";

        var sys = await connection.QueryFirstOrDefaultAsync(sysSql, new { ConcernId = concernId })
                  ?? await connection.QueryFirstOrDefaultAsync(
                      "SELECT TOP 1 ConcernID, Name, Address, TelephoneNo, EmailAddress, WebAddress, BRTAName, BRTAAddress, VATRegistrationNo, VATBIN, VATRegPerson, VATCIssueAddress, VATChallanCode, IsNewSInvoiceEnable FROM SystemInformations");

        decimal parsedCC = ParseCC(veh?.CC);
        int regType = reg.RegType;
        var brtaRates = await connection.QueryAsync<BRTARate>(
            "SELECT * FROM BRTARates WHERE RegType = @RegType ORDER BY ConcernID DESC, ID ASC", new { RegType = regType });
        var ratesList = brtaRates.AsList();

        BRTARate? matchedRate = null;
        if (ratesList.Count > 0)
        {
            var concernRates = ratesList.Where(r => r.ConcernID == concernId).ToList();
            var targetRates = concernRates.Count > 0 ? concernRates : ratesList;

            matchedRate = parsedCC <= 100
                ? targetRates.FirstOrDefault(r => r.CC <= 100)
                : targetRates.FirstOrDefault(r => r.CC > 100);

            matchedRate ??= targetRates.FirstOrDefault();
        }

        string thanaName = cust.ThanaName ?? string.Empty;
        string distName = cust.DistrictName ?? string.Empty;
        string village = cust.Village ?? string.Empty;
        string post = cust.Post ?? string.Empty;
        string fullCustomerAddress = $"VILL: {village} , POST: {post} , THANA: {thanaName} , DIST: {distName}";

        int? age = null;
        DateTime? dob = cust.DOB;
        if (dob.HasValue && dob.Value > DateTime.MinValue)
        {
            age = DateTime.Today.Year - dob.Value.Year;
            if (dob.Value.Date > DateTime.Today.AddYears(-age.Value)) age--;
        }

        var customerDto = new RegDocCustomerDto
        {
            CustomerId = cust.CustomerID,
            Code = cust.Code ?? string.Empty,
            Name = cust.Name ?? string.Empty,
            FatherName = cust.FName,
            MotherName = cust.MName,
            HusbandOrWifeName = cust.HusbandOrWifeName,
            Sex = cust.SexType == 1 ? "Male" : cust.SexType == 2 ? "Female" : "Other",
            ContactNo = cust.ContactNo,
            NID = cust.NID,
            DOB = cust.DOB,
            FormattedDOB = dob?.ToString("dd MMM yyyy"),
            Age = age,
            Village = village,
            Post = post,
            Thana = thanaName,
            District = distName,
            CustomerAddress = fullCustomerAddress,
            PermanentAddress = cust.PermanentAddress,
            GuardianName = cust.RefName,
            IsDocTransfer = isTransferred,
            TransferDate = reg.TransferDate,
            OriginalCustomerName = isTransferred ? originalCustName : null
        };

        var companyDto = new RegDocCompanyDto
        {
            ConcernId = sys?.ConcernID ?? concernId,
            Name = sys?.Name ?? string.Empty,
            Address = sys?.Address ?? string.Empty,
            TelephoneNo = sys?.TelephoneNo ?? string.Empty,
            EmailAddress = sys?.EmailAddress ?? string.Empty,
            WebAddress = sys?.WebAddress ?? string.Empty,
            BRTACircle = sys?.BRTAName ?? string.Empty,
            BRTACircleAddress = sys?.BRTAAddress ?? string.Empty,
            VATRegistrationNo = sys?.VATRegistrationNo ?? string.Empty,
            VATBIN = sys?.VATBIN ?? string.Empty,
            VATRegPerson = sys?.VATRegPerson ?? string.Empty,
            VATCIssueAddress = sys?.VATCIssueAddress ?? string.Empty,
            VATChallanCode = sys?.VATChallanCode ?? string.Empty
        };

        var vehicleDto = new RegDocVehicleDto
        {
            ProductId = veh?.ProductID ?? reg.ProductID,
            ProductName = veh?.ProductName ?? string.Empty,
            ProductCode = veh?.ProductCode ?? string.Empty,
            CategoryName = veh?.CategoryName ?? string.Empty,
            CompanyName = veh?.CompanyName ?? string.Empty,
            ClassOfProduct = "Motor Cycle",
            ChassisNo = veh?.ChassisNo ?? string.Empty,
            EngineNo = veh?.EngineNo ?? string.Empty,
            KeyNo = veh?.KeyNo ?? string.Empty,
            Color = veh?.ColorName ?? string.Empty,
            CC = veh?.CC ?? string.Empty,
            ParsedCC = parsedCC,
            AssembleYear = veh?.MFGYear ?? string.Empty,
            NoOfCylinder = veh?.NoofCylinder ?? string.Empty,
            VehicleSize = veh?.VehicleSize ?? string.Empty,
            Weight = veh?.Weight ?? string.Empty,
            UnLadenWeight = veh?.UnLadenWeight ?? string.Empty,
            TireSize = veh?.TiyerSize ?? string.Empty,
            RingSize = veh?.RingSize ?? string.Empty,
            SeatCapacity = veh?.SeatingCapacity ?? string.Empty,
            HorsePower = veh?.HorsePower ?? string.Empty,
            RPM = veh?.RPM ?? string.Empty,
            MakersName = string.IsNullOrWhiteSpace(veh?.MakersName) ? (veh?.CompanyName ?? string.Empty) : veh!.MakersName,
            MadeIn = veh?.MadeIn ?? string.Empty,
            UnitPrice = veh?.UnitPrice ?? 0,
            SalesRate = veh?.SalesRate ?? 0,
            Quantity = veh?.Quantity ?? 1,
            TotalAmount = veh?.TotalAmount ?? 0,
            VatAmount = veh?.VatAmount ?? 0
        };

        DateTime? salesDate = veh?.SalesDate ?? reg.SalesDate;
        string invoiceNo = veh?.InvoiceNo ?? string.Empty;
        var saleDto = new RegDocSaleDto
        {
            RegistrationId = reg.RegistrationID,
            RegNo = reg.RegNo,
            REGMRNo = reg.REGMRNo,
            RegApplyDate = reg.RegApplyDate,
            SaleType = veh?.SaleType ?? "Cash",
            OrderId = veh?.OrderId,
            InvoiceNo = invoiceNo,
            RefNo = !string.IsNullOrWhiteSpace(invoiceNo) ? "Ref:" + invoiceNo : string.Empty,
            ChallanNo = veh?.ChallanNo ?? string.Empty,
            SalesDate = salesDate,
            FormattedSalesDate = salesDate?.ToString("dd MMM yyyy") ?? string.Empty,
            ReceivedAmount = veh?.ReceivedAmount ?? 0,
            RegistrationFee = reg.RegFee,
            InsuranceFee = reg.InsuranceAmt
        };

        decimal totalBrta = matchedRate?.TotalAmount ?? (reg.BRTARegFee > 0 ? reg.BRTARegFee : 0);
        var brtaFeeDto = new RegDocBRTAFeeDto
        {
            FeeAmt = reg.RegFee,
            NewRegMF = matchedRate?.NewRegMF ?? 0,
            NewRegIF = matchedRate?.NewRegIF ?? 0,
            NewRegTF = (matchedRate?.NewRegMF ?? 0) + (matchedRate?.NewRegIF ?? 0),
            IssueTaxTokenMF = matchedRate?.IssueTaxTokeMF ?? 0,
            IssueTaxTokenTF = matchedRate?.IssueTaxTokeMF ?? 0,
            RegCertificateMF = matchedRate?.RegCertificateMF ?? 0,
            RegCertificateTF = matchedRate?.RegCertificateMF ?? 0,
            SuppDutyMF = matchedRate?.SupplementaryMF ?? 0,
            SuppDutyTF = matchedRate?.SupplementaryMF ?? 0,
            Contribution = matchedRate?.Contribution ?? 0,
            NamePlate = matchedRate?.NamePlate ?? 0,
            RegYears = regType == 1 ? "2 years" : regType == 2 ? "10 years" : "BRTA",
            TotalAmount = totalBrta,
            InWordsTotal = NumberToWordsConverter.ToTakaWords(totalBrta)
        };

        var docDto = new RegistrationDocDto
        {
            DocumentType = request.DocumentType,
            Company = companyDto,
            Customer = customerDto,
            Vehicle = vehicleDto,
            Sale = saleDto,
            BRTAFee = brtaFeeDto
        };

        string concernName = companyDto.Name;

        switch (request.DocumentType)
        {
            case 1:
                docDto.DocumentTitle = "Vehicle Assessment Form";
                docDto.TemplateName = "rptAssesmentForm";
                docDto.TemplatePath = "Registration\\rptAssesmentForm.rdlc";
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                docDto.Language = "en";
                break;

            case 2:
                bool isHForm = concernName.Contains("M.S AUTOMOBILE", StringComparison.OrdinalIgnoreCase);
                docDto.DocumentTitle = isHForm ? "H FORM" : "FORM OF APPLICATION FOR THE REGISTRATION OF MOTOR VEHICLES";
                docDto.TemplateName = isHForm ? "rptHForm" : "rptRegistration2";
                docDto.TemplatePath = isHForm ? "Registration\\rptHForm.rdlc" : "Registration\\rptRegistration2.rdlc";
                docDto.IsMultiPage = true;
                docDto.PageCount = 2;
                docDto.Language = "en";
                docDto.MultiPage = new RegDocMultiPageDto
                {
                    Page1Section = new RegDocSection1Dto
                    {
                        Title = "Section-I (To be filled by the office)",
                        ClassOfVehicle = "Motor Cycle",
                        MakersName = vehicleDto.MakersName,
                        TypeOfBody = "Solo",
                        YearOfManufacture = vehicleDto.AssembleYear,
                        NumberOfCylinders = vehicleDto.NoOfCylinder,
                        ChassisNo = vehicleDto.ChassisNo,
                        EngineNo = vehicleDto.EngineNo,
                        HorsePower = vehicleDto.HorsePower,
                        CC = vehicleDto.CC,
                        SeatingCapacity = vehicleDto.SeatCapacity,
                        UnladenWeight = vehicleDto.UnLadenWeight,
                        LadenWeight = vehicleDto.Weight,
                        Color = vehicleDto.Color,
                        FuelUsed = "Petrol / Octane",
                        TireSize = vehicleDto.TireSize
                    },
                    Page2Section = new RegDocSection2Dto
                    {
                        Title = "Section-II & III (Application Particulars & Specimen Signatures)",
                        FullName = customerDto.Name,
                        FatherOrHusbandName = customerDto.FatherName ?? customerDto.HusbandOrWifeName ?? string.Empty,
                        MotherName = customerDto.MotherName ?? string.Empty,
                        PresentAddress = customerDto.CustomerAddress,
                        PermanentAddress = customerDto.PermanentAddress ?? customerDto.CustomerAddress,
                        DateOfBirth = customerDto.FormattedDOB ?? string.Empty,
                        Age = customerDto.Age?.ToString() ?? string.Empty,
                        Nationality = "Bangladeshi",
                        NID = customerDto.NID ?? string.Empty,
                        MobileNo = customerDto.ContactNo ?? string.Empty,
                        Declaration = "I hereby declare that the particulars given above are true and that the vehicle has not been registered elsewhere."
                    }
                };
                break;

            case 3:
                docDto.DocumentTitle = "OWNER'S PARTICULARS / SPECIMEN SIGNATURE";
                docDto.TemplateName = "rptRegistration3";
                docDto.TemplatePath = "Registration\\rptRegistration3.rdlc";
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                docDto.Language = "en";
                break;

            case 4:
                if (HasName(concernName, "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE"))
                {
                    docDto.DocumentTitle = "Agreement Letter";
                    docDto.TemplateName = "rptMSAUTOMOBILEAggrement";
                    docDto.TemplatePath = "Registration\\rptMSAUTOMOBILEAggrement.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "MRIDHA"))
                {
                    docDto.DocumentTitle = "অঙ্গীকারনামা";
                    docDto.TemplateName = "rptMridhaAggrement";
                    docDto.TemplatePath = "Registration\\rptMridhaAggrement.rdlc";
                    docDto.Language = "bn";
                    docDto.DeclarationText = $"আমি মেসার্স মৃধা মটরস্ স্বত্বাধিকারী । আমার প্রতিষ্ঠান হইতে একটি মটর সাইকেল যাহার মডেল নং: {vehicleDto.ProductName}, ইন্জিন নং: {vehicleDto.EngineNo}, চেসিস নং: {vehicleDto.ChassisNo} বিক্রয় করা হইয়াছে । উক্ত মোটরসাইকেলটিতে কোনো প্রকার ত্রুটি বা জালিয়াতি পাওয়া গেলে আমি এককভাবে দায়ী থাকিব ।";
                }
                else
                {
                    docDto.DocumentTitle = "Agreement Letter";
                    docDto.TemplateName = "rptRegistration4";
                    docDto.TemplatePath = "Registration\\rptRegistration4.rdlc";
                    docDto.Language = "en";
                    docDto.DeclarationText = $"To The Registration Authority, B.R.T.A. Dear Sir, We hereby declare that the motorcycle with Engine No: {vehicleDto.EngineNo} and Chassis No: {vehicleDto.ChassisNo} has been legally sold to {customerDto.Name}.";
                }
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                break;

            case 5:
                if (HasName(concernName, "KUSHTIA", "TAJ MOTORS"))
                {
                    docDto.DocumentTitle = "TO WHOM IT MAY CONCERN";
                    docDto.TemplateName = "rptRegistrationKushtia";
                    docDto.TemplatePath = "Registration\\rptRegistrationKushtia.rdlc";
                    docDto.IsMultiPage = false;
                    docDto.PageCount = 1;
                }
                else if (HasName(concernName, "SALMAN MOTORS"))
                {
                    docDto.DocumentTitle = "TO WHOM IT MAY CONCERN";
                    docDto.TemplateName = "rptRegistrationSalmanMotors";
                    docDto.TemplatePath = "Registration\\rptRegistrationSalmanMotors.rdlc";
                    docDto.IsMultiPage = false;
                    docDto.PageCount = 1;
                }
                else
                {
                    docDto.DocumentTitle = "Reference Information & Delivery Document";
                    docDto.TemplateName = "rptRegistration";
                    docDto.TemplatePath = "Registration\\rptRegistration.rdlc";
                    docDto.IsMultiPage = true;
                    docDto.PageCount = 2;
                }
                docDto.Language = "en";
                docDto.Remarks = $"{companyDto.Name}\n{companyDto.Address}\n{companyDto.TelephoneNo}";
                break;

            case 6:
                if (HasName(concernName, "NEW PARK"))
                {
                    docDto.DocumentTitle = "বিক্রয় প্রত্যয়নপত্র";
                    docDto.TemplateName = "rptNewParkRegCertificate";
                    docDto.TemplatePath = "Registration\\rptNewParkRegCertificate.rdlc";
                    docDto.Language = "bn";
                    docDto.DeclarationText = "বরাবর, রেজিষ্ট্রেশন কর্তৃপক্ষ, বিআরটিএ । এই মর্মে প্রত্যয়ন করা যাইতেছে যে, উপরোক্ত মটরসাইকেলটি আমাদের প্রতিষ্ঠান হইতে যথাযথ নিয়ম মানিয়া বিক্রয় করা হইয়াছে ।";
                }
                else if (HasName(concernName, "KUSHTIA"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptKushtiaBajajRegCertificate";
                    docDto.TemplatePath = "Registration\\rptKushtiaBajajRegCertificate.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "SALMAN MOTORS"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptSALMANMOTORSRegCertificate";
                    docDto.TemplatePath = "Registration\\rptSALMANMOTORSRegCertificate.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "TAJ MOTORS"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptTajMotorsRegCertificate";
                    docDto.TemplatePath = "Registration\\rptTajMotorsRegCertificate.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptRegistration5";
                    docDto.TemplatePath = "Registration\\rptRegistration5.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "T. R MOTORS", "T.R MOTORS", "TR MOTORS"))
                {
                    docDto.DocumentTitle = "অঙ্গীকারনামা ও বিক্রয় প্রত্যয়নপত্র";
                    docDto.TemplateName = "rptTRMOTORS";
                    docDto.TemplatePath = "Registration\\rptTRMOTORS.rdlc";
                    docDto.IsMultiPage = true;
                    docDto.PageCount = 2;
                    docDto.Language = "bn";
                }
                else if (HasName(concernName, "MOLLA MOTORS", "T.L.A", "TLA"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptTLARegCertificate";
                    docDto.TemplatePath = "Registration\\rptTLARegCertificate.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "MRIDHA"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptMridhaRegCertificate";
                    docDto.TemplatePath = "Registration\\rptMridhaRegCertificate.rdlc";
                    docDto.Language = "en";
                }
                else if (HasName(concernName, "SUZUKI"))
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptRegCertificateSuzuki";
                    docDto.TemplatePath = "Registration\\rptRegCertificateSuzuki.rdlc";
                    docDto.Language = "en";
                }
                else
                {
                    docDto.DocumentTitle = "Sales Certificate";
                    docDto.TemplateName = "rptRegCertificate";
                    docDto.TemplatePath = "Registration\\rptRegCertificate.rdlc";
                    docDto.Language = "en";
                }
                break;

            case 7:
                bool isCredit = string.Equals(saleDto.SaleType, "Credit", StringComparison.OrdinalIgnoreCase);
                if (isCredit)
                {
                    docDto.DocumentTitle = "Credit Sales Invoice";
                    docDto.TemplateName = "REGNewPCreditInvoice";
                    docDto.TemplatePath = "NewParkInvoice\\REGNewPCreditInvoice.rdlc";
                }
                else if (sys?.IsNewSInvoiceEnable == 1)
                {
                    docDto.DocumentTitle = "Sales Invoice";
                    docDto.TemplateName = "AMSalesInvoice";
                    docDto.TemplatePath = "Sales\\AMSalesInvoice.rdlc";
                }
                else if (HasName(concernName, "NEW PARK"))
                {
                    docDto.DocumentTitle = "Sales Invoice";
                    docDto.TemplateName = "RetNewPInvoice";
                    docDto.TemplatePath = "NewParkInvoice\\RetNewPInvoice.rdlc";
                }
                else if (HasName(concernName, "T. R MOTORS", "T.R MOTORS", "TR MOTORS", "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE"))
                {
                    docDto.DocumentTitle = "Sales Invoice";
                    docDto.TemplateName = "AMSalesInvoiceTR";
                    docDto.TemplatePath = "Sales\\AMSalesInvoiceTR.rdlc";
                }
                else if (HasName(concernName, "MRIDHA"))
                {
                    docDto.DocumentTitle = "Sales Invoice";
                    docDto.TemplateName = "Mridha";
                    docDto.TemplatePath = "Sales\\Mridha.rdlc";
                }
                else
                {
                    docDto.DocumentTitle = "Sales Invoice";
                    docDto.TemplateName = "RetMaxInvoice";
                    docDto.TemplatePath = "Sales\\RetMaxInvoice.rdlc";
                }
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                docDto.Language = "en";
                break;

            case 8:
                decimal sRateWMushok = veh?.SRateWMushok ?? 0;
                decimal mushokAmt = veh?.Mushok ?? 0;
                decimal qty = veh?.Quantity ?? 1;
                decimal salesWithoutMushok = sRateWMushok * qty;
                decimal salesMushok = mushokAmt * qty;
                decimal totalWithMushok = salesWithoutMushok + salesMushok;

                docDto.DocumentTitle = "মূসক ৬.৩ কর চালানপত্র";
                docDto.TemplateName = "rptChallanLetter";
                docDto.TemplatePath = "VAT\\rptChallanLetter.rdlc";
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                docDto.Language = "bn";
                docDto.Mushok = new RegDocMushokDto
                {
                    ChallanNoText = "চালানপত্র নম্বর: " + saleDto.ChallanNo,
                    SalesWithoutMushok = salesWithoutMushok,
                    SalesMushok = salesMushok,
                    TotalWithMushok = totalWithMushok,
                    InWordsTotal = NumberToWordsConverter.ToTakaWords(totalWithMushok),
                    DescriptionOfGoods = $"{vehicleDto.ProductName}\nEngine No: {vehicleDto.EngineNo}\nChassis No: {vehicleDto.ChassisNo}"
                };
                break;

            case 9:
                docDto.DocumentTitle = "Gate Pass";
                docDto.TemplateName = HasName(concernName, "SALMAN MOTORS") ? "GatePassForSalmanMotors" : "GatePass";
                docDto.TemplatePath = HasName(concernName, "SALMAN MOTORS") ? "Registration\\GatePassForSalmanMotors.rdlc" : "Registration\\GatePass.rdlc";
                docDto.IsMultiPage = false;
                docDto.PageCount = 1;
                docDto.Language = "en";
                docDto.GatePass = new RegDocGatePassDto
                {
                    GatePassNo = saleDto.InvoiceNo,
                    Notice = "N.B: This pass is valid for the day of issue only",
                    DescriptionOfGoods = "Motorcycle"
                };
                break;

            default:
                throw new BadRequestException($"Invalid registration document type: {request.DocumentType}");
        }

        return docDto;
    }

    private static decimal ParseCC(string? ccStr)
    {
        if (string.IsNullOrWhiteSpace(ccStr)) return 0;
        var match = Regex.Match(ccStr, @"\d+(\.\d+)?");
        if (match.Success && decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var cc))
        {
            return cc;
        }
        return 0;
    }

    private static bool HasName(string concernName, params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(concernName)) return false;
        foreach (var kw in keywords)
        {
            if (concernName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
