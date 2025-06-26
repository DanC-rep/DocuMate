using System.Text;
using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using FileInfo = Domain.FileInfo;

namespace DocumentationGenerator.Documentation;

public class PromptGenerator : IPromptGenerator
{
    private readonly ILogger<PromptGenerator> _logger;

    public PromptGenerator(ILogger<PromptGenerator> logger)
    {
        _logger = logger;
    }
    
    public Result<string, Error> PreparePrompt(FileInfo fileInfo)
    {
        try
        {
            var sb = new StringBuilder();

            sb.AppendLine($"File: {Path.GetFileName(fileInfo.FilePath)}");
            sb.AppendLine($"Location: {fileInfo.FilePath}");
            sb.AppendLine();

            if (fileInfo.Usings.Count > 0)
            {
                sb.AppendLine("Imported namespaces:");
                foreach (var usingDirective in fileInfo.Usings)
                    sb.AppendLine($"- {usingDirective}");
                sb.AppendLine();
            }

            if (fileInfo.AssemblyAttributes.Count > 0)
            {
                sb.AppendLine("Assembly attributes:");
                foreach (var attr in fileInfo.AssemblyAttributes)
                {
                    sb.AppendLine($"- {attr.Name}");
                    if (attr.Arguments.Count > 0)
                        sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
                }

                sb.AppendLine();
            }

            foreach (var ns in fileInfo.Namespaces)
            {
                sb.AppendLine($"Namespace: {ns.Name}");

                foreach (var member in ns.Members)
                {
                    switch (member)
                    {
                        case ClassInfo classInfo:
                            sb.Append(PrepareClassInfo(classInfo));
                            break;
                        case StructInfo structInfo:
                            sb.Append(PrepareStructInfo(structInfo));
                            break;
                        case InterfaceInfo interfaceInfo:
                            sb.Append(PrepareInterfaceInfo(interfaceInfo));
                            break;
                        case EnumInfo enumInfo:
                            sb.Append(PrepareEnumInfo(enumInfo));
                            break;
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while generation prompt for file {file}: {message}", fileInfo.FilePath, ex.Message);
            
            return Error.Failure("generation.prompt", "Error while generation prompt");
        }
    }

    private string PrepareClassInfo(ClassInfo classInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Class: {classInfo.Name}");
        
        if (classInfo.Modifiers.Count > 0)
        {
            sb.AppendLine($"Modifiers: {string.Join(" ", classInfo.Modifiers)}");
        }
        
        if (classInfo.Attributes.Count > 0)
        {
            sb.AppendLine("Attributes:");
            foreach (var attr in classInfo.Attributes)
            {
                sb.AppendLine($"- {attr.Name}");
                if (attr.Arguments.Count > 0)
                {
                    sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
                }
            }
        }
        
        if (classInfo.BaseTypes.Count > 0)
        {
            sb.AppendLine($"Inherits: {string.Join(", ", classInfo.BaseTypes)}");
        }
        
        sb.AppendLine();
        sb.AppendLine("Fields:");
        foreach (var field in classInfo.Fields)
        {
            sb.AppendLine($"- {field.Type} {field.Name}");
            if (!string.IsNullOrEmpty(field.Initializer))
            {
                sb.AppendLine($"  Initializer: {field.Initializer}");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Properties:");
        foreach (var prop in classInfo.Properties)
        {
            sb.AppendLine($"- {prop.Type} {prop.Name} {{ {prop.Getter}/{prop.Setter} }}");
        }
        
        sb.AppendLine();
        sb.AppendLine("Constructors:");
        foreach (var ctor in classInfo.Constructors)
        {
            sb.AppendLine($"- {classInfo.Name}({string.Join(", ", ctor.Parameters.Select(p => $"{p.Type} {p.Name}"))})");
            if (ctor.Parameters.Count > 0)
            {
                sb.AppendLine("  Parameters:");
                foreach (var param in ctor.Parameters)
                {
                    sb.AppendLine($"  - {param.Type} {param.Name}");
                    if (!string.IsNullOrEmpty(param.DefaultValue))
                    {
                        sb.AppendLine($"    Default: {param.DefaultValue}");
                    }
                }
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Methods:");
        foreach (var method in classInfo.Methods)
        {
            sb.AppendLine($"- {method.ReturnType} {method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})");
            if (method.Parameters.Count > 0)
            {
                sb.AppendLine("  Parameters:");
                foreach (var param in method.Parameters)
                {
                    sb.AppendLine($"  - {param.Type} {param.Name}");
                }
            }

            sb.AppendLine("  Body:");
            sb.AppendLine(method.Body);
        }
        
        sb.AppendLine();
        sb.AppendLine("Events:");
        foreach (var evt in classInfo.Events)
        {
            sb.AppendLine($"- {evt.Type} {evt.Name}");
        }
        
        return sb.ToString();
    }

    private string PrepareStructInfo(StructInfo structInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Struct: {structInfo.Name}");
        
        if (structInfo.Modifiers.Count > 0)
        {
            sb.AppendLine($"Modifiers: {string.Join(" ", structInfo.Modifiers)}");
        }
        
        if (structInfo.Attributes.Count > 0)
        {
            sb.AppendLine("Attributes:");
            foreach (var attr in structInfo.Attributes)
            {
                sb.AppendLine($"- {attr.Name}");
                if (attr.Arguments.Count > 0)
                {
                    sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
                }
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Fields:");
        foreach (var field in structInfo.Fields)
        {
            sb.AppendLine($"- {field.Type} {field.Name}");
            if (!string.IsNullOrEmpty(field.Initializer))
            {
                sb.AppendLine($"  Initializer: {field.Initializer}");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Methods:");
        foreach (var method in structInfo.Methods)
        {
            sb.AppendLine($"- {method.ReturnType} {method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})");
        }
        
        return sb.ToString();
    }

    private string PrepareInterfaceInfo(InterfaceInfo interfaceInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Interface: {interfaceInfo.Name}");
        
        if (interfaceInfo.Modifiers.Count > 0)
        {
            sb.AppendLine($"Modifiers: {string.Join(" ", interfaceInfo.Modifiers)}");
        }
        
        if (interfaceInfo.Attributes.Count > 0)
        {
            sb.AppendLine("Attributes:");
            foreach (var attr in interfaceInfo.Attributes)
            {
                sb.AppendLine($"- {attr.Name}");
                if (attr.Arguments.Count > 0)
                {
                    sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
                }
            }
        }
        
        if (interfaceInfo.BaseInterfaces.Count > 0)
        {
            sb.AppendLine($"Implements: {string.Join(", ", interfaceInfo.BaseInterfaces)}");
        }
        
        sb.AppendLine();
        sb.AppendLine("Methods:");
        foreach (var method in interfaceInfo.Methods)
        {
            sb.AppendLine($"- {method.ReturnType} {method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})");
        }
        
        sb.AppendLine();
        sb.AppendLine("Properties:");
        foreach (var prop in interfaceInfo.Properties)
        {
            sb.AppendLine($"- {prop.Type} {prop.Name} {{ {prop.Getter}/{prop.Setter} }}");
        }
        
        return sb.ToString();
    }

    private string PrepareEnumInfo(EnumInfo enumInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Enum: {enumInfo.Name}");
        
        if (enumInfo.Modifiers.Count > 0)
        {
            sb.AppendLine($"Modifiers: {string.Join(" ", enumInfo.Modifiers)}");
        }
        
        if (enumInfo.Attributes.Count > 0)
        {
            sb.AppendLine("Attributes:");
            foreach (var attr in enumInfo.Attributes)
            {
                sb.AppendLine($"- {attr.Name}");
                if (attr.Arguments.Count > 0)
                {
                    sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
                }
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("Members:");
        foreach (var member in enumInfo.Members)
        {
            sb.AppendLine($"- {member.Name} = {member.Value}");
        }
        
        return sb.ToString();
    }
}