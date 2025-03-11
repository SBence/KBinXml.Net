using System;

namespace KbinXml.Net.HighPerformance;

public readonly ref struct SpanReadResult
{
    public readonly ReadOnlySpan<byte> Span;
#if USELOG
    public readonly ReadStatus ReadStatus;
#endif

    public SpanReadResult(ReadOnlySpan<byte> span
#if USELOG
        , ReadStatus readStatus
#endif
    )
    {
        Span = span;
#if USELOG
        ReadStatus = readStatus;
#endif
    }
}