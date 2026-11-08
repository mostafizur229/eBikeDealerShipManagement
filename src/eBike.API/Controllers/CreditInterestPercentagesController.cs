using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.CreditInterestPercentages.Commands;
using eBike.Application.Features.CreditInterestPercentages.DTOs;
using eBike.Application.Features.CreditInterestPercentages.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
[Route("api/credit-interest-percentages")]
[Route("api/credit-interest")]
public class CreditInterestPercentagesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.View)]
    public async Task<ActionResult<List<CreditInterestPercentageDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCreditInterestPercentagesQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.View)]
    public async Task<ActionResult<CreditInterestPercentageDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCreditInterestPercentageByIdQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Credit Interest Percentage with ID {id} not found." });
        }
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextCreditInterestCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateCreditInterestPercentageCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { intPercentageId = id, message = "Credit Interest Percentage has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCreditInterestPercentageCommand command, CancellationToken cancellationToken)
    {
        if (id != command.IntPercentageId)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Credit Interest Percentage has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.CreditInterestPercentages, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCreditInterestPercentageCommand(id), cancellationToken);
        return Ok(new { message = "Credit Interest Percentage has been deleted successfully." });
    }
}
