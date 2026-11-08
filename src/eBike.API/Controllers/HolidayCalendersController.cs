using eBike.API.Authorization;
using eBike.Application.Common.Security;
using eBike.Application.Features.HolidayCalenders.Commands;
using eBike.Application.Features.HolidayCalenders.DTOs;
using eBike.Application.Features.HolidayCalenders.Queries;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[Authorize]
public class HolidayCalendersController : BaseApiController
{
    [HttpGet]
    [HasPermission(ModuleNames.HolidayCalenders, PermissionAction.View)]
    public async Task<ActionResult<List<HolidayCalenderDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetHolidayCalendersQuery(fromDate, toDate, year, month), cancellationToken);
        return Ok(result);
    }

    [HttpGet("month-grid")]
    [HasPermission(ModuleNames.HolidayCalenders, PermissionAction.View)]
    public async Task<ActionResult<List<HolidayMonthDayDto>>> GetMonthGrid(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMonthHolidayGridQuery(year, month), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(ModuleNames.HolidayCalenders, PermissionAction.Create)]
    public async Task<ActionResult> Create([FromBody] CreateHolidayCalenderCommand command, CancellationToken cancellationToken)
    {
        var id = await Sender.Send(command, cancellationToken);
        return Ok(new { holidayId = id, message = "Holiday has been saved successfully." });
    }

    [HttpPost("batch")]
    [HasPermission(ModuleNames.HolidayCalenders, PermissionAction.Update)]
    public async Task<ActionResult> BatchUpdate([FromBody] BatchUpdateHolidaysCommand command, CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return Ok(new { message = command.IsRemoveAction ? "Holidays removed successfully." : "Holidays saved successfully." });
    }

    [HttpDelete("{id:int}")]
    [HasPermission(ModuleNames.HolidayCalenders, PermissionAction.Delete)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Sender.Send(new DeleteHolidayCalenderCommand(id), cancellationToken);
        return Ok(new { message = "Holiday has been deleted successfully." });
    }
}
