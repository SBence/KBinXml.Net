using System;

namespace KbinXml.Net.HighPerformance;

internal interface IKBinReader
{
    public SpanReadResult ReadBytes(int count);
    public ValueReadResult<sbyte> ReadS8();
    public ValueReadResult<short> ReadS16();
    public ValueReadResult<int> ReadS32();
    public ValueReadResult<long> ReadS64();
    public ValueReadResult<byte> ReadU8();
    public ValueReadResult<ushort> ReadU16();
    public ValueReadResult<uint> ReadU32();
    public ValueReadResult<ulong> ReadU64();
}

internal interface IKBinWriter
{
    public void WriteByte(byte singleByte);
    public void WriteBytes(ReadOnlySpan<byte> buffer);
    public void WriteS8(sbyte value);
    public void WriteU8(byte value);
    public void WriteS16(short value);
    public void WriteU16(ushort value);
    public void WriteS32(int value);
    public void WriteU32(uint value);
    public void WriteS64(long value);
    public void WriteU64(ulong value);
}