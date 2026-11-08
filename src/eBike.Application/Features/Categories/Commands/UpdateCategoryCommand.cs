using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(
    int Id,
    string Code,
    string Name,
    int? ConcernId
) : IRequest;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Category ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Category Code is required.")
            .MaximumLength(150).WithMessage("Category Code must not exceed 150 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category Name is required.");
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        int targetConcernId = request.ConcernId ?? category.ConcernID;

        var isDuplicate = await _context.Categories
            .AnyAsync(c => c.CategoryID != request.Id &&
                           c.Description.ToLower() == request.Name.Trim().ToLower() &&
                           c.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Category with same name already exists in the system. Please try with a different name.");
        }

        var oldDescription = category.Description.Trim();
        var newDescription = request.Name.Trim();

        category.Code = request.Code.Trim();
        category.Description = newDescription;
        category.ConcernID = targetConcernId;

        // Auto-replicate update to connected family concerns where description matches
        if (!string.Equals(oldDescription, newDescription, StringComparison.OrdinalIgnoreCase))
        {
            var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
                .GetConnectedFamilyConcernsAsync(_context, category.ConcernID, cancellationToken);
            var familyConcernIds = familyConcerns.Select(f => f.ConcernID).ToList();

            var matchingOtherCategories = await _context.Categories
                .Where(c => familyConcernIds.Contains(c.ConcernID) && c.Description.ToLower() == oldDescription.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var other in matchingOtherCategories)
            {
                other.Description = newDescription;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
