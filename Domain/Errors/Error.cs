namespace Domain.Errors;

public record Error
{
    public Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        ErrorType = type;
        InvalidField = invalidField;
    }
    
    public string Code { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string? InvalidField { get; set; }
    
    public ErrorType ErrorType { get; set; }

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);
}