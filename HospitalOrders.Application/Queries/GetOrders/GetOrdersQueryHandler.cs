using HospitalOrders.Application.DTOs;
using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Enums;
using HospitalOrders.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HospitalOrders.Application.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<OrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork, ILogger<GetOrdersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        OrderStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var parsed))
            {
                _logger.LogWarning("Filtro de estado inválido recibido: {Status}", request.Status);
                return Enumerable.Empty<OrderResponse>();
            }
            statusFilter = parsed;
        }

        var orders = await _unitOfWork.Orders.GetAllAsync(request.PatientId, statusFilter);

        return orders.Select(MapToResponse);
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
