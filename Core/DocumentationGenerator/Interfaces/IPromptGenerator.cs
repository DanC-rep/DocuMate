using CSharpFunctionalExtensions;
using Domain.Errors;
using FileInfo = Domain.FileInfo;

namespace DocumentationGenerator.Interfaces;

public interface IPromptGenerator
{
    public Result<string, Error> PreparePrompt(FileInfo fileInfo);
}