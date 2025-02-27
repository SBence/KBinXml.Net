using System;
using System.Runtime.CompilerServices;

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

                // 提取四个6位块并组合为24位
                uint sixBits0 = (chunk & 0x0000003F);         // 最低 6 位
                uint sixBits1 = (chunk & 0x00003F00) >> 8;    // 第 8-13 位
                uint sixBits2 = (chunk & 0x003F0000) >> 16;   // 第 16-21 位
                uint sixBits3 = (chunk & 0x3F000000) >> 24;   // 第 24-29 位
                uint combined = (sixBits0 << 18) | (sixBits1 << 12) | (sixBits2 << 6) | sixBits3;

                // 计算起始写入位置
                int outputByte = globalBitIndex >> 3;
                int bitOffset = globalBitIndex & 7;

                // 一次性写入24位
                Write24Bits(outPtr, outLength, outputByte, bitOffset, combined);
                globalBitIndex += 24;
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
                // 读取24位数据
                uint combined = Read24Bits(buf, bufLength, globalBitIndex);
                globalBitIndex += 24;

                // 从24位中提取4个6位块
                byte sixBits0 = (byte)((combined >> 18) & 0x3F);
                byte sixBits1 = (byte)((combined >> 12) & 0x3F);
                byte sixBits2 = (byte)((combined >> 6) & 0x3F);
                byte sixBits3 = (byte)(combined & 0x3F);

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

    private static unsafe void Write24Bits(byte* outPtr, int outLength, int outputByte, int bitOffset, uint combined)
    {
        if (bitOffset == 0) // 对齐情况直接写入3字节
        {
            if (outputByte + 2 < outLength) // 确保3字节可写入
            {
                outPtr[outputByte] = (byte)(combined >> 16);
                outPtr[outputByte + 1] = (byte)(combined >> 8);
                outPtr[outputByte + 2] = (byte)combined;
                return;
            }
        }

        // 非对齐通用写入
        ulong buffer = (ulong)(combined & 0xFFFFFF) << (32 - bitOffset); // 左移到高位对齐
        int bytesToWrite = Math.Min((24 + bitOffset + 7) >> 3, outLength - outputByte);
        for (int i = 0; i < bytesToWrite; i++)
        {
            outPtr[outputByte + i] |= (byte)(buffer >> (24 - 8 * i));
        }
    }

    /// <summary>
    /// 从缓冲区读取24位数据
    /// </summary>
    private static unsafe uint Read24Bits(byte* buf, int bufLength, int globalBitIndex)
    {
        int bufferByte = globalBitIndex >> 3; // 当前字节位置
        int bitOffset = globalBitIndex & 7;  // 当前字节内的位偏移

        if (bitOffset == 0) // 对齐情况
        {
            if (bufferByte + 2 < bufLength)
            {
                // 直接读取3个字节组成24位
                return (uint)(buf[bufferByte] << 16) | (uint)(buf[bufferByte + 1] << 8) | buf[bufferByte + 2];
            }
            else
            {
                // 不足3字节，逐字节读取
                uint value = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (bufferByte + i < bufLength)
                    {
                        value |= (uint)buf[bufferByte + i] << (16 - 8 * i);
                    }
                }
                return value;
            }
        }
        else // 非对齐情况
        {
            uint value = 0;
            int bitsRead = 0;
            while (bitsRead < 24)
            {
                int bitsToRead = Math.Min(8 - bitOffset, 24 - bitsRead);
                if (bufferByte >= bufLength) break;

                // 从当前字节中提取需要的位
                uint mask = (uint)((1 << bitsToRead) - 1) << bitOffset;
                uint bits = (uint)(buf[bufferByte] & mask) >> bitOffset;

                value |= bits << (24 - bitsRead - bitsToRead);
                bitsRead += bitsToRead;
                bitOffset += bitsToRead;
                if (bitOffset >= 8)
                {
                    bitOffset -= 8;
                    bufferByte++;
                }
            }
            return value;
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