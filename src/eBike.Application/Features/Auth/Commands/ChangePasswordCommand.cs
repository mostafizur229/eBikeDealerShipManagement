using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = eBike.Application.Common.Exceptions.ValidationException;

namespace eBike.Application.Features.Auth.Commands;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<bool>
{
    public int UserId { get; init; }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("New password and confirmation do not match.");
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptoService;

    public ChangePasswordCommandHandler(IApplicationDbContext context, ICryptographyService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        bool isCurrentValid = _cryptoService.VerifyPassword(
            request.CurrentPassword, 
            user.PasswordHash ?? string.Empty, 
            user.SecurityStamp ?? string.Empty);

        if (!isCurrentValid)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CurrentPassword), "Current password is incorrect.")
            });
        }

        var (newHash, _) = _cryptoService.CreatePasswordHash(request.NewPassword);
        user.PasswordHash = newHash;
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
