using System.Net;

namespace eBike.Infrastructure.Services;

public static class EmailTemplateBuilder
{
    public static string BuildServiceStatusNotificationHtml(
        string customerName,
        string ticketNumber,
        string statusName,
        string bikeModel,
        string notes,
        string supportPhone = "01700000000")
    {
        string encodedCustomer = WebUtility.HtmlEncode(customerName);
        string encodedTicket = WebUtility.HtmlEncode(ticketNumber);
        string encodedStatus = WebUtility.HtmlEncode(statusName);
        string encodedModel = WebUtility.HtmlEncode(bikeModel);
        string encodedNotes = WebUtility.HtmlEncode(notes);
        string encodedPhone = WebUtility.HtmlEncode(supportPhone);

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>e-Bike Service Update - {encodedTicket}</title>
    <style>
        body {{ margin:0; padding:0; background-color:#f1f5f9; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color:#1e293b; }}
        .container {{ max-width:600px; margin:20px auto; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 6px -1px rgba(0,0,0,0.1); }}
        .header {{ background:#0f766e; color:#ffffff; padding:24px; text-align:center; }}
        .header h1 {{ margin:0; font-size:20px; font-weight:600; letter-spacing:0.5px; }}
        .body {{ padding:30px 24px; line-height:1.6; }}
        .status-box {{ background:#f0fdf4; border-left:4px solid #16a34a; padding:16px; margin:20px 0; border-radius:4px; }}
        .ticket-id {{ font-size:18px; font-weight:bold; color:#0f766e; }}
        .footer {{ background:#f8fafc; border-top:1px solid #e2e8f0; padding:16px 24px; text-align:center; font-size:12px; color:#64748b; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>e-Bike Enterprise Service Update</h1>
        </div>
        <div class=""body"">
            <p>Dear <strong>{encodedCustomer}</strong>,</p>
            <p>Your e-bike service job card has been updated. Here are the latest details:</p>
            
            <div class=""status-box"">
                <div>Job Card No: <span class=""ticket-id"">{encodedTicket}</span></div>
                <div>Model: <strong>{encodedModel}</strong></div>
                <div>Current Status: <strong style=""color:#16a34a;"">{encodedStatus}</strong></div>
            </div>

            {(string.IsNullOrWhiteSpace(encodedNotes) ? "" : $"<p><strong>Technician Notes:</strong> {encodedNotes}</p>")}

            <p>If you have any questions, please contact our helpline at <strong>{encodedPhone}</strong>.</p>
            <p>Thank you for choosing our e-bike service center.</p>
        </div>
        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} eBike Management System. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }
}
