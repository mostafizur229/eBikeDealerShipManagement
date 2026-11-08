using eBike.Application.Common.Interfaces;
using eBike.Application.Features.HolidayCalenders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.HolidayCalenders.Queries;

public record GetHolidayCalendersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? Year = null,
    int? Month = null
) : IRequest<List<HolidayCalenderDto>>;

public class GetHolidayCalendersQueryHandler : IRequestHandler<GetHolidayCalendersQuery, List<HolidayCalenderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHolidayCalendersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayCalenderDto>> Handle(GetHolidayCalendersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.HolidayCalenders.AsNoTracking();

        if (request.FromDate.HasValue)
        {
            query = query.Where(h => h.Date >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(h => h.Date <= request.ToDate.Value);
        }

        if (request.Year.HasValue && request.Month.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, request.Month.Value, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            query = query.Where(h => h.Date >= firstDay && h.Date <= lastDay);
        }
        else if (request.Year.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, 1, 1);
            var lastDay = new DateTime(request.Year.Value, 12, 31);
            query = query.Where(h => h.Date >= firstDay && h.Date <= lastDay);
        }

        var items = await query.OrderBy(h => h.Date).ToListAsync(cancellationToken);

        return items.Select(h => new HolidayCalenderDto
        {
            ID = h.ID,
            Date = h.Date,
            Description = h.Description,
            Type = h.Type,
            Status = h.Status,
            ConcernID = h.ConcernID
        }).ToList();
    }
}

public record GetMonthHolidayGridQuery(int Year, int Month) : IRequest<List<HolidayMonthDayDto>>;

public class GetMonthHolidayGridQueryHandler : IRequestHandler<GetMonthHolidayGridQuery, List<HolidayMonthDayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthHolidayGridQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayMonthDayDto>> Handle(GetMonthHolidayGridQuery request, CancellationToken cancellationToken)
    {
        var firstDay = new DateTime(request.Year, request.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var existingHolidays = await _context.HolidayCalenders
            .Where(h => h.Date >= firstDay && h.Date <= lastDay)
            .ToDictionaryAsync(h => h.Date.Date, h => h, cancellationToken);

        var list = new List<HolidayMonthDayDto>();

        for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
        {
            var isAssigned = existingHolidays.TryGetValue(date.Date, out var holiday);
            list.Add(new HolidayMonthDayDto
            {
                Date = date,
                IsHoliday = isAssigned,
                HolidayId = holiday?.ID,
                Type = holiday?.Type ?? (date.DayOfWeek == DayOfWeek.Friday ? 1 : 2),
                Description = holiday?.Description ?? (date.DayOfWeek == DayOfWeek.Friday ? "Weekly Holiday (Friday)" : null)
            });
        }

        return list;
    }
}
