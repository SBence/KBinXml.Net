using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KbinXml.Net.Internal.TypeConverters;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#else
using System.Linq;
#endif

namespace KbinXml.Net.Internal;

internal static class NodeTypeFactory
{
    private static readonly IReadOnlyDictionary<byte, NodeType> NodesDictionary = new Dictionary<byte, NodeType>
            {
                { 2, new NodeType(1, 1, "s8", S8Converter.Instance) },
                { 3, new NodeType(1, 1, "u8", U8Converter.Instance) },
                { 4, new NodeType(2, 1, "s16", S16Converter.Instance) },
                { 5, new NodeType(2, 1, "u16", U16Converter.Instance) },
                { 6, new NodeType(4, 1, "s32", S32Converter.Instance) },
                { 7, new NodeType(4, 1, "u32", U32Converter.Instance) },
                { 8, new NodeType(8, 1, "s64", S64Converter.Instance) },
                { 9, new NodeType(8, 1, "u64", U64Converter.Instance) },
                { 10, new NodeType(0, 0, "bin", DummyBinConverter.Instance) },
                { 11, new NodeType(0, 0, "str", DummyStrConverter.Instance) },
                { 12, new NodeType(4, 1, "ip4", Ip4Converter.Instance) },
                { 13, new NodeType(4, 1, "time", U32Converter.Instance) },
                { 14, new NodeType(4, 1, "float", FloatConverter.Instance) },
                { 15, new NodeType(8, 1, "double", DoubleConverter.Instance) },

                { 16, new NodeType(1, 2, "2s8", S8Converter.Instance) },
                { 17, new NodeType(1, 2, "2u8", U8Converter.Instance) },
                { 18, new NodeType(2, 2, "2s16", S16Converter.Instance) },
                { 19, new NodeType(2, 2, "2u16", U16Converter.Instance) },
                { 20, new NodeType(4, 2, "2s32", S32Converter.Instance) },
                { 21, new NodeType(4, 2, "2u32", U32Converter.Instance) },
                { 22, new NodeType(8, 2, "vs64", S64Converter.Instance) },
                { 23, new NodeType(8, 2, "vu64", U64Converter.Instance) },
                { 24, new NodeType(4, 2, "2f", FloatConverter.Instance) },
                { 25, new NodeType(8, 2, "vd", DoubleConverter.Instance) },

                { 26, new NodeType(1, 3, "3s8", S8Converter.Instance) },
                { 27, new NodeType(1, 3, "3u8", U8Converter.Instance) },
                { 28, new NodeType(2, 3, "3s16", S16Converter.Instance) },
                { 29, new NodeType(2, 3, "3u16", U16Converter.Instance) },
                { 30, new NodeType(4, 3, "3s32", S32Converter.Instance) },
                { 31, new NodeType(4, 3, "3u32", U32Converter.Instance) },
                { 32, new NodeType(8, 3, "3s64", S64Converter.Instance) },
                { 33, new NodeType(8, 3, "3u64", U64Converter.Instance) },
                { 34, new NodeType(4, 3, "3f", FloatConverter.Instance) },
                { 35, new NodeType(8, 3, "3d", DoubleConverter.Instance) },

                { 36, new NodeType(1, 4, "4s8", S8Converter.Instance) },
                { 37, new NodeType(1, 4, "4u8", U8Converter.Instance) },
                { 38, new NodeType(2, 4, "4s16", S16Converter.Instance) },
                { 39, new NodeType(2, 4, "4u16", U16Converter.Instance) },
                { 40, new NodeType(4, 4, "vs32", S32Converter.Instance) },
                { 41, new NodeType(4, 4, "vu32", U32Converter.Instance) },
                { 42, new NodeType(8, 4, "4s64", S64Converter.Instance) },
                { 43, new NodeType(8, 4, "4u64", U64Converter.Instance) },
                { 44, new NodeType(4, 4, "vf", FloatConverter.Instance) },
                { 45, new NodeType(8, 4, "4d", DoubleConverter.Instance) },

                { 48, new NodeType(1, 16, "vs8", S8Converter.Instance) },
                { 49, new NodeType(1, 16, "vu8", U8Converter.Instance) },
                { 50, new NodeType(2, 8, "vs16", S16Converter.Instance) },
                { 51, new NodeType(2, 8, "vu16", U16Converter.Instance) },
                { 52, new NodeType(1, 1, "bool", U8Converter.Instance) },
                { 53, new NodeType(1, 2, "2b", U8Converter.Instance) },
                { 54, new NodeType(1, 3, "3b", U8Converter.Instance) },
                { 55, new NodeType(1, 4, "4b", U8Converter.Instance) },
                { 56, new NodeType(1, 16, "vb", U8Converter.Instance) },
            }
#if NET8_0_OR_GREATER
            .ToFrozenDictionary()
#endif
        ;

    private static readonly NodeType?[] NodesArray = new NodeType?[57];

    private static readonly IReadOnlyDictionary<string, byte> ReverseTypeMap = NodesDictionary
#if NET8_0_OR_GREATER
            .ToFrozenDictionary(x => x.Value.Name, x => x.Key)
#else
            .ToDictionary(x => x.Value.Name, x => x.Key)
#endif
        ;

    static NodeTypeFactory()
    {
        foreach (var nodeType in NodesDictionary)
        {
            NodesArray[nodeType.Key] = nodeType.Value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNodeType(byte typeCode,
#if NET6_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out NodeType? nodeType)
    {
        nodeType = NodesArray[typeCode];
        return nodeType != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NodeType GetNodeType(byte typeCode)
    {
        var nodeType = NodesArray[typeCode];
        if (nodeType == null) throw new InvalidOperationException($"Unknown type code: {typeCode}");
        return nodeType;
    }

    /// <summary>
    /// Get an instance of a <see cref="NodeType"/> from the internal type map.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The found type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NodeType GetNodeType(string name)
    {
        return GetNodeType(GetNodeTypeId(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetNodeTypeId(string name)
    {
        return ReverseTypeMap[name];
    }
}