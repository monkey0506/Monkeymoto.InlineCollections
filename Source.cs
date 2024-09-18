using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static Monkeymoto.InlineCollections.ExceptionMessages;

namespace Monkeymoto.InlineCollections
{
    internal static partial class Source
    {
        private static readonly string ExceptionMessages_SourceText;
        public const string FileName = "Monkeymoto.InlineCollections.GeneratedCollections.g.cs";
        public static readonly string InlineCollectionAttribute_FileName = $"{InlineCollectionAttribute_Name}.g.cs";
        public const string InlineCollectionAttribute_Name = "Monkeymoto.InlineCollections.InlineCollectionAttribute";
        public const string InlineCollectionFlags_FileName = "Monkeymoto.InlineCollections.InlineCollectionFlags.g.cs";
        private static readonly string InlineCollectionFlags_SourceText;

        static Source()
        {
            var assembly = typeof(InlineCollectionFlags).Assembly;
            var resources = assembly.GetManifestResourceNames();
            ExceptionMessages_SourceText = GetExceptionMessagesSourceFromAssembly(assembly, resources);
            InlineCollectionFlags_SourceText = GetInlineCollectionFlagsSourceFromAssembly(assembly, resources);
        }

        private static StringBuilder AppendIf(this StringBuilder sb, bool condition, string stringToAppend) =>
            condition ? sb.Append(stringToAppend) : sb;

        private static StringBuilder AppendInterface(this StringBuilder sb, string interfaceToAppend) => sb
            .AppendIf(sb.Length != 0, ", ")
            .Append(interfaceToAppend);

        private static StringBuilder AppendInterfaceIf(this StringBuilder sb, bool condition, string interfaceToAppend) =>
            condition ? sb.AppendInterface(interfaceToAppend) : sb;

        private static StringBuilder AppendMember(this StringBuilder sb, string memberToAppend) => sb
            .AppendSeparatorIf(memberToAppend.Contains(" {"))
            .AppendIf(memberToAppend != string.Empty, memberToAppend)
            .AppendSeparator(ifLastCharIs: '}');

        private static StringBuilder AppendSeparator(this StringBuilder sb, char ifLastCharIs = ';') => sb
            .AppendSeparatorIf(true, ifLastCharIs);

        private static StringBuilder AppendSeparatorIf(this StringBuilder sb, bool condition, char ifLastCharIs = ';') => sb
            .AppendIf((ifLastCharIs == sb.LastOrDefault()) && condition, Template_NewLineIndent2);

        public static string GenerateForType(in InlineCollectionTypeInfo typeInfo) => string.Format
        (
            Template_InlineCollection,
            typeInfo.Namespace,                                     // 0
            GetTypeDeclarationsSource(in typeInfo),                 // 1
            GetCollectionBuilderClassDeclarationSource(in typeInfo) // 2
        );

        private static string GetArrayConversionOperatorsSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_ArrayConversionOperators,
            InlineCollectionFlags.ArrayConversionOperators,
            typeInfo.Flags,
            typeInfo.ElementType, // 0
            typeInfo.FullName     // 1
        );

        private static string GetAsSpanReadOnlySpanMethodsSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceByFlag
        (
            Template_AsSpanReadOnlySpanMethods,
            InlineCollectionFlags.AsSpanReadOnlySpanMethods,
            typeInfo.Flags,
            "public ",                     // 0
            typeInfo.ElementType,          // 1
            string.Empty,                  // 2
            typeInfo.ElementZeroFieldName, // 3
            length                         // 4
        );

        private static string GetClearMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_ClearMethod,
            InlineCollectionFlags.ClearMethod,
            typeInfo.Flags
        );

        private static string GetCollectionBuilderAttributeSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_CollectionBuilderAttribute,
            InlineCollectionFlags.CollectionBuilder,
            typeInfo.Flags,
            typeInfo.CollectionBuilderName // 0
        );

        private static string GetCollectionBuilderClassDeclarationSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_CollectionBuilderClassDeclaration,
            InlineCollectionFlags.CollectionBuilder,
            typeInfo.Flags,
            typeInfo.CollectionBuilderName,              // 0
            typeInfo.FullNameWithContainingTypeNames,    // 1
            typeInfo.CollectionBuilderTypeParameterList, // 2
            typeInfo.ElementType                         // 3
        );

        private static string GetContainsMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_ContainsMethod,
            InlineCollectionFlags.ContainsMethod,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        private static string GetCopyToMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_CopyToMethod,
            InlineCollectionFlags.CopyToMethod,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        private static string GetDefaultConstructor(in InlineCollectionTypeInfo typeInfo) => string.Format
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

        private static string GetFillMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_FillMethod,
            InlineCollectionFlags.FillMethod,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        private static string GetGetEnumeratorBodySource(string length) => string.Format(Template_GetEnumerator_Body, length);

        private static string GetGetEnumeratorMethodSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceByFlag
        (
            Template_GetEnumeratorMethod,
            InlineCollectionFlags.GetEnumeratorMethod,
            typeInfo.Flags,
            typeInfo.ElementType,              // 0
            GetGetEnumeratorBodySource(length) // 1
        );

        private static string GetICollectionSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceByFlag
        (
            Template_ICollection,
            InlineCollectionFlags.ICollection,
            typeInfo.Flags,
            length,                                                       // 0
            nameof(NotSupportedException_SynchronizedAccessNotSupported), // 1
            typeInfo.ElementType                                          // 2
        );

        private static string GetICollectionT_ContainsMethod_Source(in InlineCollectionTypeInfo typeInfo) => GetSourceIf
        (
            typeInfo.Flags.HasNoneOf(InlineCollectionFlags.ContainsMethod, InlineCollectionFlags.Everything),
            Template_ICollectionT_Contains_ImplExplicit,
            typeInfo.ElementType // 0
        );

        private static string GetICollectionTSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceByFlag
        (
            Template_ICollectionT,
            InlineCollectionFlags.ICollectionT,
            typeInfo.Flags,
            typeInfo.ElementType,                              // 0
            length,                                            // 1
            nameof(NotSupportedException_FixedSizeCollection), // 2
            GetICollectionT_ContainsMethod_Source(in typeInfo) // 3
        );

        private static string GetIEnumerableSource(in InlineCollectionTypeInfo typeInfo, string length) =>
            typeInfo.Flags.HasFlag(InlineCollectionFlags.IEnumerable) ?
                typeInfo.Flags.HasFlag(InlineCollectionFlags.GetEnumeratorMethod) ?
                    Template_IEnumerable_ImplMethod :
                    typeInfo.Flags.HasFlag(InlineCollectionFlags.IEnumerableT) ?
                        string.Format(Template_IEnumerable_ImplIEnumerableT, typeInfo.ElementType) :
                        string.Format(Template_IEnumerable_ImplExplicit, GetGetEnumeratorBodySource(length)) :
                string.Empty;

        private static string GetIEnumerableTSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceIf
        (
            typeInfo.Flags.HasFlag(InlineCollectionFlags.IEnumerableT) &&
                !typeInfo.Flags.HasFlag(InlineCollectionFlags.GetEnumeratorMethod),
            Template_IEnumerableT_ImplExplicit,
            typeInfo.ElementType,              // 0
            GetGetEnumeratorBodySource(length) // 1
        );

        private static string GetIInlineCollectionSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceIf
        (
            !typeInfo.Flags.HasFlag(InlineCollectionFlags.AsSpanReadOnlySpanMethods),
            Template_IInlineCollection_ImplExplicit,
            string.Empty,                                  // 0
            typeInfo.ElementType,                          // 1
            $"IInlineCollection<{typeInfo.ElementType}>.", // 2
            typeInfo.ElementZeroFieldName,                 // 3
            length                                         // 4
        );

        private static string GetIListSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IList,
            InlineCollectionFlags.IList,
            typeInfo.Flags,
            typeInfo.ElementType,                             // 0
            nameof(NotSupportedException_FixedSizeCollection) // 1
        );

        private static string GetIListT_IndexOfMethod_Source(in InlineCollectionTypeInfo typeInfo) => GetSourceIf
        (
            typeInfo.Flags.HasNoneOf(InlineCollectionFlags.IndexOfMethod, InlineCollectionFlags.Everything),
            Template_IListT_IndexOf_ImplExplict,
            typeInfo.ElementType // 0
        );

        private static string GetIListTSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IListT,
            InlineCollectionFlags.IListT,
            typeInfo.Flags,
            typeInfo.ElementType,                             // 0
            GetIListT_IndexOfMethod_Source(in typeInfo),      // 1
            nameof(NotSupportedException_FixedSizeCollection) // 2
        );

        private static string GetIndexOfMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IndexOfMethod,
            InlineCollectionFlags.IndexOfMethod,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        public static string GetInlineCollectionAttributeSource() => Template_InlineCollectionAttribute;

        private static string GetInlineCollectionBody(in InlineCollectionTypeInfo typeInfo) => new StringBuilder()
            .AppendMember(GetLengthPropertySource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetArrayConversionOperatorsSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetDefaultConstructor(in typeInfo))
            .AppendMember(GetReadOnlySpanConstructorSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetAsSpanReadOnlySpanMethodsSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendMember(GetClearMethodSource(in typeInfo))
            .AppendMember(GetContainsMethodSource(in typeInfo))
            .AppendMember(GetCopyToMethodSource(in typeInfo))
            .AppendMember(GetFillMethodSource(in typeInfo))
            .AppendMember(GetGetEnumeratorMethodSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendMember(GetIndexOfMethodSource(in typeInfo))
            .AppendMember(GetToArrayMethodSource(in typeInfo))
            .AppendMember(GetTryCopyToMethodSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetICollectionSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetICollectionTSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetIEnumerableSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetIEnumerableTSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetIInlineCollectionSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetIListSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetIListTSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetIReadOnlyCollectionTSource(in typeInfo, typeInfo.LengthPropertyOrValue))
            .AppendSeparator()
            .AppendMember(GetIReadOnlyListTSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetIStructuralComparableSource(in typeInfo))
            .AppendSeparator()
            .AppendMember(GetIStructuralEquatableSource(in typeInfo))
            .TrimEnd()
            .ToString();

        public static string GetInlineCollectionFlagsSource() => InlineCollectionFlags_SourceText;

        private static string GetInlineCollectionFlagsSourceFromAssembly(Assembly assembly, string[] resources)
        {
            var resourcePath = resources.Where(x => x.EndsWith($"{nameof(InlineCollectionFlags)}.cs")).Single();
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            using var streamReader = new StreamReader(stream);
            var sb = new StringBuilder("// <auto-generated>").AppendLine();
            return sb.Append(streamReader.ReadToEnd()).ToString();
        }

        private static string GetInterfaceList(in InlineCollectionTypeInfo typeInfo)
        {
            bool isICollection = typeInfo.Flags.HasFlag(InlineCollectionFlags.ICollection);
            bool isICollectionT = typeInfo.Flags.HasFlag(InlineCollectionFlags.ICollectionT);
            bool isIEnumerable = typeInfo.Flags.HasFlag(InlineCollectionFlags.IEnumerable);
            bool isIEnumerableT = typeInfo.Flags.HasFlag(InlineCollectionFlags.IEnumerableT);
            bool isIList = typeInfo.Flags.HasFlag(InlineCollectionFlags.IList);
            bool isIListT = typeInfo.Flags.HasFlag(InlineCollectionFlags.IListT);
            bool isIReadOnlyCollectionT = typeInfo.Flags.HasFlag(InlineCollectionFlags.IReadOnlyCollectionT);
            bool isIReadOnlyListT = typeInfo.Flags.HasFlag(InlineCollectionFlags.IReadOnlyListT);
            bool isIStructuralComparable = typeInfo.Flags.HasFlag(InlineCollectionFlags.IStructuralComparable);
            bool isIStructuralEquatable = typeInfo.Flags.HasFlag(InlineCollectionFlags.IStructuralEquatable);
            bool explicitICollection = !isIList && isICollection;
            bool explicitICollectionT = !isIListT && isICollectionT;
            bool explicitIEnumerableT = !isICollectionT && !isIListT && !isIReadOnlyCollectionT && !isIReadOnlyListT && isIEnumerableT;
            bool explicitIEnumerable = !isICollection && !isIList && !explicitIEnumerableT && isIEnumerable;
            bool explicitIReadOnlyCollectionT = !isIReadOnlyListT && isIReadOnlyCollectionT;
            return new StringBuilder()
                .AppendInterfaceIf(explicitICollection, "ICollection")
                .AppendInterfaceIf(explicitICollectionT, $"ICollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(explicitIEnumerable, "IEnumerable")
                .AppendInterfaceIf(explicitIEnumerableT, $"IEnumerable<{typeInfo.ElementType}>")
                .AppendInterface($"IInlineCollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIList, "IList")
                .AppendInterfaceIf(isIListT, $"IList<{typeInfo.ElementType}>")
                .AppendInterfaceIf(explicitIReadOnlyCollectionT, $"IReadOnlyCollection<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIReadOnlyListT, $"IReadOnlyList<{typeInfo.ElementType}>")
                .AppendInterfaceIf(isIStructuralComparable, "IStructuralComparable")
                .AppendInterfaceIf(isIStructuralEquatable, "IStructuralEquatable")
                .ToString();
        }

        private static string GetIReadOnlyCollectionTSource(in InlineCollectionTypeInfo typeInfo, string length) => GetSourceByFlag
        (
            Template_IReadOnlyCollectionT,
            InlineCollectionFlags.IReadOnlyCollectionT,
            typeInfo.Flags,
            typeInfo.ElementType, // 0
            length                // 1
        );

        private static string GetIReadOnlyListTSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IReadOnlyListT,
            InlineCollectionFlags.IReadOnlyListT,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        private static string GetIStructuralComparableSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IStructuralComparable,
            InlineCollectionFlags.IStructuralComparable,
            typeInfo.Flags,
            typeInfo.FullName,   // 0
            typeInfo.ElementType // 1
        );

        private static string GetIStructuralEquatableSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_IStructuralEquatable,
            InlineCollectionFlags.IStructuralEquatable,
            typeInfo.Flags,
            typeInfo.FullName,   // 0
            typeInfo.ElementType // 1
        );

        private static string GetLengthPropertySource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_LengthProperty,
            InlineCollectionFlags.LengthProperty,
            typeInfo.Flags,
            typeInfo.Length.ToString() // 0
        );

        private static string GetLengthPropertyOrValueForSource(in InlineCollectionTypeInfo typeInfo) =>
            typeInfo.Flags.HasFlag(InlineCollectionFlags.LengthProperty) ? "Length" : typeInfo.Length.ToString();

        private static string GetReadOnlySpanConstructorSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_ReadOnlySpanConstructor,
            InlineCollectionFlags.ReadOnlySpanConstructor,
            typeInfo.Flags,
            typeInfo.Name,        // 0
            typeInfo.ElementType, // 1
            typeInfo.FullName     // 2
        );

        private static string GetSourceByFlag(string template, InlineCollectionFlags flagToCheck, InlineCollectionFlags flags, params string[] formatArgs)
        {
            bool hasFlag = flags.HasFlag(flagToCheck);
            return GetSourceIf(hasFlag, template, formatArgs);
        }

        private static string GetSourceIf(bool condition, string template, params string[] formatArgs) =>
            condition ? string.Format(template, formatArgs) : string.Empty;

        private static string GetStructDeclarationSource(in InlineCollectionTypeInfo typeInfo) => new StringBuilder
        (
            string.Format
            (
                Template_InlineCollection_StructDeclaration,
                typeInfo.Length,                                  // 0
                GetCollectionBuilderAttributeSource(in typeInfo), // 1
                typeInfo.Modifiers,                               // 2
                typeInfo.FullName,                                // 3
                GetInterfaceList(in typeInfo),                    // 4
                GetInlineCollectionBody(in typeInfo)              // 5
            )
        ).Replace("\r\n", "\n")
            .Replace("\n\r", "\n")
            .Replace("\n", $"{Template_NewLine}{new string(' ', 4 * (typeInfo.TypeList.Length - 1))}")
            .ToString();

        private static string GetToArrayMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_ToArrayMethod,
            InlineCollectionFlags.ToArrayMethod,
            typeInfo.Flags,
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

        private static string GetTryCopyToMethodSource(in InlineCollectionTypeInfo typeInfo) => GetSourceByFlag
        (
            Template_TryCopyToMethod,
            InlineCollectionFlags.TryCopyToMethod,
            typeInfo.Flags,
            typeInfo.ElementType // 0
        );

        private static bool HasNoneOf(this InlineCollectionFlags thisFlags, params InlineCollectionFlags[] flagsToCheck) =>
            !flagsToCheck.Any(x => thisFlags.HasFlag(x));
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
