namespace KbinXml.Net.Internal;

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