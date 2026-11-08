using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.HolidayCalenders.Commands;

public record CreateHolidayCalenderCommand(
    DateTime Date,
    string? Description,
    int Type = 2,
    int Status = 1
) : IRequest<int>;

public class CreateHolidayCalenderCommandHandler : IRequestHandler<CreateHolidayCalenderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateHolidayCalenderCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateHolidayCalenderCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.HolidayCalenders
            .FirstOrDefaultAsync(h => h.Date.Date == request.Date.Date, cancellationToken);

        if (existing != null)
        {
            existing.Description = request.Description;
            existing.Type = request.Type;
            existing.Status = request.Status;
            existing.ModifiedBy = _currentUser.UserId;
            existing.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.ID;
        }

        var holiday = new HolidayCalender
        {
            Date = request.Date.Date,
            Description = request.Description,
            Type = request.Type,
            Status = request.Status,
            ConcernID = _currentUser.ConcernId ?? 1,
            CreatedBy = _currentUser.UserId ?? 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.HolidayCalenders.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        return holiday.ID;
    }
}

public record DeleteHolidayCalenderCommand(int Id) : IRequest<bool>;

public class DeleteHolidayCalenderCommandHandler : IRequestHandler<DeleteHolidayCalenderCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteHolidayCalenderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteHolidayCalenderCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.HolidayCalenders.FirstOrDefaultAsync(h => h.ID == request.Id, cancellationToken);
        if (holiday == null)
        {
            throw new NotFoundException("HolidayCalender", request.Id.ToString());
        }

        _context.HolidayCalenders.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class HolidayBatchItem
{
    public DateTime Date { get; set; }
    public bool IsSelected { get; set; }
    public int Type { get; set; }
    public string? Description { get; set; }
}

public record BatchUpdateHolidaysCommand(
    List<HolidayBatchItem> Items,
    bool IsRemoveAction = false
) : IRequest<bool>;

public class BatchUpdateHolidaysCommandHandler : IRequestHandler<BatchUpdateHolidaysCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public BatchUpdateHolidaysCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(BatchUpdateHolidaysCommand request, CancellationToken cancellationToken)
    {
        var concernId = _currentUser.ConcernId ?? 1;
        var userId = _currentUser.UserId ?? 1;

        var selectedDates = request.Items.Where(i => i.IsSelected).ToList();
        if (!selectedDates.Any()) return true;

        var minDate = selectedDates.Min(d => d.Date.Date);
        var maxDate = selectedDates.Max(d => d.Date.Date);

        var existingHolidays = await _context.HolidayCalenders
            .Where(h => h.Date >= minDate && h.Date <= maxDate)
            .ToListAsync(cancellationToken);

        if (request.IsRemoveAction)
        {
            foreach (var item in selectedDates)
            {
                var match = existingHolidays.FirstOrDefault(h => h.Date.Date == item.Date.Date);
                if (match != null)
                {
                    _context.HolidayCalenders.Remove(match);
                }
            }
        }
        else
        {
            foreach (var item in selectedDates)
            {
                var match = existingHolidays.FirstOrDefault(h => h.Date.Date == item.Date.Date);
                if (match != null)
                {
                    match.Type = item.Type;
                    match.Description = item.Description ?? (item.Type == 1 ? "Weekly Holiday" : "Special Holiday");
                    match.ModifiedBy = userId;
                    match.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.HolidayCalenders.Add(new HolidayCalender
                    {
                        Date = item.Date.Date,
                        Description = item.Description ?? (item.Type == 1 ? "Weekly Holiday" : "Special Holiday"),
                        Type = item.Type,
                        Status = 1,
                        ConcernID = concernId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
