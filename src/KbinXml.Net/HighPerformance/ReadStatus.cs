namespace KbinXml.Net.HighPerformance;

public ref struct ReadStatus
{
    public int Offset { get; set; }
    public int Length { get; set; }
    public string? Flag { get; set; }
}