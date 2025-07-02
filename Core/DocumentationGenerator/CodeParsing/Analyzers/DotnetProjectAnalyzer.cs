using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FileInfo = Domain.FileInfo;

namespace DocumentationGenerator.CodeParsing.Analyzers;

public class DotnetProjectAnalyzer : IProjectAnalyzer
{
    private readonly ILogger<DotnetProjectAnalyzer> _logger;
    
    public DotnetProjectAnalyzer(ILogger<DotnetProjectAnalyzer> logger)
    {
        _logger = logger;
    }
    
    public Result<ProjectAnalysisResult, Error> AnalyzeProject(string projectPath)
    {
        _logger.LogWarning("Logger working!");
        
        var result = new ProjectAnalysisResult();

        if (Directory.GetFiles(projectPath, "*.sln").Length == 0)
        {
            _logger.LogWarning("In folder {path} there is no .sln file", projectPath);
            
            return Errors.NotFound(null, "Solution file");
        }

        var csFiles = GetCSharpFiles(projectPath);
        foreach (var file in csFiles)
        {
            try
            {
                var fileResult = AnalyzeFile(file);
                var folder = fileResult.FolderPath;

                result.FilesInfo.Add(fileResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing file {0}: {1}", file, ex.Message);
                return new Error("file.analyze", $"Error analyzing file {file}: {ex.Message}", ErrorType.Failure);
            }
        }

        return result;
    }
    
    private IEnumerable<string> GetCSharpFiles(string rootPath)
    {
        var excludedDirs = new[] { "bin", "obj", ".git", "packages", "node_modules" };
        return Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(file => !excludedDirs.Any(dir =>
                file.Contains($"{Path.DirectorySeparatorChar}{dir}{Path.DirectorySeparatorChar}")));
    }

    private FileInfo AnalyzeFile(string filePath)
    {
        var code = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var result = new FileInfo
        {
            FilePath = filePath,
            RawContent = code,
            Usings = GetUsings(root),
            AssemblyAttributes = GetAssemblyAttributes(root)
        };

        // обработка обычных namespaces
        var namespaceDeclarations = root.DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .ToList();

        foreach (var ns in namespaceDeclarations)
        {
            result.Namespaces.Add(ParseNamespace(ns));
        }

        // file-scoped namespaces
        var fileScopedNamespace = root.DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (fileScopedNamespace != null)
        {
            var nsInfo = new NamespaceInfo { Name = fileScopedNamespace.Name.ToString() };
        
            foreach (var member in fileScopedNamespace.Members)
            {
                if (member is BaseTypeDeclarationSyntax typeDecl)
                {
                    nsInfo.Members.Add(ParseBaseType(typeDecl));
                }
            }
        
            result.Namespaces.Add(nsInfo);
        }

        // обработка типов в глобальном пространстве имен
        var globalTypes = root.DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .Where(t => t.Parent is CompilationUnitSyntax)
            .ToList();

        foreach (var type in globalTypes)
        {
            AddTypeToResult(result, type);
        }

        return result;
    }

    private List<string> GetUsings(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .ToList();
    }

    private List<AttributeInfo> GetAssemblyAttributes(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<AttributeListSyntax>()
            .Where(al => al.Target.IsKind(SyntaxKind.AssemblyKeyword))
            .SelectMany(al => al.Attributes.Select(ParseAttribute))
            .ToList();
    }

    private NamespaceInfo ParseNamespace(NamespaceDeclarationSyntax ns)
    {
        var nsInfo = new NamespaceInfo { Name = ns.Name.ToString() };

        foreach (var member in ns.Members)
        {
            if (member is BaseTypeDeclarationSyntax typeDecl)
            {
                nsInfo.Members.Add(ParseBaseType(typeDecl));
            }
        }

        return nsInfo;
    }

    private void AddTypeToResult(FileInfo result, SyntaxNode type)
    {
        switch (type)
        {
            case ClassDeclarationSyntax classDecl:
                result.Classes.Add(ParseClass(classDecl));
                break;
            case StructDeclarationSyntax structDecl:
                result.Structs.Add(ParseStruct(structDecl));
                break;
            case InterfaceDeclarationSyntax interfaceDecl:
                result.Interfaces.Add(ParseInterface(interfaceDecl));
                break;
            case EnumDeclarationSyntax enumDecl:
                result.Enums.Add(ParseEnum(enumDecl));
                break;
        }
    }

    private BaseTypeInfo ParseBaseType(BaseTypeDeclarationSyntax typeDecl)
    {
        return typeDecl switch
        {
            ClassDeclarationSyntax classDecl => ParseClass(classDecl),
            StructDeclarationSyntax structDecl => ParseStruct(structDecl),
            InterfaceDeclarationSyntax interfaceDecl => ParseInterface(interfaceDecl),
            EnumDeclarationSyntax enumDecl => ParseEnum(enumDecl),
            RecordDeclarationSyntax recordDecl => ParseRecord(recordDecl),
            _ => new BaseTypeInfo
            {
                Name = typeDecl.Identifier.Text,
                Kind = typeDecl.Kind(),
                Attributes = ParseAttributes(typeDecl)
            }
        };
    }

    private ClassInfo ParseClass(ClassDeclarationSyntax classDecl)
    {
        var classInfo = new ClassInfo
        {
            Name = classDecl.Identifier.Text,
            Kind = classDecl.Kind(),
            Modifiers = classDecl.Modifiers.Select(m => m.Text).ToList(),
            BaseTypes = classDecl.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? new List<string>(),
            Attributes = ParseAttributes(classDecl),
        };

        foreach (var member in classDecl.Members)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    classInfo.Fields.AddRange(ParseFields(field));
                    break;
                case PropertyDeclarationSyntax property:
                    classInfo.Properties.Add(ParseProperty(property));
                    break;
                case MethodDeclarationSyntax method:
                    classInfo.Methods.Add(ParseMethod(method));
                    break;
                case ConstructorDeclarationSyntax ctor:
                    classInfo.Constructors.Add(ParseConstructor(ctor));
                    break;
                case EventDeclarationSyntax @event:
                    classInfo.Events.Add(ParseEvent(@event));
                    break;
            }
        }

        return classInfo;
    }
    
    private RecordInfo ParseRecord(RecordDeclarationSyntax recordDecl)
    {
        var recordInfo = new RecordInfo
        {
            Name = recordDecl.Identifier.Text,
            Kind = recordDecl.Kind(),
            Modifiers = recordDecl.Modifiers.Select(m => m.Text).ToList(),
            BaseTypes = recordDecl.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? new List<string>(),
            Attributes = ParseAttributes(recordDecl),
        };

        foreach (var member in recordDecl.Members)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    recordInfo.Fields.AddRange(ParseFields(field));
                    break;
                case PropertyDeclarationSyntax property:
                    recordInfo.Properties.Add(ParseProperty(property));
                    break;
                case MethodDeclarationSyntax method:
                    recordInfo.Methods.Add(ParseMethod(method));
                    break;
                case ConstructorDeclarationSyntax ctor:
                    recordInfo.Constructors.Add(ParseConstructor(ctor));
                    break;
            }
        }

        return recordInfo;
    }

    private StructInfo ParseStruct(StructDeclarationSyntax structDecl)
    {
        var structInfo = new StructInfo
        {
            Name = structDecl.Identifier.Text,
            Kind = structDecl.Kind(),
            Modifiers = structDecl.Modifiers.Select(m => m.Text).ToList(),
            Attributes = ParseAttributes(structDecl),
        };

        foreach (var member in structDecl.Members)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    structInfo.Fields.AddRange(ParseFields(field));
                    break;
                case MethodDeclarationSyntax method:
                    structInfo.Methods.Add(ParseMethod(method));
                    break;
            }
        }

        return structInfo;
    }

    private InterfaceInfo ParseInterface(InterfaceDeclarationSyntax interfaceDecl)
    {
        var interfaceInfo = new InterfaceInfo
        {
            Name = interfaceDecl.Identifier.Text,
            Kind = interfaceDecl.Kind(),
            Modifiers = interfaceDecl.Modifiers.Select(m => m.Text).ToList(),
            BaseInterfaces = interfaceDecl.BaseList?.Types.Select(t => t.Type.ToString()).ToList() ?? new List<string>(),
            Attributes = ParseAttributes(interfaceDecl),
        };

        foreach (var member in interfaceDecl.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    interfaceInfo.Methods.Add(ParseMethod(method));
                    break;
                case PropertyDeclarationSyntax property:
                    interfaceInfo.Properties.Add(ParseProperty(property));
                    break;
            }
        }

        return interfaceInfo;
    }

    private EnumInfo ParseEnum(EnumDeclarationSyntax enumDecl)
    {
        var enumInfo = new EnumInfo
        {
            Name = enumDecl.Identifier.Text,
            Kind = enumDecl.Kind(),
            Modifiers = enumDecl.Modifiers.Select(m => m.Text).ToList(),
            Attributes = ParseAttributes(enumDecl),
            Members = enumDecl.Members.Select(m => new EnumMemberInfo
            {
                Name = m.Identifier.Text,
                Value = m.EqualsValue?.Value.ToString() ?? string.Empty
            }).ToList()
        };

        return enumInfo;
    }

    private List<FieldInfo> ParseFields(FieldDeclarationSyntax field)
    {
        return field.Declaration.Variables.Select(v => new FieldInfo
        {
            Name = v.Identifier.Text,
            Type = field.Declaration.Type.ToString(),
            Modifiers = field.Modifiers.Select(m => m.Text).ToList(),
            Initializer = v.Initializer?.Value.ToString() ?? string.Empty,
            Attributes = ParseAttributes(field)
        }).ToList();
    }

    private PropertyInfo ParseProperty(PropertyDeclarationSyntax property)
    {
        return new PropertyInfo
        {
            Name = property.Identifier.Text,
            Type = property.Type.ToString(),
            Modifiers = property.Modifiers.Select(m => m.Text).ToList(),
            Getter = property.AccessorList?.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration)?.Body?.ToString() ?? string.Empty,
            Setter = property.AccessorList?.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.SetAccessorDeclaration)?.Body?.ToString() ?? string.Empty,
            Attributes = ParseAttributes(property),
        };
    }

    private MethodInfo ParseMethod(MethodDeclarationSyntax method)
    {
        return new MethodInfo
        {
            Name = method.Identifier.Text,
            ReturnType = method.ReturnType.ToString(),
            Modifiers = method.Modifiers.Select(m => m.Text).ToList(),
            Parameters = ParseParameters(method.ParameterList),
            Body = method.Body?.ToString() ?? method.ExpressionBody?.ToString() ?? string.Empty,
            Attributes = ParseAttributes(method),
        };
    }

    private ConstructorInfo ParseConstructor(ConstructorDeclarationSyntax ctor)
    {
        return new ConstructorInfo
        {
            Modifiers = ctor.Modifiers.Select(m => m.Text).ToList(),
            Parameters = ParseParameters(ctor.ParameterList),
            Body = ctor.Body?.ToString() ?? string.Empty,
            Attributes = ParseAttributes(ctor),
        };
    }

    private EventInfo ParseEvent(EventDeclarationSyntax @event)
    {
        return new EventInfo
        {
            Name = @event.Identifier.Text,
            Type = @event.Type.ToString(),
            Modifiers = @event.Modifiers.Select(m => m.Text).ToList(),
            Attributes = ParseAttributes(@event),
        };
    }

    private List<ParameterInfo> ParseParameters(BaseParameterListSyntax parameterList)
    {
        return parameterList?.Parameters.Select(p => new ParameterInfo
        {
            Name = p.Identifier.Text,
            Type = p.Type?.ToString() ?? string.Empty,
            DefaultValue = p.Default?.Value.ToString() ?? string.Empty,
            Attributes = ParseAttributes(p)
        }).ToList() ?? [];
    }

    private List<AttributeInfo> ParseAttributes(SyntaxNode node)
    {
        return node.DescendantNodes()
            .OfType<AttributeListSyntax>()
            .SelectMany(al => al.Attributes.Select(ParseAttribute))
            .ToList();
    }

    private AttributeInfo ParseAttribute(AttributeSyntax attribute)
    {
        return new AttributeInfo
        {
            Name = attribute.Name.ToString(),
            Arguments = attribute.ArgumentList?.Arguments.Select(a => a.ToString()).ToList() ?? new List<string>()
        };
    }
}