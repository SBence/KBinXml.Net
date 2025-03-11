using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net.HighPerformance.Readers;
using KbinXml.Net.Internal;
using KbinXml.Net.Internal.Debugging;
using KbinXml.Net.Internal.Providers;
using KbinXml.Net.Utils;

namespace KbinXml.Net.HighPerformance;

public static class KBinReader
{
#if USELOG
    internal static ConsoleLogger Logger { get; } = new ConsoleLogger();
#else
    internal static NullLogger Logger { get; } = new NullLogger();
#endif

#if !NET5_0_OR_GREATER
    private static readonly Type ControlTypeT = typeof(ControlType);
#endif
    private static readonly HashSet<byte> ControlTypes =
#if NET5_0_OR_GREATER
        new(Enum.GetValues<ControlType>().Cast<byte>());
#else
        new(Enum.GetValues(ControlTypeT).Cast<byte>());
#endif

    internal static string GetActualName(string name, string? repairedPrefix)
    {
        if (repairedPrefix is not null && name.StartsWith(repairedPrefix, StringComparison.Ordinal))
        {
            return name.Substring(repairedPrefix.Length);
        }
        else
        {
            return name;
        }
    }

    internal static string GetRepairedName(string name, string? repairedPrefix)
    {
        if (repairedPrefix is null) return name;
        if (name.Length < 1 || name[0] < 48 || name[0] > 57) return name;

        return repairedPrefix + name;
    }

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
    public static XDocument ReadXmlLinq(ReadOnlySpan<byte> sourceBuffer, ReadOptions? readOptions = null)
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
    /// <inheritdoc cref="ReadXmlLinq(ReadOnlySpan{byte}, ReadOptions?)"/>
    public static XDocument ReadXmlLinq(ReadOnlySpan<byte> sourceBuffer, out KnownEncodings knownEncodings,
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
    /// <inheritdoc cref="ReadXmlLinq(ReadOnlySpan{byte}, ReadOptions?)"/>
    /// <remarks>
    /// The resulting byte array contains standard XML 1.0 formatted data without
    /// Byte Order Mark (BOM) by default.
    /// </remarks>
    public static byte[] ReadXmlBytes(ReadOnlySpan<byte> sourceBuffer, ReadOptions? readOptions = null)
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
    /// <inheritdoc cref="ReadXmlBytes(ReadOnlySpan{byte}, ReadOptions?)"/>
    public static byte[] ReadXmlBytes(ReadOnlySpan<byte> sourceBuffer, out KnownEncodings knownEncodings,
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
    /// <inheritdoc cref="ReadXmlLinq(ReadOnlyMemory{byte}, ReadOptions?)"/>
    /// <remarks>
    /// This method uses the classic <see cref="XmlDocument"/> API which implements
    /// the W3C Document Object Model (DOM) Level 1 Core specification.
    /// </remarks>
    public static XmlDocument ReadXml(ReadOnlySpan<byte> sourceBuffer, ReadOptions? readOptions = null)
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
    /// <inheritdoc cref="ReadXml(ReadOnlySpan{byte}, ReadOptions?)"/>
    public static XmlDocument ReadXml(ReadOnlySpan<byte> sourceBuffer, out KnownEncodings knownEncodings,
        ReadOptions? readOptions = null)
    {
        readOptions ??= new ReadOptions();
        var xmlDocument = (XmlDocument)ReaderImpl(sourceBuffer, e => new XmlDocumentProvider(e, readOptions),
            out knownEncodings);
        return xmlDocument;
    }

    private static object ReaderImpl(ReadOnlySpan<byte> sourceBuffer, Func<Encoding, WriterProvider> createWriterProvider,
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
            var nodeTypeResult = nodeReader.ReadU8();
            var nodeType = nodeTypeResult.Value;

            //Array flag is on the second bit
            var array = (nodeType & 0x40) > 0;
            nodeType = (byte)(nodeType & ~0x40);
            if (ControlTypes.Contains(nodeType))
            {
                Logger.LogNodeControl(nodeType, nodeTypeResult.Value, array);

                var controlType = (ControlType)nodeType;
                switch (controlType)
                {
                    case ControlType.NodeStart:
                        if (holdValue != null)
                        {
                            writerProvider.WriteElementValue(holdValue);
                            holdValue = null;
                        }

                        var elementNameResult = nodeReader.ReadString();
                        var elementName = elementNameResult.Value;
#if USELOG
                        Logger.LogStructElement(elementName, elementNameResult.ReadStatus.Offset);
#endif
                        writerProvider.WriteStartElement(elementName);
                        break;
                    case ControlType.Attribute:
                        var attrResult = nodeReader.ReadString();
                        var attr = attrResult.Value;
#if USELOG
                        Logger.LogAttributeName(attr, attrResult.ReadStatus.Offset);
#endif
                        var strLenResult = dataReader.ReadS32();
                        var strLen = strLenResult.Value;
#if USELOG
                        Logger.LogAttributeLength(strLen, strLenResult.ReadStatus.Offset, strLenResult.ReadStatus.Flag);
#endif
                        var valueResult = dataReader.ReadString(strLen);
                        var value = valueResult.Value;
#if USELOG
                        Logger.LogAttributeValue(value, valueResult.ReadStatus.Offset, valueResult.ReadStatus.Flag);
#endif
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
                Logger.LogNodeData(propertyType, nodeTypeResult.Value, array);

                if (holdValue != null)
                {
                    writerProvider.WriteElementValue(holdValue);
                    holdValue = null;
                }

                var elementNameResult = nodeReader.ReadString();
                var elementName = elementNameResult.Value;
#if USELOG
                Logger.LogDataElement(elementName, elementNameResult.ReadStatus.Offset);
#endif
                writerProvider.WriteStartElement(elementName);

                writerProvider.WriteStartAttribute("__type");
                writerProvider.WriteAttributeValue(propertyType.Name);
                writerProvider.WriteEndAttribute();

                currentType = propertyType.Name;

                int arraySize;
                if (array || propertyType.Name is "str" or "bin")
                {
                    var valueReadResult = dataReader.ReadS32();
                    arraySize = valueReadResult.Value; // Total size.
#if USELOG
                    Logger.LogArraySize(arraySize, valueReadResult.ReadStatus.Offset, valueReadResult.ReadStatus.Flag);
#endif
                }
                else
                {
                    arraySize = propertyType.Size * propertyType.Count;
                }

                if (propertyType.Name == "str")
                {
                    var valueReadResult = dataReader.ReadString(arraySize);
                    holdValue = valueReadResult.Value;
#if USELOG
                    Logger.LogStringValue(holdValue, valueReadResult.ReadStatus.Offset, valueReadResult.ReadStatus.Flag);
#endif
                }
                else if (propertyType.Name == "bin")
                {
                    writerProvider.WriteStartAttribute("__size");
                    writerProvider.WriteAttributeValue(arraySize.ToString());
                    writerProvider.WriteEndAttribute();
                    var valueReadResult = dataReader.ReadBinary(arraySize);
                    holdValue = valueReadResult.Value;
#if USELOG
                    Logger.LogBinaryValue(holdValue, valueReadResult.ReadStatus.Offset, valueReadResult.ReadStatus.Flag);
#endif
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
                    var spanResult = array
                        ? dataReader.ReadBytes32BitAligned(arraySize)
                        : dataReader.ReadBytes(arraySize);
                    var span = spanResult.Span;
                    var stringBuilder = new ValueStringBuilder(charSpan);
                    var loopCount = arraySize / propertyType.Size;
                    for (var i = 0; i < loopCount; i++)
                    {
                        var subSpan = span.Slice(i * propertyType.Size, propertyType.Size);
#if NET6_0_OR_GREATER
                        propertyType.AppendString(ref stringBuilder, subSpan);
#else
                        stringBuilder.Append(propertyType.GetString(subSpan));
#endif
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
#if USELOG
                    Logger.LogArrayValue(holdValue, spanResult.ReadStatus.Offset, spanResult.ReadStatus.Flag);
#endif
                }
            }
            else
            {
                throw new KbinException($"Unknown node type: {nodeType}");
            }
        }
    }

    private static ReadContext GetReadContext(ReadOnlySpan<byte> sourceBuffer,
        Func<Encoding, WriterProvider> createWriterProvider)
    {
        //Read header section.

        var binaryBuffer = new BigEndianReader(sourceBuffer);
        var signature = binaryBuffer.ReadU8();
#if USELOG
        Logger.LogSignature(signature.Value, signature.ReadStatus.Offset);
#endif

        var compressionFlag = binaryBuffer.ReadU8();
#if USELOG
        Logger.LogCompression(compressionFlag.Value, compressionFlag.ReadStatus.Offset);
#endif

        var encodingFlag = binaryBuffer.ReadU8();
#if USELOG
        Logger.LogEncoding(encodingFlag.Value, encodingFlag.ReadStatus.Offset);
#endif
        var encodingFlagNot = binaryBuffer.ReadU8();
#if USELOG
        Logger.LogEncodingNot(encodingFlagNot.Value, encodingFlagNot.ReadStatus.Offset);
#endif

        //Verify magic.
        if (signature.Value != 0xA0)
            throw new KbinException($"Signature was invalid. 0x{signature.Value:X2} != 0xA0");

        //Encoding flag should be an inverse of the fourth byte.
        if ((byte)~encodingFlag.Value != encodingFlagNot.Value)
            throw new KbinException(
                $"Third byte was not an inverse of the fourth. {~encodingFlag.Value} != {encodingFlagNot.Value}");

        var compressed = compressionFlag.Value == 0x42;
        var encoding = EncodingDictionary.EncodingMap[encodingFlag.Value];

        //Get buffer lengths and load.
        var nodeLength = binaryBuffer.ReadS32();
#if USELOG
        Logger.LogNodeLength(nodeLength.Value, nodeLength.ReadStatus.Offset);
#endif
        var nodeReader = new NodeReader(sourceBuffer.Slice(8, nodeLength.Value), encoding, compressed);

        var dataLength = BitConverterHelper.ToBeInt32(sourceBuffer.Slice(nodeLength.Value + 8, 4));
        Logger.LogDataLength(dataLength, nodeLength.Value + 12);
        var dataReader = new DataReader(sourceBuffer.Slice(nodeLength.Value + 12, dataLength), encoding);

        var readProvider = createWriterProvider(encoding);

        var readContext = new ReadContext(nodeReader, dataReader, readProvider, encoding.ToKnownEncoding());
        return readContext;
    }

    private ref struct ReadContext : IDisposable
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
}