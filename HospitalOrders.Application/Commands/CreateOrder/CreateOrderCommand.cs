using HospitalOrders.Application.DTOs;
using MediatR;

namespace HospitalOrders.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    string PatientId,
    string PatientName,
    string ServiceCode,
    string ServiceDescription,
    string Priority
) : IRequest<OrderResponse>;
