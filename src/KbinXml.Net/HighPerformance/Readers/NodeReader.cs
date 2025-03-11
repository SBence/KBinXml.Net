using System;
using System.Runtime.CompilerServices;
using System.Text;
using KbinXml.Net.Internal;

namespace KbinXml.Net.HighPerformance.Readers;

internal ref partial struct NodeReader : IKBinReader
{
    private readonly ReadOnlySpan<byte> _span;
    private readonly Encoding _encoding;
    private readonly bool _compressed;

    public NodeReader(ReadOnlySpan<byte> span, Encoding encoding, bool compressed)
    {
        _span = span;
        _compressed = compressed;
        _encoding = encoding;
    }

    public int Position { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueReadResult<string> ReadString()
    {
        var result = ReadU8();
        var length = result.Result;
        return _compressed
            ? ReadCompressedString(length)
            : ReadUncompressedString(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueReadResult<string> ReadCompressedString(byte length)
    {
        var spanResult = ReadBytes((int)Math.Ceiling(length * 6 / 8.0));
        var readString = SixbitHelper.Decode(spanResult.Span, length);
        return new ValueReadResult<string>
        {
            Result = readString,
#if USELOG
            ReadStatus = spanResult.ReadStatus
#endif
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ValueReadResult<string> ReadUncompressedString(byte length)
    {
        var readSpanResult = ReadBytes((length & 0xBF) + 1);

#if NETSTANDARD2_1 || NETCOREAPP3_1_OR_GREATER
        return new ValueReadResult<string>
        {
            Result = _encoding.GetString(readSpanResult.Span),
#if USELOG
            ReadStatus = readSpanResult.ReadStatus
#endif
        };
#else
        fixed (byte* p = readSpanResult.Span)
        {
            return new ValueReadResult<string>
            {
                Result = _encoding.GetString(p, readSpanResult.Span.Length),
#if USELOG
                ReadStatus = readSpanResult.ReadStatus
#endif
            };
        }
#endif
    }
}