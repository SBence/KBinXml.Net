using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Internal;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

internal partial struct DataWriter : IKBinWriter, IDisposable
{
    internal readonly RecyclableMemoryStream Stream;

    private readonly Encoding _encoding;

#if NETCOREAPP3_1_OR_GREATER
    private readonly int _shiftVal;
#endif

    private int _pos32;
    private int _pos16;
    private int _pos8;

    public DataWriter(Encoding encoding, int capacity = 0)
    {
        _encoding = encoding;
#if NETCOREAPP3_1_OR_GREATER
        _shiftVal = EncodingDictionary.ReverseEncodingMap[encoding] switch
        {
            0x00 => 1,
            0x20 => 1,
            0x40 => 1,
            0x60 => 2,
            0x80 => 2,
            0xA0 => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(encoding), encoding, null)
        };
#endif
        Stream = KbinConverter.RecyclableMemoryStreamManager.GetStream("wd", capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte singleByte)
    {
        Write8BitAligned(singleByte);
    }

    public void WriteBytes(scoped ReadOnlySpan<byte> buffer)
    {
        switch (buffer.Length)
        {
            case 1:
                Write8BitAligned(buffer[0]);
                break;

            case 2:
                Write16BitAligned(buffer);
                break;

            default:
                Write32BitAligned(buffer);
                break;
        }
    }

    public void WriteString(string value)
    {
        var bytes = _encoding.GetBytes(value);

        var length = bytes.Length + 1;
        byte[]? arr = null;
        Span<byte> span = length <= Constants.MaxStackLength
            ? stackalloc byte[length]
            : (arr = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);
        try
        {
            bytes.CopyTo(span);

            WriteU32((uint)length);
            Write32BitAligned(span);
        }
        finally
        {
            if (arr != null) ArrayPool<byte>.Shared.Return(arr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBinary(string value)
    {
        var length = value.Length >> 1;
        WriteU32((uint)length);
        byte[]? arr = null;
        Span<byte> span = length <= Constants.MaxStackLength
            ? stackalloc byte[length]
            : (arr = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length);
        try
        {
            HexConverter.TryDecodeFromUtf16(value.AsSpan(), span);
            Write32BitAligned(span);
        }
        finally
        {
            if (arr != null) ArrayPool<byte>.Shared.Return(arr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write8BitAligned(byte value)
    {
        PadStream(_pos8);

        if ((_pos8 & 3) == 0)
        {
            _pos32 += 4;
        }

        WriteSingleByte(value, ref _pos8);

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write16BitAligned(scoped ReadOnlySpan<byte> buffer)
    {
        PadStream(_pos16);

        if ((_pos16 & 3) == 0)
        {
            _pos32 += 4;
        }

        WriteMultiBytes(buffer, ref _pos16);

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write32BitAligned(scoped ReadOnlySpan<byte> streamRentBuffer)
    {
        PadStream(_pos32);

        WriteMultiBytes(streamRentBuffer, ref _pos32);

        var left = _pos32 & 3;
        if (left != 0)
        {
            _pos32 += 4 - left;
        }

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PadStream()
    {
        while ((Stream.Length & 3) != 0)
        {
            Stream.WriteByte(0);
        }
    }

    public void PadStream(int pointer)
    {
        int incrementLength = (int)(pointer - Stream.Length);
        if (incrementLength <= 0) return;
        if (incrementLength == 1) Stream.WriteByte(0);
        else
        {
            Stream.GetSpan(incrementLength).Slice(0, incrementLength).Clear();
            Stream.Advance(incrementLength);
        }
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

    public void WriteS16(short value)
    {
        const int size = sizeof(short);
        ref var pointer = ref _pos16;
        PadStream(pointer);

        if ((pointer & 3) == 0) // pointer % 4
        {
            _pos32 += 4;
        }

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        Realign16_8();
    }

    public void WriteU16(ushort value)
    {
        const int size = sizeof(ushort);
        ref var pointer = ref _pos16;
        PadStream(pointer);

        if ((pointer & 3) == 0) // pointer % 4
        {
            _pos32 += 4;
        }

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        Realign16_8();
    }

    public void WriteS32(int value)
    {
        const int size = sizeof(int);
        ref var pointer = ref _pos32;
        PadStream(pointer);

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        var left = pointer & 3;
        if (left != 0)
        {
            pointer += 4 - left;
        }

        Realign16_8();
    }

    public void WriteU32(uint value)
    {
        const int size = sizeof(uint);
        ref var pointer = ref _pos32;
        PadStream(pointer);

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        var left = pointer & 3;
        if (left != 0)
        {
            pointer += 4 - left;
        }

        Realign16_8();
    }

    public void WriteS64(long value)
    {
        const int size = sizeof(long);
        ref var pointer = ref _pos32;
        PadStream(pointer);

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        var left = pointer & 3;
        if (left != 0)
        {
            pointer += 4 - left;
        }

        Realign16_8();
    }

    public void WriteU64(ulong value)
    {
        const int size = sizeof(ulong);
        ref var pointer = ref _pos32;
        PadStream(pointer);

        if (pointer == Stream.Length)
        {
            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytes(span, value);
            Stream.Advance(size);

            pointer += size;
            Stream.Position = streamPosition;
        }

        var left = pointer & 3;
        if (left != 0)
        {
            pointer += 4 - left;
        }

        Realign16_8();
    }

    public void Dispose()
    {
        Stream.Dispose();
    }

    //private ref int GetPointer(int size)
    //{
    //    switch (size)
    //    {
    //        case 1: return ref _pos8;
    //        case 2: return ref _pos16;
    //        default: return ref _pos32;
    //    }
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Realign16_8()
    {
        if ((_pos8 & 3) == 0)
        {
            _pos8 = _pos32;
        }

        if ((_pos16 & 3) == 0)
        {
            _pos16 = _pos32;
        }
    }

    private void WriteMultiBytes(scoped ReadOnlySpan<byte> buffer, ref int pointer)
    {
        if (pointer == Stream.Length)
        {
            Stream.WriteSpan(buffer);
            pointer += buffer.Length;
        }
        else
        {
            var streamPosition = Stream.Position;

            Stream.Position = pointer;
            Stream.WriteSpan(buffer);
            pointer += buffer.Length;

            // fix the problem if the buffer length is greater than list count
            // but looks safe for kbin algorithm
            //if (offset <= Stream.Length)
            Stream.Position = streamPosition;
        }
    }

    private void WriteSingleByte(byte value, ref int pointer)
    {
        if (pointer == Stream.Length)
        {
            Stream.WriteByte(value);
            pointer += 1;
        }
        else
        {
            var streamPosition = Stream.Position;

            Stream.Position = pointer;
            Stream.WriteByte(value);
            pointer += 1;

            Stream.Position = streamPosition;
        }
    }
}
