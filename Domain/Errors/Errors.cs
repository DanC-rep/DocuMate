namespace Domain.Errors;

public static class Errors
{
    public static Error ValueIsInvalid(string? name = null) =>
        Error.Validation("value.is.invalid", $"{name ?? "value"} is invalid");
    
    public static Error ValueIsRequired(string? name = null) =>
        Error.Validation("value.required", $"invalid {name ?? "value"} length");
    
    public static Error NotFound(Guid? id = null, string? name = null)
    {
        var forId = id == null ? "" : $" for Id '{id}";
        return Error.NotFound("record.not.found", $"{name ?? "value"} not found{forId}");
    }
    
    public static Error AlreadyExists(string name, string key, string? value = null)
    {
        var withValue = value == null ? "" : $" = {value}";
        return Error.Conflict("record.already.exists", $"{name} already exists with {key + value}");
    }
        
    public static Error AlreadyUsed(Guid id) =>
        Error.Conflict("value.already.exists", $"{id} is already used");

    public static Error Null(string? name = null)
    {
        var label = name ?? "value";
        return Error.Null("Null.entity", $"{label} is null");
    }
}