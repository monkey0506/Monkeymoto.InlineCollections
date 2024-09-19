# InlineCollections
**Monkeymoto.InlineCollections**

C# incremental generator to add collection-style features to
[inline arrays](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/inline-arrays).

## How to use this project

This project provides an
[incremental generator](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
which can be added to your project. A NuGet package will be provided at a later
time.

After adding the generator to your project, you can declare a `struct` as an
*inline collection* using the `InlineCollectionAttribute`:

````C#
[InlineCollection(Length = 8)]
public partial struct InlineCollection8<T>
{
	private T element0;
}
````

By default, this will only add the
[InlineArrayAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.inlinearrayattribute)
to your type. All other supported features are explicitly opt-in, and can be
selected for code generation using the
[InlineCollectionOptions](InlineCollectionOptions.cs) constructor:

````C#
[InlineCollection(InlineCollectionOptions.IListT, Length = 8)]
public partial struct InlineList8<T>
{
	private T element0;
}
````

The `InlineList8<T>` type will implement the
[IList&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1)
interface.

## Requirements for inline collection types

Types marked for code generation with the `InlineCollectionAttribute` have the
following requirements:

- The `InlineCollectionAttribute` must have a positive, non-zero `Length`
      specified.
- The type must **not** have the
      [InlineArrayAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.inlinearrayattribute).
      This attribute will be added by the generator to the type.
- If the `InlineCollectionOptions.CollectionBuilder` feature is selected, the
      type must **not** have the
      [CollectionBuilderAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.collectionbuilderattribute).
      This attribute will be added by the generator to the type when this
      feature has been selected.
    - Additionally, the type and all containing types must be declared as
          `public` or `internal` when this feature is selected.
- The type must **not** be declared as `file` local or `readonly` and must
      **not** be declared inside of a `file` local type.
- The type must be declared as a `partial struct`. If the type is nested, all
      containing types must also be declared as `partial`.
- The type must declare one and only one instance field. The field type will be
      the element type of the collection. The instance field must **not** be
      declared as `required`, `readonly`, `volatile`, or as a fixed size
      buffer.
- The type must not declare any methods, properties, or explicit interface
      implementations that conflict with the `InlineCollectionOptions` passed
      to the `InlineCollectionAttribute` constructor. For example, if using
      `InlineCollectionOptions.IndexOfMethod`, your type must not declare a
      method with the signature `int IndexOf(T)` (where `T` is the collection's
      element type).

## `InlineCollectionOptions` features

All inline collection features are explicitly opt-in on a per-type basis via
the `InlineCollectionOptions` passed to the `InlineCollectionAttribute`
constructor. The `InlineCollectionOptions` can be combined using the bitwise
`OR` operator:

````C#
[InlineCollection(InlineCollectionOptions.IListT | InlineCollectionOptions.LengthProperty, Length = 8)]
internal struct MyCollection
{
    private int element0;
}
````

This would select both the `InlineCollectionOptions.IListT` and
`InlineCollectionOptions.LengthProperty` features for your collection type
`MyCollection`.

### Interface features

Interface features (those whose name begins with an `interface` name, such as
`InlineCollectionOptions.IEnumerable`) will implement that interface for your
collection type. Generic interface names have a `-T` suffix (such as
`InlineCollectionOptions.IEnumerableT`). The normal interface inheritance rules
are applied, so `InlineCollectionOptions.IEnumerableT` *implies* and will
always include `InlineCollectionOptions.IEnumerable`.

### Method features

Method features (those with a `-Method` suffix, such as
`InlineCollectionOptions.ContainsMethod`) will produce `public` instance
methods for your collection type.

### Operator features

Operator features (those with an `-Operator[s]` suffix, such as
`InlineCollectionOptions.ArrayConversionOperators`) will produce `public
static` operator methods for your collection type.

### Property features

Property features (those with a `-Property` suffix, such as
`InlineCollectionOptions.LengthProperty`) will produce `public readonly`
properties for your collection type. Currently there are no property features
that support modifying the collection, as indexer support is already built-in
for inline arrays.

### Other features

The `InlineCollectionOptions.CollectionBuilder` feature will add support for
collection expressions by generating a collection builder class for your type.
Analyzers will check collection expressions for your type to help detect if
the collection expression is too large for your type.

The `InlineCollectionOptions.RefStructEnumerator` feature will add a
`public ref struct Enumerator` nested type inside your type. When included,
this type will be the return type of the `GetEnumerator()` method (the
`InlineCollectionOptions.GetEnumeratorMethod` feature is implied), and the
[IEnumerable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable)
and
[IEnumerable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)
interfaces' respective `GetEnumerator()` methods (if your type includes those
features) will throw a
[NotSupportedException](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception)
at runtime.

The special feature `InlineCollectionOptions.Everything` will include all
available features for your collection type.

### Selecting the right features

Because inline collections are **value types**, special consideration must be
given if opting-in to any of the [interface features](#Interface-features).
Consuming your inline collection directly via these interfaces will result in a
boxing conversion, which can potentially be expensive.

Additionally, some interface members cannot be meaningfully represented by an
inline collection, such as the
[ICollection&lt;T&gt;.Add(T)](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.add)
method. Inline collections have a fixed size; adding or removing items is not
possible. These unsupported interface members will produce a
[NotSupportedException](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception)
if they are accessed. If selecting to use these interface features, you should
document that these interface members are unsupported. (*This is why all
features are explicitly opt-in.*)

#### Avoiding boxing

If you want to include one or more interface features while still avoiding
expensive boxing conversions, the following technique may be sufficient for
your needs:

````C#
// without `struct` generic constraint: no boxing!
public int GetListCount<T>(T list) where T : IList<T> => list.Count;

// with `struct` generic constraint: no boxing!
public int GetListCountStruct(T list) where T : struct, IList<T> => list.Count;
````

In both of these use cases, `list` is **not** boxed, even when accessing the
`Count` interface property. If you pass your collection *directly* as a method
argument of an interface type, or store a reference of interface type to your
collection value, then boxing **will** occur:

````C#
// with interface-typed parameter: boxing!
public int GetBoxedCount<T>(IList<T> list) => list.Count;

// interface-typed reference: boxing!
MyCollection collection = [1, 2, 3, 4]; // inline collection (value type)
IList<int> list = collection; // interface conversion = boxing
````

#### Avoiding boxing: Enumerators

The
[IEnumerable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable)
and
[IEnumerable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)
interfaces will **always** result in a boxing conversion if used by your
collection. Because these interfaces are implicitly included by many of the
other interface features, it is unavoidable to include those features without
also including these interfaces.

Boxing conversions during enumeration may still be avoided by including the
`InlineCollectionOptions.RefStructEnumerator` feature. This type will allow
your collection to implement an explicit `GetEnumerator()` method that does not
require boxing.

*Note: Inline arrays already support loop-based enumeration out-of-the-box.
This feature is designed for scenarios where you may want explicit control over
the enumerator.*

## Motivation

The
[C# LDT](https://github.com/dotnet/csharplanghttps://github.com/dotnet/csharplang)
have made clear their position on array-like and collection-like features being
built-in to the inline array feature:

> InlineArray is intended to be the hidden backing storage, that is not used
> directly. Instead, you can just wrap it freely as a span, and then get all
> the niceties of that higher level abstraction.
> —[CyrusNajmabadi via csharplang](https://github.com/dotnet/csharplang/discussions/8177)

> ...you \[can\] safely access [an inline array] through a Span or
> ReadOnlySpan. That's the published and expected pattern for working with
inline arrays.
> 
> We don't have a desire for the compiler to reimplement the span surface area
> on these backing segments. The idea is to then use span to uniformly work
> with them like any other segment of memory (on the stack, heap, data
> segment whatever). —*CyrusNajmabadi*

This is a reasonable position for them to take, as otherwise they would
constantly be chasing the surface area changes of arrays and spans (among other
types). However, there is no technical or practical reason that an end-user
cannot simply implement features such as a `Length` property themselves:

````C#
[InlineArray(8)]
public struct Buffer8<T>
{
	private T element0;

	public int Length => 8;
}
````

The compiler knows at all times that the constant size of a `Buffer8<T>` is
exactly 8 elements, but the consumer may not necessarily know the exact size,
and `ref struct`s (like `Span<T>`) aren't suited to every *conceivable*
use-case of an inline array.

A `Length` property is achieved simply, but adding additional features becomes
more complex. For example, adding collection expression support:

````C#
[CollectionBuilder(Buffer8_T_Builder, "Create")]
[InlineArray(8)]
public struct Buffer8<T>
{
	private T element0;

	public int Length => 8;
}

file static class Buffer8_T_Builder
{
	public static Buffer8<T> Create<T>(ReadOnlySpan<T> span)
	{
		var buffer = new Buffer8<T>();
		span.CopyTo(buffer);
		return buffer;
	}
}
````

This will enable `Buffer8<T>` to be used with collection expressions, but if
the user supplies too many elements in the collection expression the compiler
will not (*currently*) detect this (even though it knows the size of the inline
array!). The user will be left with a runtime exception being thrown.

Because a collection expression may contain one or more spread elements, it is
not possible for static analysis to determine the exact size that a collection
expression will expand to. However, it is possible to count the number of
elements in the collection expression that are not spread elements, or to
examine if the total number of elements *suggests* an expansion to a size
larger than our inline array type. For this, we need an analyzer.

While it is consistent and logical that the LDT does not want to add built-in
support for using inline arrays as normal collection types, much of the surface
area between arrays, spans, and collection-type interfaces can be meaningfully
represented by an inline array. The aim of this project is to support
generating the boilerplate necessary to support these features, and where
possible provide meaningful diagnostics at compile-time and prevent runtime
exceptions.
