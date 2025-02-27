using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class DoubleConverter : ITypeConverter
{
    private DoubleConverter()
    {
    }

    public static DoubleConverter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        return BitConverterHelper.WriteBeBytes(ref builder, ParseHelper.ParseDouble(str, ConvertHelper.UsNumberFormat));
        // 返回 8（大端字节序写入 8 个字节）
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        return BitConverterHelper.ToBeDouble(bytes).ToString("0.000000"); // 保留 6 位小数
    }
}