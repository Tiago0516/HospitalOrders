namespace HospitalOrders.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    Task<int> CommitAsync();
}
