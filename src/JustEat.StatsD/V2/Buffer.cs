using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.V2
{
    internal ref struct Buffer
    {
        public Buffer(Span<byte> source) : this()
        {
            Tail = source;
            Written = 0;
        }

        public Span<byte> Tail;
        public int Written;
    }

    internal static class BufferImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, Span<byte> bytes)
        {
            if (bytes.Length > src.Tail.Length) return false;

            bytes.CopyTo(src.Tail);
            src.Tail = src.Tail.Slice(bytes.Length);
            src.Written += bytes.Length;
            return true;
        }

        public static bool TryWriteUtf8String(this ref Buffer src, string str)
        {
#if NETCOREAPP2_1
            try
            {
                var written = Encoding.UTF8.GetBytes(str, src.Tail);
                src.Tail = src.Tail.Slice(written);
                src.Written += written;
                return true;
            }
            catch
            {
                return false;
            }
#else
            var bucketBytes = Encoding.UTF8.GetBytes(str);
            return src.TryWriteBytes(bucketBytes);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, byte chr)
        {
            const int length = 1;
            if (src.Tail.Length < length) return false;

            src.Tail[0] = chr;
            src.Tail = src.Tail.Slice(length);
            src.Written += length;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, byte chr1, byte chr2)
        {
            const int length = 2;
            if (src.Tail.Length < length) return false;

            src.Tail[0] = chr1;
            src.Tail[1] = chr2;
            src.Tail = src.Tail.Slice(length);
            src.Written += length;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, byte chr1, byte chr2, byte chr3)
        {
            const int length = 3;
            if (src.Tail.Length < length) return false;

            src.Tail[0] = chr1;
            src.Tail[1] = chr2;
            src.Tail[2] = chr3;
            src.Tail = src.Tail.Slice(length);
            src.Written += length;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteLong(this ref Buffer src, long val)
        {
            if (!Utf8Formatter.TryFormat(val, src.Tail, out var consumed)) return false;
            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteDouble(this ref Buffer src, double val)
        {
            if (!Utf8Formatter.TryFormat((decimal) val, src.Tail, out var consumed)) return false;
            src.Tail = src.Tail.Slice(consumed);
            src.Written += consumed;

            return true;
        }
    }
}
