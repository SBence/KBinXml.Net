using System;
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
    /// Kbin基本功能测试 - 测试KbinConverter的基本读写功能
    /// </summary>
    public class KbinBasicTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public KbinBasicTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region 基本读取功能测试

        [Fact]
        public void ReadXmlLinq_ValidKbin_ReturnsXDocument()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin读取XDocument
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证结果
            Assert.NotNull(result);
            Assert.NotNull(result.Root);
            Assert.Equal("root", result.Root.Name.ToString());
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        [Fact]
        public void ReadXmlLinq_WithEncodingOutput_ReturnsEncodingAndXDocument()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin读取XDocument并获取编码信息
            KnownEncodings encoding;
            var result = KbinConverter.ReadXmlLinq(kbin, out encoding);
            
            // 验证结果
            Assert.NotNull(result);
            Assert.Equal(KnownEncodings.UTF8, encoding);
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        [Fact]
        public void ReadXmlBytes_ValidKbin_ReturnsXmlBytes()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin读取XML字节
            var xmlBytes = KbinConverter.ReadXmlBytes(kbin);
            
            // 验证结果
            Assert.NotNull(xmlBytes);
            Assert.True(xmlBytes.Length > 0);
            
            // 将字节转换回字符串并验证内容
            var resultStr = Encoding.UTF8.GetString(xmlBytes);
            Assert.Contains("<root>", resultStr);
            Assert.Contains("<value", resultStr);
            Assert.Contains("测试", resultStr);
            Assert.Contains("</root>", resultStr);
        }

        [Fact]
        public void GetXmlStream_ValidKbin_ReturnsMemoryStream()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin获取MemoryStream
            using var stream = KbinConverter.GetXmlStream(kbin);
            
            // 验证结果
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
            
            // 读取流并验证内容
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);
            var resultStr = reader.ReadToEnd();
            Assert.Contains("<root>", resultStr);
            Assert.Contains("<value", resultStr);
            Assert.Contains("测试", resultStr);
            Assert.Contains("</root>", resultStr);
        }

        [Fact]
        public void ReadXml_ValidKbin_ReturnsXmlDocument()
        {
            // 准备有效的XML
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 从Kbin读取XmlDocument
            var result = KbinConverter.ReadXml(kbin);
            
            // 验证结果
            Assert.NotNull(result);
            Assert.NotNull(result.DocumentElement);
            Assert.Equal("root", result.DocumentElement.Name);
            Assert.Equal("测试", result.DocumentElement.SelectSingleNode("value").InnerText);
        }

        #endregion

        #region 基本写入功能测试

        [Fact]
        public void Write_XmlDocument_ReturnsKbinBytes()
        {
            // 准备XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<root><value __type=\"str\">测试</value></root>");
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(doc, KnownEncodings.UTF8);
            
            // 验证结果
            Assert.NotNull(kbin);
            Assert.True(kbin.Length > 0);
            
            // 验证可以被读回
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XContainer_ReturnsKbinBytes()
        {
            // 准备XDocument
            var doc = new XDocument(
                new XElement("root",
                    new XElement("value", new XAttribute("__type", "str"), "测试")
                )
            );
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(doc, KnownEncodings.UTF8);
            
            // 验证结果
            Assert.NotNull(kbin);
            Assert.True(kbin.Length > 0);
            
            // 验证可以被读回
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XmlString_ReturnsKbinBytes()
        {
            // 准备XML字符串
            var xml = "<root><value __type=\"str\">测试</value></root>";
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            
            // 验证结果
            Assert.NotNull(kbin);
            Assert.True(kbin.Length > 0);
            
            // 验证可以被读回
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        [Fact]
        public void Write_XmlBytes_ReturnsKbinBytes()
        {
            // 准备XML字节
            var xmlStr = "<root><value __type=\"str\">测试</value></root>";
            var xmlBytes = Encoding.UTF8.GetBytes(xmlStr);
            
            // 转换为Kbin
            var kbin = KbinConverter.Write(xmlBytes, KnownEncodings.UTF8);
            
            // 验证结果
            Assert.NotNull(kbin);
            Assert.True(kbin.Length > 0);
            
            // 验证可以被读回
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal("测试", result.Root.Element("value").Value);
        }

        #endregion

        #region 读写结合测试

        [Fact]
        public void ReadWriteSimple_PreservesContent()
        {
            // 准备简单XML
            var xml = "<root><value __type=\"str\">Simple Test</value><version __type=\"str\">1.0</version></root>";
            
            // 转换为Kbin并读回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证内容一致性
            Assert.Equal("Simple Test", result.Root.Element("value").Value);
            Assert.Equal("1.0", result.Root.Element("version").Value);
        }

        [Fact]
        public void ReadWriteWithAttributes_PreservesAttributes()
        {
            // 准备带属性的XML
            var xml = "<root><item id=\"1\" name=\"测试项\" enabled=\"true\" __type=\"str\">带属性的项目</item></root>";
            
            // 转换为Kbin并读回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证属性一致性
            var itemElement = result.Root.Element("item");
            Assert.Equal("1", itemElement.Attribute("id").Value);
            Assert.Equal("测试项", itemElement.Attribute("name").Value);
            Assert.Equal("true", itemElement.Attribute("enabled").Value);
            Assert.Equal("带属性的项目", itemElement.Value);
        }

        [Fact]
        public void ReadWriteNestedElements_PreservesStructure()
        {
            // 准备嵌套XML
            var xml = @"
            <root>
                <parent>
                    <child1 __type=""str"">值1</child1>
                    <child2 __type=""str"">值2</child2>
                    <subParent>
                        <subChild __type=""str"">嵌套值</subChild>
                    </subParent>
                </parent>
            </root>";
            
            // 转换为Kbin并读回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证嵌套结构一致性
            var parentElement = result.Root.Element("parent");
            Assert.Equal("值1", parentElement.Element("child1").Value);
            Assert.Equal("值2", parentElement.Element("child2").Value);
            Assert.Equal("嵌套值", parentElement.Element("subParent").Element("subChild").Value);
        }

        [Fact]
        public void ReadWriteElementArray_PreservesArray()
        {
            // 准备数组XML
            var xml = @"
            <root>
                <items>
                    <item __type=""str"">第一项</item>
                    <item __type=""str"">第二项</item>
                    <item __type=""str"">第三项</item>
                </items>
            </root>";
            
            // 转换为Kbin并读回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数组一致性
            var itemElements = result.Root.Element("items").Elements("item").ToList();
            Assert.Equal(3, itemElements.Count);
            Assert.Equal("第一项", itemElements[0].Value);
            Assert.Equal("第二项", itemElements[1].Value);
            Assert.Equal("第三项", itemElements[2].Value);
        }

        [Fact]
        public void ReadWriteWithAttributesAndElements_PreservesAll()
        {
            // 准备复杂XML（混合属性和元素）
            var xml = @"
            <config version=""1.0"">
                <server id=""main"" port=""8080"">
                    <name __type=""str"">主服务器</name>
                    <services>
                        <service type=""http"" __type=""str"">Web服务</service>
                        <service type=""ftp"" __type=""str"">文件服务</service>
                    </services>
                </server>
            </config>";
            
            // 转换为Kbin并读回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证复杂结构一致性
            Assert.Equal("1.0", result.Root.Attribute("version").Value);
            
            var serverElement = result.Root.Element("server");
            Assert.Equal("main", serverElement.Attribute("id").Value);
            Assert.Equal("8080", serverElement.Attribute("port").Value);
            Assert.Equal("主服务器", serverElement.Element("name").Value);
            
            var serviceElements = serverElement.Element("services").Elements("service").ToList();
            Assert.Equal(2, serviceElements.Count);
            Assert.Equal("http", serviceElements[0].Attribute("type").Value);
            Assert.Equal("Web服务", serviceElements[0].Value);
            Assert.Equal("ftp", serviceElements[1].Attribute("type").Value);
            Assert.Equal("文件服务", serviceElements[1].Value);
        }

        #endregion

        #region 性能测试

        [Fact(Skip = "性能测试仅在需要时运行")]
        public void WriteRead_Performance_IsReasonable()
        {
            // 准备大型XML
            var largeXmlBuilder = new StringBuilder();
            largeXmlBuilder.Append("<root>");
            
            for (int i = 0; i < 10000; i++)
            {
                largeXmlBuilder.Append($"<item id=\"{i}\" __type=\"s32\">{i}</item>");
            }
            
            largeXmlBuilder.Append("</root>");
            
            var largeXml = largeXmlBuilder.ToString();
            
            // 计时写入操作
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var kbin = KbinConverter.Write(largeXml, KnownEncodings.UTF8);
            stopwatch.Stop();
            var writeTime = stopwatch.ElapsedMilliseconds;
            
            // 计时读取操作
            stopwatch.Restart();
            var result = KbinConverter.ReadXmlLinq(kbin);
            stopwatch.Stop();
            var readTime = stopwatch.ElapsedMilliseconds;
            
            // 输出性能指标
            _outputHelper.WriteLine($"写入时间: {writeTime}ms, 读取时间: {readTime}ms");
            
            // 验证结果正确
            Assert.Equal(10000, result.Root.Elements("item").Count());
            Assert.Equal("42", result.Root.Elements("item").First(e => e.Attribute("id").Value == "42").Value);
            
            // 性能要求 - 可调整具体阈值
            Assert.True(writeTime < 5000, $"写入操作耗时过长: {writeTime}ms");
            Assert.True(readTime < 5000, $"读取操作耗时过长: {readTime}ms");
        }

        #endregion
    }
} 