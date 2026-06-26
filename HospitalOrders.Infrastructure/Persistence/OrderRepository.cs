using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Enums;
using HospitalOrders.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalOrders.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(string? patientId = null, OrderStatus? status = null)
    {
        var query = _context.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(patientId))
            query = query.Where(o => o.PatientId == patientId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPendingAsync()
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }
}
