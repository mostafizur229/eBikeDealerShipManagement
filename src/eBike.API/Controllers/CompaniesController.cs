using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.Companies.Commands;
using eBike.Application.Features.Companies.DTOs;
using eBike.Application.Features.Companies.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class CompaniesController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.Companies, PermissionAction.View)]
    public async Task<ActionResult<List<CompanyDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? concernId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCompaniesQuery(search, concernId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(ModuleNames.Companies, PermissionAction.View)]
    public async Task<ActionResult<CompanyDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCompanyByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("next-code")]
    [HasPermission(ModuleNames.Companies, PermissionAction.View)]
    public async Task<ActionResult> GetNextCode([FromQuery] int? concernId, CancellationToken cancellationToken)
    {
        var code = await Sender.Send(new GetNextCompanyCodeQuery(concernId), cancellationToken);
        return Ok(new { code });
    }

    [HttpPost]
    [HasPermission(ModuleNames.Companies, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { companyId = id, message = "Company has been saved successfully." });
    }

    [HttpPut("{id:int}")]
    [HasPermission(ModuleNames.Companies, PermissionAction.Update)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateCompanyCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "Route ID does not match command ID." });
        }

        await Sender.Send(command, cancellationToken);
        return Ok(new { message = "Company has been updated successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.Companies, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteCompanyCommand(id), cancellationToken);
        return Ok(new { message = "Company has been deleted successfully." });
    }
}
