using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using static Monkeymoto.InlineCollections.Diagnostics;

namespace Monkeymoto.InlineCollections
{
    internal readonly partial struct InlineCollectionTypeInfo : IEquatable<InlineCollectionTypeInfo>
    {
        public readonly string CollectionBuilderName = default!;
        public readonly string CollectionBuilderTypeParameterList = default!;
        public readonly ImmutableArray<Diagnostic> Diagnostics = default;
        public readonly string ElementType = default!;
        public readonly string ElementZeroFieldName = default!;
        public readonly string FullName = default!;
        public readonly string FullNameWithContainingTypeNames = default!;
        public readonly bool HasRefStructInterfaceFeature = default!;
        public readonly int Length = default;
        public readonly string LengthPropertyOrValue = default!;
        public readonly string Modifiers = default!;
        public readonly string Name = default!;
        public readonly string Namespace = default!;
        public readonly InlineCollectionOptions Options = default;
        public readonly ImmutableArray<TypeListNode> TypeList = default!;
        public readonly INamedTypeSymbol TypeSymbol = default!;

        public static bool operator ==(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) =>
            left.Equals(right);
        public static bool operator !=(InlineCollectionTypeInfo left, InlineCollectionTypeInfo right) =>
            !(left == right);

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

        private static ImmutableArray<Diagnostic> GetDiagnostics
        (
            in ConstructorArgs args,
            bool hasCollectionBuilder,
            in TypeListNode type
        )
        {
            var diagnostics = new List<Diagnostic>();
            if (hasCollectionBuilder && args.TypeList.Any(static x => !x.IsPublicOrInternal))
            {
                diagnostics.AddDiagnostic(MMIC1000_CollectionBuilderTypeInaccessible_Descriptor, args.Location);
            }
            if (args.Length <= 0)
            {
                diagnostics.AddDiagnostic(MMIC1001_MustHaveValidLength_Descriptor, args.Location);
            }
            if (args.TypeList[0].IsFileLocal)
            {
                diagnostics.AddDiagnostic(MMIC1002_MustNotBeFileLocalType_Descriptor, args.TypeList[0].Location);
            }
            var nonPartialTypes = args.TypeList.Where(x => !x.IsPartial);
            foreach (var nonPartialType in nonPartialTypes)
            {
                diagnostics.AddDiagnostic
                (
                    MMIC1003_MustBePartialType_Descriptor,
                    nonPartialType.Location,
                    nonPartialType.FullName
                );
            }
            if (type.IsReadOnly)
            {
                diagnostics.AddDiagnostic(MMIC1004_MustNotBeReadOnly_Descriptor, args.Location);
            }
            if (args.FieldSymbol is null)
            {
                diagnostics.AddDiagnostic(MMIC1005_MustDefineExactlyOneField_Descriptor, args.Location);
            }
            else if
            (
                args.FieldSymbol.IsRequired ||
                args.FieldSymbol.IsReadOnly ||
                args.FieldSymbol.IsVolatile ||
                args.FieldSymbol.IsFixedSizeBuffer
            )
            {
                diagnostics.AddDiagnostic
                (
                    MMIC1006_InvalidElementFieldModifiers_Descriptor,
                    args.FieldSymbol.Locations.FirstOrDefault()
                );
            }
            return [.. diagnostics];
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

        private static InlineCollectionOptions GetOptions
        (
            InlineCollectionOptions options,
            bool hasRefStructInterfaceFeature
        )
        {
            // combined options are set in the definition (e.g., ICollection has the IEnumerable option)
            // this is a redundancy against user-input patterns like (ICollection ^ IEnumerable)
            if (options.HasFlag(InlineCollectionOptions.IReadOnlyListT))
            {
                options |= InlineCollectionOptions.IEnumerable | InlineCollectionOptions.IEnumerableT |
                    InlineCollectionOptions.IReadOnlyCollectionT;
            }
            else if (options.HasFlag(InlineCollectionOptions.IReadOnlyCollectionT))
            {
                options |= InlineCollectionOptions.IEnumerable | InlineCollectionOptions.IEnumerableT;
            }
            if (options.HasFlag(InlineCollectionOptions.IListT))
            {
                options |= InlineCollectionOptions.ICollectionT | InlineCollectionOptions.IEnumerable |
                    InlineCollectionOptions.IEnumerableT;
            }
            else if (options.HasFlag(InlineCollectionOptions.ICollectionT))
            {
                options |= InlineCollectionOptions.IEnumerable | InlineCollectionOptions.IEnumerableT;
            }
            if (options.HasFlag(InlineCollectionOptions.IList))
            {
                options |= InlineCollectionOptions.ICollection | InlineCollectionOptions.IEnumerable;
            }
            else if (options.HasFlag(InlineCollectionOptions.ICollection))
            {
                options |= InlineCollectionOptions.IEnumerable;
            }
            if (options.HasFlag(InlineCollectionOptions.IEnumerableT))
            {
                options |= InlineCollectionOptions.IEnumerable;
            }
            if (options.HasFlag(InlineCollectionOptions.RefStructEnumerator))
            {
                options |= InlineCollectionOptions.GetEnumeratorMethod;
            }
            if (!hasRefStructInterfaceFeature ||
                (
                    (options &
                    (
                        InlineCollectionOptions.ICollection |
                        InlineCollectionOptions.ICollectionT |
                        InlineCollectionOptions.IList |
                        InlineCollectionOptions.IListT |
                        InlineCollectionOptions.IReadOnlyCollectionT |
                        InlineCollectionOptions.IReadOnlyListT |
                        InlineCollectionOptions.IStructuralComparable |
                        InlineCollectionOptions.IStructuralEquatable
                    )) == 0
                ))
            {
                options &= ~InlineCollectionOptions.CollectionProxyStruct;
            }
            return options;
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
            CancellationToken cancellationToken,
            bool hasRefStructInterfaceFeature
        )
        {
            var typeDeclaration = (StructDeclarationSyntax)context.TargetNode;
            var args = new ConstructorArgs
            {
                HasRefStructInterfaceFeature = hasRefStructInterfaceFeature,
                Location = typeDeclaration.GetLocation(),
                TypeSymbol = (INamedTypeSymbol)context.TargetSymbol.OriginalDefinition
            };
            var inlineCollectionAttributeSymbol = context.SemanticModel.Compilation
                .GetTypeByMetadataName(Source.InlineCollectionAttribute_Name);
            var attributes = args.TypeSymbol.GetAttributes();
            var inlineCollectionAttributeData = attributes
                .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, inlineCollectionAttributeSymbol))
                .SingleOrDefault();
            args.FieldSymbol = args.TypeSymbol.GetMembers().Where(static x => x.Kind == SymbolKind.Field)
                .SingleOrDefault() as IFieldSymbol;
            if (inlineCollectionAttributeData.ConstructorArguments.Any())
            {
                var value = (IConvertible?)inlineCollectionAttributeData.ConstructorArguments[0].Value;
                args.Options = GetOptions
                (
                    (InlineCollectionOptions)(value?.ToInt32(null) ?? (int)InlineCollectionOptions.CollectionBuilder),
                    hasRefStructInterfaceFeature
                );
            }
            args.Length = (int?)inlineCollectionAttributeData.NamedArguments.Where(static x => x.Key == "Length")
                .SingleOrDefault().Value.Value ?? -1;
            args.TypeList = GetTypeList(args.TypeSymbol, cancellationToken);
            return new(in args);
        }

        private InlineCollectionTypeInfo(in ConstructorArgs args)
        {
            var hasCollectionBuilder = args.Options.HasFlag(InlineCollectionOptions.CollectionBuilder);
            var type = args.TypeList.Last();
            Diagnostics = GetDiagnostics(in args, hasCollectionBuilder, in type);
            if (Diagnostics.Any())
            {
                return;
            }
            CollectionBuilderName = hasCollectionBuilder ? GetCollectionBuilderName(in args.TypeList) : string.Empty;
            CollectionBuilderTypeParameterList = hasCollectionBuilder ?
                GetCollectionBuilderTypeParameterList(in args.TypeList) :
                string.Empty;
            ElementType = args.FieldSymbol!.Type.ToDisplayString();
            ElementZeroFieldName = args.FieldSymbol.Name;
            Options = args.Options;
            FullName = type.FullName;
            FullNameWithContainingTypeNames = GetFullNameWithContainingTypeNames(in args.TypeList);
            HasRefStructInterfaceFeature = args.HasRefStructInterfaceFeature;
            Length = args.Length;
            LengthPropertyOrValue = HasOptions(InlineCollectionOptions.LengthProperty) ? "Length" : Length.ToString();
            Modifiers = type.Modifiers;
            Name = args.TypeSymbol.Name;
            Namespace = args.TypeSymbol.ContainingNamespace.ToDisplayString();
            TypeList = args.TypeList;
            TypeSymbol = args.TypeSymbol;
        }

        public override bool Equals(object obj) => obj is InlineCollectionTypeInfo other && Equals(other);
        public bool Equals(InlineCollectionTypeInfo other) =>
            SymbolEqualityComparer.Default.Equals(TypeSymbol, other.TypeSymbol);
        public bool HasOptions(InlineCollectionOptions options) => Options.HasFlag(options);
        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(TypeSymbol);
    }
}
