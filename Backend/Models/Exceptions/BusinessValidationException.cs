namespace Backend.Models.Exceptions;

public class BusinessValidationException(string? message) : Exception(message);