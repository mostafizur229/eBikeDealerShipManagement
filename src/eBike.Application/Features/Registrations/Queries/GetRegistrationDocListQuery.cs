using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Domain.Entities;
using MediatR;

namespace eBike.Application.Features.Registrations.Queries;

public record GetRegistrationDocListQuery(int RegistrationId) : IRequest<List<RegistrationDocListItemDto>>;

public class GetRegistrationDocListQueryHandler : IRequestHandler<GetRegistrationDocListQuery, List<RegistrationDocListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetRegistrationDocListQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<RegistrationDocListItemDto>> Handle(GetRegistrationDocListQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                r.RegistrationID,
                r.ConcernID,
                r.SorderDetailID,
                r.CreditSaleDetailsID,
                COALESCE(si.Name, sc.Name, '') AS ConcernName,
                COALESCE(si.IsNewSInvoiceEnable, 0) AS IsNewSInvoiceEnable
            FROM Registrations r
            LEFT JOIN SystemInformations si ON r.ConcernID = si.ConcernID
            LEFT JOIN SisterConcerns sc ON r.ConcernID = sc.ConcernID
            WHERE r.RegistrationID = @Id";

        var reg = await connection.QueryFirstOrDefaultAsync(sql, new { Id = request.RegistrationId });
        if (reg == null)
        {
            throw new NotFoundException(nameof(Registration), request.RegistrationId);
        }

        string concernName = reg.ConcernName ?? string.Empty;
        int isNewSInvoiceEnable = (int)reg.IsNewSInvoiceEnable;
        bool isCreditSale = (reg.CreditSaleDetailsID != null && (int)reg.CreditSaleDetailsID > 0) 
            && (reg.SorderDetailID == null || (int)reg.SorderDetailID == 0);

        bool isMSAutomobile = HasName(concernName, "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE");
        bool isMridha = HasName(concernName, "MRIDHA");
        bool isKushtiaOrTaj = HasName(concernName, "KUSHTIA", "TAJ MOTORS");
        bool isSalmanMotors = HasName(concernName, "SALMAN MOTORS");
        bool isTRMotors = HasName(concernName, "T. R MOTORS", "T.R MOTORS", "TR MOTORS");
        bool isNewPark = HasName(concernName, "NEW PARK");

        string invoiceTemplate;
        if (isCreditSale)
        {
            invoiceTemplate = "REGNewPCreditInvoice";
        }
        else if (isNewSInvoiceEnable == 1)
        {
            invoiceTemplate = "AMSalesInvoice";
        }
        else if (isNewPark)
        {
            invoiceTemplate = "RetNewPInvoice";
        }
        else if (isTRMotors || isMSAutomobile)
        {
            invoiceTemplate = "AMSalesInvoiceTR";
        }
        else if (isMridha)
        {
            invoiceTemplate = "Mridha";
        }
        else
        {
            invoiceTemplate = "RetMaxInvoice";
        }

        var list = new List<RegistrationDocListItemDto>
        {
            new()
            {
                DocumentType = 1,
                Title = "Assesment Form",
                ButtonColor = "primary",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = "rptAssesmentForm",
                Language = "en"
            },

            new()
            {
                DocumentType = 2,
                Title = isMSAutomobile ? "H FORM" : "Form of application",
                ButtonColor = "info",
                IsMultiPage = true,
                PageCount = 2,
                TemplateName = isMSAutomobile ? "rptHForm" : "rptRegistration2",
                Language = "en"
            },

            new()
            {
                DocumentType = 3,
                Title = "Owner’s particulars",
                ButtonColor = "success",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = "rptRegistration3",
                Language = "en"
            },

            new()
            {
                DocumentType = 4,
                Title = "Aggrement letter",
                ButtonColor = "warning",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = GetAgreementTemplate(concernName),
                Language = isMridha ? "bn" : "en"
            },

            new()
            {
                DocumentType = 5,
                Title = "Reference info",
                ButtonColor = "danger",
                IsMultiPage = !(isKushtiaOrTaj || isSalmanMotors),
                PageCount = (isKushtiaOrTaj || isSalmanMotors) ? 1 : 2,
                TemplateName = GetReferenceTemplate(concernName),
                Language = "en"
            },

            new()
            {
                DocumentType = 6,
                Title = "Sales certificate",
                ButtonColor = "primary",
                IsMultiPage = isTRMotors,
                PageCount = isTRMotors ? 2 : 1,
                TemplateName = GetSalesCertTemplate(concernName),
                Language = (isNewPark || isTRMotors) ? "bn" : "en"
            },
            
            new()
            {
                DocumentType = 7,
                Title = isCreditSale ? "Credit Sales Invoice" : "Sales Invoice",
                ButtonColor = "info",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = invoiceTemplate,
                Language = "en"
            },

            new()
            {
                DocumentType = 8,
                Title = "Mushok 6.3",
                ButtonColor = "primary",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = "rptChallanLetter",
                Language = "bn"
            },

            new()
            {
                DocumentType = 9,
                Title = "Gate Pass",
                ButtonColor = "primary",
                IsMultiPage = false,
                PageCount = 1,
                TemplateName = isSalmanMotors ? "GatePassForSalmanMotors" : "GatePass",
                Language = "en"
            }
        };

        return list;
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

    private static string GetAgreementTemplate(string concernName)
    {
        if (HasName(concernName, "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE"))
            return "rptMSAUTOMOBILEAggrement";
        if (HasName(concernName, "MRIDHA"))
            return "rptMridhaAggrement";
        return "rptRegistration4";
    }

    private static string GetReferenceTemplate(string concernName)
    {
        if (HasName(concernName, "KUSHTIA", "TAJ MOTORS"))
            return "rptRegistrationKushtia";
        if (HasName(concernName, "SALMAN MOTORS"))
            return "rptRegistrationSalmanMotors";
        return "rptRegistration";
    }

    private static string GetSalesCertTemplate(string concernName)
    {
        if (HasName(concernName, "NEW PARK")) return "rptNewParkRegCertificate";
        if (HasName(concernName, "KUSHTIA")) return "rptKushtiaBajajRegCertificate";
        if (HasName(concernName, "SALMAN MOTORS")) return "rptSALMANMOTORSRegCertificate";
        if (HasName(concernName, "TAJ MOTORS")) return "rptTajMotorsRegCertificate";
        if (HasName(concernName, "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE")) return "rptRegistration5";
        if (HasName(concernName, "T. R MOTORS", "T.R MOTORS", "TR MOTORS")) return "rptTRMOTORS";
        if (HasName(concernName, "MOLLA MOTORS", "T.L.A", "TLA")) return "rptTLARegCertificate";
        if (HasName(concernName, "MRIDHA")) return "rptMridhaRegCertificate";
        if (HasName(concernName, "SUZUKI")) return "rptRegCertificateSuzuki";
        return "rptRegCertificate";
    }
}
