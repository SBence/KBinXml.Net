using System;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

internal partial struct DataWriter : IKBinWriter, IDisposable
{
    internal readonly RecyclableMemoryStream Stream;
    private readonly Encoding _encoding;

    private int _pos32;
    private int _pos16;
    private int _pos8;

    public DataWriter(Encoding encoding, int capacity = 0)
    {
        _encoding = encoding;
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
        // 计算编码后的字节长度（包括结尾的0字节）
        int byteCount = _encoding.GetByteCount(value) + 1;

        // 先写入长度
        WriteU32((uint)byteCount);

        // 准备写入数据（32位对齐）
        PadStream(_pos32);

        // 获取足够大小的Span
        if (_pos32 == Stream.Length)
        {
            var span = Stream.GetSpan(byteCount);
#if NETCOREAPP3_1_OR_GREATER
            int bytesWritten = _encoding.GetBytes(value.AsSpan(), span);
            span[bytesWritten] = 0; // 添加结尾的0字节
#else
            var bytes = _encoding.GetBytes(value);
            bytes.CopyTo(span);
            span[bytes.Length] = 0; // 添加结尾的0字节
#endif
            Stream.Advance(byteCount);

            _pos32 += byteCount;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = _pos32;

            var span = Stream.GetSpan(byteCount);
#if NETCOREAPP3_1_OR_GREATER
            int bytesWritten = _encoding.GetBytes(value.AsSpan(), span);
            span[bytesWritten] = 0; // 添加结尾的0字节
#else
            var bytes = _encoding.GetBytes(value);
            bytes.CopyTo(span);
            span[bytes.Length] = 0; // 添加结尾的0字节
#endif
            Stream.Advance(byteCount);

            _pos32 += byteCount;
            Stream.Position = streamPosition;
        }

        // 处理对齐
        var left = _pos32 & 3;
        if (left != 0)
        {
            _pos32 += 4 - left;
        }

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBinary(string value)
    {
        // 计算二进制数据的长度（每两个字符表示一个字节）
        int length = value.Length >> 1;

        // 先写入长度
        WriteU32((uint)length);

        // 准备写入数据（32位对齐）
        PadStream(_pos32);

        // 获取足够大小的Span并写入数据
        if (_pos32 == Stream.Length)
        {
            var span = Stream.GetSpan(length);
            HexConverter.TryDecodeFromUtf16(value.AsSpan(), span.Slice(0, length));
            Stream.Advance(length);

            _pos32 += length;
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = _pos32;

            var span = Stream.GetSpan(length);
            HexConverter.TryDecodeFromUtf16(value.AsSpan(), span.Slice(0, length));
            Stream.Advance(length);

            _pos32 += length;
            Stream.Position = streamPosition;
        }

        // 处理对齐
        var left = _pos32 & 3;
        if (left != 0)
        {
            _pos32 += 4 - left;
        }

        Realign16_8();
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
