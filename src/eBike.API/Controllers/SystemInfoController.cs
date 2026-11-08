using System.Security.Cryptography;
using System.Text;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBike.API.Controllers;

[Authorize]
public class SystemInfoController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public SystemInfoController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSystemInfo(CancellationToken ct)
    {
        var concernId = CurrentConcernId;

        var sysInfo = await _context.SystemInformations
            .AsNoTracking()
            .Include(s => s.SisterConcern)
            .FirstOrDefaultAsync(s => concernId > 0 ? s.ConcernID == concernId : true, ct);

        if (sysInfo == null)
        {
            // Fallback to first available record in the database
            sysInfo = await _context.SystemInformations
                .AsNoTracking()
                .Include(s => s.SisterConcern)
                .FirstOrDefaultAsync(ct);
        }

        if (sysInfo == null)
        {
            return Ok(new
            {
                SystemInfoID = 0,
                Name = "Royal Enfield Bangladesh",
                Address = "Tejgaon I/A, Dhaka-1208, Bangladesh",
                TelephoneNo = "+880 1700-000000",
                EmailAddress = "info@royalenfield-bd.com",
                WebAddress = "https://royalenfield-bd.com",
                SystemStartDate = DateTime.UtcNow,
                ConcernID = concernId > 0 ? concernId : 1,
                ConcernName = "Head Office",
                SMSServiceEnable = 0,
                DaysBeforeSendSMS = 3,
                SMSProviderID = 1,
                SMSSendToOwner = 0,
                IsNewSInvoiceEnable = 1,
                IsRetailSMSEnable = 0,
                smsCharge = 0.50m,
                CustomerDueLimitApply = 1,
                EmployeeDueLimitApply = 1,
                ApprovalSystemEnable = 0,
                IsAutoCreditInterestPer = 0,
                MRNoRequired = 1,
                REGMRNoRequired = 1,
                LedgerNoRequired = 0
            });
        }

        return Ok(new
        {
            sysInfo.SystemInfoID,
            sysInfo.Name,
            sysInfo.Address,
            sysInfo.TelephoneNo,
            sysInfo.EmailAddress,
            sysInfo.WebAddress,
            sysInfo.SystemStartDate,
            sysInfo.ConcernID,
            ConcernName = sysInfo.SisterConcern != null ? sysInfo.SisterConcern.Name : null,
            sysInfo.VATRegistrationNo,
            sysInfo.VATChallanBank,
            sysInfo.BRTAName,
            sysInfo.BRTAAddress,
            sysInfo.VATRegPerson,
            sysInfo.VATBIN,
            sysInfo.VATCIssueAddress,
            sysInfo.VATChallanCode,
            sysInfo.DeviceIP,
            sysInfo.DeviceSerialNO,
            sysInfo.APIKey,
            sysInfo.ExpireDate,
            sysInfo.ExpireMessage,
            sysInfo.WarningMsg,

            // SMS Configuration
            sysInfo.SMSServiceEnable,
            sysInfo.InsuranceContactNo,
            sysInfo.DaysBeforeSendSMS,
            sysInfo.SMSProviderID,
            sysInfo.SMSSendToOwner,
            sysInfo.SenderId,
            sysInfo.CompanyURL,
            sysInfo.IsNewSInvoiceEnable,
            sysInfo.IsRetailSMSEnable,
            sysInfo.smsCharge,

            // Dealership Policies & Checkboxes
            sysInfo.CustomerDueLimitApply,
            sysInfo.EmployeeDueLimitApply,
            sysInfo.ApprovalSystemEnable,
            sysInfo.IsAutoCreditInterestPer,
            sysInfo.MRNoRequired,
            sysInfo.REGMRNoRequired,
            sysInfo.LedgerNoRequired,

            // Logos
            CompanyLogo = sysInfo.CompanyLogo != null && sysInfo.CompanyLogo.Length > 0
                ? $"data:{sysInfo.LogoMimeType ?? "image/png"};base64,{Convert.ToBase64String(sysInfo.CompanyLogo)}"
                : null,
            BrandLogo = sysInfo.BrandLogo != null && sysInfo.BrandLogo.Length > 0
                ? $"data:{sysInfo.BLogoMimeType ?? "image/png"};base64,{Convert.ToBase64String(sysInfo.BrandLogo)}"
                : null,
            sysInfo.LogoMimeType,
            sysInfo.BLogoMimeType
        });
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo([FromQuery] int? concernId, CancellationToken ct)
    {
        var targetId = concernId > 0 ? concernId.Value : CurrentConcernId;
        var info = await _context.SystemInformations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => targetId > 0 ? s.ConcernID == targetId : true, ct);

        if (info?.CompanyLogo != null && info.CompanyLogo.Length > 0)
        {
            return File(info.CompanyLogo, info.LogoMimeType ?? "image/png");
        }

        if (info?.BrandLogo != null && info.BrandLogo.Length > 0)
        {
            return File(info.BrandLogo, info.BLogoMimeType ?? "image/png");
        }

        return NotFound();
    }

    [HttpGet("generate-api-key")]
    public ActionResult<string> GenerateApiKey()
    {
        const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var bytes = RandomNumberGenerator.GetBytes(50);
        var sb = new StringBuilder(50);
        foreach (var b in bytes)
        {
            sb.Append(characters[b % characters.Length]);
        }
        return Ok(new { apiKey = sb.ToString() });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSystemInfo([FromBody] UpdateSystemInfoDto dto, CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId > 0 ? CurrentConcernId : (dto.ConcernID > 0 ? dto.ConcernID : 1);

        var sysInfo = await _context.SystemInformations.FirstOrDefaultAsync(s => s.SystemInfoID == dto.SystemInfoID, ct);

        if (sysInfo == null)
        {
            sysInfo = new SystemInformation
            {
                Name = dto.Name,
                Address = dto.Address,
                TelephoneNo = dto.TelephoneNo,
                EmailAddress = dto.EmailAddress,
                WebAddress = dto.WebAddress,
                SystemStartDate = dto.SystemStartDate ?? DateTime.UtcNow,
                ConcernID = targetConcernId,
                VATRegistrationNo = dto.VATRegistrationNo,
                VATBIN = dto.VATBIN,
                BRTAName = dto.BRTAName,
                BRTAAddress = dto.BRTAAddress,
                VATChallanBank = dto.VATChallanBank,
                VATRegPerson = dto.VATRegPerson,
                VATCIssueAddress = dto.VATCIssueAddress,
                VATChallanCode = dto.VATChallanCode,
                DeviceIP = dto.DeviceIP,
                DeviceSerialNO = dto.DeviceSerialNO,
                APIKey = dto.APIKey,
                ExpireDate = dto.ExpireDate,
                ExpireMessage = dto.ExpireMessage,
                WarningMsg = dto.WarningMsg,

                SMSServiceEnable = dto.SMSServiceEnable ? 1 : 0,
                InsuranceContactNo = dto.InsuranceContactNo,
                DaysBeforeSendSMS = dto.DaysBeforeSendSMS,
                SMSProviderID = dto.SMSProviderID,
                SMSSendToOwner = dto.SMSSendToOwner ? 1 : 0,
                SenderId = dto.SenderId,
                CompanyURL = dto.CompanyURL,
                IsNewSInvoiceEnable = dto.IsNewSInvoiceEnable ? 1 : 0,
                IsRetailSMSEnable = dto.IsRetailSMSEnable ? 1 : 0,
                smsCharge = dto.smsCharge,

                CustomerDueLimitApply = dto.CustomerDueLimitApply ? 1 : 0,
                EmployeeDueLimitApply = dto.EmployeeDueLimitApply ? 1 : 0,
                ApprovalSystemEnable = dto.ApprovalSystemEnable ? 1 : 0,
                IsAutoCreditInterestPer = dto.IsAutoCreditInterestPer ? 1 : 0,
                MRNoRequired = dto.MRNoRequired ? 1 : 0,
                REGMRNoRequired = dto.REGMRNoRequired ? 1 : 0,
                LedgerNoRequired = dto.LedgerNoRequired ? 1 : 0
            };
            _context.SystemInformations.Add(sysInfo);
        }
        else
        {
            sysInfo.Name = dto.Name;
            sysInfo.Address = dto.Address;
            sysInfo.TelephoneNo = dto.TelephoneNo;
            sysInfo.EmailAddress = dto.EmailAddress;
            sysInfo.WebAddress = dto.WebAddress;
            if (dto.SystemStartDate.HasValue) sysInfo.SystemStartDate = dto.SystemStartDate.Value;
            if (dto.ConcernID > 0) sysInfo.ConcernID = dto.ConcernID;
            sysInfo.VATRegistrationNo = dto.VATRegistrationNo;
            sysInfo.VATBIN = dto.VATBIN;
            sysInfo.BRTAName = dto.BRTAName;
            sysInfo.BRTAAddress = dto.BRTAAddress;
            sysInfo.VATChallanBank = dto.VATChallanBank;
            sysInfo.VATRegPerson = dto.VATRegPerson;
            sysInfo.VATCIssueAddress = dto.VATCIssueAddress;
            sysInfo.VATChallanCode = dto.VATChallanCode;
            sysInfo.DeviceIP = dto.DeviceIP;
            sysInfo.DeviceSerialNO = dto.DeviceSerialNO;
            sysInfo.APIKey = dto.APIKey;
            sysInfo.ExpireDate = dto.ExpireDate;
            sysInfo.ExpireMessage = dto.ExpireMessage;
            sysInfo.WarningMsg = dto.WarningMsg;

            sysInfo.SMSServiceEnable = dto.SMSServiceEnable ? 1 : 0;
            sysInfo.InsuranceContactNo = dto.InsuranceContactNo;
            sysInfo.DaysBeforeSendSMS = dto.DaysBeforeSendSMS;
            sysInfo.SMSProviderID = dto.SMSProviderID;
            sysInfo.SMSSendToOwner = dto.SMSSendToOwner ? 1 : 0;
            sysInfo.SenderId = dto.SenderId;
            sysInfo.CompanyURL = dto.CompanyURL;
            sysInfo.IsNewSInvoiceEnable = dto.IsNewSInvoiceEnable ? 1 : 0;
            sysInfo.IsRetailSMSEnable = dto.IsRetailSMSEnable ? 1 : 0;
            sysInfo.smsCharge = dto.smsCharge;

            sysInfo.CustomerDueLimitApply = dto.CustomerDueLimitApply ? 1 : 0;
            sysInfo.EmployeeDueLimitApply = dto.EmployeeDueLimitApply ? 1 : 0;
            sysInfo.ApprovalSystemEnable = dto.ApprovalSystemEnable ? 1 : 0;
            sysInfo.IsAutoCreditInterestPer = dto.IsAutoCreditInterestPer ? 1 : 0;
            sysInfo.MRNoRequired = dto.MRNoRequired ? 1 : 0;
            sysInfo.REGMRNoRequired = dto.REGMRNoRequired ? 1 : 0;
            sysInfo.LedgerNoRequired = dto.LedgerNoRequired ? 1 : 0;
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "System information updated successfully." });
    }
}

public record UpdateSystemInfoDto(
    int SystemInfoID,
    string Name,
    string Address,
    string? TelephoneNo,
    string? EmailAddress,
    string? WebAddress,
    DateTime? SystemStartDate,
    int ConcernID,
    string? VATRegistrationNo,
    string? VATBIN,
    string? BRTAName,
    string? BRTAAddress,
    string? VATChallanBank,
    string? VATRegPerson,
    string? VATCIssueAddress,
    string? VATChallanCode,
    string? DeviceIP,
    string? DeviceSerialNO,
    string? APIKey,
    DateTime? ExpireDate,
    string? ExpireMessage,
    string? WarningMsg,

    // SMS
    bool SMSServiceEnable,
    string? InsuranceContactNo,
    int DaysBeforeSendSMS,
    int SMSProviderID,
    bool SMSSendToOwner,
    string? SenderId,
    string? CompanyURL,
    bool IsNewSInvoiceEnable,
    bool IsRetailSMSEnable,
    decimal smsCharge,

    // Policies
    bool CustomerDueLimitApply,
    bool EmployeeDueLimitApply,
    bool ApprovalSystemEnable,
    bool IsAutoCreditInterestPer,
    bool MRNoRequired,
    bool REGMRNoRequired,
    bool LedgerNoRequired
);
