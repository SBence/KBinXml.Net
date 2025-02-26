using System;
using System.Diagnostics;
using KbinXml.Net.Utils;
using Xunit;

namespace GeneralUnitTests
{
    public class SixbitTest
    {
        [Fact]
        public void VerifyBitEquivalence()
        {
            //byte[] testData = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            
            var total = 1024 * 1024;
            Random rnd = new();
            byte[] testData = new byte[total];
            rnd.NextBytes(testData);

            Span<byte> outputOriginal = new byte[testData.Length * 6 / 8];
            Span<byte> outputOptimize = new byte[testData.Length * 6 / 8];
            Span<byte> outputSuperOptimize = new byte[testData.Length * 6 / 8];
            Span<byte> outputSuperOptimize2 = new byte[testData.Length * 6 / 8];

            // 执行两个版本
            SixbitHelperOriginal.EncodeFillOutput(testData, ref outputOriginal);
            SixbitHelperOptimized.Encode(testData, outputOptimize);
            SixbitHelperSuperOptimized.Encode(testData, outputSuperOptimize);
            SixbitHelperCoreClrOptimized.Encode(testData, outputSuperOptimize2);

            // 逐位比较
            for (int i = 0; i < outputSuperOptimize.Length; i++)
            {
                int originalBit = outputOriginal[i];
                int optimizeBit = outputOptimize[i];
                int superOptimizeBit = outputSuperOptimize[i];
                int superOptimize2Bit = outputSuperOptimize2[i];
                Debug.Assert(originalBit == optimizeBit,
                    $"Bit mismatch at position {i}: Original={originalBit}, Optimize={optimizeBit}");
                Debug.Assert(originalBit == superOptimizeBit,
                    $"Bit mismatch at position {i}: Original={originalBit}, SuperOptimize={superOptimizeBit}");
                Debug.Assert(superOptimizeBit == superOptimize2Bit,
                    $"Bit mismatch at position {i}: Original={superOptimizeBit}, OptimizeFinal2={superOptimize2Bit}");
            }
        }

        [Fact]
        public void VerifyBitEquivalence2()
        {
            //byte[] testData = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            
            var total = 1024 * 1024;
            Random rnd = new();
            byte[] testData = new byte[total];
            rnd.NextBytes(testData);

            Span<byte> output = new byte[testData.Length * 6 / 8];
            SixbitHelperOriginal.EncodeFillOutput(testData, ref output);

            Span<byte> inputOriginal = new byte[output.Length * 8 / 6];
            Span<byte> inputOptimize = new byte[output.Length * 8 / 6];
            Span<byte> inputSuperOptimize = new byte[output.Length * 8 / 6];
            Span<byte> inputSuperOptimize2 = new byte[output.Length * 8 / 6];

            // 执行两个版本
            SixbitHelperOriginal.DecodeFillInput(output, ref inputOriginal);
            SixbitHelperOptimized.Decode(output, inputOptimize);
            SixbitHelperSuperOptimized.Decode(output, inputSuperOptimize);
            SixbitHelperCoreClrOptimized.Decode(output, inputSuperOptimize2);

            // 逐位比较
            for (int i = 0; i < inputSuperOptimize.Length; i++)
            {
                int originalBit = inputOriginal[i];
                int optimizeBit = inputOptimize[i];
                int superOptimizeBit = inputSuperOptimize[i];
                int superOptimize2Bit = inputSuperOptimize2[i];
                Debug.Assert(originalBit == optimizeBit,
                    $"Bit mismatch at position {i}: Original={originalBit}, Optimize={optimizeBit}");
                Debug.Assert(originalBit == superOptimizeBit,
                    $"Bit mismatch at position {i}: Original={originalBit}, SuperOptimize={superOptimizeBit}");
                Debug.Assert(originalBit == superOptimize2Bit,
                    $"Bit mismatch at position {i}: Original={originalBit}, OptimizeFinal2={superOptimize2Bit}");
            }
        }
    }
}
