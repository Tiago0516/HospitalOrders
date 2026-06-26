namespace HospitalOrders.Application.DTOs;

public class CreateOrderRequest
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}
