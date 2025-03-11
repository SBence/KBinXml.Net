using System;

namespace KbinXml.Net.HighPerformance;

public ref struct SpanReadResult
{
    public ReadOnlySpan<byte> Span { get; set; }
#if USELOG
    public ReadStatus ReadStatus { get; set; }
#endif
}