namespace HospitalOrders.Domain.Interfaces;

public interface IOrderQueue
{
    ValueTask EnqueueAsync(Guid orderId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken);
}
