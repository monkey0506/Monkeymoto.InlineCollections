using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Monkeymoto.InlineCollections
{
    internal readonly struct InlineCollectionTypeInfo : IEquatable<InlineCollectionTypeInfo>
    {
        public readonly string CollectionBuilderName = default!;
        public readonly string CollectionBuilderTypeParameterList = default!;
        public readonly ImmutableArray<Diagnostic> Diagnostics = default;
        public readonly string ElementType = default!;
        public readonly string ElementZeroFieldName = default!;
        public readonly InlineCollectionFlags Flags = default;
        public readonly string FullName = default!;
        public readonly string FullNameWithContainingTypeNames = default!;
        public readonly int Length = default;
        public readonly string LengthPropertyOrValue = default!;
        public readonly string Modifiers = default!;
        public readonly string Name = default!;
        public readonly string Namespace = default!;
        public readonly ImmutableArray<TypeListNode> TypeList = default!;
        public readonly INamedTypeSymbol TypeSymbol = default!;

        private struct ConstructorArgs
        {
            public IFieldSymbol? FieldSymbol;
            public InlineCollectionFlags Flags;
            public int Length;
            public Location Location;
            public ImmutableArray<TypeListNode> TypeList;
            public INamedTypeSymbol TypeSymbol;
        }

        public readonly struct TypeListNode
        {
            public readonly string DeclarationKind = default!;
            public readonly string FullName = default!;
            public readonly bool IsPartial = default;
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
                    _ => throw new NotSupportedException($"Unsupported TypeDeclarationSyntax kind: {typeDeclaration.Kind()}")
                };
                FullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                IsPartial = typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword));
                Location = typeDeclaration.GetLocation();
                Modifiers = typeDeclaration.Modifiers.ToString();
                Name = typeSymbol.Name;
                TypeSymbol = typeSymbol;
            }
        }

        public static bool operator ==(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) => left.Equals(right);
        public static bool operator !=(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) => !(left == right);

        private static string GetCollectionBuilderName(in ImmutableArray<TypeListNode> typeList)
        {
            var sb = new StringBuilder();
            foreach (var node in typeList)
            {
                if (sb.Length != 0)
                {
                    _ = sb.Append('_');
                }
                _ = sb.Append(node.Name);
                if (node.TypeSymbol.Arity != 0)
                {
                    _ = sb.Append($"_{node.TypeSymbol.Arity}");
                }
            }
            return sb.Append("_CollectionBuilder").ToString();
        }

        private static string GetCollectionBuilderTypeParameterList(in ImmutableArray<TypeListNode> typeList)
        {
            var sb = new StringBuilder();
            foreach (var node in typeList)
            {
                if (node.TypeSymbol.Arity == 0)
                {
                    continue;
                }
                if (sb.Length == 0)
                {
                    _ = sb.Append('<');
                }
                foreach (var typeParameter in node.TypeSymbol.TypeParameters)
                {
                    if (sb[sb.Length - 1] != '<')
                    {
                        _ = sb.Append(", ");
                    }
                    _ = sb.Append(typeParameter.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                }
            }
            if (sb.Length != 0)
            {
                _ = sb.Append('>');
            }
            return sb.ToString();
        }

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

        private static string GetFullNameWithContainingTypeNames(in ImmutableArray<TypeListNode> typeList)
        {
            var sb = new StringBuilder();
            foreach (var node in typeList)
            {
                if (sb.Length != 0)
                {
                    _ = sb.Append('.');
                }
                _ = sb.Append(node.FullName);
            }
            return sb.ToString();
        }

        private static ImmutableArray<TypeListNode> GetTypeList
        (
            INamedTypeSymbol targetTypeSymbol,
            CancellationToken cancellationToken
        )
        {
            var typeList = new LinkedList<TypeListNode>();
            for
            (
                INamedTypeSymbol? typeSymbol = targetTypeSymbol;
                typeSymbol is not null;
                typeSymbol = typeSymbol.ContainingType
            )
            {
                typeList.AddFirst(new TypeListNode(typeSymbol, cancellationToken));
            }
            return [.. typeList];
        }

        public static InlineCollectionTypeInfo GetTypeInfo
        (
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken
        )
        {
            var typeDeclaration = (StructDeclarationSyntax)context.TargetNode;
            var args = new ConstructorArgs
            {
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
                args.Flags = GetFlags
                (
                    (InlineCollectionFlags)(value?.ToInt32(null) ?? (int)InlineCollectionFlags.CollectionBuilder)
                );
            }
            args.Length = (int?)inlineCollectionAttributeData.NamedArguments.Where(static x => x.Key == "Length")
                .SingleOrDefault().Value.Value ?? -1;
            args.TypeList = GetTypeList(args.TypeSymbol, cancellationToken);
            return new(in args);
        }

        private InlineCollectionTypeInfo(in ConstructorArgs args)
        {
            var diagnostics = new List<Diagnostic>();
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
            var nonPartialTypes = args.TypeList.Where(x => !x.IsPartial);
            foreach (var nonPartialType in nonPartialTypes)
            {
                diagnostics.Add
                (
                    Diagnostic.Create
                    (
                        InlineCollections.Diagnostics.MMIC1002_MustBePartialType_Descriptor,
                        nonPartialType.Location,
                        nonPartialType.FullName
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
            var type = args.TypeList.Last();
            var hasCollectionBuilder = args.Flags.HasFlag(InlineCollectionFlags.CollectionBuilder);
            CollectionBuilderName = hasCollectionBuilder ? GetCollectionBuilderName(in args.TypeList) : string.Empty;
            CollectionBuilderTypeParameterList = hasCollectionBuilder ?
                GetCollectionBuilderTypeParameterList(in args.TypeList) :
                string.Empty;
            Diagnostics = [];
            ElementType = args.FieldSymbol!.Type.ToDisplayString();
            ElementZeroFieldName = args.FieldSymbol.Name;
            Flags = args.Flags;
            FullName = type.FullName;
            FullNameWithContainingTypeNames = GetFullNameWithContainingTypeNames(in args.TypeList);
            Length = args.Length;
            LengthPropertyOrValue = Flags.HasFlag(InlineCollectionFlags.LengthProperty) ? "Length" : Length.ToString();
            Modifiers = type.Modifiers;
            Name = args.TypeSymbol.Name;
            Namespace = args.TypeSymbol.ContainingNamespace.ToDisplayString();
            TypeList = args.TypeList;
            TypeSymbol = args.TypeSymbol;
        }

        public override bool Equals(object obj) => obj is InlineCollectionTypeInfo other && Equals(other);
        public bool Equals(InlineCollectionTypeInfo other) =>
            SymbolEqualityComparer.Default.Equals(TypeSymbol, other.TypeSymbol);
        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(TypeSymbol);
    }
}
