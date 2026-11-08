using eBike.Application.Features.CreditSalesOrders.DTOs;
using MediatR;

namespace eBike.Application.Features.CreditSalesOrders.Queries;

public record CalculateInstallmentScheduleQuery(
    decimal TotalAmount,
    decimal DownPayment,
    int NoOfInstallments,
    DateTime InstallmentDate,
    decimal InterestAmount = 0m
) : IRequest<CalculateInstallmentScheduleResultDto>;

public class CalculateInstallmentScheduleQueryHandler : IRequestHandler<CalculateInstallmentScheduleQuery, CalculateInstallmentScheduleResultDto>
{
    public Task<CalculateInstallmentScheduleResultDto> Handle(CalculateInstallmentScheduleQuery request, CancellationToken cancellationToken)
    {
        var remainingAmount = Math.Max(0, request.TotalAmount - request.DownPayment);
        var noOfInstallments = request.NoOfInstallments > 0 ? request.NoOfInstallments : 1;
        var monthlyInstallment = Math.Round(remainingAmount / noOfInstallments, 2);

        var netRemainingAmount = Math.Max(0, request.TotalAmount - (request.InterestAmount + request.DownPayment));
        var netInstallmentAmt = Math.Round(netRemainingAmount / noOfInstallments, 2);
        var hireValue = request.InterestAmount > 0 ? Math.Round(request.InterestAmount / noOfInstallments, 2) : 0m;

        var result = new CalculateInstallmentScheduleResultDto
        {
            TotalAmount = request.TotalAmount,
            DownPayment = request.DownPayment,
            RemainingAmount = remainingAmount,
            NoOfInstallment = noOfInstallments,
            MonthlyInstallmentAmount = monthlyInstallment,
            Schedules = new List<CalculatedScheduleItemDto>()
        };

        var currentBalance = remainingAmount;
        var scheduleDate = request.InstallmentDate.Date.AddMonths(1);

        for (int i = 1; i <= noOfInstallments; i++)
        {
            var closingBalance = Math.Max(0, currentBalance - monthlyInstallment);
            if (i == noOfInstallments)
            {
                closingBalance = 0m;
            }

            result.Schedules.Add(new CalculatedScheduleItemDto
            {
                ScheduleNo = i,
                MonthDate = scheduleDate,
                OpeningBalance = Math.Round(currentBalance, 2),
                NetValue = netInstallmentAmt,
                HireValue = hireValue,
                InstallmentAmount = monthlyInstallment,
                ClosingBalance = Math.Round(closingBalance, 2),
                PaymentStatus = "Due"
            });

            currentBalance = closingBalance;
            scheduleDate = scheduleDate.AddMonths(1);
        }

        return Task.FromResult(result);
    }
}
