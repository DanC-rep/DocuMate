using System.Text;
using DocumentationGenerator.Interfaces;

namespace DocumentationGenerator.DocumentationTemplates;

public class DotnetDocumentationTemplate : IDocumentationTemplate
{
    public string GetTemplate()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("You need to generate documentation for several C# files in .md format. Follow the template exactly and don't add anything extra. " +
                      "Don`t forgive to add description for each method." +
                      "If you can`t fill some points, just skip it. I want your answer to start with \"# File Overview\":");
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
        sb.AppendLine("   For each class/struct/interface/enum/record:");
        sb.AppendLine("   - Name: [name]");
        sb.AppendLine("   - Type: [class/struct/interface/enum/record]");
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
        sb.AppendLine("   - Please provide typical usage scenarios, code examples if applicable");
        sb.AppendLine();
        sb.AppendLine("Now analyzing the following code:");
        sb.AppendLine("==================================");
        sb.AppendLine();

        return sb.ToString();
    }
}