using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Monkeymoto.InlineCollections
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor MMIC1000_CollectionBuilderTypeInaccessible_Descriptor = new
        (
            "MMIC1000",
            "MMIC1000: Inline collection inaccessible to collection builder",
            "Collection builder requires inline collection struct and all containing types to be `public` or `internal`",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1001_MustHaveValidLength_Descriptor = new
        (
            "MMIC1001",
            "MMIC1001: Inline collection must have positive, non-zero length",
            "Include a positive, non-zero `Length` property in InlineCollection attribute",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1002_MustNotBeFileLocalType_Descriptor = new
        (
            "MMIC1002",
            "MMIC1002: Inline collection or top-level type cannot be declared as a file local type",
            "Inline collection struct or top-level containing type cannot be declared as a `file` local type",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1003_MustBePartialType_Descriptor = new
        (
            "MMIC1003",
            "MMIC1003: Inline collection and all containing types must be partial",
            "Include `partial` modifier on type `{0}` definition which contains an inline collection",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1004_MustNotBeReadOnly_Descriptor = new
        (
            "MMIC1004",
            "MMIC1004: Inline collection struct cannot be declared as readonly",
            "Inline collection struct cannot be declared as `readonly`",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1005_MustDefineExactlyOneField_Descriptor = new
        (
            "MMIC1005",
            "MMIC1005: Inline collection struct must declare one and only one instance field",
            "Inline collection struct must declare one and only one instance field",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1006_InvalidElementFieldModifiers_Descriptor = new
        (
            "MMIC1006",
            "MMIC1006: Inline collection element field has invalid modifiers",
            "Inline collection element field cannot be declared as required, readonly, volatile, or as a fixed size buffer",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1007_CollectionExpressionTooLarge_Descriptor = new
        (
            "MMIC1007",
            "MMIC1007: Collection expression is larger than target inline collection",
            "Collection expression is larger than target inline collection of length `{0}`",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MMIC1008_CollectionExpressionMaybeTooLarge_Descriptor = new
        (
            "MMIC1008",
            "MMIC1008: Collection expression may be larger than target inline collection",
            "Collection expression may be larger than target inline collection of length `{0}` when all elements are expanded",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static void AddDiagnostic
        (
            this ICollection<Diagnostic> diagnostics,
            DiagnosticDescriptor descriptor,
            Location? location,
            params object?[]? messageArgs
        )
        {
            diagnostics.Add(Diagnostic.Create(descriptor, location, messageArgs));
        }
    }
}
