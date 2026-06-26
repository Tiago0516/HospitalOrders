using HospitalOrders.Application.Commands.CreateOrder;
using HospitalOrders.Application.DTOs;
using HospitalOrders.Application.Queries.GetOrder;
using HospitalOrders.Application.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalOrders.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva orden médica.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.PatientId,
            request.PatientName,
            request.ServiceCode,
            request.ServiceDescription,
            request.Priority
        );

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene el listado de órdenes con filtros opcionales.
    /// </summary>
    /// <param name="patientId">Filtrar por ID de paciente.</param>
    /// <param name="status">Filtrar por estado: Pending, Processing, Processed, Failed.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] string? patientId, [FromQuery] string? status)
    {
        var query = new GetOrdersQuery(patientId, status);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una orden por su ID.
    /// </summary>
    /// <param name="id">Identificador único de la orden (GUID).</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderQuery(id);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound(new { error = $"Orden con Id '{id}' no encontrada." });

        return Ok(result);
    }
}
