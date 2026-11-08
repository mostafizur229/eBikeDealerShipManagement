using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Categories.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Categories.Queries;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryID == request.Id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        return new CategoryDto
        {
            Id = category.CategoryID,
            Code = category.Code,
            Name = category.Description,
            ConcernId = category.ConcernID,
            ConcernName = category.SisterConcern != null ? category.SisterConcern.Name : null
        };
    }
}
