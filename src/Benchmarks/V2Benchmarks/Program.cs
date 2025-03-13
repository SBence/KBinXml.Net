using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class NodeWriterU8Task
{
    [Benchmark(Baseline = true)]
    public object? V1()
    {
        var nodeWriter = new KbinXml.Net.Internal.Writers.NodeWriter(true, Encoding.UTF8);
        nodeWriter.WriteByte(0x80);
        return nodeWriter.ToArray();
    }

    [Benchmark]
    public object? V2()
    {
        var nodeWriter = new KbinXml.Net.HighPerformance.Writers.NodeWriter(true, Encoding.UTF8);
        nodeWriter.WriteByte(0x80);
        return nodeWriter.ToArray();
    }
}