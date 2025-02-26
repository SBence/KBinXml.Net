using System;

namespace KbinXml.Net.Utils;

internal static class SixbitHelperCoreClrOptimized
{
    /// <summary>
    /// 将6位编码流编码为字节流
    /// </summary>
    /// <param name="buffer">输入缓冲区（每个字节包含6位有效数据）</param>
    /// <param name="output">输出缓冲区（长度应至少为 (buffer.Length * 6 + 7) / 8）</param>
    public static unsafe void Encode(ReadOnlySpan<byte> buffer, Span<byte> output)
    {
        if (buffer.IsEmpty) return;
        int requiredOutputSize = (buffer.Length * 6 + 7) / 8;
        if (output.Length < requiredOutputSize)
            throw new ArgumentException("Output buffer is too small.", nameof(output));

        fixed (byte* bufferPtr = buffer, outputPtr = output)
        {
            byte* buf = bufferPtr;
            byte* outPtr = outputPtr;
            var outLength = output.Length;
            int length = buffer.Length;
            int globalBitIndex = 0;

            // 批处理，每次处理4个6位块（24位）
            int batchCount = length / 4;
            for (int i = 0; i < batchCount; i++)
            {
                uint chunk = *(uint*)buf;
                buf += 4;

                // 提取4个6位块
                byte sixBits0 = (byte)((chunk >> 0) & 0x3F);
                byte sixBits1 = (byte)((chunk >> 8) & 0x3F);
                byte sixBits2 = (byte)((chunk >> 16) & 0x3F);
                byte sixBits3 = (byte)((chunk >> 24) & 0x3F);

                // 计算输出字节索引和位偏移
                int outputByte = globalBitIndex >> 3;
                int bitOffset = globalBitIndex & 7;

                // 将4个6位块写入输出缓冲区
                WriteSixBits(outPtr, outLength, outputByte, bitOffset, sixBits0);
                globalBitIndex += 6;
                outputByte = globalBitIndex >> 3;
                bitOffset = globalBitIndex & 7;
                WriteSixBits(outPtr, outLength, outputByte, bitOffset, sixBits1);
                globalBitIndex += 6;
                outputByte = globalBitIndex >> 3;
                bitOffset = globalBitIndex & 7;
                WriteSixBits(outPtr, outLength, outputByte, bitOffset, sixBits2);
                globalBitIndex += 6;
                outputByte = globalBitIndex >> 3;
                bitOffset = globalBitIndex & 7;
                WriteSixBits(outPtr, outLength, outputByte, bitOffset, sixBits3);
                globalBitIndex += 6;
            }

            // 处理尾部数据
            int remaining = length % 4;
            for (int i = 0; i < remaining; i++)
            {
                byte sixBits = buf[i];
                int outputByte = globalBitIndex >> 3;
                int bitOffset = globalBitIndex & 7;
                WriteSixBits(outPtr, outLength, outputByte, bitOffset, sixBits);
                globalBitIndex += 6;
            }
        }
    }

    /// <summary>
    /// 将字节流解码为6位编码流
    /// </summary>
    /// <param name="buffer">输入字节流</param>
    /// <param name="input">输出缓冲区（长度应至少为 (buffer.Length * 8) / 6）</param>
    public static unsafe void Decode(ReadOnlySpan<byte> buffer, Span<byte> input)
    {
        if (buffer.IsEmpty) return;
        int maxOutputLength = (buffer.Length * 8) / 6;
        if (input.Length > maxOutputLength)
            throw new ArgumentException("Input buffer capacity exceeds maximum decodable length.", nameof(input));

        fixed (byte* bufferPtr = buffer, inputPtr = input)
        {
            byte* buf = bufferPtr;
            int bufLength = buffer.Length;
            byte* inPtr = inputPtr;
            int length = input.Length;
            int globalBitIndex = 0;

            // 批处理，每次处理4个6位块（24位）
            int batchCount = length / 4;
            for (int i = 0; i < batchCount; i++)
            {
                // 提取4个6位块
                byte sixBits0 = ReadSixBits(buf, bufLength, globalBitIndex);
                globalBitIndex += 6;
                byte sixBits1 = ReadSixBits(buf, bufLength, globalBitIndex);
                globalBitIndex += 6;
                byte sixBits2 = ReadSixBits(buf, bufLength, globalBitIndex);
                globalBitIndex += 6;
                byte sixBits3 = ReadSixBits(buf, bufLength, globalBitIndex);
                globalBitIndex += 6;

                // 写入输出缓冲区
                *inPtr++ = sixBits0;
                *inPtr++ = sixBits1;
                *inPtr++ = sixBits2;
                *inPtr++ = sixBits3;
            }

            // 处理尾部数据
            int remaining = length % 4;
            for (int i = 0; i < remaining; i++)
            {
                byte sixBits = ReadSixBits(buf, bufLength, globalBitIndex);
                globalBitIndex += 6;
                *inPtr++ = sixBits;
            }
        }
    }

    private static unsafe void WriteSixBits(byte* outPtr, int outLength, int outputByte, int bitOffset,
        byte sixBits)
    {
        if (bitOffset <= 2) // 6位全在一个字节内
        {
            outPtr[outputByte] |= (byte)(sixBits << (2 - bitOffset));
        }
        else // 6位跨字节
        {
            int bitsInFirst = 8 - bitOffset;
            int bitsInSecond = 6 - bitsInFirst;
            outPtr[outputByte] |= (byte)(sixBits >> bitsInSecond);
            if (outputByte + 1 < outLength)
            {
                outPtr[outputByte + 1] |= (byte)((sixBits & ((1 << bitsInSecond) - 1)) << (8 - bitsInSecond));
            }
        }
    }

    private static unsafe byte ReadSixBits(byte* buf, int bufLength, int globalBitIndex)
    {
        int bufferByte = globalBitIndex >> 3;
        int bitOffset = globalBitIndex & 7;
        int availableBits = 8 - bitOffset;

        byte sixBits;
        if (availableBits >= 6) // 6位全在一个字节内
        {
            sixBits = (byte)((buf[bufferByte] >> (availableBits - 6)) & 0x3F);
        }
        else // 6位跨字节
        {
            int bitsFromFirst = availableBits;
            int bitsFromSecond = 6 - bitsFromFirst;
            sixBits = (byte)((buf[bufferByte] & ((1 << bitsFromFirst) - 1)) << bitsFromSecond);
            if (bufferByte + 1 < bufLength)
            {
                sixBits |= (byte)(buf[bufferByte + 1] >> (8 - bitsFromSecond));
            }
        }
        return sixBits;
    }
}