using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using KbinXml.Net.Internal;
using KbinXml.Net.Internal.Debugging;
using Microsoft.IO;

namespace KbinXml.Net;

/// <summary>
/// Provides methods for converting between KBin binary format and XML representations.
/// </summary>
public static partial class KbinConverter
{
#if USELOG
    internal static ConsoleLogger Logger { get; } = new ConsoleLogger();
#else
    internal static NullLogger Logger { get; } = new NullLogger();
#endif

#if !NET5_0_OR_GREATER
    private static readonly Type ControlTypeT = typeof(ControlType);
#endif
    private static readonly HashSet<byte> ControlTypes =
#if NET5_0_OR_GREATER
        new(Enum.GetValues<ControlType>().Cast<byte>());
#else
        new(Enum.GetValues(ControlTypeT).Cast<byte>());
#endif

    internal static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new()
    {
        Settings =
        {
            //BlockSize = 1024,
            AggressiveBufferReturn = true,
            //LargeBufferMultiple = 1024 * 128,
        }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetActualName(string name, string? repairedPrefix)
    {
        if (string.IsNullOrEmpty(repairedPrefix))
        {
            return name;
        }

        if (name.Length < repairedPrefix.Length)
        {
            return name;
        }

        if (name.StartsWith(repairedPrefix, StringComparison.Ordinal))
        {
            return name.Substring(repairedPrefix.Length);
        }

        return name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetRepairedName(string name, string? repairedPrefix)
    {
        if (repairedPrefix is null)
        {
            return name;
        }

        if (name.Length == 0 || !IsDigit(name[0]))
        {
            return name;
        }

        return repairedPrefix + name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }
}