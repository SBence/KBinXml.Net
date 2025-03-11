namespace KbinXml.Net.HighPerformance;

public ref struct ValueReadResult<T>
{
    public T Result;
#if USELOG
    public ReadStatus ReadStatus;
#endif
}