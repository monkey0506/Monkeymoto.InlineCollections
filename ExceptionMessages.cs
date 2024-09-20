namespace Monkeymoto.InlineCollections
{
    internal static class ExceptionMessages
    {
        public const string ArgumentException_InvalidArgumentType =
            "Argument type did not match the type of the inline collection.";
        public const string ArgumentException_InvalidDestinationArray =
            "Destination array must be one-dimensional array of the same element type as the inline collection.";
        public const string ArgumentOutOfRangeException_IndexOutOfArrayRange =
            "Index out of range of the array bounds.";
        public const string ArgumentOutOfRangeException_IndexOutOfCollectionRange =
            "Index out of range of the inline collection bounds.";
        public const string ArgumentOutOfRangeException_SourceTooLargeForDestination =
            "Source was larger than the destination.";
        public const string NotSupportedException_FixedSizeCollection = "Inline collection was of a fixed size.";
        public const string NotSupportedException_RefStructEnumeratorConversion =
            "Collection with `ref struct` enumerator does not support conversion to the target enumerator interface.";
        public const string NotSupportedException_SynchronizedAccessNotSupported =
            "Inline collection does not support synchronized access.";
    }
}
