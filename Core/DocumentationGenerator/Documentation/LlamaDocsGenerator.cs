using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;
using Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace DocumentationGenerator.Documentation;

public class LlamaDocsGenerator : IDocumentationGenerator
{
    private readonly ILogger<LlamaDocsGenerator> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPromptGenerator _promptGenerator;
    private readonly IDocumentationTemplate _documentationTemplate;

    public LlamaDocsGenerator(
        ILogger<LlamaDocsGenerator> logger,
        IConfiguration configuration,
        IPromptGenerator promptGenerator,
        IDocumentationTemplate documentationTemplate)
    {
        _logger = logger;
        _configuration = configuration;
        _promptGenerator = promptGenerator;
        _documentationTemplate = documentationTemplate;
    }
    
    public async Task<Result<List<string>, Error>> GenerateDocumentation(ProjectAnalysisResult projectInfo)
    {
        var client = InitClient();
        var template = _documentationTemplate.GetTemplate();

        var templateResult = await SendDocumentationTemplate(client, template);
        
        if (templateResult.IsFailure)
            return templateResult.Error;

        List<string> documentationFiles = [];

        foreach (var folder in projectInfo.FilesByFolder)
        {
            foreach (var fileInfo in folder.Value)
            {
                var promptResult = _promptGenerator.PreparePrompt(fileInfo);

                if (promptResult.IsFailure)
                    continue;
                
                var documentation = await GenerateFileDocumentation(client, promptResult.Value);
                
                if (documentation.IsFailure)
                    continue;
                
                documentationFiles.Add(documentation.Value);
            }
        }

        return documentationFiles;
    }
    
    private Chat InitClient()
    {
        var uri = new Uri(_configuration.GetConnectionString(ConnectionStrings.Llama)!);

        var client = new OllamaApiClient(uri)
        {
            SelectedModel = "llama3"
        };

        return new Chat(client);
    }

    private async Task<UnitResult<Error>> SendDocumentationTemplate(Chat client, string template)
    {
        try
        {
            await foreach (var answerToken in client.SendAsync(template))
               continue;

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while preparing template for docs: {message}", ex.Message);
            
            return Error.Failure("send.docs.template", "Error while sending docs template");
        }
    }
    
    private async Task<Result<string, Error>> GenerateFileDocumentation(Chat client, string prompt)
    {
        try
        {
            List<string> response = [];

            await foreach (var answerToken in client.SendAsync(prompt))
                response.Add(answerToken);

            return string.Join("", response);
        }
        catch (Exception ex)
        {  
            _logger.LogError("Error while generating documentation: {message}", ex.Message);

            return Error.Failure("generation.documentation", "Error while generation documentation");
        }
    }

    
}