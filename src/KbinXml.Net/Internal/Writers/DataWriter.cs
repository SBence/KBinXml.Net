﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.Internal.Writers;

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
            Debug.Assert(increment >= 0);
            _pos32 += 4;
        }
        else
        {
            Debug.Assert(increment <= 0);
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
            Debug.Assert(increment >= 0);
            _pos32 += 4;
        }
        else
        {
            Debug.Assert(increment <= 0);
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
        var remainder = (int)(Stream.Length & 3);
        switch (remainder)
        {
            case 1: Stream.WriteByte(0); Stream.WriteByte(0); Stream.WriteByte(0); break;
            case 2: Stream.WriteByte(0); Stream.WriteByte(0); break;
            case 3: Stream.WriteByte(0); break;
        }
    }

    public void Dispose()
    {
        Stream.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteStringCore(string value, int increment, int byteCount)
    {
        var sizeHint = increment > 0 ? byteCount + increment : byteCount;
        var span = Stream.GetSpan(sizeHint);

        if (increment > 0)
        {
            ClearSpan(span, increment);
            span = span.Slice(increment);
        }

#if NETCOREAPP3_1_OR_GREATER
        int bytesWritten = _encoding.GetBytes(value.AsSpan(), span);
        span[bytesWritten] = 0; // 添加结尾的0字节
#else
        int bytesWritten = _encoding.GetByteCount(value);
        using (var rentedArray = new RentedArray<byte>(ArrayPool<byte>.Shared, bytesWritten))
        {
            int bytesEncoded = _encoding.GetBytes(value, 0, value.Length, rentedArray.Array, 0);
            rentedArray.Array.AsSpan(0, bytesEncoded).CopyTo(span);
        }

        span[bytesWritten] = 0; // 添加结尾的0字节
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
            ClearSpan(span, increment);
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
                ClearSpan(span, increment);
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

        pointer++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteMultiBytes(scoped ReadOnlySpan<byte> buffer, int increment, ref int pointer)
    {
        if (increment >= 0)
        {
            var sizeHint = increment + buffer.Length;
            var span = Stream.GetSpan(sizeHint);
            if (increment > 0)
            {
                ClearSpan(span, increment);
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
        return pointer - (int)Stream.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Realign16_8()
    {
        if ((_pos8 & 3) == 0) _pos8 = _pos32;
        if ((_pos16 & 3) == 0) _pos16 = _pos32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ClearSpan(Span<byte> span, int increment)
    {
        if (increment == 1)
        {
            span[0] = 0;
        }
        else if (increment == 2)
        {
            span[0] = 0;
            span[1] = 0;
        }
        else if (increment == 3)
        {
            span[0] = 0;
            span[1] = 0;
            span[2] = 0;
        }
        else if (increment > 0)
        {
            span.Slice(0, increment).Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AlignTo4Bytes(ref int pointer)
    {
        pointer = (pointer + 3) & ~3;
    }
}