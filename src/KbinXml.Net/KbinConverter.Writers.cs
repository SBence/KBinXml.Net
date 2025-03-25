﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net.Internal;
using KbinXml.Net.Internal.Writers;
using KbinXml.Net.Utils;

namespace KbinXml.Net;

public static partial class KbinConverter
{
    /// <summary>
    /// Converts an XML document to KBin-formatted binary data.
    /// </summary>
    /// <param name="xml">The XML document to convert.</param>
    /// <param name="knownEncodings">The text encoding specification for the output KBin data.</param>
    /// <param name="writeOptions">Configuration options for the conversion process.</param>
    /// <returns>A byte array containing the KBin-formatted data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="knownEncodings"/> specifies an unsupported encoding.</exception>
    /// <exception cref="KbinException">Invalid XML structure or data conversion error occurs.</exception>
    /// <remarks>
    /// <para>This method supports both compressed and uncompressed KBin formats.</para>
    /// <para>If <paramref name="writeOptions"/> is null, default options will be used.</para>
    /// </remarks>
    public static byte[] Write(XmlDocument xml, KnownEncodings knownEncodings, WriteOptions? writeOptions = null)
    {
        var encoding = knownEncodings.ToEncoding();
        writeOptions ??= new WriteOptions();
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding),
            writeOptions);

        using XmlReader reader = new XmlNodeReader(xml);

        try
        {
            return WriterImpl(encoding, ref context, reader, writeOptions);
        }
        finally
        {
            context.DataWriter.Dispose();
            context.NodeWriter.Dispose();
        }
    }

    /// <summary>
    /// Converts a LINQ-to-XML element/document to KBin-formatted binary data.
    /// </summary>
    /// <param name="xml">The XML element or document to convert. Must be a valid <see cref="XContainer"/> (XElement or XDocument).</param>
    /// <param name="knownEncodings">The text encoding specification for the output KBin data. See supported values in <see cref="KnownEncodings"/>.</param>
    /// <param name="writeOptions">Configuration options for serialization. When null, uses default compression and validation settings.</param>
    /// <returns>A byte array containing structured KBin data with proper section alignment.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> contains a null reference.</exception>
    /// <inheritdoc cref="Write(XmlDocument, KnownEncodings, WriteOptions?)"/>
    public static byte[] Write(XContainer xml, KnownEncodings knownEncodings, WriteOptions? writeOptions = null)
    {
        var encoding = knownEncodings.ToEncoding();
        writeOptions ??= new WriteOptions();
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding),
            writeOptions);

        using var reader = xml.CreateReader();

        try
        {
            return WriterImpl(encoding, ref context, reader, writeOptions);
        }
        finally
        {
            context.DataWriter.Dispose();
            context.NodeWriter.Dispose();
        }
    }

    /// <summary>
    /// Converts XML text to KBin-formatted binary data.
    /// </summary>
    /// <param name="xmlText">The XML string to convert. Must be well-formed XML 1.0 text.</param>
    /// <param name="knownEncodings">The character encoding scheme for text conversion. Affects string storage in KBin format.</param>
    /// <param name="writeOptions">Serialization control parameters. Null values enable default compression and error handling behavior.</param>
    /// <returns>A byte array containing the KBin binary output with proper header validation.</returns>
    /// <exception cref="ArgumentException"><paramref name="xmlText"/> contains invalid XML syntax.</exception>
    /// <inheritdoc cref="Write(XmlDocument, KnownEncodings, WriteOptions?)"/>
    public static byte[] Write(string xmlText, KnownEncodings knownEncodings, WriteOptions? writeOptions = null)
    {
        var encoding = knownEncodings.ToEncoding();
        writeOptions ??= new WriteOptions();
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding),
            writeOptions);

        using var textReader = new StringReader(xmlText);
        using var reader = XmlReader.Create(textReader, new XmlReaderSettings { IgnoreWhitespace = true });

        try
        {
            return WriterImpl(encoding, ref context, reader, writeOptions);
        }
        finally
        {
            context.DataWriter.Dispose();
            context.NodeWriter.Dispose();
        }
    }

    /// <summary>
    /// Converts UTF-8 encoded XML bytes to KBin-formatted binary data.
    /// </summary>
    /// <param name="xmlBytes">The XML data to convert. Must be valid UTF-8 encoded bytes (with or without BOM).</param>
    /// <param name="knownEncodings">The target text encoding specification. Determines how strings are stored in the KBin output.</param>
    /// <param name="writeOptions">Serialization configuration parameters. Controls compression and validation behavior.</param>
    /// <returns>A byte array containing the complete KBin structure with node and data sections.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="xmlBytes"/> is a null reference.</exception>
    /// <inheritdoc cref="Write(XmlDocument, KnownEncodings, WriteOptions?)"/>
    public static byte[] Write(byte[] xmlBytes, KnownEncodings knownEncodings, WriteOptions? writeOptions = null)
    {
        var encoding = knownEncodings.ToEncoding();
        writeOptions ??= new WriteOptions();
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding),
            writeOptions);

        using var ms = new MemoryStream(xmlBytes);
        using var reader = XmlReader.Create(ms, new XmlReaderSettings { IgnoreWhitespace = true });

        try
        {
            return WriterImpl(encoding, ref context, reader, writeOptions);
        }
        finally
        {
            context.DataWriter.Dispose();
            context.NodeWriter.Dispose();
        }
    }

    private static byte[] WriterImpl(Encoding encoding, ref WriteContext context, XmlReader reader,
        WriteOptions writeOptions)
    {
        if (!EncodingDictionary.ReverseEncodingMap.TryGetValue(encoding, out var encodingBytes))
        {
            throw new ArgumentOutOfRangeException(nameof(encoding), encoding, "Unsupported encoding for KBin");
        }

        var repairedPrefix = writeOptions.RepairedPrefix;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    context.FlushPendingData();

                    if (reader.AttributeCount > 0)
                    {
                        ProcessAttributes(reader, ref context, repairedPrefix);
                    }

                    var readerName = reader.Name;
                    if (context.TypeStr == null)
                    {
                        context.NodeWriter.WriteU8(1);
                    }
                    else
                    {
                        if (!NodeTypeFactory.TryGetNodeTypeId(context.TypeStr, out var typeId)) // 内部为字典操作
                        {
                            throw new KbinTypeNotFoundException(context.TypeStr);
                        }

                        context.TypeId = typeId;
                        if (context.ArrayCountStr != null)
                        {
                            context.NodeWriter.WriteU8((byte)(context.TypeId | 0x40));
                        }
                        else
                        {
                            context.NodeWriter.WriteU8(context.TypeId);
                        }
                    }

                    context.NodeWriter.WriteString(KbinConverter.GetActualName(readerName, repairedPrefix));

                    if (reader.IsEmptyElement)
                    {
                        context.FlushPendingData();
                        context.NodeWriter.WriteU8(0xFE);
                    }

                    break;
                case XmlNodeType.Text:
                    context.PendingValue = reader.Value;
                    break;
                case XmlNodeType.EndElement:
                    context.FlushPendingData();
                    context.NodeWriter.WriteU8(0xFE);
                    break;
                default:
                    //Console.WriteLine("Other node {0} with value {1}",
                    //    reader.NodeType, reader.Value);
                    break;
            }
        }

        context.FlushPendingData();

        context.NodeWriter.WriteU8(255);
        context.NodeWriter.Pad();
        context.DataWriter.PadStream();

        return FinalizeOutput(ref context, encodingBytes);
    }

    private static void ProcessAttributes(XmlReader reader, ref WriteContext context, string? repairedPrefix)
    {
        var attrCount = reader.AttributeCount;
        for (int i = 0; i < attrCount; i++)
        {
            reader.MoveToAttribute(i);
            var name = reader.Name;
            var value = reader.Value;

            switch (name)
            {
                case "__type":
                    context.TypeStr = value;
                    break;
                case "__count":
                    context.ArrayCountStr = value;
                    break;
                case "__size":
                    // ignore
                    break;
                default:
                    context.PendingAttributes.Add(
                        new KeyValuePair<string, string>(KbinConverter.GetActualName(name, repairedPrefix), value));
                    break;
            }
        }

        reader.MoveToElement();
    }

    private static byte[] FinalizeOutput(ref WriteContext context, byte encodingBytes)
    {
        var nodeLength = (int)context.NodeWriter.Stream.Length;
        var dataLength = (int)context.DataWriter.Stream.Length;

        // 预先计算总输出大小，避免动态扩容
        var totalSize = 8 + 4 + nodeLength + 4 + dataLength;

        using var output = new BigEndianWriter(totalSize);

        //Write header data
        output.WriteU8(0xA0); // Signature
        output.WriteU8((byte)(context.NodeWriter.Compressed ? 0x42 : 0x45)); // Compression flag
        output.WriteU8(encodingBytes);
        output.WriteU8((byte)~encodingBytes);

        //Write node buffer length and contents.
        output.WriteS32(nodeLength);
        context.NodeWriter.Stream.WriteTo(output.Stream);

        //Write data buffer length and contents.
        output.WriteS32(dataLength);
        context.DataWriter.Stream.WriteTo(output.Stream);

        return output.ToArray();
    }

    private ref struct WriteContext
    {
        public readonly WriteOptions WriteOptions;
        public readonly List<KeyValuePair<string, string>> PendingAttributes;
        public NodeWriter NodeWriter;
        public DataWriter DataWriter;

        public string PendingValue;
        public string? TypeStr;
        public string? ArrayCountStr;
        public byte TypeId;

        public WriteContext(NodeWriter nodeWriter, DataWriter dataWriter, WriteOptions writeOptions)
        {
            NodeWriter = nodeWriter;
            DataWriter = dataWriter;
            WriteOptions = writeOptions;

            PendingAttributes = new List<KeyValuePair<string, string>>(8); // 预分配一个合理容量
            PendingValue = string.Empty;
            TypeStr = null;
            ArrayCountStr = null;
            TypeId = 0;
        }

        public void FlushPendingData()
        {
            if (TypeStr != null)
            {
                ProcessTypeData();
            }

            if (PendingAttributes.Count > 0)
            {
                ProcessAttributes();
            }
        }

        private void ProcessTypeData()
        {
            // 使用switch提高性能
            switch (TypeStr)
            {
                case "str":
                    DataWriter.WriteString(PendingValue);
                    break;
                case "bin":
                    DataWriter.WriteBinary(PendingValue);
                    break;
                default:
                    ProcessComplexTypeData();
                    break;
            }

            // 重置状态
            TypeStr = null;
            ArrayCountStr = null;
            PendingValue = string.Empty;
            TypeId = 0;
        }

        private void ProcessComplexTypeData()
        {
            var type = NodeTypeFactory.GetNodeType(TypeId);
            var values = PendingValue.SpanSplit(' '); // 已优化为Span操作

            var typeSize = type.Size;
            var requiredBytes = (uint)(typeSize * type.Count);
            if (ArrayCountStr != null)
            {
                if (!uint.TryParse(ArrayCountStr, out var count))
                {
                    throw new KbinException($"Invalid array count: {ArrayCountStr}");
                }

                requiredBytes *= count;
                DataWriter.WriteU32(requiredBytes);
            }

            if (requiredBytes > int.MaxValue)
            {
                throw new KbinException("Required bytes exceed maximum array size");
            }

            var iRequiredBytes = (int)requiredBytes;
            // 避免小数组的堆分配
            byte[]? arr = null;
            var span = iRequiredBytes <= Constants.MaxStackLength
                ? stackalloc byte[iRequiredBytes]
                : (arr = ArrayPool<byte>.Shared.Rent(iRequiredBytes)).AsSpan(0, iRequiredBytes);

            var builder = new ValueListBuilder<byte>(span);

            try
            {
                int bytesWritten = 0;
                var strictMode = WriteOptions.StrictMode;
                foreach (var s in values)
                {
                    try
                    {
                        if (bytesWritten == iRequiredBytes)
                        {
                            if (strictMode)
                            {
                                throw new KbinArrayCountMissMatchException(ArrayCountStr, PendingValue.Split(' ').Length);
                            }

                            break;
                        }

                        var add = type.WriteString(ref builder, s);
                        if (add < typeSize)
                        {
                            builder.AppendZeros(typeSize - add);
                        }

                        bytesWritten += typeSize;
                    }
                    catch (Exception e)
                    {
                        throw new KbinException(
                            $"Error while writing data '{s.ToString()}'. See InnerException for more information.",
                            e);
                    }
                }

                // 处理可能的字节数不足情况
                if (bytesWritten != iRequiredBytes)
                {
                    if (strictMode)
                    {
                        throw new KbinArrayCountMissMatchException(ArrayCountStr, builder.Length / typeSize);
                    }

                    // 填充剩余字节
                    builder.AppendZeros(iRequiredBytes - bytesWritten);
                }

                // 根据是否为数组选择合适的写入方法
                // If array, force write 32bit
                var builderSpan = builder.AsSpan();
                if (ArrayCountStr != null)
                {
                    DataWriter.Write32BitAligned(builderSpan);
                }
                else
                {
                    DataWriter.WriteBytes(builderSpan);
                }
            }
            finally
            {
                builder.Dispose();
                if (arr != null) ArrayPool<byte>.Shared.Return(arr);
            }
        }

        private void ProcessAttributes()
        {
            // Xml Attribute排序
            PendingAttributes.Sort(static (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

            foreach (var attribute in PendingAttributes)
            {
                NodeWriter.WriteU8(0x2E);
                NodeWriter.WriteString(attribute.Key);
                DataWriter.WriteString(attribute.Value);
            }

            PendingAttributes.Clear();
        }
    }
}