using HospitalOrders.Application.DTOs;
using MediatR;

namespace HospitalOrders.Application.Queries.GetOrder;

public record GetOrderQuery(Guid Id) : IRequest<OrderResponse?>;
