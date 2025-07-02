using Microsoft.CodeAnalysis.CSharp;

namespace Domain;

public class ProjectAnalysisResult
{
    public List<FileInfo> FilesInfo { get; set; } = [];
}

public class FileInfo
{
    public string FilePath { get; init; } = null!;
    public string FolderPath => Path.GetDirectoryName(FilePath)!;
    public List<string> Usings { get; init; } = [];
    public List<NamespaceInfo> Namespaces { get; init; } = [];
    public List<ClassInfo> Classes { get; init; } = [];
    public List<StructInfo> Structs { get; init; } = [];
    public List<EnumInfo> Enums { get; init; } = [];
    public List<InterfaceInfo> Interfaces { get; init; } = [];
    public string RawContent { get; init; } = null!;
    public List<AttributeInfo> AssemblyAttributes { get; init; } = [];
}

public class NamespaceInfo
{
    public string Name { get; init; } = null!;
    public List<BaseTypeInfo> Members { get; init; } = [];
}

public class BaseTypeInfo
{
    public string Name { get; init; } = null!;
    public SyntaxKind Kind { get; init; }
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class ClassInfo : BaseTypeInfo
{
    public List<string> Modifiers { get; init; } = [];
    public List<string> BaseTypes { get; init; } = [];
    public List<FieldInfo> Fields { get; init; } = [];
    public List<PropertyInfo> Properties { get; init; } = [];
    public List<MethodInfo> Methods { get; init; } = [];
    public List<ConstructorInfo> Constructors { get; init; } = [];
    public List<EventInfo> Events { get; init; } = [];
}

public class RecordInfo : BaseTypeInfo
{
    public List<string> Modifiers { get; init; } = [];
    
    public List<string> BaseTypes { get; init; } = [];
    
    public List<FieldInfo> Fields { get; init; } = [];
    
    public List<PropertyInfo> Properties { get; init; } = [];
    
    public List<MethodInfo> Methods { get; init; } = [];
    
    public List<ConstructorInfo> Constructors { get; init; } = [];
}

public class StructInfo : BaseTypeInfo
{
    public List<string> Modifiers { get; init; } = [];
    public List<FieldInfo> Fields { get; init; } = [];
    public List<MethodInfo> Methods { get; init; } = [];
}

public class InterfaceInfo : BaseTypeInfo
{
    public List<string> Modifiers { get; init; } = [];
    public List<string> BaseInterfaces { get; init; } = [];
    public List<MethodInfo> Methods { get; init; } = [];
    public List<PropertyInfo> Properties { get; init; } = [];
}

public class EnumInfo : BaseTypeInfo
{
    public List<string> Modifiers { get; init; } = [];
    public List<EnumMemberInfo> Members { get; init; } = [];
}

public class EnumMemberInfo
{
    public string Name { get; init; } = null!;
    public string Value { get; init; } = null!;
}

public class FieldInfo
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public List<string> Modifiers { get; init; } = [];
    public string Initializer { get; init; } = null!;
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class PropertyInfo
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public List<string> Modifiers { get; init; } = [];
    public string Getter { get; init; } = null!;
    public string Setter { get; init; } = null!;
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class MethodInfo
{
    public string Name { get; init; } = null!;
    public string ReturnType { get; init; } = null!;
    public List<string> Modifiers { get; init; } = [];
    public List<ParameterInfo> Parameters { get; init; } = [];
    public string Body { get; set; } = null!;
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class ConstructorInfo
{
    public List<string> Modifiers { get; init; } = [];
    public List<ParameterInfo> Parameters { get; init; } = [];
    public string Body { get; init; } = null!;
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class EventInfo
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public List<string> Modifiers { get; init; } = [];
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class ParameterInfo
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string DefaultValue { get; init; } = null!;
    public List<AttributeInfo> Attributes { get; init; } = [];
}

public class AttributeInfo
{
    public string Name { get; init; } = null!;
    public List<string> Arguments { get; init; } = [];
}