using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class FloatConverter : ITypeConverter
{
    private FloatConverter()
    {
    }

    public static FloatConverter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        return BitConverterHelper.WriteBeBytes(ref builder, ParseHelper.ParseSingle(str, ConvertHelper.UsNumberFormat));
        // 返回 4（大端字节序写入 4 个字节）
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        return BitConverterHelper.ToBeSingle(bytes).ToString("0.000000"); // 保留 6 位小数
    }
}