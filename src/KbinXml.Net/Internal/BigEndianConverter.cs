using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal;

internal static class BigEndianConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadBytes(ReadOnlySpan<byte> buffer, int index, int count)
    {
        return buffer.Slice(index, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ToSByte(ReadOnlySpan<byte> buffer, int index)
    {
        return (sbyte)ReadBytes(buffer, index, sizeof(sbyte))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ToInt16(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeInt16(ReadBytes(buffer, index, sizeof(short)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeInt32(ReadBytes(buffer, index, sizeof(int)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToInt64(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeInt64(ReadBytes(buffer, index, sizeof(long)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ToByte(ReadOnlySpan<byte> buffer, int index)
    {
        return ReadBytes(buffer, index, sizeof(byte))[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ToUInt16(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeUInt16(ReadBytes(buffer, index, sizeof(ushort)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt32(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeUInt32(ReadBytes(buffer, index, sizeof(uint)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64(ReadOnlySpan<byte> buffer, int index)
    {
        return BitConverterHelper.ToBeUInt64(ReadBytes(buffer, index, sizeof(ulong)));
    }
}