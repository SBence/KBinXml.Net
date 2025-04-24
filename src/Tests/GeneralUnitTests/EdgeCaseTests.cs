using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;
using Xunit.Abstractions;

namespace GeneralUnitTests
{
    /// <summary>
    /// 边界情况测试 - 测试异常处理和边界条件
    /// </summary>
    public class EdgeCaseTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EdgeCaseTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        
        #region 异常测试
        
        [Fact]
        public void ReadXmlLinq_InvalidKbin_ThrowsKbinException()
        {
            // 准备无效的Kbin数据
            var invalidKbin = new byte[] { 0x42, 0x43, 0x44, 0x45 }; // 非Kbin格式的字节数组
            
            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.ReadXmlLinq(invalidKbin));
        }
        
        [Fact]
        public void Write_InvalidEncoding_ThrowsArgumentOutOfRangeException()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 将XML字符串转换为XmlDocument
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            
            // 使用无效的编码尝试转换
            Assert.Throws<ArgumentOutOfRangeException>(() => KbinConverter.Write(xmlDoc, (KnownEncodings)999));
        }
        
        [Fact]
        public void Write_NullXml_ThrowsArgumentNullException()
        {
            // 使用null XML尝试转换
            string xml = null;
            var stringException = Assert.Throws<ArgumentNullException>(() => KbinConverter.Write(xml, KnownEncodings.UTF8));
            Assert.IsType<ArgumentNullException>(stringException);
            Assert.Contains("xml", stringException.ParamName, StringComparison.OrdinalIgnoreCase);
            
            // 使用null XmlDocument尝试转换
            XmlDocument xmlDoc = null;
            var docException = Assert.Throws<ArgumentNullException>(() => KbinConverter.Write(xmlDoc, KnownEncodings.UTF8));
            Assert.IsType<ArgumentNullException>(docException);
            Assert.Contains("xml", docException.ParamName, StringComparison.OrdinalIgnoreCase);
            
            // 使用null XContainer尝试转换
            XContainer xContainer = null;
            var containerException = Assert.Throws<ArgumentNullException>(() => KbinConverter.Write(xContainer, KnownEncodings.UTF8));
            Assert.IsType<ArgumentNullException>(containerException);
            Assert.Contains("xml", containerException.ParamName, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void Write_InvalidXmlString_ThrowsXmlException()
        {
            // 准备无效的XML字符串（缺少关闭标签）
            var invalidXml = "<root><value __type=\"str\">测试</value>";
            
            // 验证抛出异常
            Assert.Throws<XmlException>(() => KbinConverter.Write(invalidXml, KnownEncodings.UTF8));
        }
        
        [Fact]
        public void ReadXmlBytes_InvalidKbin_ThrowsKbinException()
        {
            // 准备无效的Kbin数据
            var invalidKbin = new byte[] { 0x42, 0x43, 0x44, 0x45 }; // 非Kbin格式的字节数组
            
            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.ReadXmlBytes(invalidKbin));
        }
        
        [Fact]
        public void GetXmlStream_InvalidKbin_ThrowsKbinException()
        {
            // 准备无效的Kbin数据
            var invalidKbin = new byte[] { 0x42, 0x43, 0x44, 0x45 }; // 非Kbin格式的字节数组
            
            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.GetXmlStream(invalidKbin));
        }
        
        [Fact]
        public void ReadXml_InvalidKbin_ThrowsKbinException()
        {
            // 准备无效的Kbin数据
            var invalidKbin = new byte[] { 0x42, 0x43, 0x44, 0x45 }; // 非Kbin格式的字节数组
            
            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.ReadXml(invalidKbin));
        }
        
        #endregion
        
        #region 边界条件测试
        
        [Fact]
        public void EmptyXml_CanConvert()
        {
            // 准备空XML
            var xml = "<root></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证空XML正确转换
            Assert.NotNull(result);
            Assert.NotNull(result.Root);
            Assert.Equal("root", result.Root.Name);
            Assert.Equal(0, result.Root.Elements().Count());
        }
        
        [Fact]
        public void LargeXml_CanConvert()
        {
            // 生成大型XML
            var largeXmlBuilder = new StringBuilder();
            largeXmlBuilder.Append("<root>");
            
            for (int i = 0; i < 1000; i++)
            {
                largeXmlBuilder.Append($"<item id=\"{i}\" __type=\"s32\">{i}</item>");
            }
            
            largeXmlBuilder.Append("</root>");
            
            var largeXml = largeXmlBuilder.ToString();
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(largeXml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证大型XML正确转换
            Assert.NotNull(result);
            Assert.NotNull(result.Root);
            Assert.Equal(1000, result.Root.Elements("item").Count());
            
            // 检查某些特定元素的值
            var items = result.Root.Elements("item").ToList();
            var item0 = items.FirstOrDefault(e => e.Attribute("id")?.Value == "0");
            var item999 = items.FirstOrDefault(e => e.Attribute("id")?.Value == "999");
            
            Assert.NotNull(item0);
            Assert.NotNull(item999);
            Assert.Equal("0", item0.Value);
            Assert.Equal("999", item999.Value);
        }
        
        [Fact]
        public void DeepNestedXml_CanConvert()
        {
            // 生成深度嵌套的XML
            var deepXmlBuilder = new StringBuilder();
            deepXmlBuilder.Append("<root>");
            
            string currentTag = "<level1>";
            deepXmlBuilder.Append(currentTag);
            
            // 创建20层深度的XML
            for (int i = 2; i <= 19; i++)
            {
                currentTag = $"<level{i}>";
                deepXmlBuilder.Append(currentTag);
            }
            
            // 最内层添加__type属性
            deepXmlBuilder.Append("<level20 __type=\"str\">");
            deepXmlBuilder.Append("最深处");
            deepXmlBuilder.Append("</level20>");
            
            // 关闭所有标签
            for (int i = 19; i >= 1; i--)
            {
                deepXmlBuilder.Append($"</level{i}>");
            }
            
            deepXmlBuilder.Append("</root>");
            
            var deepXml = deepXmlBuilder.ToString();
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(deepXml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证深度嵌套XML正确转换
            Assert.NotNull(result);
            Assert.NotNull(result.Root);
            
            // 导航到最深层并检查值
            var element = result.Root.Element("level1");
            for (int i = 2; i <= 20; i++)
            {
                element = element.Element($"level{i}");
                Assert.NotNull(element);
            }
            
            Assert.Equal("最深处", element.Value);
        }
        
        [Fact]
        public void XmlWithSpecialChars_CanConvert()
        {
            // 准备包含特殊字符的XML
            var xml = "<root><value __type=\"str\">特殊字符: &lt;&gt;&amp;&quot;&apos;</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证包含特殊字符的XML正确转换
            Assert.Equal("特殊字符: <>&\"'", result.Root.Element("value").Value);
        }
        
        [Fact]
        public void MaxSizeArrays_CanConvert()
        {
            // 生成一个包含大量元素的数组
            var arrayBuilder = new StringBuilder();
            arrayBuilder.Append("<root><array __type=\"u8\" __count=\"1000\">");
            
            for (int i = 0; i < 1000; i++)
            {
                arrayBuilder.Append(i % 256);
                if (i < 999) arrayBuilder.Append(" ");
            }
            
            arrayBuilder.Append("</array></root>");
            
            var arrayXml = arrayBuilder.ToString();
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(arrayXml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数组信息正确
            var array = result.Root.Element("array");
            Assert.NotNull(array);
            Assert.Equal("u8", array.Attribute("__type").Value);
            Assert.Equal("1000", array.Attribute("__count").Value);
            
            // 验证数组内容
            var values = array.Value.Split(' ');
            Assert.Equal(1000, values.Length);
        }
        
        [Fact]
        public void ZeroLengthData_CanConvert()
        {
            // 准备包含零长度数据的XML
            var xml = "<root><bin __type=\"bin\" __size=\"0\"></bin></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证零长度数据正确转换
            var bin = result.Root.Element("bin");
            Assert.NotNull(bin);
            Assert.Equal("bin", bin.Attribute("__type").Value);
            Assert.Equal("0", bin.Attribute("__size").Value);
            Assert.Equal("", bin.Value);
        }
        
        #endregion
        
        #region 异常类测试
        
        [Fact]
        public void KbinException_DefaultConstructor_CreatesInstance()
        {
            // 调用默认构造函数
            var exception = new KbinException();
            
            // 验证实例已创建
            Assert.NotNull(exception);
            Assert.Equal("Exception of type 'KbinXml.Net.KbinException' was thrown.", exception.Message);
        }
        
        [Fact]
        public void KbinException_MessageConstructor_SetsMessage()
        {
            // 预期的错误消息
            const string expectedMessage = "测试错误消息";
            
            // 使用消息构造异常
            var exception = new KbinException(expectedMessage);
            
            // 验证消息已正确设置
            Assert.Equal(expectedMessage, exception.Message);
        }
        
        [Fact]
        public void KbinException_InnerExceptionConstructor_SetsMessageAndInnerException()
        {
            // 准备内部异常和消息
            const string expectedMessage = "外部异常消息";
            var innerException = new InvalidOperationException("内部异常消息");
            
            // 使用消息和内部异常构造异常
            var exception = new KbinException(expectedMessage, innerException);
            
            // 验证消息和内部异常已正确设置
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }
        
        [Fact]
        public void KbinException_DerivesFromException()
        {
            // 创建异常实例
            var exception = new KbinException();
            
            // 验证继承自Exception
            Assert.IsAssignableFrom<Exception>(exception);
        }
        
        [Fact]
        public void KbinTypeNotFoundException_DerivesFromKbinException()
        {
            // 创建类型未找到异常
            var typeException = new KbinTypeNotFoundException("test");
            
            // 验证继承自KbinException
            Assert.IsAssignableFrom<KbinException>(typeException);
        }

        [Fact]
        public void KbinTypeNotFoundException_IncludesTypeNameInMessage()
        {
            // 测试类型名
            const string typeName = "invalidType";
            
            // 创建异常
            var exception = new KbinTypeNotFoundException(typeName);
            
            // 验证类型名包含在消息中
            Assert.Contains(typeName, exception.Message);
        }
        
        #endregion
    }
} 