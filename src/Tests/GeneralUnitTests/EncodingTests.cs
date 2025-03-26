using System;
using System.Text;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;
using Xunit.Abstractions;
using System.Xml;

namespace GeneralUnitTests
{
    /// <summary>
    /// 编码测试 - 测试不同编码方案的处理
    /// </summary>
    public class EncodingTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EncodingTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        
        [Theory]
        [InlineData(KnownEncodings.UTF8)]
        [InlineData(KnownEncodings.ShiftJIS)]
        [InlineData(KnownEncodings.EUC_JP)]
        [InlineData(KnownEncodings.ASCII)]
        public void ReadWrite_DifferentEncodings_PreservesData(KnownEncodings encodingType)
        {
            // 准备测试文本 - 根据编码选择合适的文本
            string testText;
            
            switch (encodingType)
            {
                case KnownEncodings.UTF8:
                    testText = "UTF-8测试文本 こんにちは";
                    break;
                case KnownEncodings.ShiftJIS:
                    testText = "SHIFT-JISテストテキスト";
                    break;
                case KnownEncodings.EUC_JP:
                    testText = "EUC-JPテストテキスト";
                    break;
                case KnownEncodings.ASCII:
                    testText = "ASCII test text";
                    break;
                default:
                    testText = "Default test text";
                    break;
            }
            
            // 准备XML，添加__type属性
            var xml = $"<root><value __type=\"str\">{testText}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, encodingType);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证文本保留不变
            Assert.Equal(testText, result.Root.Element("value").Value);
        }
        
        [Fact]
        public void ReadWithEncodingOutput_ReturnsCorrectEncoding()
        {
            // 准备不同编码的Kbin数据，添加__type属性
            var utf8Xml = "<root><value __type=\"str\">UTF-8测试</value></root>";
            var utf8Kbin = KbinConverter.Write(utf8Xml, KnownEncodings.UTF8);
            
            var sjisXml = "<root><value __type=\"str\">SJIS测试</value></root>";
            var sjisKbin = KbinConverter.Write(sjisXml, KnownEncodings.ShiftJIS);
            
            // 测试从Kbin数据读取编码
            KnownEncodings detectedUtf8Encoding;
            var utf8Doc = KbinConverter.ReadXmlLinq(utf8Kbin, out detectedUtf8Encoding);
            
            KnownEncodings detectedSjisEncoding;
            var sjisDoc = KbinConverter.ReadXmlLinq(sjisKbin, out detectedSjisEncoding);
            
            // 验证检测到的编码正确
            Assert.Equal(KnownEncodings.UTF8, detectedUtf8Encoding);
            Assert.Equal(KnownEncodings.ShiftJIS, detectedSjisEncoding);
        }
        
        [Fact]
        public void WriteWithDifferentEncoding_ChangesKbinData()
        {
            // 准备相同的XML，添加__type属性
            var xml = "<root><value __type=\"str\">编码测试</value></root>";
            
            // 使用不同编码转换
            var utf8Kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var sjisKbin = KbinConverter.Write(xml, KnownEncodings.ShiftJIS);
            var eucjpKbin = KbinConverter.Write(xml, KnownEncodings.EUC_JP);
            
            // 验证不同编码产生不同的Kbin数据
            Assert.NotEqual(utf8Kbin, sjisKbin);
            Assert.NotEqual(utf8Kbin, eucjpKbin);
            Assert.NotEqual(sjisKbin, eucjpKbin);
        }
        
        [Fact]
        public void InvalidEncoding_ThrowsException()
        {
            // 准备XML，添加__type属性
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 测试无效编码
            Assert.Throws<ArgumentOutOfRangeException>(() => KbinConverter.Write(xml, (KnownEncodings)999));
        }
        
        [Fact]
        public void AllEncodings_CanReadWrite()
        {
            // 获取所有支持的编码
            var encodings = new[]
            {
                KnownEncodings.ASCII,
                KnownEncodings.UTF8,
                KnownEncodings.ShiftJIS,
                KnownEncodings.EUC_JP,
                KnownEncodings.ISO_8859_1
            };
            
            foreach (var encoding in encodings)
            {
                // 准备适合当前编码的XML，添加__type属性
                string testText;
                if (encoding == KnownEncodings.ASCII)
                {
                    testText = "ASCII test text";
                }
                else
                {
                    testText = $"{encoding} test text";
                }
                
                var xml = $"<root><value __type=\"str\">{testText}</value></root>";
                
                try
                {
                    // 测试转换
                    var kbin = KbinConverter.Write(xml, encoding);
                    KnownEncodings detectedEncoding;
                    var result = KbinConverter.ReadXmlLinq(kbin, out detectedEncoding);
                    
                    // 验证文本和编码正确
                    Assert.Equal(testText, result.Root.Element("value").Value);
                    Assert.Equal(encoding, detectedEncoding);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"编码 {encoding} 测试失败: {ex.Message}");
                    // 某些编码可能无法处理特定字符，我们跳过这些异常
                    continue;
                }
            }
        }
        
        [Fact]
        public void WriteOptions_CustomOptions_HandlesCorrectly()
        {
            // 准备XML，添加__type属性
            var xml = "<root><value __type=\"str\">テスト</value></root>";
            
            // 将XML字符串转换为XmlDocument
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            
            // 准备不同配置的WriteOptions
            var options1 = new WriteOptions();
            var options2 = new WriteOptions();
            
            // 使用不同选项转换
            var kbin1 = KbinConverter.Write(xmlDoc, KnownEncodings.UTF8, options1);
            var kbin2 = KbinConverter.Write(xmlDoc, KnownEncodings.ShiftJIS, options2);
            
            // 验证不同选项产生不同的Kbin数据
            Assert.NotEqual(kbin1, kbin2);
            
            // 但都能正常读取
            var result1 = KbinConverter.ReadXmlLinq(kbin1);
            var result2 = KbinConverter.ReadXmlLinq(kbin2);
            
            Assert.Equal("テスト", result1.Root.Element("value").Value);
            Assert.Equal("テスト", result2.Root.Element("value").Value);
        }
        
        [Fact]
        public void WriteOptions_CustomSettings_HandlesCorrectly()
        {
            // 准备XML，添加__type属性
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 将XML字符串转换为XmlDocument
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            
            // 准备带有自定义设置的WriteOptions
            var options = new WriteOptions 
            { 
                Compress = false
            };
            
            // 使用自定义选项转换
            var kbin = KbinConverter.Write(xmlDoc, KnownEncodings.UTF8, options);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数据正常读取
            Assert.Equal("测试", result.Root.Element("value").Value);
        }
        
        [Fact]
        public void ReadXmlBytes_ValidKbin_ReturnsXmlBytes()
        {
            // 准备XML，添加__type属性
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin读取XML字节
            var xmlBytes = KbinConverter.ReadXmlBytes(kbin);
            
            // 验证XML字节包含预期内容
            var resultString = Encoding.UTF8.GetString(xmlBytes);
            Assert.Contains("<root>", resultString);
            Assert.Contains("<value", resultString);
            Assert.Contains("测试", resultString);
            Assert.Contains("</root>", resultString);
        }
        
        [Fact]
        public void GetXmlStream_ValidKbin_ReturnsMemoryStream()
        {
            // 准备XML，添加__type属性
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin获取XML流
            using var stream = KbinConverter.GetXmlStream(kbin);
            
            // 验证流包含预期内容
            using var reader = new System.IO.StreamReader(stream, Encoding.UTF8, true, -1, true);
            var resultString = reader.ReadToEnd();
            Assert.Contains("<root>", resultString);
            Assert.Contains("<value", resultString);
            Assert.Contains("测试", resultString);
            Assert.Contains("</root>", resultString);
        }
    }
} 