using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Threading;
using System;
using System.Linq;

namespace Monkeymoto.InlineCollections
{
    internal readonly partial struct InlineCollectionTypeInfo
    {
        public readonly struct TypeListNode
        {
            public readonly string DeclarationKind = default!;
            public readonly string FullName = default!;
            public readonly bool IsFileLocal = default;
            public readonly bool IsPartial = default;
            public readonly bool IsPublicOrInternal = default;
            public readonly bool IsReadOnly = default;
            public readonly Location Location = default!;
            public readonly string Modifiers = default!;
            public readonly string Name = default!;
            public readonly INamedTypeSymbol TypeSymbol = default!;

            public TypeListNode
            (
                INamedTypeSymbol typeSymbol,
                CancellationToken cancellationToken,
                TypeDeclarationSyntax? typeDeclaration = null
            )
            {
                typeSymbol = typeSymbol.OriginalDefinition;
                typeDeclaration ??= (TypeDeclarationSyntax)typeSymbol.DeclaringSyntaxReferences[0]
                    .GetSyntax(cancellationToken);
                var firstModifierKind = typeDeclaration.Modifiers.FirstOrDefault().Kind();
                DeclarationKind = typeDeclaration switch
                {
                    ClassDeclarationSyntax => "class",
                    InterfaceDeclarationSyntax => "interface",
                    StructDeclarationSyntax => "struct",
                    RecordDeclarationSyntax recordDeclaration =>
                        recordDeclaration.Kind() switch
                        {
                            SyntaxKind.RecordStructDeclaration => "record struct",
                            _ => "record class"
                        },
                    _ => throw new NotSupportedException
                        (
                            $"Unsupported TypeDeclarationSyntax kind: {typeDeclaration.Kind()}"
                        )
                };
                FullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                IsFileLocal = firstModifierKind == SyntaxKind.FileKeyword;
                IsPartial = typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword));
                IsPublicOrInternal = firstModifierKind switch
                {
                    SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword => true,
                    _ => false
                };
                IsReadOnly = typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.ReadOnlyKeyword));
                Location = typeDeclaration.GetLocation();
                Modifiers = typeDeclaration.Modifiers.ToString();
                Name = typeSymbol.Name;
                TypeSymbol = typeSymbol;
            }
        }
    }
}
