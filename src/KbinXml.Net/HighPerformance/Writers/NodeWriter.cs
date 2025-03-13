using System;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Internal;
using Microsoft.IO;

namespace KbinXml.Net.HighPerformance.Writers;

internal readonly ref partial struct NodeWriter
{
    public readonly RecyclableMemoryStream Stream;
    public readonly bool Compressed;

    private readonly Encoding _encoding;

    public NodeWriter(bool compressed, Encoding encoding, int capacity = 0)
    {
        Stream = KbinConverter.RecyclableMemoryStreamManager.GetStream("wbe", capacity);
        Compressed = compressed;
        _encoding = encoding;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string value)
    {
        if (Compressed)
        {
            WriteU8((byte)value.Length);
            SixbitHelper.EncodeAndWrite(Stream, value);
        }
        else
        {
            WriteU8((byte)(value.Length - 1 | 1 << 6));
            WriteBytes(_encoding.GetBytes(value));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("This method has degraded performance and should be avoided.")]
    public byte[] ToArray()
    {
        return Stream.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Stream.Dispose();
    }
}