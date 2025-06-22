using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.CodeParsing;

public class ProjectAnalyzer(IProjectAnalyzer projectAnalyzer)
{
    public Result<ProjectAnalysisResult, Error> AnalyzeProject(string projectPath) =>
        projectAnalyzer.AnalyzeProject(projectPath);
}