using HospitalOrders.Application.DTOs;
using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HospitalOrders.Application.Queries.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderResponse?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(IUnitOfWork unitOfWork, ILogger<GetOrderQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.Id);

        if (order is null)
        {
            _logger.LogWarning("Orden no encontrada. Id: {OrderId}", request.Id);
            return null;
        }

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order) => new()
    {
        Id = order.Id,
        PatientId = order.PatientId,
        PatientName = order.PatientName,
        ServiceCode = order.ServiceCode,
        ServiceDescription = order.ServiceDescription,
        Priority = order.Priority,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        ProcessedAt = order.ProcessedAt,
        FailureReason = order.FailureReason
    };
}
