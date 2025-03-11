using KbinXml.Net.Utils;

namespace KbinXml.Net.HighPerformance.Readers;

internal partial struct NodeReader
{
    public SpanReadResult ReadBytes(int count)
    {
        var result = _span.Slice(_position, count);
        var readSpanResult = new SpanReadResult
        (
            result
#if USELOG
            , new ReadStatus { Offset = _position, Length = count }
#endif
        );
        _position += count;
        return readSpanResult;
    }

    public ValueReadResult<sbyte> ReadS8()
    {
        var result = ReadBytes(sizeof(sbyte));
        return new ValueReadResult<sbyte>
        (
            (sbyte)result.Span[0]
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<short> ReadS16()
    {
        var result = ReadBytes(sizeof(short));
        return new ValueReadResult<short>
        (
            BitConverterHelper.ToBeInt16(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<int> ReadS32()
    {
        var result = ReadBytes(sizeof(int));
        return new ValueReadResult<int>
        (
            BitConverterHelper.ToBeInt32(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<long> ReadS64()
    {
        var result = ReadBytes(sizeof(long));
        return new ValueReadResult<long>
        (
            BitConverterHelper.ToBeInt64(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<byte> ReadU8()
    {
        var result = ReadBytes(sizeof(byte));
        return new ValueReadResult<byte>
        (
            (byte)result.Span[0]
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<ushort> ReadU16()
    {
        var result = ReadBytes(sizeof(ushort));
        return new ValueReadResult<ushort>
        (
            BitConverterHelper.ToBeUInt16(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<uint> ReadU32()
    {
        var result = ReadBytes(sizeof(uint));
        return new ValueReadResult<uint>
        (
            BitConverterHelper.ToBeUInt32(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }

    public ValueReadResult<ulong> ReadU64()
    {
        var result = ReadBytes(sizeof(ulong));
        return new ValueReadResult<ulong>
        (
            BitConverterHelper.ToBeUInt64(result.Span)
#if USELOG
            , result.ReadStatus
#endif
        );
    }
}