using System.Runtime.CompilerServices;
using System.Threading.Channels;
using HospitalOrders.Domain.Interfaces;

namespace HospitalOrders.Infrastructure.Queue;

public class InMemoryOrderQueue : IOrderQueue
{
    private readonly Channel<Guid> _channel;

    public InMemoryOrderQueue()
    {
        var options = new BoundedChannelOptions(capacity: 500)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<Guid>(options);
    }

    public async ValueTask EnqueueAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(orderId, cancellationToken);
    }

    public async IAsyncEnumerable<Guid> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var orderId in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return orderId;
        }
    }
}
