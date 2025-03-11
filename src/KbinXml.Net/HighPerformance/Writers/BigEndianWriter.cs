using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

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

//internal readonly ref struct BigEndianWriter : IDisposable
//{
//    private delegate int WriteDelegate<in T>(Span<byte> span, T value);

//    private static readonly WriteDelegate<short> WriteS16Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<ushort> WriteU16Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<int> WriteS32Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<uint> WriteU32Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<long> WriteS64Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<ulong> WriteU64Delegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<float> WriteFloatDelegate = BitConverterHelper.WriteBeBytes;
//    private static readonly WriteDelegate<double> WriteDoubleDelegate = BitConverterHelper.WriteBeBytes;

//    internal readonly RecyclableMemoryStream Stream;

//    public BigEndianWriter(int capacity = 0)
//    {
//        Stream = KbinConverter.RecyclableMemoryStreamManager.GetStream("wbe", capacity);
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteByte(byte singleByte) => Stream.WriteByte(singleByte);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteBytes(scoped ReadOnlySpan<byte> buffer) => Stream.WriteSpan(buffer);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteS8(sbyte value) => WriteByte((byte)value);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteU8(byte value) => Stream.WriteByte(value);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteS16(short value) => WriteToStream(value, sizeof(short), WriteS16Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteU16(ushort value) => WriteToStream(value, sizeof(ushort), WriteU16Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteS32(int value) => WriteToStream(value, sizeof(int), WriteS32Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteU32(uint value) => WriteToStream(value, sizeof(uint), WriteU32Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteS64(long value) => WriteToStream(value, sizeof(long), WriteS64Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteU64(ulong value) => WriteToStream(value, sizeof(ulong), WriteU64Delegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteFloat(float value) => WriteToStream(value, sizeof(float), WriteFloatDelegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void WriteDouble(double value) => WriteToStream(value, sizeof(double), WriteDoubleDelegate);

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void Dispose() => Stream.Dispose();

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public byte[] ToArray() => Stream.ToArray();

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    internal void Pad()
//    {
//        var remainder = (int)(Stream.Length & 3);
//        switch (remainder)
//        {
//            case 1: Stream.WriteByte(0); Stream.WriteByte(0); Stream.WriteByte(0); break;
//            case 2: Stream.WriteByte(0); Stream.WriteByte(0); break;
//            case 3: Stream.WriteByte(0); break;
//        }
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private void WriteToStream<T>(T value, int size, WriteDelegate<T> writeMethod)
//    {
//        var buffer = Stream.GetBuffer();
//        var position = (int)Stream.Position;

//        if (position + size > buffer.Length)
//        {
//            Span<byte> span = stackalloc byte[size];
//            writeMethod(span, value);
//            WriteBytes(span);
//        }
//        else
//        {
//            writeMethod(buffer.AsSpan(position), value);
//            Stream.Position = position + size;
//        }
//    }
//}