using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Commands;

public record UpdateProductMrpCommand(int Id, decimal MrpRate) : IRequest;

public class UpdateProductMrpCommandValidator : AbstractValidator<UpdateProductMrpCommand>
{
    public UpdateProductMrpCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Valid Product ID is required.");
        RuleFor(x => x.MrpRate).GreaterThanOrEqualTo(0).WithMessage("MRP Rate must be non-negative.");
    }
}

public class UpdateProductMrpCommandHandler : IRequestHandler<UpdateProductMrpCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductMrpCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductMrpCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == request.Id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(product), request.Id);
        }

        product.MRP = request.MrpRate;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
