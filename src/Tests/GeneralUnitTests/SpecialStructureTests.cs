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
    /// 特殊XML结构测试 - 测试处理特殊XML结构的能力
    /// </summary>
    public class SpecialStructureTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SpecialStructureTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        
        [Fact]
        public void NestedElements_PreservesStructure()
        {
            // 准备嵌套XML
            var xml = @"
            <root>
                <level1>
                    <level2>
                        <level3 __type=""str"">深度嵌套测试</level3>
                    </level2>
                </level1>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证嵌套结构保留
            Assert.NotNull(result.Root.Element("level1"));
            Assert.NotNull(result.Root.Element("level1").Element("level2"));
            Assert.NotNull(result.Root.Element("level1").Element("level2").Element("level3"));
            Assert.Equal("深度嵌套测试", result.Root.Element("level1").Element("level2").Element("level3").Value);
        }
        
        [Fact]
        public void AttributesInNodes_PreservesAttributes()
        {
            // 准备带属性的XML
            var xml = @"
            <root version=""1.0"">
                <node id=""1"" name=""节点1"" __type=""str"">值1</node>
                <node id=""2"" name=""节点2"" __type=""str"">值2</node>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证根节点属性保留
            Assert.Equal("1.0", result.Root.Attribute("version").Value);
            
            // 验证子节点属性保留
            var nodes = result.Root.Elements("node");
            int nodeCount = 0;
            
            foreach (var node in nodes)
            {
                nodeCount++;
                if (node.Attribute("id").Value == "1")
                {
                    Assert.Equal("节点1", node.Attribute("name").Value);
                    Assert.Equal("值1", node.Value);
                }
                else if (node.Attribute("id").Value == "2")
                {
                    Assert.Equal("节点2", node.Attribute("name").Value);
                    Assert.Equal("值2", node.Value);
                }
            }
            
            Assert.Equal(2, nodeCount);
        }
        
        [Fact]
        public void EmptyElements_PreservesStructure()
        {
            // 准备包含空元素的XML
            var xml = @"
            <root>
                <empty1 />
                <empty2></empty2>
                <notEmpty __type=""str"">有内容</notEmpty>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证空元素保留
            Assert.NotNull(result.Root.Element("empty1"));
            Assert.NotNull(result.Root.Element("empty2"));
            Assert.Equal(string.Empty, result.Root.Element("empty1").Value);
            Assert.Equal(string.Empty, result.Root.Element("empty2").Value);
            
            // 验证非空元素正确
            Assert.Equal("有内容", result.Root.Element("notEmpty").Value);
        }
        
        [Fact]
        public void MixedContentElements_PreservesStructure()
        {
            // 准备混合内容的XML
            var xml = @"
            <root>
                <mixed __type=""str"">文本1<inner __type=""str"">内部元素</inner>文本2</mixed>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证混合内容结构
            var mixedElement = result.Root.Element("mixed");
            Assert.NotNull(mixedElement);
            
            // 注意：Kbin XML转换后可能不支持保留混合内容，但应确保内部元素正常
            Assert.NotNull(mixedElement.Element("inner"));
            Assert.Equal("内部元素", mixedElement.Element("inner").Value);
        }
        
        [Fact]
        public void ArrayWithDifferentTypes_PreservesStructure()
        {
            // 准备包含不同类型数组的XML
            var xml = @"
            <root>
                <numbers>
                    <int __type=""s32"">10</int>
                    <int __type=""s32"">20</int>
                    <float __type=""u32"">30</float>
                </numbers>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数组结构保留
            var numbers = result.Root.Element("numbers");
            Assert.NotNull(numbers);
            
            // 验证每个元素的类型和值
            var intElements = numbers.Elements("int");
            var intCount = 0;
            foreach (var intElem in intElements)
            {
                intCount++;
                Assert.Equal("s32", intElem.Attribute("__type").Value);
                Assert.True(intElem.Value == "10" || intElem.Value == "20");
            }
            Assert.Equal(2, intCount);
            
            var floatElement = numbers.Element("float");
            Assert.NotNull(floatElement);
            Assert.Equal("u32", floatElement.Attribute("__type").Value);
            Assert.Equal("30", floatElement.Value);
        }
        
        [Fact]
        public void SameNameNodes_PreservesStructure()
        {
            // 准备具有相同名称节点的XML
            var xml = @"
            <root>
                <item __type=""str"">第一项</item>
                <item __type=""str"">第二项</item>
                <item __type=""str"">第三项</item>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证相同名称节点保留
            var items = result.Root.Elements("item");
            var values = new[] { "第一项", "第二项", "第三项" };
            int i = 0;
            
            foreach (var item in items)
            {
                Assert.Equal(values[i], item.Value);
                i++;
            }
            
            Assert.Equal(3, i);
        }
        
        [Fact]
        public void ReadWriteComplex_PreservesStructure()
        {
            // 准备复杂XML
            var xml = @"
            <root version=""2.0"">
                <header>
                    <title __type=""str"">测试文档</title>
                    <author __type=""str"">测试用户</author>
                    <date __type=""str"">2023-10-15</date>
                </header>
                <body>
                    <section id=""1"">
                        <title __type=""str"">第一节</title>
                        <paragraph __type=""str"">这是第一段落，包含<emphasis __type=""str"">强调文本</emphasis>。</paragraph>
                        <stats>
                            <value __type=""s32"">42</value>
                            <value __type=""s32"">-10</value>
                            <value __type=""u8"">255</value>
                        </stats>
                    </section>
                    <section id=""2"">
                        <title __type=""str"">第二节</title>
                        <paragraph __type=""str"">这是第二段落。</paragraph>
                        <list __type=""u8"" __count=""3"">1 2 3</list>
                    </section>
                </body>
                <footer>
                    <note __type=""str"">文档结束</note>
                </footer>
            </root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证整体结构保留
            Assert.Equal("2.0", result.Root.Attribute("version").Value);
            
            // 验证标题
            Assert.Equal("测试文档", result.Root.Element("header").Element("title").Value);
            Assert.Equal("测试用户", result.Root.Element("header").Element("author").Value);
            Assert.Equal("2023-10-15", result.Root.Element("header").Element("date").Value);
            Assert.Equal("str", result.Root.Element("header").Element("date").Attribute("__type").Value);
            
            // 验证第一节
            var section1 = result.Root.Element("body").Elements("section").First(s => s.Attribute("id").Value == "1");
            Assert.Equal("第一节", section1.Element("title").Value);
            Assert.NotNull(section1.Element("paragraph").Element("emphasis"));
            Assert.Equal("强调文本", section1.Element("paragraph").Element("emphasis").Value);
            
            var stats = section1.Element("stats").Elements("value");
            Assert.Equal(3, stats.Count());
            
            // 验证第二节
            var section2 = result.Root.Element("body").Elements("section").First(s => s.Attribute("id").Value == "2");
            Assert.Equal("第二节", section2.Element("title").Value);
            Assert.Equal("这是第二段落。", section2.Element("paragraph").Value);
            
            var list = section2.Element("list");
            Assert.Equal("u8", list.Attribute("__type").Value);
            Assert.Equal("3", list.Attribute("__count").Value);
            Assert.Equal("1 2 3", list.Value);
            
            // 验证页脚
            Assert.Equal("文档结束", result.Root.Element("footer").Element("note").Value);
        }

        #region 属性和数组测试
        
        [Fact]
        public void TestMultipleAttributes()
        {
            // 准备包含多个属性的XML
            var xml = "<root id=\"1\" name=\"test\" value=\"123\"><node attr1=\"val1\" attr2=\"val2\" /></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证属性保留
            Assert.Equal("1", result.Root.Attribute("id").Value);
            Assert.Equal("test", result.Root.Attribute("name").Value);
            Assert.Equal("123", result.Root.Attribute("value").Value);
            
            var node = result.Root.Element("node");
            Assert.NotNull(node);
            Assert.Equal("val1", node.Attribute("attr1").Value);
            Assert.Equal("val2", node.Attribute("attr2").Value);
            
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestEmptyArray()
        {
            // 准备空数组XML
            var xml = "<root><array __type=\"s32\" __count=\"0\"></array></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数组属性
            var array = result.Root.Element("array");
            Assert.NotNull(array);
            Assert.Equal("s32", array.Attribute("__type").Value);
            Assert.Equal("0", array.Attribute("__count").Value);
            Assert.Equal("", array.Value);
            
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestLargeArray()
        {
            // 生成大型数组数据
            var values = string.Join(" ", Enumerable.Range(1, 1000));
            var xml = $"<root><array __type=\"s32\" __count=\"1000\">{values}</array></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证数组属性和内容
            var array = result.Root.Element("array");
            Assert.NotNull(array);
            Assert.Equal("s32", array.Attribute("__type").Value);
            Assert.Equal("1000", array.Attribute("__count").Value);
            
            // 验证数组元素
            var resultValues = array.Value.Split(' ');
            Assert.Equal(1000, resultValues.Length);
            Assert.Equal("1", resultValues[0]);
            Assert.Equal("1000", resultValues[999]);
            
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestMixedArrayTypes()
        {
            // 准备不同类型的数组XML
            var xml = "<root>" +
                      "<array1 __type=\"s8\" __count=\"3\">1 2 3</array1>" +
                      "<array2 __type=\"u16\" __count=\"3\">1000 2000 3000</array2>" +
                      "<array3 __type=\"s32\" __count=\"3\">-1 -2 -3</array3>" +
                      "</root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证不同类型数组
            Assert.Equal("s8", result.Root.Element("array1").Attribute("__type").Value);
            Assert.Equal("3", result.Root.Element("array1").Attribute("__count").Value);
            Assert.Equal("1 2 3", result.Root.Element("array1").Value);
            
            Assert.Equal("u16", result.Root.Element("array2").Attribute("__type").Value);
            Assert.Equal("3", result.Root.Element("array2").Attribute("__count").Value);
            Assert.Equal("1000 2000 3000", result.Root.Element("array2").Value);
            
            Assert.Equal("s32", result.Root.Element("array3").Attribute("__type").Value);
            Assert.Equal("3", result.Root.Element("array3").Attribute("__count").Value);
            Assert.Equal("-1 -2 -3", result.Root.Element("array3").Value);
            
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestInvalidArrayCount()
        {
            // 准备数组计数不匹配的XML
            var xml = "<root><array __type=\"s32\" __count=\"3\">1 2</array></root>";
            
            // 设置严格模式选项
            var writeOptions = new WriteOptions()
            {
                StrictMode = true
            };
            
            // 验证抛出异常
            Assert.Throws<KbinArrayCountMissMatchException>(() => 
                KbinConverter.Write(xml, KnownEncodings.UTF8, writeOptions));
        }

        [Fact]
        public void TestInvalidArrayType()
        {
            // 准备无效类型的数组XML
            var xml = "<root><array __type=\"invalid\" __count=\"1\">1</array></root>";
            
            // 验证抛出异常
            Assert.Throws<KbinTypeNotFoundException>(() => 
                KbinConverter.Write(xml, KnownEncodings.UTF8));
        }
        
        #endregion
        
        #region 特殊情况测试
        
        [Fact]
        public void TestEmptyNode()
        {
            // 准备包含空节点的XML
            var xml = "<root><empty></empty><self_closing /></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证空节点正确转换
            Assert.NotNull(result.Root.Element("empty"));
            Assert.NotNull(result.Root.Element("self_closing"));
            Assert.Equal(string.Empty, result.Root.Element("empty").Value);

            var result2 = new StableKbin.XmlReader(kbin).ReadLinq();
            Assert.Equal(result2.ToString(SaveOptions.DisableFormatting), result.ToString(SaveOptions.DisableFormatting));
        }

        [Theory]
        [InlineData("<root><node __type=\"str\">特殊文字：&amp;&lt;&gt;'\"</node></root>", KnownEncodings.UTF8)]
        [InlineData("<root><node __type=\"str\">日本語テスト</node></root>", KnownEncodings.ShiftJIS)]
        public void TestSpecialCharactersAndEncoding(string xml, KnownEncodings encoding)
        {
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, encoding);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证特殊字符和编码正确处理
            var xmlElement = XElement.Parse(xml);
            Assert.Equal(xmlElement.Element("node").Value, result.Root.Element("node").Value);

            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestDeepNestedNodes()
        {
            // 准备深度嵌套的XML
            var xml = "<root><a><b><c><d><e __type=\"str\">深层嵌套测试</e></d></c></b></a></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证深度嵌套结构保留
            var eElement = result.Root.Element("a").Element("b").Element("c").Element("d").Element("e");
            Assert.NotNull(eElement);
            Assert.Equal("深层嵌套测试", eElement.Value);

            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestLargeXml()
        {
            // 准备大型XML
            var builder = new StringBuilder("<root>");
            for (int i = 0; i < 100; i++)
            {
                builder.AppendFormat("<item __type=\"u32\" id=\"{0}\">{0}</item>", i);
            }
            builder.Append("</root>");
            var xml = builder.ToString();

            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证大型XML结构保留
            var items = result.Root.Elements("item").ToList();
            Assert.Equal(100, items.Count);
            
            // 验证某些特定元素
            Assert.Equal("0", items[0].Value);
            Assert.Equal("0", items[0].Attribute("id").Value);
            Assert.Equal("99", items[99].Value);
            Assert.Equal("99", items[99].Attribute("id").Value);

            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestInvalidXmlThrowsException()
        {
            // 准备无效XML
            var invalidXml = "<root><unclosed>";
            
            // 验证抛出异常
            Assert.Throws<XmlException>(() => KbinConverter.Write(invalidXml, KnownEncodings.UTF8));
        }
        
        #endregion
    }
} 