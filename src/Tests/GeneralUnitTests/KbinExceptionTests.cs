using System;
using KbinXml.Net;
using Xunit;

namespace GeneralUnitTests
{
    public class KbinExceptionTests
    {
        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // 调用默认构造函数
            var exception = new KbinException();
            
            // 验证实例已创建
            Assert.NotNull(exception);
            Assert.Equal("Exception of type 'KbinXml.Net.KbinException' was thrown.", exception.Message);
        }
        
        [Fact]
        public void MessageConstructor_SetsMessage()
        {
            // 预期的错误消息
            const string expectedMessage = "Test error message";
            
            // 使用消息构造异常
            var exception = new KbinException(expectedMessage);
            
            // 验证消息已正确设置
            Assert.Equal(expectedMessage, exception.Message);
        }
        
        [Fact]
        public void InnerExceptionConstructor_SetsMessageAndInnerException()
        {
            // 准备内部异常和消息
            const string expectedMessage = "Outer exception message";
            var innerException = new InvalidOperationException("Inner exception message");
            
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
    }
} 