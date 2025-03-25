using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.Internal.Writers;

internal readonly ref struct BigEndianWriter : IKBinWriter, IDisposable
{
    internal readonly RecyclableMemoryStream Stream;

    public BigEndianWriter(int capacity = 0)
    {
        Stream = KbinConverter.RecyclableMemoryStreamManager.GetStream("wbe", capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte singleByte)
    {
        Stream.WriteByte(singleByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(scoped ReadOnlySpan<byte> buffer)
    {
        Stream.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS8(sbyte value)
    {
        WriteByte((byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU8(byte value)
    {
        WriteByte(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS16(short value)
    {
        const int size = sizeof(short);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU16(ushort value)
    {
        const int size = sizeof(ushort);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS32(int value)
    {
        const int size = sizeof(int);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU32(uint value)
    {
        const int size = sizeof(uint);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS64(long value)
    {
        const int size = sizeof(long);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU64(ulong value)
    {
        const int size = sizeof(ulong);
        BitConverterHelper.WriteBeBytes(Stream.GetSpan(size), value);
        Stream.Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Pad()
    {
        while ((Stream.Length & 3) != 0)
        {
            Stream.WriteByte(0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("This method has degraded performance and should be avoided.")]
    public byte[] ToArray()
    {
        return Stream.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Stream.Dispose();
    }
}