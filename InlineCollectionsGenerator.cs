using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Monkeymoto.InlineCollections.Diagnostics;
using GeneratedSourceInfoArgs =
(
    System.Collections.Immutable.ImmutableArray<Monkeymoto.InlineCollections.InlineCollectionTypeInfo>,
    System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Operations.ICollectionExpressionOperation?>
);

namespace Monkeymoto.InlineCollections
{
    [Generator]
    internal sealed class InlineCollectionsGenerator : IIncrementalGenerator
    {
        private static ICollectionExpressionOperation? GetCollectionExpressionOperation
        (
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken
        )
        {
            var collectionExpression = context.SemanticModel.GetOperation(context.Node, cancellationToken)
                as ICollectionExpressionOperation;
            if (collectionExpression?.SemanticModel is null ||
                collectionExpression.Parent is not IConversionOperation parent ||
                parent.Type is not INamedTypeSymbol symbol)
            {
                return null;
            }
            var attributes = symbol.GetAttributes().Select(static x => x.AttributeClass);
            if (!attributes.Any())
            {
                return null;
            }
            var inlineCollectionAttributeSymbol = collectionExpression.SemanticModel.Compilation
                .GetTypeByMetadataName(Source.InlineCollectionAttribute_Name);
            return attributes.Where(x => SymbolEqualityComparer.Default.Equals(x, inlineCollectionAttributeSymbol))
                .Any() ? collectionExpression : null;
        }

        private static GeneratedSourceInfo GetGeneratedSourceInfo
        (
            GeneratedSourceInfoArgs args,
            CancellationToken cancellationToken
        )
        {
            (var inlineCollectionTypes, var collectionExpressions) = args;
            var diagnostics = new List<Diagnostic>();
            var sb = new StringBuilder(Source.GetFileHeader());
            var inlineCollectionLengths = new Dictionary<INamedTypeSymbol, int>(SymbolEqualityComparer.Default);
            foreach (var typeInfo in inlineCollectionTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (typeInfo.Diagnostics.Any())
                {
                    diagnostics.AddRange(typeInfo.Diagnostics);
                    continue;
                }
                _ = sb.AppendLine().Append(Source.GenerateForType(in typeInfo));
                inlineCollectionLengths[typeInfo.TypeSymbol] = typeInfo.Length;
            }
            _ = sb.AppendLine(Source.GetFileFooter());
            foreach (var collectionExpression in collectionExpressions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var collectionType = (INamedTypeSymbol)collectionExpression!.Parent!.Type!.OriginalDefinition;
                if (!inlineCollectionLengths.TryGetValue(collectionType, out int collectionLength))
                {
                    continue;
                }
                if (collectionExpression.Elements.Length > collectionLength)
                {
                    int knownExpressionLength = collectionExpression.Elements.Where(x => x is not ISpreadOperation).Count();
                    diagnostics.AddDiagnostic
                    (
                        knownExpressionLength > collectionLength ?
                            MMIC1007_CollectionExpressionTooLarge_Descriptor :
                            MMIC1008_CollectionExpressionMaybeTooLarge_Descriptor,
                        collectionExpression.Syntax.GetLocation(),
                        collectionLength
                    );
                }
            }
            return new GeneratedSourceInfo([.. diagnostics], sb.ToString());
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static context =>
            {
                context.AddSource(Source.InlineCollectionAttribute_FileName, Source.GetInlineCollectionAttributeSource());
                context.AddSource(Source.InlineCollectionFlags_FileName, Source.GetInlineCollectionFlagsSource());
            });
            var inlineCollectionTypes = context.SyntaxProvider.ForAttributeWithMetadataName
            (
                Source.InlineCollectionAttribute_Name,
                static (_, _) => true,
                InlineCollectionTypeInfo.GetTypeInfo
            ).Collect();
            var collectionExpressions = context.SyntaxProvider.CreateSyntaxProvider
            (
                static (node, _) => node is CollectionExpressionSyntax,
                GetCollectionExpressionOperation
            ).Where(static x => x is not null).Collect();
            var generatedSourceInfo = inlineCollectionTypes.Combine(collectionExpressions).Select(GetGeneratedSourceInfo);
            context.RegisterImplementationSourceOutput
            (
                generatedSourceInfo,
                static (context, generatedSourceInfo) =>
                {
                    foreach (var diagnostic in generatedSourceInfo.Diagnostics)
                    {
                        context.ReportDiagnostic(diagnostic);
                    }
                    context.AddSource(Source.FileName, generatedSourceInfo.SourceText);
                }
            );
        }
    }
}
