using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class S16Converter : ITypeConverter
{
    private S16Converter()
    {
    }

    public static S16Converter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        return BitConverterHelper.WriteBeBytes(ref builder, ParseHelper.ParseInt16(str));
        // 返回 2（大端字节序写入 2 个字节）
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        return BitConverterHelper.ToBeInt16(bytes).ToString();
    }
}