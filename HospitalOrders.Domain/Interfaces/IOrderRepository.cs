using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Enums;

namespace HospitalOrders.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync(string? patientId = null, OrderStatus? status = null);
    Task<IEnumerable<Order>> GetPendingAsync();
    Task AddAsync(Order order);
}
