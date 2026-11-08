using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Categories.Commands;

public record CreateCategoryCommand(
    string? Code,
    string Name,
    int? ConcernId
) : IRequest<int>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category Name is required.");

        RuleFor(x => x.Code)
            .MaximumLength(150).WithMessage("Category Code must not exceed 150 characters.");
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateCategoryCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a category.");

        var isDuplicate = await _context.Categories
            .AnyAsync(c => c.Description.ToLower() == request.Name.Trim().ToLower() && c.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Category with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextCategoryCodeQuery(concernId), cancellationToken);
        }

        var category = new Category
        {
            Code = code,
            Description = request.Name.Trim(),
            ConcernID = concernId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-replicate to connected family concerns (matching MCSRMIMSWEB GetFamilyTree)
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var existsInOther = await _context.Categories
                .AnyAsync(c => c.ConcernID == familyConcern.ConcernID && c.Description.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var otherCode = await _sender.Send(new Queries.GetNextCategoryCodeQuery(familyConcern.ConcernID), cancellationToken);
                _context.Categories.Add(new Category
                {
                    Code = otherCode,
                    Description = request.Name.Trim(),
                    ConcernID = familyConcern.ConcernID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return category.CategoryID;
    }
}
