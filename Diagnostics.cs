using Microsoft.CodeAnalysis;

namespace Monkeymoto.InlineCollections
{
    internal static class Diagnostics
    {
        public static DiagnosticDescriptor MMIC1000_MustBeTopLevelType_Descriptor = new
        (
            "MMIC1000",
            "MMIC1000: Inline collection must be a top-level type",
            "Inline collection must be a top-level type",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static DiagnosticDescriptor MMIC1001_MustHaveValidLength_Descriptor = new
        (
            "MMIC1001",
            "MMIC1001: Inline collection must have positive, non-zero length",
            "Include a positive, non-zero length in InlineCollection attribute",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static DiagnosticDescriptor MMIC1002_MustBePartialStruct_Descriptor = new
        (
            "MMIC1002",
            "MMIC1002: Inline collection struct must be partial",
            "Include `partial` modifier on inline collection struct definition",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static DiagnosticDescriptor MMIC1003_MustDefineExactlyOneField_Descriptor = new
        (
            "MMIC1003",
            "MMIC1003: Inline collection struct must declare one and only one instance field",
            "Inline collection struct must declare one and only one instance field",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static DiagnosticDescriptor MMIC1004_CollectionExpressionTooLarge_Descriptor = new
        (
            "MMIC1004",
            "MMIC1004: Collection expression is larger than target inline collection",
            "Collection expression is larger than target inline collection of length {0}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static DiagnosticDescriptor MMIC1005_CollectionExpressionMaybeTooLarge_Descriptor = new
        (
            "MMIC1005",
            "MMIC1005: Collection expression may be larger than target inline collection",
            "Collection expression may be larger than target inline collection of length {0} when all elements are expanded",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    }
}
