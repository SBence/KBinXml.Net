using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net.Internal;
using KbinXml.Net.Internal.Providers;
using KbinXml.Net.Readers;
using KbinXml.Net.Utils;
using KbinXml.Net.Writers;

namespace KbinXml.Net;

public static partial class KbinConverter
{
    /// <summary>
    /// Converts KBin binary data to an <see cref="XDocument"/> representation.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>An <see cref="XDocument"/> containing the parsed XML structure.</returns>
    /// <exception cref="KbinException">
    /// Thrown when invalid KBin data is detected (invalid signature, encoding mismatch, or unknown node types).
    /// </exception>
    /// <remarks>
    /// <para>This method uses LINQ-to-XML for document construction.</para>
    /// <para>If <paramref name="readOptions"/> is null, default read options will be used.</para>
    /// </remarks>
    public static XDocument ReadXmlLinq(Memory<byte> sourceBuffer, ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var xDocument = (XDocument)ReaderImpl(sourceBuffer, e => new XDocumentProvider(e, readOptions), out _);
        return xDocument;
    }

    /// <summary>
    /// Converts KBin binary data to an <see cref="XDocument"/> and outputs the detected encoding.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="knownEncodings">When this method returns, contains the detected encoding used in the KBin data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>An <see cref="XDocument"/> containing the parsed XML structure.</returns>
    /// <inheritdoc cref="ReadXmlLinq(Memory{byte}, ReadOptions?)"/>
    public static XDocument ReadXmlLinq(Memory<byte> sourceBuffer, out KnownEncodings knownEncodings,
        ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var xDocument =
            (XDocument)ReaderImpl(sourceBuffer, e => new XDocumentProvider(e, readOptions), out knownEncodings);
        return xDocument;
    }

    /// <summary>
    /// Converts KBin binary data to raw XML bytes.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>A byte array containing the XML document in UTF-8 encoding.</returns>
    /// <inheritdoc cref="ReadXmlLinq(Memory{byte}, ReadOptions?)"/>
    /// <remarks>
    /// The resulting byte array contains standard XML 1.0 formatted data without
    /// Byte Order Mark (BOM) by default.
    /// </remarks>
    public static byte[] ReadXmlBytes(Memory<byte> sourceBuffer, ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var bytes = (byte[])ReaderImpl(sourceBuffer, e => new XmlWriterProvider(e, readOptions), out _);
        return bytes;
    }

    /// <summary>
    /// Converts KBin binary data to raw XML bytes and outputs the detected encoding.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="knownEncodings">When this method returns, contains the detected encoding used in the KBin data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>A byte array containing the XML document in UTF-8 encoding.</returns>
    /// <inheritdoc cref="ReadXmlBytes(Memory{byte}, ReadOptions?)"/>
    public static byte[] ReadXmlBytes(Memory<byte> sourceBuffer, out KnownEncodings knownEncodings,
        ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var bytes = (byte[])ReaderImpl(sourceBuffer, e => new XmlWriterProvider(e, readOptions), out knownEncodings);
        return bytes;
    }

    /// <summary>
    /// Converts KBin binary data to an <see cref="XmlDocument"/> representation.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>An <see cref="XmlDocument"/> containing the parsed XML structure.</returns>
    /// <inheritdoc cref="ReadXmlLinq(Memory{byte}, ReadOptions?)"/>
    /// <remarks>
    /// This method uses the classic <see cref="XmlDocument"/> API which implements
    /// the W3C Document Object Model (DOM) Level 1 Core specification.
    /// </remarks>
    public static XmlDocument ReadXml(Memory<byte> sourceBuffer, ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var xmlDocument = (XmlDocument)ReaderImpl(sourceBuffer, e => new XmlDocumentProvider(e, readOptions),
            out var knownEncoding);
        return xmlDocument;
    }

    /// <summary>
    /// Converts KBin binary data to an <see cref="XmlDocument"/> and outputs the detected encoding.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer containing KBin-formatted data.</param>
    /// <param name="knownEncodings">When this method returns, contains the detected encoding used in the KBin data.</param>
    /// <param name="readOptions">Optional reading configuration options.</param>
    /// <returns>An <see cref="XmlDocument"/> containing the parsed XML structure.</returns>
    /// <inheritdoc cref="ReadXml(Memory{byte}, ReadOptions?)"/>
    public static XmlDocument ReadXml(Memory<byte> sourceBuffer, out KnownEncodings knownEncodings,
        ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var xmlDocument = (XmlDocument)ReaderImpl(sourceBuffer, e => new XmlDocumentProvider(e, readOptions),
            out knownEncodings);
        return xmlDocument;
    }

    private static object ReaderImpl(Memory<byte> sourceBuffer, Func<Encoding, WriterProvider> createWriterProvider,
        out KnownEncodings knownEncoding)
    {
        using var readContext = GetReadContext(sourceBuffer, createWriterProvider);
        knownEncoding = readContext.KnownEncoding;
        var writerProvider = readContext.WriterProvider;
        var nodeReader = readContext.NodeReader;
        var dataReader = readContext.DataReader;

        writerProvider.WriteStartDocument();
        string? currentType = null;
        string? holdValue = null;
        Span<char> charSpan = stackalloc char[Constants.MaxStackLength];
        while (true)
        {
            var nodeType = nodeReader.ReadU8(out var pos, out _);

            //Array flag is on the second bit
            var array = (nodeType & 0x40) > 0;
            nodeType = (byte)(nodeType & ~0x40);
            if (ControlTypes.Contains(nodeType))
            {
                Logger.LogNodeControl(nodeType, pos, array);

                var controlType = (ControlType)nodeType;
                switch (controlType)
                {
                    case ControlType.NodeStart:
                        if (holdValue != null)
                        {
                            writerProvider.WriteElementValue(holdValue);
                            holdValue = null;
                        }

                        var elementName = nodeReader.ReadString(out pos);
                        Logger.LogStructElement(elementName, pos);
                        writerProvider.WriteStartElement(elementName);
                        break;
                    case ControlType.Attribute:
                        var attr = nodeReader.ReadString(out pos);
                        Logger.LogAttributeName(attr, pos);
                        var strLen = dataReader.ReadS32(out pos, out var flag);
                        Logger.LogAttributeLength(strLen, pos, flag);
                        var value = dataReader.ReadString(strLen, out pos, out flag);
                        Logger.LogAttributeValue(value, pos, flag);
                        // Size has been written below
                        if (currentType != "bin" || attr != "__size")
                        {
                            writerProvider.WriteStartAttribute(attr);
                            writerProvider.WriteAttributeValue(value);
                            writerProvider.WriteEndAttribute();
                        }

                        break;
                    case ControlType.NodeEnd:
                        if (holdValue != null)
                        {
                            writerProvider.WriteElementValue(holdValue);
                            holdValue = null;
                        }

                        writerProvider.WriteEndElement();
                        break;
                    case ControlType.FileEnd:
                        return writerProvider.GetResult();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (NodeTypeFactory.TryGetNodeType(nodeType, out var propertyType))
            {
                Logger.LogNodeData(propertyType, pos, array);

                if (holdValue != null)
                {
                    writerProvider.WriteElementValue(holdValue);
                    holdValue = null;
                }

                var elementName = nodeReader.ReadString(out pos);
                Logger.LogDataElement(elementName, pos);
                writerProvider.WriteStartElement(elementName);

                writerProvider.WriteStartAttribute("__type");
                writerProvider.WriteAttributeValue(propertyType.Name);
                writerProvider.WriteEndAttribute();

                currentType = propertyType.Name;

                int arraySize;
                if (array || propertyType.Name is "str" or "bin")
                {
                    arraySize = dataReader.ReadS32(out pos, out var flag); // Total size.
                    Logger.LogArraySize(arraySize, pos, flag);
                }
                else
                {
                    arraySize = propertyType.Size * propertyType.Count;
                }

                if (propertyType.Name == "str")
                {
                    holdValue = dataReader.ReadString(arraySize, out pos, out var flag);
                    Logger.LogStringValue(holdValue, pos, flag);
                }
                else if (propertyType.Name == "bin")
                {
                    writerProvider.WriteStartAttribute("__size");
                    writerProvider.WriteAttributeValue(arraySize.ToString());
                    writerProvider.WriteEndAttribute();
                    holdValue = dataReader.ReadBinary(arraySize, out pos, out var flag);
                    Logger.LogBinaryValue(holdValue, pos, flag);
                }
                else
                {
                    if (array)
                    {
                        var size = (arraySize / (propertyType.Size * propertyType.Count)).ToString();
                        writerProvider.WriteStartAttribute("__count");
                        writerProvider.WriteAttributeValue(size);
                        writerProvider.WriteEndAttribute();
                    }

                    // force to read as 32bit if is array
                    var span = array
                        ? dataReader.Read32BitAligned(arraySize, out pos, out var flag)
                        : dataReader.ReadBytes(arraySize, out pos, out flag);
                    var stringBuilder = new ValueStringBuilder(charSpan);
                    var loopCount = arraySize / propertyType.Size;
                    for (var i = 0; i < loopCount; i++)
                    {
                        var subSpan = span.Slice(i * propertyType.Size, propertyType.Size);
                        stringBuilder.Append(propertyType.GetString(subSpan.Span));
                        if (i != loopCount - 1)
                        {
#if NETCOREAPP3_1_OR_GREATER
                            stringBuilder.Append(' ');
#else
                            stringBuilder.Append(" ");
#endif
                        }
                    }

                    holdValue = stringBuilder.ToString();
                    Logger.LogArrayValue(holdValue, pos, flag);
                }
            }
            else
            {
                throw new KbinException($"Unknown node type: {nodeType}");
            }
        }
    }

    private static ReadContext GetReadContext(Memory<byte> sourceBuffer,
        Func<Encoding, WriterProvider> createWriterProvider)
    {
        //Read header section.
        int pos;
        var binaryBuffer = new BeBinaryReader(sourceBuffer);
        var signature = binaryBuffer.ReadU8(out pos, out _);
        Logger.LogSignature(signature, pos);

        var compressionFlag = binaryBuffer.ReadU8(out pos, out _);
        Logger.LogCompression(signature, pos);

        var encodingFlag = binaryBuffer.ReadU8(out pos, out _);
        Logger.LogEncoding(encodingFlag, pos);
        var encodingFlagNot = binaryBuffer.ReadU8(out pos, out _);
        Logger.LogEncodingNot(encodingFlagNot, pos);

        //Verify magic.
        if (signature != 0xA0)
            throw new KbinException($"Signature was invalid. 0x{signature:X2} != 0xA0");

        //Encoding flag should be an inverse of the fourth byte.
        if ((byte)~encodingFlag != encodingFlagNot)
            throw new KbinException(
                $"Third byte was not an inverse of the fourth. {~encodingFlag} != {encodingFlagNot}");

        var compressed = compressionFlag == 0x42;
        var encoding = EncodingDictionary.EncodingMap[encodingFlag];

        //Get buffer lengths and load.
        var nodeLength = binaryBuffer.ReadS32(out pos, out _);
        Logger.LogNodeLength(nodeLength, pos);
        var nodeReader = new NodeReader(sourceBuffer.Slice(8, nodeLength), 8, compressed, encoding);

        var dataLength = BitConverterHelper.ToBeInt32(sourceBuffer.Slice(nodeLength + 8, 4).Span);
        Logger.LogDataLength(nodeLength, pos);
        var dataReader = new DataReader(sourceBuffer.Slice(nodeLength + 12, dataLength), nodeLength + 12, encoding);

        var readProvider = createWriterProvider(encoding);

        var readContext = new ReadContext(nodeReader, dataReader, readProvider, encoding.ToKnownEncoding());
        return readContext;
    }

    private class ReadContext : IDisposable
    {
        public ReadContext(NodeReader nodeReader, DataReader dataReader, WriterProvider writerProvider,
            KnownEncodings knownEncoding)
        {
            NodeReader = nodeReader;
            DataReader = dataReader;
            WriterProvider = writerProvider;
            KnownEncoding = knownEncoding;
        }

        public NodeReader NodeReader { get; set; }
        public DataReader DataReader { get; set; }
        public WriterProvider WriterProvider { get; set; }
        public KnownEncodings KnownEncoding { get; }

        public void Dispose()
        {
            WriterProvider.Dispose();
        }
    }

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
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding));

        using XmlReader reader = new XmlNodeReader(xml);

        try
        {
            return WriterImpl(encoding, context, reader, writeOptions);
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
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding));

        using var reader = xml.CreateReader();

        try
        {
            return WriterImpl(encoding, context, reader, writeOptions);
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
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding));

        using var textReader = new StringReader(xmlText);
        using var reader = XmlReader.Create(textReader, new XmlReaderSettings { IgnoreWhitespace = true });

        try
        {
            return WriterImpl(encoding, context, reader, writeOptions);
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
        var context = new WriteContext(new NodeWriter(writeOptions.Compress, encoding), new DataWriter(encoding));

        using var ms = new MemoryStream(xmlBytes);
        using var reader = XmlReader.Create(ms, new XmlReaderSettings { IgnoreWhitespace = true });

        try
        {
            return WriterImpl(encoding, context, reader, writeOptions);
        }
        finally
        {
            context.DataWriter.Dispose();
            context.NodeWriter.Dispose();
        }
    }

    private static byte[] WriterImpl(Encoding encoding, WriteContext context, XmlReader reader,
        WriteOptions writeOptions)
    {
        if (!EncodingDictionary.ReverseEncodingMap.TryGetValue(encoding, out var encodingBytes))
            throw new ArgumentOutOfRangeException(nameof(encoding), encoding, "Unsupported encoding for KBin");

        var xmlState = new XmlParsingState();

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    xmlState.ProcessHolding(ref context, writeOptions);
                    //Console.WriteLine("Start Element {0}", reader.Name);
                    if (reader.AttributeCount > 0)
                    {
                        ProcessAttributes(reader, ref xmlState, writeOptions);
                    }

                    if (xmlState.TypeStr == null)
                    {
                        context.NodeWriter.WriteU8(1);
                        context.NodeWriter.WriteString(GetActualName(reader.Name, writeOptions.RepairedPrefix));
                    }
                    else
                    {
                        xmlState.TypeId = NodeTypeFactory.GetNodeTypeId(xmlState.TypeStr); // 内部为字典操作
                        if (xmlState.ArrayCountStr != null)
                            context.NodeWriter.WriteU8((byte)(xmlState.TypeId | 0x40));
                        else
                            context.NodeWriter.WriteU8(xmlState.TypeId);

                        context.NodeWriter.WriteString(GetActualName(reader.Name, writeOptions.RepairedPrefix));
                    }

                    if (reader.IsEmptyElement)
                    {
                        xmlState.ProcessHolding(ref context, writeOptions);
                        context.NodeWriter.WriteU8(0xFE);
                    }

                    break;
                case XmlNodeType.Text:
                    xmlState.HoldingValue = reader.Value;
                    break;
                case XmlNodeType.EndElement:
                    xmlState.ProcessHolding(ref context, writeOptions);
                    context.NodeWriter.WriteU8(0xFE);
                    break;
                default:
                    //Console.WriteLine("Other node {0} with value {1}",
                    //    reader.NodeType, reader.Value);
                    break;
            }
        }

        xmlState.ProcessHolding(ref context, writeOptions);

        context.NodeWriter.WriteU8(255);
        context.NodeWriter.Pad();
        context.DataWriter.Pad();

        return FinalizeOutput(ref context, encodingBytes);
    }

    private static void ProcessAttributes(XmlReader reader, ref XmlParsingState state, WriteOptions writeOptions)
    {
        for (int i = 0; i < reader.AttributeCount; i++)
        {
            reader.MoveToAttribute(i);

            switch (reader.Name)
            {
                case "__type":
                    state.TypeStr = reader.Value;
                    break;
                case "__count":
                    state.ArrayCountStr = reader.Value;
                    break;
                case "__size":
                    // ignore
                    break;
                default:
                    state.HoldingAttributes.Add(new KeyValuePair<string, string>(
                        GetActualName(reader.Name, writeOptions.RepairedPrefix),
                        reader.Value));
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

        using var output = new BeBinaryWriter(totalSize);

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
}