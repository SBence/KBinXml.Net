using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

internal struct DataWriter : IKBinWriter, IDisposable
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        ref var pointer = ref _pos32;
        var increment = GetIncrementLength(pointer);

        // 获取足够大小的Span并写入数据
        if (increment >= 0)
        {
            WriteStringCore(value, increment, byteCount);
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;
            WriteStringCore(value, 0, byteCount);
            Stream.Position = streamPosition;
        }

        pointer += byteCount;
        AlignTo4Bytes(ref pointer);
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
        ref var pointer = ref _pos32;
        var increment = GetIncrementLength(pointer);

        // 获取足够大小的Span并写入数据
        if (increment >= 0)
        {
            WriteBinaryCore(value, increment, length);
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;
            WriteBinaryCore(value, 0, length);
            Stream.Position = streamPosition;
        }

        pointer += length;
        AlignTo4Bytes(ref pointer);
        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write8BitAligned(byte value)
    {
        var increment = GetIncrementLength(_pos8);

        if ((_pos8 & 3) == 0)
        {
            _pos32 += 4;
        }

        WriteSingleByte(value, increment, ref _pos8);

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write16BitAligned(scoped ReadOnlySpan<byte> buffer)
    {
        var increment = GetIncrementLength(_pos16);

        if ((_pos16 & 3) == 0)
        {
            _pos32 += 4;
        }

        WriteMultiBytes(buffer, increment, ref _pos16);

        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write32BitAligned(scoped ReadOnlySpan<byte> streamRentBuffer)
    {
        var increment = GetIncrementLength(_pos32);

        WriteMultiBytes(streamRentBuffer, increment, ref _pos32);

        AlignTo4Bytes(ref _pos32);

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
        Write16BitAlignedInternal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU16(ushort value)
    {
        Write16BitAlignedInternal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS32(int value)
    {
        Write32BitAlignedInternal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU32(uint value)
    {
        Write32BitAlignedInternal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteS64(long value)
    {
        Write32BitAlignedInternal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteU64(ulong value)
    {
        Write32BitAlignedInternal(value);
    }

    public void Dispose()
    {
        Stream.Dispose();
    }

    private void Write16BitAlignedInternal<T>(T value) where T : unmanaged
    {
        // 统一实现16位写入的核心逻辑
        const int size = 2; // sizeof(short) or sizeof(ushort)
        ref var pointer = ref _pos16;
        var increment = GetIncrementLength(pointer);

        if ((pointer & 3) == 0) // 如果16位指针是4字节对齐的
        {
            _pos32 += 4;
        }

        // 合并increment==0和increment>0的分支逻辑
        if (increment >= 0)
        {
            var sizeHint = increment + size;
            var span = Stream.GetSpan(sizeHint);
            if (increment > 0)
            {
                span.Slice(0, increment).Clear();
                BitConverterHelper.WriteBeBytesT(span.Slice(increment), value);
            }
            else
            {
                BitConverterHelper.WriteBeBytesT(span, value);
            }

            Stream.Advance(sizeHint);
        }
        else
        {
            Debug.Assert(false);
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytesT(span, value);
            Stream.Advance(size);

            Stream.Position = streamPosition;
        }

        pointer += size;
        Realign16_8();
    }

    private void Write32BitAlignedInternal<T>(T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        ref var pointer = ref _pos32;
        var increment = GetIncrementLength(pointer);

        // 合并increment==0和increment>0的分支逻辑
        if (increment >= 0)
        {
            var sizeHint = increment + size;
            var span = Stream.GetSpan(sizeHint);
            if (increment > 0)
            {
                span.Slice(0, increment).Clear();
                BitConverterHelper.WriteBeBytesT(span.Slice(increment), value);
            }
            else
            {
                BitConverterHelper.WriteBeBytesT(span, value);
            }

            Stream.Advance(sizeHint);
        }
        else
        {
            Debug.Assert(false);
            var streamPosition = Stream.Position;
            Stream.Position = pointer;

            var span = Stream.GetSpan(size);
            BitConverterHelper.WriteBeBytesT(span, value);
            Stream.Advance(size);

            Stream.Position = streamPosition;
        }

        pointer += size;
        AlignTo4Bytes(ref pointer);
        Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteStringCore(string value, int increment, int byteCount)
    {
        var sizeHint = increment > 0 ? byteCount + increment : byteCount;
        var span = Stream.GetSpan(sizeHint);

        if (increment > 0)
        {
            span.Slice(0, increment).Clear();
            span = span.Slice(increment);
        }

#if NETCOREAPP3_1_OR_GREATER
        int bytesWritten = _encoding.GetBytes(value.AsSpan(), span);
        span[bytesWritten] = 0; // 添加结尾的0字节
#else
        var bytes = _encoding.GetBytes(value);
        bytes.CopyTo(span);
        span[bytes.Length] = 0; // 添加结尾的0字节
#endif
        Stream.Advance(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteBinaryCore(string value, int increment, int length)
    {
        var sizeHint = increment > 0 ? length + increment : length;
        var span = Stream.GetSpan(sizeHint);

        if (increment > 0)
        {
            span.Slice(0, increment).Clear();
            span = span.Slice(increment);
        }

        HexConverter.TryDecodeFromUtf16(value.AsSpan(), span.Slice(0, length));
        Stream.Advance(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteSingleByte(byte value, int increment, ref int pointer)
    {
        if (increment >= 0)
        {
            var sizeHint = increment + 1;
            var span = Stream.GetSpan(sizeHint);
            if (increment > 0)
            {
                span.Slice(0, increment).Clear();
                span[increment] = value;
            }
            else
            {
                span[0] = value;
            }

            Stream.Advance(sizeHint);
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;
            Stream.WriteByte(value);
            Stream.Position = streamPosition;
        }

        pointer += 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteMultiBytes(scoped ReadOnlySpan<byte> buffer, int increment, ref int pointer)
    {
        if (increment >= 0)
        {
            var length = buffer.Length;
            var sizeHint = increment + length;
            var span = Stream.GetSpan(sizeHint);
            if (increment > 0)
            {
                span.Slice(0, increment).Clear();
                buffer.CopyTo(span.Slice(increment));
            }
            else
            {
                buffer.CopyTo(span);
            }

            Stream.Advance(sizeHint);
        }
        else
        {
            var streamPosition = Stream.Position;
            Stream.Position = pointer;
            Stream.Write(buffer);

            // fix the problem if the buffer length is greater than list count
            // but looks safe for kbin algorithm
            //if (offset <= Stream.Length)
            Stream.Position = streamPosition;
        }

        pointer += buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIncrementLength(int pointer)
    {
        return (int)(pointer - Stream.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AlignTo4Bytes(ref int pointer)
    {
        var remainder = pointer & 3;
        if (remainder != 0)
        {
            pointer += 4 - remainder;
        }
    }

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
}