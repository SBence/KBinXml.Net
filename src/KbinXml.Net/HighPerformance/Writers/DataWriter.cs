using System;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

#if NET8_0_OR_GREATER
internal ref partial struct DataWriter : IKBinWriter, IDisposable
{
    private ref int _pos32;
    private ref int _pos16;
    private ref int _pos8;

    internal readonly RecyclableMemoryStream Stream;

    public DataWriter(int capacity = 0)
    {
        Stream = KbinConverter.RecyclableMemoryStreamManager.GetStream("wd", capacity);
    }

    public void WriteS16(short value)
    {
        const int size = sizeof(short);
        ref var pointer = ref GetPointer(size);
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

    private void PadStream(int pointer)
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

    private ref int GetPointer(int size)
    {
        switch (size)
        {
            case 1: return ref _pos8;
            case 2: return ref _pos16;
            default: return ref _pos32;
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

    public void Dispose()
    {
        Stream.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(scoped ReadOnlySpan<byte> buffer, int wroteCount)
    {
        //if (wroteCount == 1)
        //    Write8BitAligned(buffer, wroteCount);
        //else if (wroteCount == 2)
        //    Write16BitAligned(buffer);
        //else
        //    Write32BitAligned(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write8BitAligned(scoped ReadOnlySpan<byte> buffer, int wroteCount)
    {
        //Pad(_pos8);

        //if ((_pos8 & 3) == 0)
        //{
        //    _pos32 += 4;
        //}

        //DirectWriteByte(value, ref _pos8);
        //Realign16_8();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Pad(int target)
    {
        int left = (int)(target - Stream.Length);
        if (left <= 0) return;
#if NETCOREAPP3_1_OR_GREATER
        if (left == 1) Stream.WriteByte(0);
        else
        {
            // looks safe for kbin algorithm
            //Span<byte> readOnlySpan = stackalloc byte[left];
            Stream.Write(stackalloc byte[left]);
            //byte[]? arr = null;
            //Span<byte> span = left <= Constants.MaxStackLength
            //    ? stackalloc byte[left]
            //    : arr = ArrayPool<byte>.Shared.Rent(left);
            //if (arr != null) span = span.Slice(0, left);
            //try
            //{
            //    Stream.Write(span);
            //}
            //finally
            //{
            //    if (arr != null) ArrayPool<byte>.Shared.Return(arr);
            //}
        }
#else
        for (int i = 0; i < left; i++)
            Stream.WriteByte(0);
#endif
    }

}
#endif