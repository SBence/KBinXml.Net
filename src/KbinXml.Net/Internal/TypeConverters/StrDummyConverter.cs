using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class StrDummyConverter : ITypeConverter
{
    private StrDummyConverter()
    {
    }

    public static StrDummyConverter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        throw new NotSupportedException("String data should not be written as string.");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        throw new NotSupportedException("String data should not be converted to string.");
    }
}