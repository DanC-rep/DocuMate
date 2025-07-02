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

            AppendFileHeader(sb, fileInfo);
            AppendUsings(sb, fileInfo);
            AppendAssemblyAttributes(sb, fileInfo);
            AppendNamespaces(sb, fileInfo);

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while generation prompt for file {file}: {message}", 
                fileInfo.FilePath, ex.Message);
            
            return Error.Failure("generation.prompt", "Error while generation prompt");
        }
    }

    private void AppendFileHeader(StringBuilder sb, FileInfo fileInfo)
    {
        sb.AppendLine($"File: {Path.GetFileName(fileInfo.FilePath)}");
        sb.AppendLine($"Location: {fileInfo.FilePath}");
        sb.AppendLine();
    }

    private void AppendUsings(StringBuilder sb, FileInfo fileInfo)
    {
        if (fileInfo.Usings.Count == 0) return;
        
        sb.AppendLine("Imported namespaces:");
        foreach (var usingDirective in fileInfo.Usings)
            sb.AppendLine($"- {usingDirective}");
        sb.AppendLine();
    }

    private void AppendAssemblyAttributes(StringBuilder sb, FileInfo fileInfo)
    {
        if (fileInfo.AssemblyAttributes.Count == 0) return;
        
        sb.AppendLine("Assembly attributes:");
        foreach (var attr in fileInfo.AssemblyAttributes)
        {
            sb.AppendLine($"- {attr.Name}");
            if (attr.Arguments.Count > 0)
                sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
        }
        sb.AppendLine();
    }

    private void AppendNamespaces(StringBuilder sb, FileInfo fileInfo)
    {
        foreach (var ns in fileInfo.Namespaces)
        {
            sb.AppendLine($"Namespace: {ns.Name}");

            foreach (var member in ns.Members)
            {
                sb.Append(member switch
                {
                    ClassInfo classInfo => PrepareTypeInfo(classInfo, "Class"),
                    RecordInfo recordInfo => PrepareTypeInfo(recordInfo, "Record"),
                    StructInfo structInfo => PrepareTypeInfo(structInfo, "Struct"),
                    InterfaceInfo interfaceInfo => PrepareInterfaceInfo(interfaceInfo),
                    EnumInfo enumInfo => PrepareEnumInfo(enumInfo),
                    _ => string.Empty
                });
                sb.AppendLine();
            }
        }
    }

    private string PrepareTypeInfo(BaseTypeInfo typeInfo, string typeKind)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"{typeKind}: {typeInfo.Name}");
        AppendAttributes(sb, typeInfo.Attributes);

        switch (typeInfo)
        {
            case ClassInfo classInfo:
                AppendModifiers(sb, classInfo.Modifiers);
                AppendBaseTypes(sb, classInfo.BaseTypes, "Inherits");
                AppendFields(sb, classInfo.Fields);
                AppendProperties(sb, classInfo.Properties);
                AppendConstructors(sb, classInfo.Constructors, classInfo.Name);
                AppendMethods(sb, classInfo.Methods);
                AppendEvents(sb, classInfo.Events);
                break;
                
            case RecordInfo recordInfo:
                AppendModifiers(sb, recordInfo.Modifiers);
                AppendBaseTypes(sb, recordInfo.BaseTypes, "Inherits");
                AppendFields(sb, recordInfo.Fields);
                AppendProperties(sb, recordInfo.Properties);
                AppendConstructors(sb, recordInfo.Constructors, recordInfo.Name);
                AppendMethods(sb, recordInfo.Methods);
                break;
                
            case StructInfo structInfo:
                AppendModifiers(sb, structInfo.Modifiers);
                AppendFields(sb, structInfo.Fields);
                AppendMethods(sb, structInfo.Methods);
                break;
        }

        return sb.ToString();
    }

    private string PrepareInterfaceInfo(InterfaceInfo interfaceInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Interface: {interfaceInfo.Name}");
        AppendModifiers(sb, interfaceInfo.Modifiers);
        AppendAttributes(sb, interfaceInfo.Attributes);
        AppendBaseTypes(sb, interfaceInfo.BaseInterfaces, "Implements");
        
        AppendMethods(sb, interfaceInfo.Methods);
        AppendProperties(sb, interfaceInfo.Properties);

        return sb.ToString();
    }

    private string PrepareEnumInfo(EnumInfo enumInfo)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Enum: {enumInfo.Name}");
        AppendModifiers(sb, enumInfo.Modifiers);
        AppendAttributes(sb, enumInfo.Attributes);
        
        sb.AppendLine();
        sb.AppendLine("Members:");
        foreach (var member in enumInfo.Members)
        {
            sb.AppendLine($"- {member.Name} = {member.Value}");
        }

        return sb.ToString();
    }

    private void AppendModifiers(StringBuilder sb, List<string> modifiers)
    {
        if (modifiers.Count > 0)
        {
            sb.AppendLine($"Modifiers: {string.Join(" ", modifiers)}");
        }
    }

    private void AppendAttributes(StringBuilder sb, List<AttributeInfo> attributes)
    {
        if (attributes.Count == 0) return;
        
        sb.AppendLine("Attributes:");
        foreach (var attr in attributes)
        {
            sb.AppendLine($"- {attr.Name}");
            if (attr.Arguments.Count > 0)
            {
                sb.AppendLine($"  Arguments: {string.Join(", ", attr.Arguments)}");
            }
        }
    }

    private void AppendBaseTypes(StringBuilder sb, List<string> baseTypes, string label)
    {
        if (baseTypes.Count > 0)
        {
            sb.AppendLine($"{label}: {string.Join(", ", baseTypes)}");
        }
    }

    private void AppendFields(StringBuilder sb, List<FieldInfo> fields)
    {
        if (fields.Count == 0) return;
        
        sb.AppendLine();
        sb.AppendLine("Fields:");
        foreach (var field in fields)
        {
            sb.AppendLine($"- {field.Type} {field.Name}");
            if (!string.IsNullOrEmpty(field.Initializer))
            {
                sb.AppendLine($"  Initializer: {field.Initializer}");
            }
        }
    }

    private void AppendProperties(StringBuilder sb, List<PropertyInfo> properties)
    {
        if (properties.Count == 0) return;
        
        sb.AppendLine();
        sb.AppendLine("Properties:");
        foreach (var prop in properties)
        {
            sb.AppendLine($"- {prop.Type} {prop.Name} {{ {prop.Getter}/{prop.Setter} }}");
        }
    }

    private void AppendConstructors(StringBuilder sb, List<ConstructorInfo> constructors, string typeName)
    {
        if (constructors.Count == 0) return;
        
        sb.AppendLine();
        sb.AppendLine("Constructors:");
        foreach (var ctor in constructors)
        {
            sb.AppendLine($"- {typeName}({string.Join(", ", ctor.Parameters.Select(p => $"{p.Type} {p.Name}"))})");
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
    }

    private void AppendMethods(StringBuilder sb, List<MethodInfo> methods)
    {
        if (methods.Count == 0) return;
        
        sb.AppendLine();
        sb.AppendLine("Methods:");
        foreach (var method in methods)
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
    }

    private void AppendEvents(StringBuilder sb, List<EventInfo> events)
    {
        if (events.Count == 0) return;
        
        sb.AppendLine();
        sb.AppendLine("Events:");
        foreach (var evt in events)
        {
            sb.AppendLine($"- {evt.Type} {evt.Name}");
        }
    }
}