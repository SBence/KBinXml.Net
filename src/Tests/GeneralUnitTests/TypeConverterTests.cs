using System;
using System.Text;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;

namespace GeneralUnitTests
{
    public class TypeConverterTests
    {
        public TypeConverterTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Theory]
        [InlineData("u8", "0", 0)]
        [InlineData("u8", "127", 127)]
        [InlineData("u8", "255", 255)]
        public void NumericTypeU8_ConversionIsCorrect(string type, string value, byte expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("s8", "0", 0)]
        [InlineData("s8", "-128", -128)]
        [InlineData("s8", "127", 127)]
        public void NumericTypeS8_ConversionIsCorrect(string type, string value, sbyte expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("u16", "0", 0U)]
        [InlineData("u16", "32767", 32767U)]
        [InlineData("u16", "65535", 65535U)]
        public void NumericTypeU16_ConversionIsCorrect(string type, string value, ushort expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("s16", "0", 0)]
        [InlineData("s16", "-32768", -32768)]
        [InlineData("s16", "32767", 32767)]
        public void NumericTypeS16_ConversionIsCorrect(string type, string value, short expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("u32", "0", 0U)]
        [InlineData("u32", "2147483647", 2147483647U)]
        [InlineData("u32", "4294967295", 4294967295U)]
        public void NumericTypeU32_ConversionIsCorrect(string type, string value, uint expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("s32", "0", 0)]
        [InlineData("s32", "-2147483648", -2147483648)]
        [InlineData("s32", "2147483647", 2147483647)]
        public void NumericTypeS32_ConversionIsCorrect(string type, string value, int expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("u64", "0", "0")]
        [InlineData("u64", "9223372036854775807", "9223372036854775807")]
        [InlineData("u64", "18446744073709551615", "18446744073709551615")]
        public void NumericTypeU64_ConversionIsCorrect(string type, string value, string expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("s64", "0", "0")]
        [InlineData("s64", "-9223372036854775808", "-9223372036854775808")]
        [InlineData("s64", "9223372036854775807", "9223372036854775807")]
        public void NumericTypeS64_ConversionIsCorrect(string type, string value, string expected)
        {
            // 准备XML
            var xml = $"<root><value __type=\"{type}\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal(type, result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("Hello World")]
        [InlineData("测试字符串")]
        [InlineData("Special chars")]
        [InlineData("")]
        public void StringType_ConversionIsCorrect(string value)
        {
            // 准备XML
            var xml = $"<root><value __type=\"str\">{value}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(value, result.Root.Element("value").Value);
            
            // 验证类型属性保留
            Assert.Equal("str", result.Root.Element("value").Attribute("__type").Value);
        }
        
        [Theory]
        [InlineData("DEADBEEF", 4)]
        [InlineData("0123456789ABCDEF", 8)]
        [InlineData("", 0)]
        public void BinaryType_ConversionIsCorrect(string hexValue, int size)
        {
            // 准备XML
            var xml = $"<root><value __type=\"bin\" __size=\"{size}\">{hexValue}</value></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变（忽略大小写）
            Assert.Equal(hexValue.ToUpperInvariant(), result.Root.Element("value").Value.ToUpperInvariant());
            
            // 验证类型属性和大小属性保留
            Assert.Equal("bin", result.Root.Element("value").Attribute("__type").Value);
            Assert.Equal(size.ToString(), result.Root.Element("value").Attribute("__size").Value);
        }
        
        [Theory]
        [InlineData("s8", "0 1 -1 127 -128", 5)]
        [InlineData("u16", "0 65535 1 2 3", 5)]
        [InlineData("s32", "-2147483648 0 2147483647", 3)]
        public void ArrayType_ConversionIsCorrect(string type, string values, int count)
        {
            // 准备XML
            var xml = $"<root><array __type=\"{type}\" __count=\"{count}\">{values}</array></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal(values, result.Root.Element("array").Value);
            
            // 验证类型属性和数量属性保留
            Assert.Equal(type, result.Root.Element("array").Attribute("__type").Value);
            Assert.Equal(count.ToString(), result.Root.Element("array").Attribute("__count").Value);
        }
        
        [Fact]
        public void Ip4Type_ConversionIsCorrect()
        {
            // 准备XML
            var xml = "<root><ip __type=\"ip4\">192.168.1.1</ip></root>";
            
            // 转换为Kbin并返回
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            
            // 验证值未改变
            Assert.Equal("192.168.1.1", result.Root.Element("ip").Value);
            
            // 验证类型属性保留
            Assert.Equal("ip4", result.Root.Element("ip").Attribute("__type").Value);
        }
        
        [Fact]
        public void InvalidValue_ThrowsException()
        {
            // 准备无效值的XML（超出类型范围）
            var xml = "<root><value __type=\"u8\">256</value></root>";
            
            // 验证抛出异常
            Assert.Throws<KbinException>(() => KbinConverter.Write(xml, KnownEncodings.UTF8));
        }
        
        [Fact]
        public void InvalidType_ThrowsException()
        {
            // 准备无效类型的XML
            var xml = "<root><value __type=\"invalid_type\">123</value></root>";
            
            // 验证抛出异常
            Assert.Throws<KbinTypeNotFoundException>(() => KbinConverter.Write(xml, KnownEncodings.UTF8));
        }
    }
} 