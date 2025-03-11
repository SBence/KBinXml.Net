using KbinXml.Net.Utils;

namespace KbinXml.Net.HighPerformance.Readers;

internal partial struct DataReader
{
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