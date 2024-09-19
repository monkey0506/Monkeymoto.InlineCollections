using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if !MONKEYMOTO_INLINECOLLECTIONS_TYPES_ALREADY_DEFINED

namespace Monkeymoto.InlineCollections
{
    /// <summary>
    /// Represents a set of options that control how an inline collection is generated.
    /// </summary>
    /// <seealso cref="InlineCollectionAttribute"/>
    [Flags]
    internal enum InlineCollectionOptions
    {
        /// <summary>
        /// Generates the <see cref="InlineArrayAttribute">InlineArrayAttribute</see> for the collection, but adds no
        /// additional features.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This option is ignored if any other options are included.
        /// </para>
        /// </remarks>
        None = 0,
        /// <summary>
        /// Generates explicit conversion operators between the collection type and T[].
        /// </summary>
        /// <seealso cref="ToArrayMethod"/>
        ArrayConversionOperators = 1 << 0,
        /// <summary>
        /// Generates <c>public readonly <see cref="ReadOnlySpan{T}">ReadOnlySpan&lt;T&gt;</see> AsReadOnlySpan()</c> and
        /// <c>public <see cref="Span{T}">Span&lt;T&gt;</see> AsSpan()</c> methods for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Inline arrays are implicitly convertible to <see cref="ReadOnlySpan{T}">ReadOnlySpan&lt;T&gt;</see> and
        /// <see cref="Span{T}">Span&lt;T&gt;</see>. However, these methods may be more readable than an explicit
        /// cast where the cast is not implicit.
        /// </para>
        /// </remarks>
        AsSpanReadOnlySpanMethods = 1 << 1,
        /// <summary>
        /// Generates a <c>public void Clear()</c> method for the collection.
        /// </summary>
        /// <seealso cref="Array.Clear(Array)"/>
        /// <seealso cref="Span{T}.Clear()"/>
        ClearMethod = 1 << 2,
        /// <summary>
        /// Generates collection expression support for the collection by including a
        /// <see cref="CollectionBuilderAttribute">CollectionBuilderAttribute</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The collection builder class is generated from the collection type name.
        /// </para>
        /// </remarks>
        CollectionBuilder = 1 << 3,
        /// <summary>
        /// Generates a <c>public readonly bool Contains(T)</c> method for the collection.
        /// </summary>
        /// <seealso cref="IList.Contains(object)"/>
        /// <seealso cref="ICollection{T}.Contains(T)"/>
        ContainsMethod = 1 << 4,
        /// <summary>
        /// Generates a <c>public readonly void CopyTo(<see cref="Span{T}">Span&lt;T&gt;</see>)</c> method for the
        /// collection.
        /// </summary>
        /// <seealso cref="TryCopyToMethod"/>
        /// <seealso cref="Array.CopyTo(Array, int)"/>
        /// <seealso cref="ReadOnlySpan{T}.CopyTo(Span{T})"/>
        /// <seealso cref="Span{T}.CopyTo(Span{T})"/>
        CopyToMethod = 1 << 5,
        /// <summary>
        /// Generates a <c>public void Fill(T)</c> method for the collection.
        /// </summary>
        /// <seealso cref="Array.Fill{T}(T[], T)"/>
        /// <seealso cref="Span{T}.Fill(T)"/>
        FillMethod = 1 << 6,
        /// <summary>
        /// Generates a <c>public GetEnumerator()</c> method for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This option does not include <see cref="IEnumerable"/>, <see cref="IEnumerableT"/>, or
        /// <see cref="RefStructEnumerator"/>.
        /// </para><para>
        /// If the <see cref="RefStructEnumerator"/> option is included, then the return type of the generated
        /// <c>GetEnumerator</c>() method will be the generated <c>Enumerator</c> type; otherwise, the return type will be
        /// <see cref="IEnumerator{T}">IEnumerator&lt;T&gt;</see>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="RefStructEnumerator"/>
        /// <seealso cref="Array.GetEnumerator()"/>
        /// <seealso cref="IEnumerable.GetEnumerator()"/>
        /// <seealso cref="IEnumerable{T}.GetEnumerator()"/>
        /// <seealso cref="Span{T}.GetEnumerator()"/>
        GetEnumeratorMethod = 1 << 7,
        /// <summary>
        /// Generates an implementation of the <see cref="System.Collections.ICollection">ICollection</see> interface for
        /// the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para><para>
        /// The <see cref="ICollection.SyncRoot">SyncRoot</see> property always throws a
        /// <see cref="NotSupportedException">NotSupportedException</see>.
        /// </para><para>
        /// This option does not include <see cref="ICollectionT"/>.
        /// </para><para>
        /// This option includes <see cref="IEnumerable"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="ICollectionT"/>
        /// <seealso cref="System.Collections.ICollection"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        ICollection = (1 << 8) | IEnumerable,
        /// <summary>
        /// Generates an implementation of the
        /// <see cref="ICollection{T}">ICollection&lt;T&gt;</see> interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="ContainsMethod"/> option is included, then <see cref="ICollection{T}.Contains(T)"/> is
        /// implemented implicitly through that method; otherwise, this method is implemented explicitly. All other
        /// interface members are implemented explicitly.
        /// </para><para>
        /// The <see cref="ICollection{T}.Add(T)">Add(T)</see>, <see cref="ICollection{T}.Clear()">Clear()</see>, and
        /// <see cref="ICollection{T}.Remove(T)">Remove(T)</see> methods always throw a
        /// <see cref="NotSupportedException">NotSupportedException</see>.
        /// </para><para>
        /// This option does not include <see cref="ICollection"/>.
        /// </para><para>
        /// This option includes <see cref="IEnumerable"/> and <see cref="IEnumerableT"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="ICollection"/>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="ICollection{T}"/>
        /// <seealso cref="IEnumerable{T}"/>
        ICollectionT = (1 << 9) | IEnumerable | IEnumerableT,
        /// <summary>
        /// Generates an implementation of the <see cref="System.Collections.IEnumerable">IEnumerable</see> interface for
        /// the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para><para>
        /// If the <see cref="RefStructEnumerator"/> option is included,
        /// <see cref="IEnumerable.GetEnumerator()">GetEnumerator</see>() will throw a
        /// <see cref="NotSupportedException">NotSupportedException</see> at runtime.
        /// </para><para>
        /// This option does not include <see cref="IEnumerableT"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetEnumeratorMethod"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="RefStructEnumerator"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        IEnumerable = 1 << 10,
        /// <summary>
        /// Generates an implementation of the
        /// <see cref="IEnumerable{T}">IEnumerable&lt;T&gt;</see> interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="GetEnumeratorMethod"/> option is included and the <see cref="RefStructEnumerator"/> option is
        /// not included, then this interface is implemented implicitly through that method; otherwise, all interface
        /// members are implemented explicitly.
        /// </para><para>
        /// If the <see cref="RefStructEnumerator"/> option is included,
        /// <see cref="IEnumerable{T}.GetEnumerator()">GetEnumerator</see>() will throw a
        /// <see cref="NotSupportedException">NotSupportedException</see> at runtime.
        /// </para><para>
        /// This option includes <see cref="IEnumerable"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetEnumeratorMethod"/>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="RefStructEnumerator"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="IEnumerable{T}"/>
        IEnumerableT = (1 << 11) | IEnumerable,
        /// <summary>
        /// Generates an implementation of the <see cref="System.Collections.IList">IList</see> interface for the
        /// collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para><para>
        /// The <see cref="IList.Add(object)">Add(object)</see>,
        /// <see cref="IList.Clear()">Clear()</see>,
        /// <see cref="IList.Insert(int, object)">Insert(int, object)</see>,
        /// <see cref="IList.Remove(object)">Remove(object)</see>,
        /// and <see cref="IList.RemoveAt(int)">RemoveAt(int)</see> methods always throw a
        /// <see cref="NotSupportedException">NotSupportedException</see>.
        /// </para><para>
        /// This option does not include <see cref="IListT"/>.
        /// </para><para>
        /// This option includes <see cref="ICollection"/> and <see cref="IEnumerable"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="ICollection"/>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IListT"/>
        /// <seealso cref="System.Collections.ICollection"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="System.Collections.IList"/>
        IList = (1 << 12) | ICollection | IEnumerable,
        /// <summary>
        /// Generates an implementation of the
        /// <see cref="IList{T}">IList&lt;T&gt;</see> interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="IndexOfMethod"/> option is included, then <see cref="IList{T}.IndexOf(T)"/> is implemented
        /// implicitly through that method; otherwise, this method is implemented explicitly. All other interface members
        /// are implemented explicitly.
        /// </para><para>
        /// The <see cref="IList{T}.Insert(int, T)">Insert(int, T)</see> and
        /// <see cref="IList{T}.RemoveAt(int)">RemoveAt(int)</see> methods always throw a
        /// <see cref="NotSupportedException">NotSupportedException</see>.
        /// </para><para>
        /// This option does not include <see cref="IList"/>.
        /// </para><para>
        /// This option includes <see cref="ICollectionT"/>, <see cref="IEnumerable"/>, and <see cref="IEnumerableT"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="ICollectionT"/>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="ICollection{T}"/>
        /// <seealso cref="IEnumerable{T}"/>
        /// <seealso cref="IList{T}"/>
        IListT = (1 << 13) | ICollectionT | IEnumerable | IEnumerableT,
        /// <summary>
        /// Generates a <c>public readonly int IndexOf(T)</c> method for the collection.
        /// </summary>
        /// <seealso cref="Array.IndexOf(T[], T)"/>
        /// <seealso cref="IList.IndexOf(object)"/>
        /// <seealso cref="IList{T}.IndexOf(T)"/>
        IndexOfMethod = 1 << 14,
        /// <summary>
        /// Generates an implementation of the <see cref="IReadOnlyCollection{T}">IReadOnlyCollection&lt;T&gt;</see>
        /// interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para><para>
        /// This option includes <see cref="IEnumerable"/> and <see cref="IEnumerableT"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="IEnumerable{T}"/>
        /// <seealso cref="IReadOnlyCollection{T}"/>
        IReadOnlyCollectionT = (1 << 15) | IEnumerable | IEnumerableT,
        /// <summary>
        /// Generates an implementation of the <see cref="IReadOnlyList{T}">IReadOnlyList&lt;T&gt;</see> interface for the
        /// collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para><para>
        /// This option includes <see cref="IEnumerable"/>, <see cref="IEnumerableT"/>, and
        /// <see cref="IReadOnlyCollectionT"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="IReadOnlyCollectionT"/>
        /// <seealso cref="System.Collections.IEnumerable"/>
        /// <seealso cref="IEnumerable{T}"/>
        /// <seealso cref="IReadOnlyCollection{T}"/>
        IReadOnlyListT = (1 << 16) | IEnumerable | IEnumerableT | IReadOnlyCollectionT,
        /// <summary>
        /// Generates an implementation of the
        /// <see cref="System.Collections.IStructuralComparable">IStructuralComparable</see> interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para>
        /// </remarks>
        /// <seealso cref="System.Collections.IStructuralComparable"/>
        IStructuralComparable = 1 << 17,
        /// <summary>
        /// Generates an implementation of the
        /// <see cref="System.Collections.IStructuralEquatable">IStructuralEquatable</see> interface for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All interface members are implemented explicitly.
        /// </para>
        /// </remarks>
        /// <seealso cref="System.Collections.IStructuralEquatable"/>
        IStructuralEquatable = 1 << 18,
        /// <summary>
        /// Generates a <c>public readonly int Length</c> property for the collection.
        /// </summary>
        /// <seealso cref="Array.Length"/>
        /// <seealso cref="ReadOnlySpan{T}.Length"/>
        /// <seealso cref="Span{T}.Length"/>
        LengthProperty = 1 << 19,
        /// <summary>
        /// Generates a public constructor for the collection which takes a
        /// <see cref="ReadOnlySpan{T}">ReadOnlySpan&lt;T&gt;</see> argument from which the elements of the new instance
        /// will be initialized.
        /// </summary>
        /// <seealso cref="ReadOnlySpan{T}"/>
        ReadOnlySpanConstructor = 1 << 20,
        /// <summary>
        /// Generates a public <c>Enumerator</c> <see langword="ref struct"/> for the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This option includes <see cref="GetEnumeratorMethod"/>, but does not include <see cref="IEnumerable"/> or
        /// <see cref="IEnumerableT"/>.
        /// </para><para>
        /// If the <see cref="IEnumerable"/> or <see cref="IEnumerableT"/> options are included, their respective
        /// <c>GetEnumerator</c>() implementations will throw a
        /// <see cref="NotSupportedException">NotSupportedException</see> at runtime.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetEnumeratorMethod"/>
        /// <seealso cref="IEnumerable"/>
        /// <seealso cref="IEnumerableT"/>
        /// <seealso cref="Span{T}.GetEnumerator()"/>
        RefStructEnumerator = 1 << 21,
        /// <summary>
        /// Generates a <c>public readonly T[] ToArray()</c> method for the collection.
        /// </summary>
        /// <seealso cref="ArrayConversionOperators"/>
        ToArrayMethod = 1 << 22,
        /// <summary>
        /// Generates a <c>public readonly bool TryCopyTo(<see cref="Span{T}">Span&lt;T&gt;</see>)</c> method for the
        /// collection.
        /// </summary>
        /// <seealso cref="CopyToMethod"/>
        TryCopyToMethod = 1 << 23,
        /// <summary>
        /// Generates a collection with all options enabled.
        /// </summary>
        Everything = int.MaxValue
    }
}

#endif // !MONKEYMOTO_INLINECOLLECTIONS_TYPES_ALREADY_DEFINED
