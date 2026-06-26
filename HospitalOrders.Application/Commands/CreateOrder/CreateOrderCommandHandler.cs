using HospitalOrders.Application.DTOs;
using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HospitalOrders.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderQueue _orderQueue;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IOrderQueue orderQueue,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _orderQueue = orderQueue;
        _logger = logger;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            request.PatientId,
            request.PatientName,
            request.ServiceCode,
            request.ServiceDescription,
            request.Priority
        );

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CommitAsync();

        await _orderQueue.EnqueueAsync(order.Id, cancellationToken);

        _logger.LogInformation(
            "Orden creada. Id: {OrderId} | Paciente: {PatientId} | Servicio: {ServiceCode} | Prioridad: {Priority}",
            order.Id, order.PatientId, order.ServiceCode, order.Priority);

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
