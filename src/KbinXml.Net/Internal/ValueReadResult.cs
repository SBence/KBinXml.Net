namespace KbinXml.Net.Internal;

internal readonly ref struct ValueReadResult<T>
{
    public readonly T Value;
#if USELOG
    public readonly ReadStatus ReadStatus;
#endif

    public ValueReadResult(T value
#if USELOG
        , ReadStatus readStatus
#endif
        )
    {
        Value = value;
#if USELOG
        ReadStatus = readStatus;
#endif
    }
}