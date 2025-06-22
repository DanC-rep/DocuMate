using CSharpFunctionalExtensions;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IProjectAnalyzer
{
    Result<ProjectAnalysisResult, Error> AnalyzeProject(string filePath);
}