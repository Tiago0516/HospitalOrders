using HospitalOrders.Application.DTOs;
using MediatR;

namespace HospitalOrders.Application.Queries.GetOrders;

public record GetOrdersQuery(
    string? PatientId = null,
    string? Status = null
) : IRequest<IEnumerable<OrderResponse>>;
