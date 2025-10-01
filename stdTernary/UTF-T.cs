using System;
using System.Collections.Generic;
using System.Text;

namespace stdTernary
{
    public static class UtfT
    {
        // Prefix markers (negative to avoid ASCII collision)
        private const short Prefix2 = -1; // prefix + 2 continuations (3 total CharT)
        private const short Prefix3 = -2; // prefix + 3 continuations (4 total CharT)

        // Each continuation CharT carries 9 payload bits via bias in ShortValue: [-256..+255] ↔ [0..511]
        private const int PayloadBits = 9;
        private const int PayloadMask = (1 << PayloadBits) - 1; // 0x1FF
        private const int PayloadBias = 1 << (PayloadBits - 1);  // 256

        // Safety: require at least 6 trits so ShortValue covers [-256..+255]
        private static void EnsureCapacity()
        {
            if (CharT.N_TRITS_PER_CHART < 6)
                throw new InvalidOperationException("UTF-T requires CharT.N_TRITS_PER_CHART >= 6.");
            if (CharT.MinValue > -PayloadBias || CharT.MaxValue < (PayloadBias - 1))
                throw new InvalidOperationException("CharT ShortValue range must include [-256..+255].");
        }

        // ---------- Single code point encode/decode ----------

        public static CharT[] EncodeCodePoint(int codePoint)
        {
            EnsureCapacity();
            ValidateScalar(codePoint);

            // ASCII fast path
            if (codePoint <= 0x7F)
                return new[] { new CharT((short)codePoint) };

            if (codePoint <= 0xFFFF)
            {
                // 18 bits payload: 2 continuations (2 * 9 bits)
                Span<CharT> out3 = stackalloc CharT[3];
                out3[0] = new CharT(Prefix2);
                // Big-endian chunks: top 9, then low 9
                int c1 = (codePoint >> PayloadBits) & PayloadMask;
                int c2 = codePoint & PayloadMask;
                out3[1] = new CharT((short)(c1 - PayloadBias));
                out3[2] = new CharT((short)(c2 - PayloadBias));
                return out3.ToArray();
            }
            else
            {
                // Up to 21 bits payload: 3 continuations (3 * 9 = 27 bits)
                Span<CharT> out4 = stackalloc CharT[4];
                out4[0] = new CharT(Prefix3);
                int c1 = (codePoint >> (2 * PayloadBits)) & PayloadMask; // top 9
                int c2 = (codePoint >> (1 * PayloadBits)) & PayloadMask; // mid 9
                int c3 = (codePoint >> (0 * PayloadBits)) & PayloadMask; // low 9
                out4[1] = new CharT((short)(c1 - PayloadBias));
                out4[2] = new CharT((short)(c2 - PayloadBias));
                out4[3] = new CharT((short)(c3 - PayloadBias));
                return out4.ToArray();
            }
        }

        public static int DecodeCodePoint(ReadOnlySpan<CharT> input, out int consumed)
        {
            EnsureCapacity();
            if (input.Length == 0) throw new ArgumentException("Empty input.", nameof(input));

            short first = input[0].ShortValue;

            // ASCII (single CharT)
            if (first >= 0 && first <= 0x7F)
            {
                consumed = 1;
                return first;
            }

            if (first == Prefix2)
            {
                if (input.Length < 3) throw new ArgumentException("Truncated UTF-T 3-CharT sequence.");
                int c1 = input[1].ShortValue + PayloadBias;
                int c2 = input[2].ShortValue + PayloadBias;
                if (((uint)c1 > PayloadMask) || ((uint)c2 > PayloadMask))
                    throw new ArgumentException("Invalid UTF-T continuation value.");
                int code = (c1 << PayloadBits) | c2;
                ValidateScalar(code);
                consumed = 3;
                return code;
            }

            if (first == Prefix3)
            {
                if (input.Length < 4) throw new ArgumentException("Truncated UTF-T 4-CharT sequence.");
                int c1 = input[1].ShortValue + PayloadBias;
                int c2 = input[2].ShortValue + PayloadBias;
                int c3 = input[3].ShortValue + PayloadBias;
                if (((uint)c1 > PayloadMask) || ((uint)c2 > PayloadMask) || ((uint)c3 > PayloadMask))
                    throw new ArgumentException("Invalid UTF-T continuation value.");
                int code = (c1 << (2 * PayloadBits)) | (c2 << PayloadBits) | c3;
                ValidateScalar(code);
                consumed = 4;
                return code;
            }

            throw new ArgumentException($"Unsupported UTF-T prefix CharT ShortValue={first}.");
        }

        private static void ValidateScalar(int codePoint)
        {
            // Unicode scalar: U+0000..U+10FFFF excluding surrogate range U+D800..U+DFFF
            if ((uint)codePoint > 0x10FFFF || (codePoint >= 0xD800 && codePoint <= 0xDFFF))
                throw new ArgumentOutOfRangeException(nameof(codePoint), "Not a valid Unicode scalar value.");
        }

        // ---------- String <-> CharT[] ----------

        public static CharT[] EncodeString(string s)
        {
            EnsureCapacity();
            var list = new List<CharT>(s.Length); // lower bound
            for (int i = 0; i < s.Length; )
            {
               int codePoint;
               if (char.IsSurrogatePair(s, i))
               {
                  codePoint = char.ConvertToUtf32(s, i);
                  i += 2; // advance past both high+low surrogate
               }
               else
               {
                  codePoint = s[i];
                  i += 1;
               }
               list.AddRange(EncodeCodePoint(codePoint));
            }
            return list.ToArray();
        }

        public static string DecodeString(ReadOnlySpan<CharT> data)
        {
            EnsureCapacity();
            var sb = new StringBuilder(data.Length);
            int i = 0;
            while (i < data.Length)
            {
                int cp = DecodeCodePoint(data[i..], out int consumed);
                i += consumed;
                sb.Append(char.ConvertFromUtf32(cp));
            }
            return sb.ToString();
        }
    }
}
