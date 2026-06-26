using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Enums;
using Xunit;

namespace HospitalOrders.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnOrderWithPendingStatus()
    {
        var order = Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");

        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal("P001", order.PatientId);
        Assert.Equal("LAB001", order.ServiceCode);
        Assert.Equal("Normal", order.Priority);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Null(order.ProcessedAt);
        Assert.Null(order.FailureReason);
    }

    [Fact]
    public void Create_WithEmptyPatientId_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Order.Create("", "Juan Perez", "LAB001", "Hemograma", "Normal"));

        Assert.Contains("PatientId", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyServiceCode_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Order.Create("P001", "Juan Perez", "", "Hemograma", "Normal"));

        Assert.Contains("ServiceCode", exception.Message);
    }

    [Fact]
    public void Create_WithInvalidPriority_ShouldThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Critica"));

        Assert.Contains("Priority", exception.Message);
    }

    [Fact]
    public void MarkAsProcessing_WhenPending_ShouldChangeStatusToProcessing()
    {
        var order = Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");

        order.MarkAsProcessing();

        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void MarkAsProcessed_WhenProcessing_ShouldChangeStatusAndSetProcessedAt()
    {
        var order = Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");
        order.MarkAsProcessing();

        order.MarkAsProcessed();

        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.NotNull(order.ProcessedAt);
    }

    [Fact]
    public void MarkAsProcessed_WhenPending_ShouldThrowInvalidOperationException()
    {
        var order = Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");

        Assert.Throws<InvalidOperationException>(() => order.MarkAsProcessed());
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusAndFailureReason()
    {
        var order = Order.Create("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");
        order.MarkAsProcessing();

        order.MarkAsFailed("Servicio externo no disponible");

        Assert.Equal(OrderStatus.Failed, order.Status);
        Assert.Equal("Servicio externo no disponible", order.FailureReason);
        Assert.NotNull(order.ProcessedAt);
    }
}
