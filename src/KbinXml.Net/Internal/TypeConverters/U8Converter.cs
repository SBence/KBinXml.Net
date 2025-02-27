using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal.TypeConverters;

internal sealed class U8Converter : ITypeConverter
{
    private U8Converter()
    {
    }

    public static U8Converter Instance { get; } = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str)
    {
        var numberStyle = ConvertHelper.GetNumberStyle(str, out str);
        builder.Append(ParseHelper.ParseByte(str, numberStyle));
        return 1; // 写入 1 个字节
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(ReadOnlySpan<byte> bytes)
    {
        return bytes[0].ToString();
    }
}