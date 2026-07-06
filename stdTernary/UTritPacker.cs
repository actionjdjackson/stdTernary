using System;

namespace stdTernary;

public static class UTritPacker
{
    public static uint PackTrits(UTrit[] trits)
    {
        if (trits.Length > 16)
            throw new ArgumentException("Can only pack up to 16 unbalanced trits into a uint.");

        uint packed = 0;
        for (int i = 0; i < trits.Length; i++)
        {
            packed |= ((uint)(byte)trits[i]) << (i * 2);
        }

        return packed;
    }

    public static UTrit[] UnpackTrits(uint packed, int count)
    {
        if (count > 16)
            throw new ArgumentException("Can only unpack up to 16 unbalanced trits from a uint.");

        UTrit[] trits = new UTrit[count];
        for (int i = 0; i < count; i++)
        {
            uint bits = (packed >> (i * 2)) & 0b11u;
            if (bits > 2)
                throw new InvalidOperationException("Invalid 2-bit unbalanced trit encoding.");
            trits[i] = new UTrit((byte)bits);
        }

        return trits;
    }

    public static ulong PackTrits64(UTrit[] trits)
    {
        if (trits.Length > 32)
            throw new ArgumentException("Can only pack up to 32 unbalanced trits into a ulong.");

        ulong packed = 0;
        for (int i = 0; i < trits.Length; i++)
        {
            packed |= ((ulong)(byte)trits[i]) << (i * 2);
        }

        return packed;
    }

    public static UTrit[] UnpackTrits64(ulong packed, int count)
    {
        if (count > 32)
            throw new ArgumentException("Can only unpack up to 32 unbalanced trits from a ulong.");

        UTrit[] trits = new UTrit[count];
        for (int i = 0; i < count; i++)
        {
            ulong bits = (packed >> (i * 2)) & 0b11UL;
            if (bits > 2)
                throw new InvalidOperationException("Invalid 2-bit unbalanced trit encoding.");
            trits[i] = new UTrit((byte)bits);
        }

        return trits;
    }
}
