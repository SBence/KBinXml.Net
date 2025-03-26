using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;
using Xunit.Abstractions;

namespace GeneralUnitTests
{
    public class KbinConverterTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public KbinConverterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void ReadXmlLinq_ValidKbin_ReturnsXDocument()
        {
            // 准备简单的XML并转换为Kbin
            var xml = "<root><value __type=\"s32\">42</value></root>";
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 调用被测试的方法
            var result = KbinConverter.ReadXmlLinq(kbinData);

            // 验证结果
            Assert.NotNull(result);
            Assert.Equal("root", result.Root.Name.LocalName);
            Assert.Equal("42", result.Root.Element("value").Value);
        }

        [Fact]
        public void ReadXmlLinq_WithEncodingOutput_ReturnsEncodingAndXDocument()
        {
            // 准备简单的XML并转换为Kbin
            var xml = "<root><value __type=\"s32\">42</value></root>";
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 调用被测试的方法
            var result = KbinConverter.ReadXmlLinq(kbinData, out var encoding);

            // 验证结果
            Assert.NotNull(result);
            Assert.Equal(KnownEncodings.UTF8, encoding);
        }

        [Fact]
        public void ReadXmlBytes_ValidKbin_ReturnsXmlBytes()
        {
            // 准备简单的XML并转换为Kbin
            var xml = "<root><value __type=\"s32\">42</value></root>";
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 调用被测试的方法
            var result = KbinConverter.ReadXmlBytes(kbinData);

            // 验证结果
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            
            // 将结果作为字符串读取并验证
            var xmlString = Encoding.UTF8.GetString(result);
            Assert.Contains("<root>", xmlString);
            Assert.Contains("<value", xmlString);
            Assert.Contains("42</value>", xmlString);
            Assert.Contains("</root>", xmlString);
        }

        [Fact]
        public void GetXmlStream_ValidKbin_ReturnsMemoryStream()
        {
            // 准备简单的XML并转换为Kbin
            var xml = "<root><value __type=\"s32\">42</value></root>";
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 调用被测试的方法
            using var stream = KbinConverter.GetXmlStream(kbinData);

            // 验证结果
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
            
            // 读取流内容并验证
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            var resultDoc = XDocument.Parse(content);
            Assert.Equal("root", resultDoc.Root.Name.LocalName);
            Assert.Equal("42", resultDoc.Root.Element("value").Value);
        }

        [Fact]
        public void ReadXml_ValidKbin_ReturnsXmlDocument()
        {
            // 准备简单的XML并转换为Kbin
            var xml = "<root><value __type=\"s32\">42</value></root>";
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 调用被测试的方法
            var result = KbinConverter.ReadXml(kbinData);

            // 验证结果
            Assert.NotNull(result);
            Assert.Equal("root", result.DocumentElement.Name);
            Assert.Equal("42", result.DocumentElement.SelectSingleNode("value").InnerText);
        }

        [Fact]
        public void Write_XmlDocument_ReturnsKbinBytes()
        {
            // 准备XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<root><value __type=\"s32\">42</value></root>");

            // 调用被测试的方法
            var result = KbinConverter.Write(doc, KnownEncodings.UTF8);

            // 验证结果
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // 将结果转换回XDocument并验证内容一致
            var resultDoc = KbinConverter.ReadXmlLinq(result);
            Assert.Equal("root", resultDoc.Root.Name.LocalName);
            Assert.Equal("42", resultDoc.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XContainer_ReturnsKbinBytes()
        {
            // 准备XDocument
            var doc = XDocument.Parse("<root><value __type=\"s32\">42</value></root>");

            // 调用被测试的方法
            var result = KbinConverter.Write(doc, KnownEncodings.UTF8);

            // 验证结果
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // 将结果转换回XDocument并验证内容一致
            var resultDoc = KbinConverter.ReadXmlLinq(result);
            Assert.Equal("root", resultDoc.Root.Name.LocalName);
            Assert.Equal("42", resultDoc.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XmlString_ReturnsKbinBytes()
        {
            // 准备XML字符串
            var xml = "<root><value __type=\"s32\">42</value></root>";

            // 调用被测试的方法
            var result = KbinConverter.Write(xml, KnownEncodings.UTF8);

            // 验证结果
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // 将结果转换回XDocument并验证内容一致
            var resultDoc = KbinConverter.ReadXmlLinq(result);
            Assert.Equal("root", resultDoc.Root.Name.LocalName);
            Assert.Equal("42", resultDoc.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XmlBytes_ReturnsKbinBytes()
        {
            // 准备XML字节数组
            var xmlBytes = Encoding.UTF8.GetBytes("<root><value __type=\"s32\">42</value></root>");

            // 调用被测试的方法
            var result = KbinConverter.Write(xmlBytes, KnownEncodings.UTF8);

            // 验证结果
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // 将结果转换回XDocument并验证内容一致
            var resultDoc = KbinConverter.ReadXmlLinq(result);
            Assert.Equal("root", resultDoc.Root.Name.LocalName);
            Assert.Equal("42", resultDoc.Root.Element("value").Value);
        }

        [Fact]
        public void ReadXmlLinq_InvalidKbin_ThrowsKbinException()
        {
            // 准备无效的Kbin数据
            var invalidData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.ReadXmlLinq(invalidData));
        }

        [Fact]
        public void Write_InvalidEncoding_ThrowsArgumentOutOfRangeException()
        {
            // 准备XML和无效的编码类型
            var xml = "<root></root>";
            var invalidEncoding = (KnownEncodings)99; // 使用一个未定义的枚举值

            // 验证抛出异常
            Assert.Throws<ArgumentOutOfRangeException>(() => KbinConverter.Write(xml, invalidEncoding));
        }

        [Fact]
        public void Write_NullXml_ThrowsArgumentNullException()
        {
            // 使用null XML文档
            XmlDocument doc = null;

            // 验证抛出异常
            Assert.Throws<ArgumentNullException>(() => KbinConverter.Write(doc, KnownEncodings.UTF8));
        }

        [Fact]
        public void Write_InvalidXmlString_ThrowsXmlException()
        {
            // 准备无效的XML字符串
            var invalidXml = "<root><unclosed>";

            // 验证抛出异常
            Assert.Throws<XmlException>(() => KbinConverter.Write(invalidXml, KnownEncodings.UTF8));
        }

        [Fact]
        public void ReadWriteComplex_PreservesStructure()
        {
            // 复杂的XML结构
            var xml = @"<data>
                <section id=""main"">
                    <item __type=""s32"">12345</item>
                    <item2 __type=""str"">测试字符串</item2>
                    <nested>
                        <value __type=""u8"">255</value>
                        <array __type=""s16"" __count=""3"">-1 0 1</array>
                    </nested>
                </section>
                <binary __type=""bin"" __size=""4"">DEADBEEF</binary>
            </data>";

            // 转换为Kbin
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 转换回XML
            var result = KbinConverter.ReadXmlLinq(kbinData);
            
            // 验证结构和值
            var root = result.Root;
            Assert.Equal("data", root.Name.LocalName);
            
            var section = root.Element("section");
            Assert.Equal("main", section.Attribute("id").Value);
            
            Assert.Equal("12345", section.Element("item").Value);
            Assert.Equal("测试字符串", section.Element("item2").Value);
            
            var nested = section.Element("nested");
            Assert.Equal("255", nested.Element("value").Value);
            
            var array = nested.Element("array");
            Assert.Equal("-1 0 1", array.Value);
            
            var binary = root.Element("binary");
            Assert.Equal("DEADBEEF", binary.Value.ToUpperInvariant());
        }

        [Theory]
        [InlineData(KnownEncodings.ASCII)]
        [InlineData(KnownEncodings.EUC_JP)]
        [InlineData(KnownEncodings.ShiftJIS)]
        [InlineData(KnownEncodings.UTF8)]
        public void ReadWrite_DifferentEncodings_PreservesData(KnownEncodings encoding)
        {
            // 简单的XML，根据编码选择适当的文本
            string testText;
            switch (encoding)
            {
                case KnownEncodings.ASCII:
                    testText = "Test text";
                    break;
                case KnownEncodings.EUC_JP:
                case KnownEncodings.ShiftJIS:
                    testText = "Text";
                    break;
                default:
                    testText = "测试文本";
                    break;
            }
            
            var xml = $"<root><value __type=\"str\">{testText}</value></root>";
            
            // 转换为Kbin
            var kbinData = KbinConverter.Write(xml, encoding);
            
            // 转换回XML并检查编码
            var result = KbinConverter.ReadXmlLinq(kbinData, out var detectedEncoding);
            
            // 验证编码和内容
            Assert.Equal(encoding, detectedEncoding);
            Assert.Equal(testText, result.Root.Element("value").Value);
        }

        [Fact]
        public void WriteOptions_CustomPrefix_HandlesCorrectly()
        {
            // 使用需要前缀的XML
            var xml = "<root><_tag __type=\"s32\">123</_tag></root>";
            
            // 使用自定义前缀
            var options = new WriteOptions { RepairedPrefix = "prefix_" };
            var kbinData = KbinConverter.Write(xml, KnownEncodings.UTF8, options);
            
            // 读取回来并验证
            var result = KbinConverter.ReadXmlLinq(kbinData);
            
            // 应该保持原标签
            Assert.Equal("123", result.Root.Element("_tag").Value);
        }
    }
} 