using HospitalOrders.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HospitalOrders.Worker.Workers;

public class OrderProcessingWorker : BackgroundService
{
    private readonly IOrderQueue _orderQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(
        IOrderQueue orderQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderProcessingWorker> logger)
    {
        _orderQueue = orderQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado. Escuchando la cola de órdenes...");

        await foreach (var orderId in _orderQueue.ReadAllAsync(stoppingToken))
        {
            await ProcessOrderAsync(orderId, stoppingToken);
        }

        _logger.LogInformation("Worker detenido.");
    }

    private async Task ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var order = await unitOfWork.Orders.GetByIdAsync(orderId);

        if (order is null)
        {
            _logger.LogWarning("Orden recibida en cola pero no encontrada en BD. Id: {OrderId}", orderId);
            return;
        }

        try
        {
            order.MarkAsProcessing();
            await unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Orden en procesamiento. Id: {OrderId} | Paciente: {PatientId} | Servicio: {ServiceCode}",
                order.Id, order.PatientId, order.ServiceCode);

            await SimulateProcessingAsync(order.Priority, cancellationToken);

            order.MarkAsProcessed();
            await unitOfWork.CommitAsync();

            _logger.LogInformation(
                "Orden procesada exitosamente. Id: {OrderId} | Duración simulada según prioridad: {Priority}",
                order.Id, order.Priority);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Procesamiento cancelado por shutdown. Orden quedó en estado intermedio. Id: {OrderId}",
                orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al procesar orden. Id: {OrderId} | Error: {ErrorMessage}",
                orderId, ex.Message);

            try
            {
                order.MarkAsFailed(ex.Message);
                await unitOfWork.CommitAsync();

                _logger.LogWarning(
                    "Orden marcada como fallida. Id: {OrderId} | Razón: {Reason}",
                    orderId, ex.Message);
            }
            catch (Exception innerEx)
            {
                _logger.LogCritical(innerEx,
                    "No se pudo marcar la orden como fallida. Id: {OrderId}",
                    orderId);
            }
        }
    }

    private static async Task SimulateProcessingAsync(string priority, CancellationToken cancellationToken)
    {
        var delay = priority == "Urgente"
            ? TimeSpan.FromSeconds(1)
            : TimeSpan.FromSeconds(3);

        await Task.Delay(delay, cancellationToken);
    }
}
