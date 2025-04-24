using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using KbinXml.Net;
using KbinXml.Net.Internal;
using KbinXml.Net.Internal.Writers;
using KbinXml.Net.Internal.Readers;
using Xunit;
using Xunit.Abstractions;

namespace GeneralUnitTests
{
    /// <summary>
    /// 调试单元测试，用于定位编码问题
    /// </summary>
    public class DebugUnitTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public DebugUnitTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void Debug_EncodingProcess()
        {
            // 准备一个简单的XML，添加__type属性
            var testXml = "<root><value __type=\"str\">测试</value></root>";
            _outputHelper.WriteLine($"原始XML: {testXml}");
            
            // 生成Kbin二进制数据
            var kbin = KbinConverter.Write(testXml, KnownEncodings.UTF8);
            _outputHelper.WriteLine($"Kbin大小: {kbin.Length} 字节");
            
            // 打印Kbin二进制数据的前64个字节，用于调试
            _outputHelper.WriteLine("Kbin数据的前64个字节:");
            for (int i = 0; i < Math.Min(64, kbin.Length); i++)
            {
                _outputHelper.WriteLine($"[{i,2}] 0x{kbin[i]:X2}");
            }

            // 尝试使用不同的编码读取
            foreach (KnownEncodings encoding in Enum.GetValues(typeof(KnownEncodings)))
            {
                try 
                {
                    _outputHelper.WriteLine($"\n尝试使用 {encoding} 解码:");
                    var result = KbinConverter.ReadXmlLinq(kbin);
                    _outputHelper.WriteLine($"解码结果: {result}");
                    
                    // 检查value元素的值
                    var valueText = result.Root?.Element("value")?.Value;
                    _outputHelper.WriteLine($"value元素的值: '{valueText}'");
                    
                    // 跟踪XML的完整结构
                    _outputHelper.WriteLine("XML结构:");
                    foreach (var element in result.Descendants())
                    {
                        _outputHelper.WriteLine($"元素: {element.Name}, 值: '{element.Value}', 类型: {element.Attribute("__type")?.Value}");
                    }
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"解码异常: {ex.Message}");
                }
            }
        }

        [Fact]
        public void Debug_EncodingBuffers()
        {
            // 测试不同的字符串编码结果
            var testString = "测试";
            _outputHelper.WriteLine($"测试字符串: '{testString}'");
            
            foreach (KnownEncodings encoding in Enum.GetValues(typeof(KnownEncodings)))
            {
                try
                {
                    var encodingObj = encoding.ToEncoding();
                    var bytes = encodingObj.GetBytes(testString);
                    _outputHelper.WriteLine($"\n编码: {encoding}");
                    _outputHelper.WriteLine($"编码后字节数: {bytes.Length}");
                    _outputHelper.WriteLine("字节内容:");
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        _outputHelper.WriteLine($"[{i}] 0x{bytes[i]:X2}");
                    }
                    
                    // 再次解码
                    var decoded = encodingObj.GetString(bytes);
                    _outputHelper.WriteLine($"解码后: '{decoded}'");
                    
                    // 尝试使用UTF8解码
                    var utf8Decoded = Encoding.UTF8.GetString(bytes);
                    _outputHelper.WriteLine($"UTF8解码: '{utf8Decoded}'");
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"编码异常: {ex.Message}");
                }
            }
        }

        [Fact/*(Skip = "NodeWriter不支持直接读取内部流")*/]
        public void Debug_NodeWriter_WriteString()
        {
            // 由于NodeWriter是不可变的ref struct，不支持直接获取内部流
            // 所以我们只测试整体XML的转换，而不是单独测试NodeWriter
            var testString = "测试";
            _outputHelper.WriteLine($"测试字符串: '{testString}'");
            
            foreach (KnownEncodings knownEncoding in Enum.GetValues(typeof(KnownEncodings)))
            {
                try
                {
                    _outputHelper.WriteLine($"\n===== 编码: {knownEncoding} =====");
                    
                    // 创建包含测试字符串的XML
                    var xml = $"<root><value __type=\"str\">{testString}</value></root>";
                    
                    // 转换为Kbin
                    var kbin = KbinConverter.Write(xml, knownEncoding);
                    
                    _outputHelper.WriteLine($"Kbin大小: {kbin.Length} 字节");
                    _outputHelper.WriteLine("Kbin数据的前32个字节:");
                    for (int i = 0; i < Math.Min(kbin.Length, 32); i++)
                    {
                        _outputHelper.WriteLine($"[{i,2}] 0x{kbin[i]:X2}");
                    }
                    
                    // 将Kbin转回XML
                    var resultXml = KbinConverter.ReadXmlLinq(kbin);
                    var valueText = resultXml.Root?.Element("value")?.Value;
                    _outputHelper.WriteLine($"解码后的值: '{valueText}'");
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"异常: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        [Fact/*(Skip = "DataReader不支持直接读取内部流")*/]
        public void Debug_DataReadWrite()
        {
            // 由于DataReader是不可变的ref struct，不支持直接读取内部流
            // 所以我们只测试整体XML的转换，而不是单独测试DataReader/DataWriter
            var testString = "测试";
            _outputHelper.WriteLine($"测试字符串: '{testString}'");
            
            foreach (KnownEncodings knownEncoding in Enum.GetValues(typeof(KnownEncodings)))
            {
                try
                {
                    _outputHelper.WriteLine($"\n===== 编码: {knownEncoding} =====");
                    
                    // 创建包含测试字符串的XML
                    var xml = $"<root><value __type=\"str\">{testString}</value></root>";
                    
                    // 转换为Kbin
                    var kbin = KbinConverter.Write(xml, knownEncoding);
                    
                    _outputHelper.WriteLine($"Kbin大小: {kbin.Length} 字节");
                    _outputHelper.WriteLine("Kbin数据的前32个字节:");
                    for (int i = 0; i < Math.Min(kbin.Length, 32); i++)
                    {
                        _outputHelper.WriteLine($"[{i,2}] 0x{kbin[i]:X2}");
                    }
                    
                    // 将Kbin转回XML
                    var resultXml = KbinConverter.ReadXmlLinq(kbin);
                    var valueText = resultXml.Root?.Element("value")?.Value;
                    _outputHelper.WriteLine($"解码后的值: '{valueText}'");
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"异常: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        [Fact]
        public void Debug_EncodingDetailTest()
        {
            var chineseStrings = new[] { "测试", "主服务器", "第一项", "有内容" };
            
            foreach (var testString in chineseStrings)
            {
                _outputHelper.WriteLine($"\n==== 测试字符串: '{testString}' ====");
                
                // 仅使用UTF8编码进行测试，避免使用不支持的编码
                foreach (KnownEncodings knownEncoding in new[] { KnownEncodings.UTF8, KnownEncodings.ASCII })
                {
                    try
                    {
                        var encoding = knownEncoding.ToEncoding();
                        _outputHelper.WriteLine($"\n=== 编码: {knownEncoding} ===");
                        
                        // 1. 编码成字节数组
                        var bytes = encoding.GetBytes(testString);
                        _outputHelper.WriteLine($"编码后字节数: {bytes.Length}");
                        _outputHelper.WriteLine("字节内容:");
                        
                        string bytesHex = BitConverter.ToString(bytes);
                        _outputHelper.WriteLine($"HEX: {bytesHex}");
                        
                        // 2. 生成XML并添加__type属性
                        var xml = $"<root><value __type=\"str\">{testString}</value></root>";
                        
                        // 3. 转换为KBin
                        var kbin = KbinConverter.Write(xml, knownEncoding);
                        _outputHelper.WriteLine($"\nKbin大小: {kbin.Length} 字节");
                        
                        // 4. 将KBin转回XML
                        var resultXml = KbinConverter.ReadXmlLinq(kbin);
                        var valueText = resultXml.Root?.Element("value")?.Value;
                        _outputHelper.WriteLine($"解码后的值: '{valueText}'");
                        
                        // 检查是否匹配
                        if (testString == valueText)
                        {
                            _outputHelper.WriteLine("✓ 匹配成功");
                        }
                        else
                        {
                            _outputHelper.WriteLine("✗ 匹配失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine($"异常: {ex.Message}");
                    }
                }
            }
        }

        [Fact]
        public void Debug_ComplexTypesTest()
        {
            _outputHelper.WriteLine("测试各种复杂类型的读写");

            // 准备测试各种数据类型的XML
            var xml = @"
            <root>
                <!-- 字符串类型 -->
                <strValue __type=""str"">测试字符串</strValue>
                
                <!-- 数值类型 -->
                <intValue __type=""s32"">42</intValue>
                <floatValue __type=""float"">3.14159</floatValue>
                
                <!-- 二进制类型 -->
                <binValue __type=""bin"" __size=""4"">DEADBEEF</binValue>
                
                <!-- 布尔类型 -->
                <boolValue __type=""bool"">1</boolValue>
                
                <!-- 数组类型 -->
                <intArray __type=""s32"" __count=""3"">10 20 30</intArray>
                <strArray>
                    <item __type=""str"">第一项</item>
                    <item __type=""str"">第二项</item>
                    <item __type=""str"">第三项</item>
                </strArray>
                
                <!-- 复杂结构 -->
                <section id=""1"">
                    <title __type=""str"">测试节</title>
                    <content __type=""str"">有内容</content>
                </section>
            </root>";

            try
            {
                // 转换为Kbin
                var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
                _outputHelper.WriteLine($"Kbin大小: {kbin.Length} 字节");
                
                // 将Kbin转回XML
                var resultXml = KbinConverter.ReadXmlLinq(kbin);
                _outputHelper.WriteLine($"读取回的XML: {resultXml}");
                
                // 验证各个元素
                _outputHelper.WriteLine("\n验证各个元素值:");
                
                var strValue = resultXml.Root?.Element("strValue")?.Value;
                _outputHelper.WriteLine($"strValue: '{strValue}'");
                
                var intValue = resultXml.Root?.Element("intValue")?.Value;
                _outputHelper.WriteLine($"intValue: '{intValue}'");
                
                var floatValue = resultXml.Root?.Element("floatValue")?.Value;
                _outputHelper.WriteLine($"floatValue: '{floatValue}'");
                
                var binValue = resultXml.Root?.Element("binValue")?.Value;
                _outputHelper.WriteLine($"binValue: '{binValue}'");
                
                var boolValue = resultXml.Root?.Element("boolValue")?.Value;
                _outputHelper.WriteLine($"boolValue: '{boolValue}'");
                
                var intArray = resultXml.Root?.Element("intArray")?.Value;
                _outputHelper.WriteLine($"intArray: '{intArray}'");
                
                var strArrayItems = resultXml.Root?.Element("strArray")?.Elements("item").Select(e => e.Value).ToList();
                if (strArrayItems != null)
                {
                    for (int i = 0; i < strArrayItems.Count; i++)
                    {
                        _outputHelper.WriteLine($"strArray[{i}]: '{strArrayItems[i]}'");
                    }
                }
                
                var sectionTitle = resultXml.Root?.Element("section")?.Element("title")?.Value;
                _outputHelper.WriteLine($"section.title: '{sectionTitle}'");
                
                var sectionContent = resultXml.Root?.Element("section")?.Element("content")?.Value;
                _outputHelper.WriteLine($"section.content: '{sectionContent}'");
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [Fact]
        public void Debug_AllNodeTypesTest()
        {
            _outputHelper.WriteLine("测试所有支持的节点类型");

            // 准备包含所有支持节点类型的XML
            var xml = @"
            <root>
                <!-- 基本数值类型 -->
                <s8Value __type=""s8"">-128</s8Value>
                <u8Value __type=""u8"">255</u8Value>
                <s16Value __type=""s16"">-32768</s16Value>
                <u16Value __type=""u16"">65535</u16Value>
                <s32Value __type=""s32"">-2147483648</s32Value>
                <u32Value __type=""u32"">4294967295</u32Value>
                <s64Value __type=""s64"">-9223372036854775808</s64Value>
                <u64Value __type=""u64"">18446744073709551615</u64Value>
                
                <!-- 字符串和二进制 -->
                <strValue __type=""str"">测试字符串</strValue>
                <binValue __type=""bin"" __size=""4"">DEADBEEF</binValue>
                
                <!-- 特殊类型 -->
                <ipValue __type=""ip4"">192.168.1.1</ipValue>
                <timeValue __type=""time"">1633046400</timeValue>
                <floatValue __type=""float"">3.14159</floatValue>
                <doubleValue __type=""double"">1.7976931348623157E+308</doubleValue>
                
                <!-- 向量类型 -->
                <vec2s8 __type=""2s8"">1 -2</vec2s8>
                <vec2u8 __type=""2u8"">3 4</vec2u8>
                <vec2f __type=""2f"">1.1 2.2</vec2f>
                
                <!-- 数组类型 -->
                <intArray __type=""s32"" __count=""3"">10 20 30</intArray>
                <boolArray __type=""bool"" __count=""4"">1 0 1 0</boolArray>
            </root>";

            try
            {
                // 转换为Kbin
                var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
                _outputHelper.WriteLine($"Kbin大小: {kbin.Length} 字节");
                
                // 将Kbin转回XML
                var resultXml = KbinConverter.ReadXmlLinq(kbin);
                
                // 验证各个元素
                _outputHelper.WriteLine("\n验证各个元素值:");
                
                foreach (var element in resultXml.Root.Elements())
                {
                    var typeName = element.Attribute("__type")?.Value;
                    _outputHelper.WriteLine($"{element.Name} ({typeName}): '{element.Value}'");
                }
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
} 