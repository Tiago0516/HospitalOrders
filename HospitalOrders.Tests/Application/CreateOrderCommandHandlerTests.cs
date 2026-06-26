using HospitalOrders.Application.Commands.CreateOrder;
using HospitalOrders.Domain.Entities;
using HospitalOrders.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HospitalOrders.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<IOrderQueue> _queueMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _queueMock = new Mock<IOrderQueue>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_repositoryMock.Object);

        _handler = new CreateOrderCommandHandler(
            _unitOfWorkMock.Object,
            _queueMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveAndEnqueueOrder()
    {
        var command = new CreateOrderCommand("P001", "Juan Perez", "LAB001", "Hemograma", "Normal");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("P001", result.PatientId);
        Assert.Equal("LAB001", result.ServiceCode);
        Assert.Equal("Pending", result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        _queueMock.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyPatientId_ShouldThrowArgumentException()
    {
        var command = new CreateOrderCommand("", "Juan Perez", "LAB001", "Hemograma", "Normal");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        _queueMock.Verify(q => q.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPriority_ShouldThrowArgumentException()
    {
        var command = new CreateOrderCommand("P001", "Juan Perez", "LAB001", "Hemograma", "Critica");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
