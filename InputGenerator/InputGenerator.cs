using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace InputGenerator;

[Generator]
public class InputGenerator : IIncrementalGenerator
{
    private const string AttributeName = "GenerateMapperInputAttribute";
    private const string InputNameSpace = "InputCodeGenerator.Inputs";
    private const string EntityNameSpace = "InputCodeGenerator.Entities";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidateClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateForGeneration(node),
                transform: static (context, _) => GetSemanticTargetForGeneration(context))
            .Where(static t => t is not null);

        context.RegisterSourceOutput(candidateClasses, static (sourceProductionContext, typeSymbol) =>
        {
            if (typeSymbol is null)
            {
                return;
            }

            var attribute = typeSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.Name == AttributeName);

            var targetType = attribute?.AttributeClass?.TypeArguments.FirstOrDefault();

            if (targetType is not INamedTypeSymbol)
            {
                return;
            }

            var sourceRecordName = typeSymbol.Name;
            var targetEntityName = targetType.Name;
            
            var sourcePropertyNames = typeSymbol.GetMembers().OfType<IPropertySymbol>().Select(p => p.Name).ToArray();
            var targetPropertyNames = targetType.GetMembers().OfType<IPropertySymbol>().Select(p => p.Name).ToArray();
            
            var commonProperties = sourcePropertyNames.Intersect(targetPropertyNames).ToArray();

            var generatedCode = GenerateMappingMethod(sourceRecordName, targetEntityName, commonProperties);

            sourceProductionContext.AddSource($"{sourceRecordName}_Mapper.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    private static bool IsCandidateForGeneration(SyntaxNode syntaxNode) => syntaxNode is RecordDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ITypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        if (context.Node is not RecordDeclarationSyntax recordDeclarationSyntax)
        {
            return null;
        }

        foreach (var attributeListSyntax in recordDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);

                if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                if (attributeSymbol.ContainingType.Name == AttributeName)
                {
                    return context.SemanticModel.GetDeclaredSymbol(recordDeclarationSyntax) as ITypeSymbol;
                }
            }
        }

        return null;
    }

    private static string GenerateMappingMethod(string sourceClass, string targetClass, string[] commonProperties)
    {
        var properties = string.Join(",\n", commonProperties.Select(p => $"{p} = {p}"));
        
        return $$"""
                 using {{EntityNameSpace}};
                 
                 namespace {{InputNameSpace}};
                 
                 public sealed partial record {{sourceClass}}
                 {
                    public {{targetClass}} To{{targetClass}}()
                    {
                        return new {{targetClass}}
                        {
                            {{properties}}
                        };
                    }
                 }
                 """;
    }
}