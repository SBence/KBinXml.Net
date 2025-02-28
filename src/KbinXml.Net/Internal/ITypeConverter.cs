﻿using System;
using KbinXml.Net.Utils;

namespace KbinXml.Net.Internal;

internal interface ITypeConverter
{
    int WriteString(ref ValueListBuilder<byte> builder, ReadOnlySpan<char> str);
    string ToString(ReadOnlySpan<byte> span);
#if NET6_0_OR_GREATER
    void AppendString(ref ValueStringBuilder stringBuilder, ReadOnlySpan<byte> span);
#endif
}