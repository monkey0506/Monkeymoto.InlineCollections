using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Monkeymoto.InlineCollections
{
    internal readonly partial struct InlineCollectionTypeInfo
    {
        private struct ConstructorArgs
        {
            public IFieldSymbol? FieldSymbol;
            public InlineCollectionFlags Flags;
            public int Length;
            public Location Location;
            public ImmutableArray<TypeListNode> TypeList;
            public INamedTypeSymbol TypeSymbol;
        }
    }
}
