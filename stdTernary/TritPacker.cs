using System;

namespace stdTernary
{
   public static class TritPacker
   {
      /// <summary>
      /// Packs up to 16 Trits into a single uint (2 bits per trit).
      /// </summary>
      public static uint PackTrits(Trit[] trits)
      {
         if (trits.Length > 16)
            throw new ArgumentException("Can only pack up to 16 trits into a uint.");

         uint packed = 0;
         for (int i = 0; i < trits.Length; i++)
         {
            uint bits = trits[i].Value switch
            {
               TritVal.z => 0b00u,
               TritVal.p => 0b01u,
               TritVal.n => 0b10u,
               _ => throw new ArgumentOutOfRangeException()
            };

            packed |= (bits << (i * 2));
         }

         return packed;
      }

      /// <summary>
      /// Unpacks a uint into a Trit array (up to 16 Trits).
      /// </summary>
      public static Trit[] UnpackTrits(uint packed, int count)
      {
         if (count > 16)
            throw new ArgumentException("Can only unpack up to 16 trits from a uint.");

         Trit[] trits = new Trit[count];

         for (int i = 0; i < count; i++)
         {
            uint bits = (packed >> (i * 2)) & 0b11u;

            trits[i] = bits switch
            {
               0b00 => new Trit(0),
               0b01 => new Trit(1),
               0b10 => new Trit(-1),
               _ => throw new InvalidOperationException("Invalid 2-bit trit encoding.")
            };
         }

         return trits;
      }

      /// <summary>
      /// Packs up to 32 trits into a single ulong (2 bits per trit).
      /// </summary>
      public static ulong PackTrits64(Trit[] trits)
      {
         if (trits.Length > 32)
            throw new ArgumentException("Can only pack up to 32 trits into a ulong.");

         ulong packed = 0;
         for (int i = 0; i < trits.Length; i++)
         {
            ulong bits = trits[i].Value switch
            {
               TritVal.z => 0b00ul,
               TritVal.p => 0b01ul,
               TritVal.n => 0b10ul,
               _ => throw new ArgumentOutOfRangeException()
            };

            packed |= (bits << (i * 2));
         }

         return packed;
      }

      /// <summary>
      /// Unpacks a ulong into a Trit array (up to 32 trits).
      /// </summary>
      public static Trit[] UnpackTrits64(ulong packed, int count)
      {
         if (count > 32)
            throw new ArgumentException("Can only unpack up to 32 trits from a ulong.");

         Trit[] trits = new Trit[count];

         for (int i = 0; i < count; i++)
         {
            ulong bits = (packed >> (i * 2)) & 0b11ul;

            trits[i] = bits switch
            {
               0b00 => new Trit(0),
               0b01 => new Trit(1),
               0b10 => new Trit(-1),
               _ => throw new InvalidOperationException("Invalid 2-bit trit encoding."),
            };
         }

         return trits;
      }
   }
}
