using System;
using System.Buffers.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(this ref Buffer<T> src, ReadOnlySpan<T> destination)
        {
            if (destination.Length > src.Tail.Length)
            {
                return false;
            }

            destination.CopyTo(src.Tail);
            src.Tail = src.Tail.Slice(destination.Length);
            src.Written += destination.Length;
            return true;
        }

        public static bool TryWriteUtf8String(this ref Buffer<byte> src, string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }

#if NETSTANDARD2_0 || NET461
            var bucketBytes = Encoding.UTF8.GetBytes(str);
            return src.TryWrite(bucketBytes);
#else
            int written = 0;
            try
            {
                written = Encoding.UTF8.GetBytes(str, src.Tail);
            }
#pragma warning disable CA1031
            catch (ArgumentException)
#pragma warning restore CA1031
            {
                return false;
            }

            src.Tail = src.Tail.Slice(written);
            src.Written += written;
            return true;
#endif
        }

        public static bool TryWriteUtf8Chars(this ref Buffer<byte> src, ReadOnlySpan<char> chars)
        {
            if (chars.Length == 0)
            {
                return true;
            }

#if NETSTANDARD2_0 || NET461
            var bytes = Encoding.UTF8.GetBytes(chars.ToArray());
            return src.TryWrite(bytes);
#else
            int written = 0;
            try
            {
                written = Encoding.UTF8.GetBytes(chars, src.Tail);
            }
#pragma warning disable CA1031
            catch (ArgumentException)
#pragma warning restore CA1031
            {
                return false;
            }

            src.Tail = src.Tail.Slice(written);
            src.Written += written;
            return true;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(this ref Buffer<T> src, T ch)
        {
            const int OneByte = 1;
            if (src.Tail.Length < OneByte)
            {
                return false;
            }

            src.Tail[0] = ch;
            src.Tail = src.Tail.Slice(OneByte);
            src.Written += OneByte;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(this ref Buffer<T> src, T ch1, T ch2)
        {
            const int TwoBytes = 2;
            if (src.Tail.Length < TwoBytes)
            {
                return false;
            }

            src.Tail[0] = ch1;
            src.Tail[1] = ch2;
            src.Tail = src.Tail.Slice(TwoBytes);
            src.Written += TwoBytes;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(this ref Buffer<T> src, T ch1, T ch2, T ch3)
        {
            const int ThreeBytes = 3;
            if (src.Tail.Length < ThreeBytes)
            {
                return false;
            }

            src.Tail[0] = ch1;
            src.Tail[1] = ch2;
            src.Tail[2] = ch3;
            src.Tail = src.Tail.Slice(ThreeBytes);
            src.Written += ThreeBytes;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64(this ref Buffer<byte> src, long val)
        {
            if (!Utf8Formatter.TryFormat(val, src.Tail, out var consumed))
            {
                return false;
            }

            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteDouble(this ref Buffer<byte> src, double val)
        {
            if (!Utf8Formatter.TryFormat((decimal)val, src.Tail, out var consumed))
            {
                return false;
            }

            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteString(this ref Buffer<char> src, string str) =>
            TryWrite(ref src, str.AsSpan());
    }
}
