using System;
using System.Buffers;
using System.Collections.Generic;
using KbinXml.Net.Internal;
using KbinXml.Net.Utils;
using KbinXml.Net.Writers;

namespace KbinXml.Net;

internal readonly struct WriteContext
{
    public WriteContext(NodeWriter nodeWriter, DataWriter dataWriter)
    {
        NodeWriter = nodeWriter;
        DataWriter = dataWriter;
    }

    public NodeWriter NodeWriter { get; }
    public DataWriter DataWriter { get; }
}

internal struct XmlParsingState
{
    public XmlParsingState()
    {
        HoldingAttributes = new List<KeyValuePair<string, string>>(8); // 预分配一个合理容量
        HoldingValue = string.Empty;
        TypeStr = null;
        ArrayCountStr = null;
        TypeId = 0;
    }

    public List<KeyValuePair<string, string>> HoldingAttributes { get; }
    public string HoldingValue { get; set; }
    public string? TypeStr { get; set; }
    public string? ArrayCountStr { get; set; }
    public byte TypeId { get; set; }

    public void ProcessHolding(ref WriteContext context, WriteOptions writeOptions)
    {
        if (TypeStr != null)
        {
            ProcessTypeData(ref context, writeOptions);
        }

        if (HoldingAttributes.Count > 0)
        {
            ProcessAttributes(ref context);
        }
    }

    private void ProcessTypeData(ref WriteContext context, WriteOptions writeOptions)
    {
        // 使用switch提高性能
        switch (TypeStr)
        {
            case "str":
                context.DataWriter.WriteString(HoldingValue);
                break;
            case "bin":
                context.DataWriter.WriteBinary(HoldingValue);
                break;
            default:
                ProcessComplexTypeData(ref context, writeOptions);
                break;
        }

        // 重置状态
        TypeStr = null;
        ArrayCountStr = null;
        HoldingValue = string.Empty;
        TypeId = 0;
    }

    private void ProcessComplexTypeData(ref WriteContext context, WriteOptions writeOptions)
    {
        var type = NodeTypeFactory.GetNodeType(TypeId);
        var value = HoldingValue.SpanSplit(' '); // 已优化为Span操作

        var requiredBytes = (uint)(type.Size * type.Count);
        if (ArrayCountStr != null)
        {
            if (uint.TryParse(ArrayCountStr, out var count))
            {
                requiredBytes *= count;
                context.DataWriter.WriteU32(requiredBytes);
            }
            else
            {
                throw new KbinException($"Invalid array count: {ArrayCountStr}");
            }
        }

        if (requiredBytes > int.MaxValue)
            throw new KbinException("uint size is greater than int.MaxValue");

        var iRequiredBytes = (int)requiredBytes;

        // 避免小数组的堆分配
        byte[]? arr = null;
        var span = iRequiredBytes <= Constants.MaxStackLength
            ? stackalloc byte[iRequiredBytes]
            : (arr = ArrayPool<byte>.Shared.Rent(iRequiredBytes)).AsSpan(0, iRequiredBytes);

        var builder = new ValueListBuilder<byte>(span);

        try
        {
            ProcessTypeValues(type, value, requiredBytes, ref builder, writeOptions);

            // 根据是否为数组选择合适的写入方法
            // If array, force write 32bit
            if (ArrayCountStr != null)
                context.DataWriter.Write32BitAligned(builder.AsSpan());
            else
                context.DataWriter.WriteBytes(builder.AsSpan());
        }
        finally
        {
            builder.Dispose();
            if (arr != null) ArrayPool<byte>.Shared.Return(arr);
        }
    }

    private void ProcessTypeValues(NodeType type, StringExtensions.SpaceSplitEnumerator values, uint requiredBytes,
        ref ValueListBuilder<byte> builder, WriteOptions writeOptions)
    {
        int bytesWritten = 0;
        foreach (var s in values)
        {
            try
            {
                if (bytesWritten == requiredBytes)
                {
                    if (writeOptions.StrictMode)
                        throw new ArgumentOutOfRangeException("Length", HoldingValue.Split(' ').Length,
                            "The array length doesn't match the \"__count\" attribute. Expect: " +
                            ArrayCountStr);
                    break;
                }

                var add = type.WriteString(ref builder, s);
                if (add < type.Size)
                {
                    var left = type.Size - add;
                    for (var j = 0; j < left; j++) builder.Append(0);
                }

                bytesWritten += type.Size;
            }
            catch (Exception e)
            {
                throw new KbinException(
                    $"Error while writing data '{s.ToString()}'. See InnerException for more information.",
                    e);
            }
        }

        // 确保达到要求的字节数
        if (bytesWritten != requiredBytes)
        {
            if (writeOptions.StrictMode)
            {
                throw new ArgumentOutOfRangeException("Length", builder.Length / type.Size,
                    $"The array length doesn't match the \"__count\" attribute. Expect: {ArrayCountStr}");
            }

            // 填充剩余字节
            while (bytesWritten < requiredBytes)
            {
                builder.Append(0);
                bytesWritten++;
            }
        }
    }

    private void ProcessAttributes(ref WriteContext context)
    {
        // Xml Attribute排序
        HoldingAttributes.Sort(static (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

        foreach (var attribute in HoldingAttributes)
        {
            context.NodeWriter.WriteU8(0x2E);
            context.NodeWriter.WriteString(attribute.Key);
            context.DataWriter.WriteString(attribute.Value);
        }

        HoldingAttributes.Clear();
    }
}