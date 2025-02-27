using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class S8Converter : ITypeConverter
{
    private S8Converter()
    {
    }

    public static S8Converter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        builder.Append((byte)ParseHelper.ParseSByte(str));
        return 1; // 写入 1 个字节
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        return ((sbyte)bytes[0]).ToString();
    }
}