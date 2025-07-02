namespace Domain.Errors;

public static class Errors
{
    public static Error NotFound(Guid? id = null, string? name = null)
    {
        var forId = id == null ? "" : $" for Id '{id}";
        return Error.NotFound("record.not.found", $"{name ?? "value"} not found{forId}");
    }
}