using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Registrations.Commands;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class RegistrationsController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<List<RegistrationListDto>>> GetAll(
        [FromQuery] int? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? customerId,
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetRegistrationsQuery(
            Status: status,
            FromDate: fromDate,
            ToDate: toDate,
            CustomerId: customerId,
            Search: search,
            ConcernId: concernId
        ), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<RegistrationDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetRegistrationByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/documents")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<List<RegistrationDocListItemDto>>> GetDocuments(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetRegistrationDocListQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/documents/{documentType:int}")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<RegistrationDocDto>> GetDocument(
        int id,
        int documentType,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetRegistrationDocQuery(id, documentType), cancellationToken);
        return Ok(result);
    }

    [HttpGet("customer/{customerId:int}/unregistered-bikes")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<List<CustomerUnregisteredBikeDto>>> GetCustomerUnregisteredBikes(
        int customerId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCustomerUnregisteredBikesQuery(customerId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("brta-rate")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.View)]
    public async Task<ActionResult<BRTARateDto>> GetBRTARate(
        [FromQuery] int regType,
        [FromQuery] decimal? cc,
        [FromQuery] int? productId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBRTARateQuery(regType, cc, productId), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = "No BRTA rate found matching criteria." });
        }
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Create)]
    public async Task<ActionResult<RegistrationDto>> Create(
        [FromBody] CreateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.RegistrationID }, result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Update)]
    public async Task<ActionResult<RegistrationDto>> Update(
        int id,
        [FromBody] UpdateRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new UpdateRegistrationCommand(
            Id: id,
            RegApplyDate: request.RegApplyDate,
            RegType: request.RegType,
            RegFee: request.RegFee,
            InsuranceAmt: request.InsuranceAmt,
            REGMRNo: request.REGMRNo
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/send-to-observation")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Update)]
    public async Task<ActionResult<RegistrationDto>> SendToObservation(
        int id,
        [FromBody] SendToObservationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SendToObservationCommand(
            Id: id,
            ObservationDate: request.ObservationDate,
            BRTATIPS: request.BRTATIPS,
            BRTARegFee: request.BRTARegFee
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/send-to-file-processing")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Update)]
    public async Task<ActionResult<RegistrationDto>> SendToFileProcessing(
        int id,
        [FromBody] SendToFileProcessingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SendToFileProcessingCommand(
            Id: id,
            FileReceivedDate: request.FileReceivedDate
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/complete")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Update)]
    public async Task<ActionResult<RegistrationDto>> Complete(
        int id,
        [FromBody] CompleteRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CompleteRegistrationCommand(
            Id: id,
            CompletionDate: request.CompletionDate,
            RegNo: request.RegNo
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/transfer-document")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Update)]
    public async Task<ActionResult<RegistrationDto>> TransferDocument(
        int id,
        [FromBody] TransferRegistrationDocRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransferRegistrationDocCommand(
            Id: id,
            NewCustomerId: request.NewCustomerId,
            TransferDate: request.TransferDate
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/approve")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Approve)]
    public async Task<ActionResult<RegistrationDto>> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ApproveRegistrationCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Registrations, PermissionAction.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteRegistrationCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateRegistrationRequest(
    DateTime RegApplyDate,
    int RegType,
    decimal RegFee,
    decimal InsuranceAmt = 0,
    string? REGMRNo = null
);

public record SendToObservationRequest(
    DateTime ObservationDate,
    decimal? BRTATIPS = null,
    decimal? BRTARegFee = null
);

public record SendToFileProcessingRequest(
    DateTime FileReceivedDate
);

public record CompleteRegistrationRequest(
    DateTime CompletionDate,
    string RegNo
);

public record TransferRegistrationDocRequest(
    int NewCustomerId,
    DateTime TransferDate
);

