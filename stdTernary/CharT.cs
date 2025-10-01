using System;
using System.Reflection.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace stdTernary;

public struct CharT : IEquatable<CharT>, IComparable<CharT>
{
   private static byte _tritCount = 6;
   private ulong _packed;

   public static byte N_TRITS_PER_CHART
   {
      get => _tritCount;
      set
      {
         if (value is < 2 or > BalancedTernaryEncoding.MaxTrits)
            throw new ArgumentOutOfRangeException(nameof(value), "Tryte must contain between 2 and 32 trits.");
         _tritCount = value;
      }
   }

   public static short MaxValue => (short)((Pow3(_tritCount) - 1) / 2);
   public static short MinValue => (short)(-MaxValue);

   public ulong PackedTrits => _packed;

   public Trit[] Value
   {
      readonly get
      {
         int count = _tritCount;
         var trits = new Trit[count];
         Span<sbyte> digits = stackalloc sbyte[count];
         BalancedTernaryEncoding.Decode(_packed, digits, count);
         for (int i = 0; i < count; i++)
         {
            trits[i] = new Trit(digits[i]);
         }
         return trits;
      }
      set => SetValue(value);
   }

   public string CharTString => ConvertToStringRepresentation();

   public Trit this[int index]
   {
      get
      {
         if ((uint)index >= _tritCount)
            throw new ArgumentOutOfRangeException(nameof(index));
         return new Trit(BalancedTernaryEncoding.DecodeTrit(_packed, index));
      }
   }

   public CharT(short value)
   {
      _packed = 0;
      SetValue(value);
   }

   public CharT(Trit[] value)
   {
      _packed = 0;
      SetValue(value);
   }

   public CharT(string value)
   {
      _packed = 0;
      SetValue(value);
   }

   public CharT(uint packedTrits) : this((ulong)packedTrits) { }

   public CharT(ulong packedTrits)
   {
      ValidatePacked(packedTrits, _tritCount);
      _packed = packedTrits;
   }

   public void SetValue(Trit[] value)
   {
      if (value.Length != _tritCount)
         throw new ArgumentException($"Expected {N_TRITS_PER_CHART} trits.", nameof(value));

      Span<sbyte> digits = stackalloc sbyte[_tritCount];
      for (int i = 0; i < _tritCount; i++)
      {
         digits[i] = (sbyte)value[i].Value;
      }

      _packed = BalancedTernaryEncoding.Encode(digits, _tritCount);
   }

   public void SetValue(string value)
   {
      if (value.Length != _tritCount)
         throw new ArgumentException($"Expected {N_TRITS_PER_CHART} characters.", nameof(value));

      Span<sbyte> digits = stackalloc sbyte[_tritCount];
      for (int i = 0; i < _tritCount; i++)
      {
         char c = value[i];
         digits[_tritCount - 1 - i] = c switch
         {
            '+' => 1,
            '-' => -1,
            '0' => 0,
            _ => throw new ArgumentException("Ternary characters must be '+', '-', or '0'.", nameof(value)),
         };
      }

      _packed = BalancedTernaryEncoding.Encode(digits, _tritCount);
   }

   public void SetValue(ulong packedTrits)
   {
      ValidatePacked(packedTrits, _tritCount);
      _packed = packedTrits;
   }

   public void SetTrit(int index, Trit trit)
   {
      if ((uint)index >= _tritCount)
         throw new ArgumentOutOfRangeException(nameof(index));
      _packed = BalancedTernaryEncoding.SetTrit(_packed, index, (sbyte)trit.Value);
   }

   public string ConvertToStringRepresentation()
   {
      return BalancedTernaryEncoding.ToTernaryString(_packed, _tritCount);
   }

   public bool Equals(CharT other) => _packed == other._packed;

   public override bool Equals([NotNullWhen(true)] object? obj) => obj is CharT other && Equals(other);

   public override int GetHashCode() => HashCode.Combine(_packed, _tritCount);

   public int CompareTo(CharT other) => BalancedTernaryEncoding.Compare(_packed, other._packed, _tritCount);

   public short ShortValue
   {
      readonly get => (short)BalancedTernaryEncoding.ToInt64(_packed, _tritCount);
      set => SetValue(value);
   }

   public void SetValue(short value)
    {
        if (value < MinValue || value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must fit in {N_TRITS_PER_CHART} balanced trits.");
        _packed = BalancedTernaryEncoding.FromInt64(value, _tritCount);
    }

   public char ToChar()
   {
      if (ShortValue > 127)
      {
         throw new InvalidOperationException($"CharT value {ShortValue} is not a valid ASCII char");
      }
      return (char)ShortValue;
   }

   public static CharT FromChar(char c)
   {
      if (c > 127)
      {
         throw new ArgumentOutOfRangeException(nameof(c), "Only ASCII chars (0-127) are supported in CharT");
      }
      return new CharT((short)c);
   }

   public static implicit operator CharT(char c) => FromChar(c);
   public static explicit operator char(CharT ct) => ct.ToChar();
   
   private static long Pow3(int exponent)
   {
      long value = 1;
      for (int i = 0; i < exponent; i++)
      {
         value *= 3;
      }
      return value;
   }

    private static void ValidatePacked(ulong packed, int count)
    {
        ulong mask = count >= 32 ? ulong.MaxValue : (1UL << (count * 2)) - 1UL;
        if ((packed & ~mask) != 0)
            throw new ArgumentOutOfRangeException(nameof(packed), "Packed value contains bits outside configured trit width.");
    }

}