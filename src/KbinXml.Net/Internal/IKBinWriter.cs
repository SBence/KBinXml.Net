using System;

namespace KbinXml.Net.Internal;

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