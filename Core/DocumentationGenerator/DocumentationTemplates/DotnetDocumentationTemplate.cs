using System.Text;
using DocumentationGenerator.Interfaces;

namespace DocumentationGenerator.DocumentationTemplates;

public class DotnetDocumentationTemplate : IDocumentationTemplate
{
    public string GetTemplate()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("You need to generate documentation for several C# files in .md format. Follow this template:");
        sb.AppendLine();
        sb.AppendLine("1. File Overview");
        sb.AppendLine("   - File Name: [filename]");
        sb.AppendLine("   - Location: [path]");
        sb.AppendLine("   - Purpose: [brief description of file's purpose]");
        sb.AppendLine();
        sb.AppendLine("2. Dependencies");
        sb.AppendLine("   - Namespaces: [list of used namespaces]");
        sb.AppendLine("   - Assembly Attributes: [if any]");
        sb.AppendLine();
        sb.AppendLine("3. Code Structure");
        sb.AppendLine("   For each class/struct/interface/enum:");
        sb.AppendLine("   - Name: [name]");
        sb.AppendLine("   - Type: [class/struct/interface/enum]");
        sb.AppendLine("   - Modifiers: [public/private/etc]");
        sb.AppendLine("   - Inheritance: [base types/interfaces]");
        sb.AppendLine("   - Description: [detailed description of purpose and functionality]");
        sb.AppendLine();
        sb.AppendLine("4. Members");
        sb.AppendLine("   - Fields: [name, type, description]");
        sb.AppendLine("   - Properties: [name, type, get/set accessors, description]");
        sb.AppendLine("   - Methods: [name, parameters, return type, description]");
        sb.AppendLine("   - Events: [name, type, description]");
        sb.AppendLine();
        sb.AppendLine("5. Usage Examples");
        sb.AppendLine("   - Provide typical usage scenarios");
        sb.AppendLine("   - Code examples if applicable");
        sb.AppendLine();
        sb.AppendLine("Now analyzing the following code:");
        sb.AppendLine("==================================");
        sb.AppendLine();

        return sb.ToString();
    }
}