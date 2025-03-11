using System;
using System.IO;
using System.Runtime.CompilerServices;
using KbinXml.Net.Utils;

namespace KbinXml.Net.HighPerformance.Writers;

#if NET8_0_OR_GREATER
internal ref partial struct DataWriter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte singleByte)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(scoped ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException();
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

    public void WriteU16(ushort value)
    {
        throw new NotImplementedException();
    }

    public void WriteS32(int value)
    {
        throw new NotImplementedException();
    }

    public void WriteU32(uint value)
    {
        throw new NotImplementedException();
    }

    public void WriteS64(long value)
    {
        throw new NotImplementedException();
    }

    public void WriteU64(ulong value)
    {
        throw new NotImplementedException();
    }
}
#endif