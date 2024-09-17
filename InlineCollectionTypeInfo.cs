using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Monkeymoto.InlineCollections
{
    internal readonly struct InlineCollectionTypeInfo : IEquatable<InlineCollectionTypeInfo>
    {
        public readonly string AccessModifier = default!;
        public readonly string CollectionBuilderName = default!;
        public readonly ImmutableArray<Diagnostic> Diagnostics = default;
        public readonly string ElementType = default!;
        public readonly string ElementZeroFieldName = default!;
        public readonly InlineCollectionFlags Flags = default;
        public readonly string FullName = default!;
        public readonly int Length = default;
        public readonly string LengthPropertyOrValue = default!;
        public readonly string Name = default!;
        public readonly string Namespace = default!;
        public readonly string TypeParameterList = default!;
        public readonly INamedTypeSymbol TypeSymbol = default!;

        private struct ConstructorArgs
        {
            public IFieldSymbol? FieldSymbol;
            public InlineCollectionFlags Flags;
            public bool IsPartial;
            public bool IsPublic;
            public int Length;
            public Location Location;
            public INamedTypeSymbol TypeSymbol;
        }

        public static bool operator ==(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) => left.Equals(right);
        public static bool operator !=(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) => !(left == right);

        private static InlineCollectionFlags GetFlags(InlineCollectionFlags flags)
        {
            // combined flags are set in the definition (e.g., ICollection has the IEnumerable flag)
            // this is a redundancy against user-input patterns like (ICollection ^ IEnumerable)
            if (flags.HasFlag(InlineCollectionFlags.IReadOnlyListT))
            {
                flags |= InlineCollectionFlags.IEnumerable | InlineCollectionFlags.IEnumerableT |
                    InlineCollectionFlags.IReadOnlyCollectionT;
            }
            else if (flags.HasFlag(InlineCollectionFlags.IReadOnlyCollectionT))
            {
                flags |= InlineCollectionFlags.IEnumerable | InlineCollectionFlags.IEnumerableT;
            }
            if (flags.HasFlag(InlineCollectionFlags.IListT))
            {
                flags |= InlineCollectionFlags.ICollectionT | InlineCollectionFlags.IEnumerable |
                    InlineCollectionFlags.IEnumerableT;
            }
            else if (flags.HasFlag(InlineCollectionFlags.ICollectionT))
            {
                flags |= InlineCollectionFlags.IEnumerable | InlineCollectionFlags.IEnumerableT;
            }
            if (flags.HasFlag(InlineCollectionFlags.IList))
            {
                flags |= InlineCollectionFlags.ICollection | InlineCollectionFlags.IEnumerable;
            }
            else if (flags.HasFlag(InlineCollectionFlags.ICollection))
            {
                flags |= InlineCollectionFlags.IEnumerable;
            }
            if (flags.HasFlag(InlineCollectionFlags.IEnumerableT))
            {
                flags |= InlineCollectionFlags.IEnumerable;
            }
            return flags;
        }

        public static InlineCollectionTypeInfo GetTypeInfo(GeneratorAttributeSyntaxContext context, CancellationToken _)
        {
            var typeDeclaration = (StructDeclarationSyntax)context.TargetNode;
            var args = new ConstructorArgs
            {
                IsPartial = typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword)),
                IsPublic = typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PublicKeyword)),
                Location = typeDeclaration.GetLocation(),
                TypeSymbol = (INamedTypeSymbol)context.TargetSymbol.OriginalDefinition
            };
            var inlineCollectionAttributeSymbol = context.SemanticModel.Compilation
                .GetTypeByMetadataName(Source.InlineCollectionAttribute_Name);
            var attributes = args.TypeSymbol.GetAttributes();
            var inlineCollectionAttributeData = attributes
                .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, inlineCollectionAttributeSymbol))
                .SingleOrDefault();
            args.FieldSymbol = args.TypeSymbol.GetMembers().Where(static x => x.Kind == SymbolKind.Field).SingleOrDefault()
                as IFieldSymbol;
            if (inlineCollectionAttributeData.ConstructorArguments.Any())
            {
                var value = (IConvertible?)inlineCollectionAttributeData.ConstructorArguments[0].Value;
                args.Flags = (InlineCollectionFlags)(value?.ToInt32(null) ?? (int)InlineCollectionFlags.CollectionBuilder);
            }
            args.Length = (int?)inlineCollectionAttributeData.NamedArguments.Where(static x => x.Key == "Length")
                .SingleOrDefault().Value.Value ?? -1;
            return new(in args);
        }

        private InlineCollectionTypeInfo(in ConstructorArgs args)
        {
            var diagnostics = new List<Diagnostic>();
            if (args.TypeSymbol.ContainingType is not null)
            {
                diagnostics.Add
                (
                    Diagnostic.Create
                    (
                        InlineCollections.Diagnostics.MMIC1000_MustBeTopLevelType_Descriptor,
                        args.Location
                    )
                );
            }
            if (args.Length <= 0)
            {
                diagnostics.Add
                (
                    Diagnostic.Create
                    (
                        InlineCollections.Diagnostics.MMIC1001_MustHaveValidLength_Descriptor,
                        args.Location
                    )
                );
            }
            if (!args.IsPartial)
            {
                diagnostics.Add
                (
                    Diagnostic.Create
                    (
                        InlineCollections.Diagnostics.MMIC1002_MustBePartialStruct_Descriptor,
                        args.Location
                    )
                );
            }
            if (args.FieldSymbol is null)
            {
                diagnostics.Add
                (
                    Diagnostic.Create
                    (
                        InlineCollections.Diagnostics.MMIC1003_MustDefineExactlyOneField_Descriptor,
                        args.Location
                    )
                );
            }
            if (diagnostics.Any())
            {
                Diagnostics = [.. diagnostics];
                return;
            }
            int arity = args.TypeSymbol.Arity;
            AccessModifier = args.IsPublic ? "public" : "internal";
            CollectionBuilderName = $"{args.TypeSymbol.Name}_{(arity != 0 ? $"{arity}_" : "")}CollectionBuilder";
            Diagnostics = [];
            ElementType = args.FieldSymbol!.Type.ToDisplayString();
            ElementZeroFieldName = args.FieldSymbol.Name;
            Flags = GetFlags(args.Flags);
            FullName = args.TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            Length = args.Length;
            LengthPropertyOrValue = Flags.HasFlag(InlineCollectionFlags.LengthProperty) ? "Length" : Length.ToString();
            Name = args.TypeSymbol.Name;
            Namespace = args.TypeSymbol.ContainingNamespace.ToDisplayString();
            TypeParameterList = arity != 0 ? FullName.Substring(FullName.IndexOf('<')) : string.Empty;
            TypeSymbol = args.TypeSymbol;
        }

        public override bool Equals(object obj) => obj is InlineCollectionTypeInfo other && Equals(other);
        public bool Equals(InlineCollectionTypeInfo other) =>
            SymbolEqualityComparer.Default.Equals(TypeSymbol, other.TypeSymbol);
        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(TypeSymbol);
    }
}
