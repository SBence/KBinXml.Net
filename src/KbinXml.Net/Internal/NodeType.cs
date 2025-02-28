using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal;

internal class NodeType
{
    public int Size { get; }

    public int Count { get; }

    public string Name { get; }

    public ITypeConverter Converter { get; }

    public NodeType(int size, int count, string name, ITypeConverter converter)
    {
        Size = size;
        Count = count;
        Name = name;
        Converter = converter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        return Converter.WriteString(ref builder, str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetString(ReadOnlySpan<byte> bytes)
    {
        return Converter.ToString(bytes);
    }
    
#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendString(ref ValueStringBuilder stringBuilder, ReadOnlySpan<byte> span)
    {
        Converter.AppendString(ref stringBuilder, span);
    }
#endif
}