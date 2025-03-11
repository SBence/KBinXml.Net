using System;
using KbinXml.Net.Utils;

namespace KbinXml.Net.HighPerformance.Readers;

internal ref struct BigEndianReader : IKBinReader
{
    private readonly ReadOnlySpan<byte> _span;

    public BigEndianReader(ReadOnlySpan<byte> span)
    {
        _span = span;
    }

    public int Position { get; private set; }

    public SpanReadResult ReadBytes(int count)
    {
        var result = _span.Slice(Position, count);
        var readSpanResult = new SpanReadResult
        {
            Span = result,
#if USELOG
            ReadStatus = new ReadStatus { Offset = _pos, Length = count }
#endif
        };
        Position += count;
        return readSpanResult;
    }

    public ValueReadResult<sbyte> ReadS8()
    {
        var result = ReadBytes(sizeof(sbyte));
        return new ValueReadResult<sbyte>
        {
            Result = (sbyte)result.Span[0],
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<short> ReadS16()
    {
        var result = ReadBytes(sizeof(short));
        return new ValueReadResult<short>
        {
            Result = BitConverterHelper.ToBeInt16(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<int> ReadS32()
    {
        var result = ReadBytes(sizeof(int));
        return new ValueReadResult<int>
        {
            Result = BitConverterHelper.ToBeInt32(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<long> ReadS64()
    {
        var result = ReadBytes(sizeof(long));
        return new ValueReadResult<long>
        {
            Result = BitConverterHelper.ToBeInt64(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<byte> ReadU8()
    {
        var result = ReadBytes(sizeof(byte));
        return new ValueReadResult<byte>
        {
            Result = (byte)result.Span[0],
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<ushort> ReadU16()
    {
        var result = ReadBytes(sizeof(ushort));
        return new ValueReadResult<ushort>
        {
            Result = BitConverterHelper.ToBeUInt16(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<uint> ReadU32()
    {
        var result = ReadBytes(sizeof(uint));
        return new ValueReadResult<uint>
        {
            Result = BitConverterHelper.ToBeUInt32(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }

    public ValueReadResult<ulong> ReadU64()
    {
        var result = ReadBytes(sizeof(ulong));
        return new ValueReadResult<ulong>
        {
            Result = BitConverterHelper.ToBeUInt64(result.Span),
#if USELOG
            ReadStatus = result.ReadStatus
#endif
        };
    }
}