using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SaleOffers.Commands;

public record CreateSaleOfferCommand(
    string? OfferCode,
    int ProductID,
    DateTime? FromDate,
    DateTime? ToDate,
    string Description,
    decimal OfferValue,
    int OfferType,
    decimal ThresholdValue,
    int Status = 1
) : IRequest<int>;

public class CreateSaleOfferCommandValidator : AbstractValidator<CreateSaleOfferCommand>
{
    public CreateSaleOfferCommandValidator()
    {
        RuleFor(x => x.ProductID).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
    }
}

public class CreateSaleOfferCommandHandler : IRequestHandler<CreateSaleOfferCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateSaleOfferCommandHandler(IApplicationDbContext context, ICurrentUser currentUser, ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateSaleOfferCommand request, CancellationToken cancellationToken)
    {
        var code = request.OfferCode;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextSaleOfferCodeQuery(), cancellationToken);
        }

        var offer = new SaleOffer
        {
            OfferCode = code,
            ProductID = request.ProductID,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Description = request.Description,
            OfferValue = request.OfferValue,
            OfferType = request.OfferType,
            ThresholdValue = request.ThresholdValue,
            Status = request.Status,
            ConcernID = _currentUser.ConcernId ?? 1,
            CreatedBy = _currentUser.UserId ?? 1,
            CreateDate = DateTime.UtcNow
        };

        _context.SaleOffers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);

        return offer.OfferID;
    }
}

public record UpdateSaleOfferCommand(
    int OfferID,
    string? OfferCode,
    int ProductID,
    DateTime? FromDate,
    DateTime? ToDate,
    string Description,
    decimal OfferValue,
    int OfferType,
    decimal ThresholdValue,
    int Status
) : IRequest<bool>;

public class UpdateSaleOfferCommandHandler : IRequestHandler<UpdateSaleOfferCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateSaleOfferCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateSaleOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.SaleOffers.FirstOrDefaultAsync(o => o.OfferID == request.OfferID, cancellationToken);
        if (offer == null)
        {
            throw new NotFoundException("SaleOffer", request.OfferID.ToString());
        }

        offer.OfferCode = request.OfferCode ?? offer.OfferCode;
        offer.ProductID = request.ProductID;
        offer.FromDate = request.FromDate;
        offer.ToDate = request.ToDate;
        offer.Description = request.Description;
        offer.OfferValue = request.OfferValue;
        offer.OfferType = request.OfferType;
        offer.ThresholdValue = request.ThresholdValue;
        offer.Status = request.Status;
        offer.ModifiedBy = _currentUser.UserId;
        offer.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteSaleOfferCommand(int Id) : IRequest<bool>;

public class DeleteSaleOfferCommandHandler : IRequestHandler<DeleteSaleOfferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSaleOfferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSaleOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.SaleOffers.FirstOrDefaultAsync(o => o.OfferID == request.Id, cancellationToken);
        if (offer == null)
        {
            throw new NotFoundException("SaleOffer", request.Id.ToString());
        }

        _context.SaleOffers.Remove(offer);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
