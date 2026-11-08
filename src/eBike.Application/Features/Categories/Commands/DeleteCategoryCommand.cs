using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        var productCount = await _context.Products
            .CountAsync(p => p.CategoryID == request.Id, cancellationToken);

        if (productCount > 0)
        {
            throw new ConflictException($"Cannot delete category '{category.Description}' because {productCount} Product(s) are associated with it. Please reassign or delete the associated products first.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
