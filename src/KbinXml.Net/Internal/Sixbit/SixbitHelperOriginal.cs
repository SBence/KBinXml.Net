using System;
using System.Runtime.CompilerServices;

namespace KbinXml.Net.Internal.Sixbit;

internal static class SixbitHelperOriginal
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Encode(ReadOnlySpan<byte> buffer, Span<byte> output)
    {
        for (var i = 0; i < buffer.Length * 6; i++)
            output[i >> 3] = (byte)(output[i >> 3] |
                                    (buffer[i / 6] >> 5 - i % 6 & 1) << 7 - (i & 7));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decode(ReadOnlySpan<byte> buffer, Span<byte> input)
    {
        for (var i = 0; i < input.Length * 6; i++)
            input[i / 6] = (byte)(input[i / 6] |
                                  (buffer[i >> 3] >> 7 - (i & 7) & 1) << 5 - i % 6);
    }
}