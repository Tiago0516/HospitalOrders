using HospitalOrders.Domain.Enums;

namespace HospitalOrders.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string PatientId { get; private set; }
    public string PatientName { get; private set; }
    public string ServiceCode { get; private set; }
    public string ServiceDescription { get; private set; }
    public string Priority { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Order()
    {
        PatientId = string.Empty;
        PatientName = string.Empty;
        ServiceCode = string.Empty;
        ServiceDescription = string.Empty;
        Priority = string.Empty;
    }

    public static Order Create(
        string patientId,
        string patientName,
        string serviceCode,
        string serviceDescription,
        string priority)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("PatientId es obligatorio.", nameof(patientId));

        if (string.IsNullOrWhiteSpace(serviceCode))
            throw new ArgumentException("ServiceCode es obligatorio.", nameof(serviceCode));

        if (priority != "Normal" && priority != "Urgente")
            throw new ArgumentException("Priority debe ser 'Normal' o 'Urgente'.", nameof(priority));

        return new Order
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            PatientName = patientName,
            ServiceCode = serviceCode,
            ServiceDescription = serviceDescription,
            Priority = priority,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Solo se puede procesar una orden en estado Pending. Estado actual: {Status}");

        Status = OrderStatus.Processing;
    }

    public void MarkAsProcessed()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Solo se puede completar una orden en estado Processing. Estado actual: {Status}");

        Status = OrderStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = OrderStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}
