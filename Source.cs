﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static Monkeymoto.InlineCollections.ExceptionMessages;

namespace Monkeymoto.InlineCollections
{
    internal static partial class Source
    {
        private static readonly string ExceptionMessages_SourceText;
        public const string FileName = "Monkeymoto.InlineCollections.GeneratedCollections.g.cs";
        public static readonly string InlineCollectionAttribute_FileName = $"{InlineCollectionAttribute_Name}.g.cs";
        public const string InlineCollectionAttribute_Name = "Monkeymoto.InlineCollections.InlineCollectionAttribute";
        public const string InlineCollectionOptions_FileName =
            "Monkeymoto.InlineCollections.InlineCollectionOptions.g.cs";
        private static readonly string InlineCollectionOptions_SourceText;

        static Source()
        {
            var assembly = typeof(InlineCollectionOptions).Assembly;
            var resources = assembly.GetManifestResourceNames();
            ExceptionMessages_SourceText = GetExceptionMessagesSourceFromAssembly(assembly, resources);
            InlineCollectionOptions_SourceText = GetInlineCollectionOptionsSourceFromAssembly(assembly, resources);
        }

        private static StringBuilder AppendIf(this StringBuilder sb, bool condition, string stringToAppend) =>
            condition ? sb.Append(stringToAppend) : sb;

        private static StringBuilder AppendInterface(this StringBuilder sb, string interfaceToAppend) => sb
            .AppendIf(sb.Length != 0, ", ")
            .Append(interfaceToAppend);

        private static StringBuilder AppendInterfaceIf
        (
            this StringBuilder sb,
            bool condition,
            string interfaceToAppend
        ) => condition ? sb.AppendInterface(interfaceToAppend) : sb;

        private static string FormatIf(bool condition, string template, params string[] formatArgs) =>
            condition ? string.Format(template, formatArgs) : string.Empty;

        public static string GenerateForType(in InlineCollectionTypeInfo typeInfo) => string.Format
        (
            Template_InlineCollection,
            typeInfo.Namespace,                                     // 0
            GetTypeDeclarationsSource(in typeInfo),                 // 1
            GetCollectionBuilderClassDeclarationSource(in typeInfo) // 2
        );

        private static string GetArrayConversionOperatorsSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ArrayConversionOperators),
            Template_ArrayConversionOperators,
            typeInfo.ElementType, // 0
            typeInfo.FullName     // 1
        );

        private static string GetAsSpanReadOnlySpanMethodsSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.AsSpanReadOnlySpanMethods),
            Template_AsSpanReadOnlySpanMethods,
            "public ",                     // 0
            typeInfo.ElementType,          // 1
            string.Empty,                  // 2
            typeInfo.ElementZeroFieldName, // 3
            typeInfo.LengthPropertyOrValue // 4
        );

        private static string GetClearMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ClearMethod),
            Template_ClearMethod
        );

        private static string GetCollectionBuilderAttributeSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.CollectionBuilder),
            Template_CollectionBuilderAttribute,
            typeInfo.CollectionBuilderName // 0
        );

        private static string GetCollectionBuilderClassDeclarationSource
        (
            in InlineCollectionTypeInfo typeInfo
        ) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.CollectionBuilder),
            Template_CollectionBuilderClassDeclaration,
            typeInfo.CollectionBuilderName,              // 0
            typeInfo.FullNameWithContainingTypeNames,    // 1
            typeInfo.CollectionBuilderTypeParameterList, // 2
            typeInfo.ElementType                         // 3
        );

        private static string GetCollectionProxyStructSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.Options.HasFlag(InlineCollectionOptions.CollectionProxyStruct),
            Template_CollectionProxyStruct,
            typeInfo.FullName,                              // 0
            GetInterfaceList(in typeInfo, true),            // 1
            typeInfo.ElementType,                           // 2
            GetCollectionProxyStructBodySource(in typeInfo) // 3
        );

        private static string GetCollectionProxyStructPropertySource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.Options.HasFlag(InlineCollectionOptions.CollectionProxyStruct),
            Template_CollectionProxyStructProperty
        );

        private static string GetCollectionProxyStructBodySource(in InlineCollectionTypeInfo typeInfo) =>
            new SourceProvider(in typeInfo)
            .AppendMemberIfProxied(GetICollectionSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetICollectionTSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIEnumerable_CollectionProxy_Source)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIEnumerableT_CollectionProxy_Source)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIListSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIListTSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIReadOnlyCollectionTSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIReadOnlyListTSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIStructuralComparableSource)
            .AppendSeparator()
            .AppendMemberIfProxied(GetIStructuralEquatableSource)
            .ReplaceThisIfProxied()
            .TrimEnd()
            .ToString();

        private static string GetContainsMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ContainsMethod),
            Template_ContainsMethod,
            typeInfo.ElementType // 0
        );

        private static string GetCopyToMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.CopyToMethod),
            Template_CopyToMethod,
            typeInfo.ElementType // 0
        );

        private static string GetDefaultConstructorSource(in InlineCollectionTypeInfo typeInfo) => string.Format
        (
            Template_DefaultConstructor,
            typeInfo.Name // 0
        );

        private static string GetExceptionMessagesSource() => ExceptionMessages_SourceText;

        private static string GetExceptionMessagesSourceFromAssembly(Assembly assembly, string[] resources)
        {
            var resourcePath = resources.Where(x => x.EndsWith($"{nameof(ExceptionMessages)}.cs")).Single();
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd().Replace("internal", "file");
        }

        public static string GetFileHeader() => string.Format
        (
            Template_FileHeader,
            GetExceptionMessagesSource(),                                     // 0
            nameof(ArgumentException_InvalidDestinationArray),                // 1
            nameof(ArgumentOutOfRangeException_IndexOutOfArrayRange),         // 2
            nameof(ArgumentOutOfRangeException_SourceTooLargeForDestination), // 3
            nameof(ArgumentOutOfRangeException_IndexOutOfCollectionRange),    // 4
            nameof(ArgumentException_InvalidArgumentType)                     // 5
        );

        public static string GetFileFooter() => Template_FileFooter;

        private static string GetFillMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.FillMethod),
            Template_FillMethod,
            typeInfo.ElementType // 0
        );

        private static string GetGetEnumeratorBodySource(string length) =>
            string.Format(Template_GetEnumerator_Body, length);

        private static string GetGetEnumeratorMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.GetEnumeratorMethod),
            typeInfo.HasOptions(InlineCollectionOptions.RefStructEnumerator) ?
                Template_GetEnumeratorMethod_ImplRefStructEnumerator :
                Template_GetEnumeratorMethod_ImplExplicit,
            typeInfo.ElementType,                                      // 0
            GetGetEnumeratorBodySource(typeInfo.LengthPropertyOrValue) // 1
        );

        private static string GetICollectionSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ICollection),
            Template_ICollection,
            typeInfo.LengthPropertyOrValue,                               // 0
            nameof(NotSupportedException_SynchronizedAccessNotSupported), // 1
            typeInfo.ElementType                                          // 2
        );

        private static string GetICollectionT_ContainsMethod_Source(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.CollectionProxyStruct) ||
            !typeInfo.HasOptions(InlineCollectionOptions.ContainsMethod),
            Template_ICollectionT_Contains_ImplExplicit,
            typeInfo.ElementType // 0
        );

        private static string GetICollectionTSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ICollectionT),
            Template_ICollectionT,
            typeInfo.ElementType,                              // 0
            typeInfo.LengthPropertyOrValue,                    // 1
            nameof(NotSupportedException_FixedSizeCollection), // 2
            GetICollectionT_ContainsMethod_Source(in typeInfo) // 3
        );

        private static string GetIEnumerableSource(in InlineCollectionTypeInfo typeInfo) =>
            typeInfo.HasOptions(InlineCollectionOptions.IEnumerable) ?
                typeInfo.HasOptions(InlineCollectionOptions.GetEnumeratorMethod) ?
                    typeInfo.HasOptions(InlineCollectionOptions.RefStructEnumerator) ?
                        Template_IEnumerable_ImplRefStructEnumerator :
                        Template_IEnumerable_ImplMethod :
                    typeInfo.HasOptions(InlineCollectionOptions.IEnumerableT) ?
                        string.Format(Template_IEnumerable_ImplIEnumerableT, typeInfo.ElementType) :
                        string.Format
                        (
                            Template_IEnumerable_ImplExplicit,
                            GetGetEnumeratorBodySource(typeInfo.LengthPropertyOrValue) // 0
                        ) :
                string.Empty;

        private static string GetIEnumerable_CollectionProxy_Source(in InlineCollectionTypeInfo typeInfo) =>
            typeInfo.HasOptions(InlineCollectionOptions.IEnumerable) ?
                Template_IEnumerable_ImplCollectionProxy :
                string.Empty;

        private static string GetIEnumerableTSource(in InlineCollectionTypeInfo typeInfo) =>
            typeInfo.HasOptions(InlineCollectionOptions.IEnumerableT) ?
                typeInfo.HasOptions(InlineCollectionOptions.GetEnumeratorMethod) ?
                    typeInfo.HasOptions(InlineCollectionOptions.RefStructEnumerator) ?
                        string.Format(Template_IEnumerableT_ImplRefStructEnumerator, typeInfo.ElementType) :
                        string.Empty :
                    string.Format
                    (
                        Template_IEnumerableT_ImplExplicit,
                        typeInfo.ElementType,                                      // 0
                        GetGetEnumeratorBodySource(typeInfo.LengthPropertyOrValue) // 1
                    ) :
                string.Empty;

        private static string GetIEnumerableT_CollectionProxy_Source(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IEnumerableT),
            Template_IEnumerableT_ImplCollectionProxy,
            typeInfo.ElementType // 0
        );

        private static string GetIInlineCollectionSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            !typeInfo.HasOptions(InlineCollectionOptions.AsSpanReadOnlySpanMethods),
            Template_IInlineCollection_ImplExplicit,
            string.Empty,                                  // 0
            typeInfo.ElementType,                          // 1
            $"IInlineCollection<{typeInfo.ElementType}>.", // 2
            typeInfo.ElementZeroFieldName,                 // 3
            typeInfo.LengthPropertyOrValue                 // 4
        );

        private static string GetIListSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IList),
            Template_IList,
            typeInfo.ElementType,                             // 0
            nameof(NotSupportedException_FixedSizeCollection) // 1
        );

        private static string GetIListT_IndexOfMethod_Source(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.CollectionProxyStruct) ||
            !typeInfo.HasOptions(InlineCollectionOptions.IndexOfMethod),
            Template_IListT_IndexOf_ImplExplict,
            typeInfo.ElementType // 0
        );

        private static string GetIListTSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IListT),
            Template_IListT,
            typeInfo.ElementType,                             // 0
            GetIListT_IndexOfMethod_Source(in typeInfo),      // 1
            nameof(NotSupportedException_FixedSizeCollection) // 2
        );

        private static string GetIndexOfMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IndexOfMethod),
            Template_IndexOfMethod,
            typeInfo.ElementType // 0
        );

        public static string GetInlineCollectionAttributeSource() => Template_InlineCollectionAttribute;

        private sealed class SourceProvider(in InlineCollectionTypeInfo typeInfo)
        {
            private readonly StringBuilder Builder = new();
            private readonly bool IsProxied = typeInfo.HasOptions(InlineCollectionOptions.CollectionProxyStruct);
            private readonly InlineCollectionTypeInfo TypeInfo = typeInfo;

            public delegate string GetMemberHandler(in InlineCollectionTypeInfo typeInfo);

            private SourceProvider AppendIf(bool condition, string stringToAppend)
            {
                _ = Builder.AppendIf(condition, stringToAppend);
                return this;
            }

            public SourceProvider AppendMember(GetMemberHandler handler) => AppendMember(handler(in TypeInfo));

            private SourceProvider AppendMember(string memberToAppend) =>
                AppendSeparatorIf(memberToAppend.Contains(" {"))
                    .AppendIf(memberToAppend != string.Empty, memberToAppend)
                    .AppendSeparator(ifLastCharIs: '}');

            public SourceProvider AppendMemberIfNotProxied(GetMemberHandler handler) => !IsProxied ?
                AppendMember(handler(in TypeInfo)) :
                this;

            public SourceProvider AppendMemberIfProxied(GetMemberHandler handler) => IsProxied ?
                AppendMemberIfProxied(handler(in TypeInfo)) :
                this;

            public SourceProvider AppendMemberIfProxied(string memberToAppend) => IsProxied ?
                AppendMember(memberToAppend.Replace(Template_NewLine, Template_NewLineIndent1)) :
                this;

            public SourceProvider AppendSeparator(char ifLastCharIs = ';') => AppendSeparatorIf(true, ifLastCharIs);

            private SourceProvider AppendSeparatorIf(bool condition, char ifLastCharIs = ';')
            {
                _ = Builder.AppendIf((ifLastCharIs == Builder.LastOrDefault()) && condition,Template_NewLineIndent2);
                return this;
            }

            public SourceProvider ReplaceThisIfProxied()
            {
                if (IsProxied)
                {
                    _ = Builder.Replace("(this", "(span").Replace(" Length", " span.Length");
                }
                return this;
            }

            public SourceProvider TrimEnd()
            {
                _ = Builder.TrimEnd();
                return this;
            }

            public override string ToString() => Builder.ToString();
        }

        private static string GetInlineCollectionBody(in InlineCollectionTypeInfo typeInfo) =>new SourceProvider
        (
            in typeInfo
        ).AppendMember(GetLengthPropertySource)
            .AppendMember(GetCollectionProxyStructPropertySource)
            .AppendMember(GetCollectionProxyStructSource)
            .AppendSeparator()
            .AppendMember(GetRefStructEnumeratorSource)
            .AppendSeparator()
            .AppendMember(GetArrayConversionOperatorsSource)
            .AppendSeparator()
            .AppendMember(GetDefaultConstructorSource)
            .AppendMember(GetReadOnlySpanConstructorSource)
            .AppendSeparator()
            .AppendMember(GetAsSpanReadOnlySpanMethodsSource)
            .AppendMember(GetClearMethodSource)
            .AppendMember(GetContainsMethodSource)
            .AppendMember(GetCopyToMethodSource)
            .AppendMember(GetFillMethodSource)
            .AppendMember(GetGetEnumeratorMethodSource)
            .AppendMember(GetIndexOfMethodSource)
            .AppendMember(GetToArrayMethodSource)
            .AppendMember(GetTryCopyToMethodSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetICollectionSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetICollectionTSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIEnumerableSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIEnumerableTSource)
            .AppendSeparator()
            .AppendMember(GetIInlineCollectionSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIListSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIListTSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIReadOnlyCollectionTSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIReadOnlyListTSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIStructuralComparableSource)
            .AppendSeparator()
            .AppendMemberIfNotProxied(GetIStructuralEquatableSource)
            .TrimEnd()
            .ToString();

        public static string GetInlineCollectionOptionsSource() => InlineCollectionOptions_SourceText;

        private static string GetInlineCollectionOptionsSourceFromAssembly(Assembly assembly, string[] resources)
        {
            var resourcePath = resources.Where(x => x.EndsWith($"{nameof(InlineCollectionOptions)}.cs")).Single();
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            using var streamReader = new StreamReader(stream);
            var sb = new StringBuilder("// <auto-generated>").AppendLine();
            return sb.Append(streamReader.ReadToEnd()).ToString();
        }

        private static string GetInterfaceList(in InlineCollectionTypeInfo typeInfo, bool isProxy)
        {
            if (!isProxy && typeInfo.HasOptions(InlineCollectionOptions.CollectionProxyStruct))
            {
                return $" : IInlineCollection<{typeInfo.ElementType}>";
            }
            bool isICollection = typeInfo.HasOptions(InlineCollectionOptions.ICollection);
            bool isICollectionT = typeInfo.HasOptions(InlineCollectionOptions.ICollectionT);
            bool isIEnumerable = typeInfo.HasOptions(InlineCollectionOptions.IEnumerable);
            bool isIEnumerableT = typeInfo.HasOptions(InlineCollectionOptions.IEnumerableT);
            bool isIList = typeInfo.HasOptions(InlineCollectionOptions.IList);
            bool isIListT = typeInfo.HasOptions(InlineCollectionOptions.IListT);
            bool isIReadOnlyCollectionT = typeInfo.HasOptions(InlineCollectionOptions.IReadOnlyCollectionT);
            bool isIReadOnlyListT = typeInfo.HasOptions(InlineCollectionOptions.IReadOnlyListT);
            bool isIStructuralComparable = typeInfo.HasOptions(InlineCollectionOptions.IStructuralComparable);
            bool isIStructuralEquatable = typeInfo.HasOptions(InlineCollectionOptions.IStructuralEquatable);
            bool explicitICollection = !isIList && isICollection;
            bool explicitICollectionT = !isIListT && isICollectionT;
            bool hasGenericCollection = isICollectionT || isIListT || isIReadOnlyCollectionT || isIReadOnlyListT;
            bool explicitIEnumerableT = !hasGenericCollection && isIEnumerableT;
            bool explicitIEnumerable = !hasGenericCollection && !isICollection && !isIList && !explicitIEnumerableT &&
                isIEnumerable;
            bool explicitIReadOnlyCollectionT = !isIReadOnlyListT && isIReadOnlyCollectionT;
            var sb = new StringBuilder()
                .AppendInterfaceIf(explicitICollection, "ICollection")
                .AppendInterfaceIf(explicitICollectionT, $"ICollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(explicitIEnumerable, "IEnumerable")
                .AppendInterfaceIf(explicitIEnumerableT, $"IEnumerable<{typeInfo.ElementType}>")
                .AppendInterfaceIf(!isProxy, $"IInlineCollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIList, "IList")
                .AppendInterfaceIf(isIListT, $"IList<{typeInfo.ElementType}>")
                .AppendInterfaceIf(explicitIReadOnlyCollectionT, $"IReadOnlyCollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIReadOnlyListT, $"IReadOnlyList<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIStructuralComparable, "IStructuralComparable")
                .AppendInterfaceIf(isIStructuralEquatable, "IStructuralEquatable");
            if (sb.Length != 0)
            {
                _ = sb.Insert(0, $" :{Template_NewLineIndent2}{(isProxy ? "    " : "")}");
            }
            return sb.ToString();
        }

        private static string GetIReadOnlyCollectionTSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IReadOnlyCollectionT),
            Template_IReadOnlyCollectionT,
            typeInfo.ElementType,          // 0
            typeInfo.LengthPropertyOrValue // 1
        );

        private static string GetIReadOnlyListTSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IReadOnlyListT),
            Template_IReadOnlyListT,
            typeInfo.ElementType // 0
        );

        private static string GetIStructuralComparableSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IStructuralComparable),
            Template_IStructuralComparable,
            typeInfo.FullName,   // 0
            typeInfo.ElementType // 1
        );

        private static string GetIStructuralEquatableSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.IStructuralEquatable),
            Template_IStructuralEquatable,
            typeInfo.FullName,   // 0
            typeInfo.ElementType // 1
        );

        private static string GetLengthPropertySource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.LengthProperty),
            Template_LengthProperty,
            typeInfo.Length.ToString() // 0
        );

        private static string GetLengthPropertyOrValueForSource(in InlineCollectionTypeInfo typeInfo) =>
            typeInfo.HasOptions(InlineCollectionOptions.LengthProperty) ? "Length" : typeInfo.Length.ToString();

        private static string GetReadOnlySpanConstructorSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ReadOnlySpanConstructor),
            Template_ReadOnlySpanConstructor,
            typeInfo.Name,        // 0
            typeInfo.ElementType, // 1
            typeInfo.FullName     // 2
        );

        private static string GetRefStructEnumeratorSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.RefStructEnumerator),
            Template_RefStructEnumerator,
            typeInfo.FullName.Replace('<', '{').Replace('>', '}'),
            typeInfo.ElementType
        );

        private static string GetStructDeclarationSource(in InlineCollectionTypeInfo typeInfo) => new StringBuilder
        (
            string.Format
            (
                Template_InlineCollection_StructDeclaration,
                typeInfo.Length,                                  // 0
                GetCollectionBuilderAttributeSource(in typeInfo), // 1
                typeInfo.Modifiers,                               // 2
                typeInfo.FullName,                                // 3
                GetInterfaceList(in typeInfo, isProxy: false),    // 4
                GetInlineCollectionBody(in typeInfo)              // 5
            )
        ).Replace(Template_NewLine, $"{Template_NewLine}{new string(' ', 4 * (typeInfo.TypeList.Length - 1))}")
            .ToString();

        private static string GetToArrayMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.ToArrayMethod),
            Template_ToArrayMethod,
            typeInfo.ElementType // 0
        );

        private static string GetTypeDeclarationsSource(in InlineCollectionTypeInfo typeInfo)
        {
            if (typeInfo.TypeList.Length == 1)
            {
                return GetStructDeclarationSource(in typeInfo);
            }
            var sb = new StringBuilder();
            var indent = new StringBuilder("    ");
            for (int i = 0; i < (typeInfo.TypeList.Length - 1); ++i)
            {
                var type = typeInfo.TypeList[i];
                _ = sb
                    .AppendLine()
                    .Append(indent)
                    .Append(type.Modifiers)
                    .Append(' ')
                    .Append(type.DeclarationKind)
                    .Append(' ')
                    .Append(type.TypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
                    .AppendLine()
                    .Append(indent)
                    .Append('{');
                _ = indent.Append("    ");
            }
            _ = sb.Append(GetStructDeclarationSource(in typeInfo));
            for (int i = 0; i < (typeInfo.TypeList.Length - 1); ++i)
            {
                _ = sb.AppendLine();
                indent.Length -= 4;
                _ = sb
                    .Append(indent)
                    .Append("}");
            }
            return sb.ToString();
        }

        private static string GetTryCopyToMethodSource(in InlineCollectionTypeInfo typeInfo) => FormatIf
        (
            typeInfo.HasOptions(InlineCollectionOptions.TryCopyToMethod),
            Template_TryCopyToMethod,
            typeInfo.ElementType // 0
        );

        private static char LastOrDefault(this StringBuilder sb) => sb.Length != 0 ? sb[sb.Length - 1] : default;

        private static StringBuilder TrimEnd(this StringBuilder sb)
        {
            int i = sb.Length;
            while ((i > 0) && char.IsWhiteSpace(sb[i - 1]))
            {
                --i;
            }
            if (i != sb.Length)
            {
                sb.Length = i;
            }
            return sb;
        }
    }
}
