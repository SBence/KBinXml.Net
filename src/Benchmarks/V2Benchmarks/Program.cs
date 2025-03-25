using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using KbinXml.Net.Internal.Writers;
using ReadBenchmark;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class NodeWriterU8Task
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        var nodeWriter = new NodeWriter(true, Encoding.UTF8, 128 * 1024);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteByte(0x80);
        return nodeWriter.Stream.GetBuffer();
    }

    [Benchmark]
    public object? V2()
    {
        var nodeWriter = new NodeWriter(true, Encoding.UTF8);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteByte(0x80);
        return nodeWriter.Stream.GetBuffer();
    }
}

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class DataWriterU8Task
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8, 128 * 1024);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteByte(0x80);
        return nodeWriter.Stream.GetBuffer();
    }

    [Benchmark]
    public object? V2()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteByte(0x80);
        return nodeWriter.Stream.GetBuffer();
    }
}

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class DataWriterU16Task
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8, 128 * 1024);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteU16(11451);
        return nodeWriter.Stream.GetBuffer();
    }

    [Benchmark]
    public object? V2()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteU16(11451);
        return nodeWriter.Stream.GetBuffer();
    }
}

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class DataWriterU32Task
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8, 128 * 1024);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteU32(11451);
        return nodeWriter.Stream.GetBuffer();
    }

    [Benchmark]
    public object? V2()
    {
        var nodeWriter = new DataWriter(Encoding.UTF8);
        for (int i = 0; i < 100_000; i++)
            nodeWriter.WriteU32(11451);
        return nodeWriter.Stream.GetBuffer();
    }
}

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class DataWriterU32Task_Multi
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        return MultiThreadUtils.DoMultiThreadWork(_ =>
        {
            var nodeWriter = new DataWriter(Encoding.UTF8, 128 * 1024);
            for (int i = 0; i < 100_000; i++)
                nodeWriter.WriteU32(11451);
            return nodeWriter.Stream.GetBuffer();
        }, 24, 15);

    }

    [Benchmark]
    public object? V2()
    {
        return MultiThreadUtils.DoMultiThreadWork(_ =>
        {
            var nodeWriter = new DataWriter(Encoding.UTF8);
            for (int i = 0; i < 100_000; i++)
                nodeWriter.WriteU32(11451);
            return nodeWriter.Stream.GetBuffer();
        }, 24, 15);
    }
}