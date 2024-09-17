using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Monkeymoto.InlineCollections
{
    internal readonly struct GeneratedSourceInfo(ImmutableArray<Diagnostic> diagnostics, string sourceText) :
        IEquatable<GeneratedSourceInfo>
    {
        public readonly ImmutableArray<Diagnostic> Diagnostics = diagnostics;
        public readonly string SourceText = sourceText;

        public static bool operator==(GeneratedSourceInfo left, GeneratedSourceInfo right) => left.Equals(right);
        public static bool operator!=(GeneratedSourceInfo left, GeneratedSourceInfo right) => !(left == right);

        public override bool Equals(object obj) => obj is GeneratedSourceInfo other && Equals(other);
        public bool Equals(GeneratedSourceInfo other) =>
            Diagnostics.SequenceEqual(other.Diagnostics) && SourceText == other.SourceText;

        /// <summary>
        /// Gets the hash code for this <see cref="GeneratedSourceInfo"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Borrowed from <see href="https://stackoverflow.com/a/1646913">Quick and Simple Hash Code Combinations - Stack
        /// Overflow</see> answer by user <see href="https://stackoverflow.com/users/22656/jon-skeet">Jon Skeet</see>, licensed
        /// under <see href="https://creativecommons.org/licenses/by-sa/2.5/">CC BY-SA 2.5</see>. Changes have been made to
        /// match the fields of this class.
        /// </para>
        /// </remarks>
        /// <returns>The hash value generated for this <see cref="GeneratedSourceInfo"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Diagnostics.GetHashCode();
                hash = hash * 31 + SourceText.GetHashCode();
                return hash;
            }
        }
    }
}
