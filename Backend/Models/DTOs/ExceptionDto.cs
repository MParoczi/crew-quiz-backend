using System.Collections;

namespace Backend.Models.DTOs;

public class ExceptionDto
{
    public required string Level { get; set; }
    public required string Message { get; set; }
    public required string TraceId { get; set; }
    public IDictionary? Details { get; set; }
}