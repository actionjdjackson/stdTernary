using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Jobs;

namespace stdTernary
{
    
    public enum TritVal : sbyte
        {
            p = 1,
            n = -1,
            z = 0
        }

    /// <summary>
    /// Balanced Ternary Trit with sbyte enum as datatype for -1 , 1, and 0 values, and conversions for ints, sbytes, characters,
    /// and overridden operators for tritwise operations.
    /// </summary>
    public struct Trit
    {
        private TritVal trit;

        public TritVal Value { get => trit; set => SetValue(value); }
        public char GetChar { get => ConvertToChar(); }

        public static Trit operator &(Trit left, Trit right) => left.AND(right);
        public static Trit operator |(Trit left, Trit right) => left.OR(right);
        public static Trit operator ^(Trit left, Trit right) => left.XOR(right);
        public static Trit operator ~(Trit trit) => trit.NEG();
        public static Trit operator !(Trit trit) => trit.NEG();
        public static Trit operator *(Trit left, Trit right) => left.MULT(right);
        public static Trit operator +(Trit left, Trit right) => left.SUM(right);
        public static bool operator ==(Trit left, Trit right) => (bool)left.EQUAL(right);
        public static bool operator !=(Trit left, Trit right) => (bool)left.NOTEQUAL(right);
        public static bool operator >(Trit left, Trit right) => left.Value > right.Value;
        public static bool operator <(Trit left, Trit right) => left.Value < right.Value;
        public static bool operator >=(Trit left, Trit right) => left.Value >= right.Value;
        public static bool operator <=(Trit left, Trit right) => left.Value <= right.Value;
        public static bool operator true(Trit trit) => trit.Value == TritVal.p;
        public static bool operator false(Trit trit) => trit.Value == TritVal.n || trit.Value == TritVal.z;

        public static explicit operator bool(Trit trit) => trit.trit == TritVal.p;
        public static explicit operator Trit(bool b) => b ? new Trit(1) : new Trit(-1);
        public static implicit operator sbyte(Trit trit) => (sbyte)trit.trit;
        public static implicit operator Trit(sbyte sb) => (sb <= 1 && sb >= -1) ? new Trit(sb) : throw new ArithmeticException("Tried to assign a value too big for Trit - keep it to -1, 0, or 1");
        public static implicit operator int(Trit trit) => (int)trit.trit;
        public static implicit operator Trit(int @int) => (@int <= 1 && @int >= -1) ? new Trit(@int) : throw new ArithmeticException("Tried to assign a value too big for Trit -  keep it to -1, 0, or 1");


        public Trit LargerOrEqual(Action func)
        {
            if (Value != TritVal.n)
            {
                func();
                return this;
            }
            else
            {
                return this;
            }
        }

        public Trit SmallerOrEqual(Action func)
        {
            if (Value != TritVal.p)
            {
                func();
                return this;
            }
            else
            {
                return this;
            }
        }

        public Trit Larger(Action func)
        {
            if (Value == TritVal.p)
            {
                func();
                return this;
            }
            else
            {
                return this;
            }
        }

        public Trit Smaller(Action func)
        {
            if (Value == TritVal.n)
            {
                func();
                return this;
            }
            else
            {
                return this;
            }
        }

        public Trit Equal(Action func)
        {
            if (Value == TritVal.z)
            {
                func();
                return this;
            }
            else
            {
                return this;
            }
        }

        public Trit Else(Action func)
        {
            func();
            return this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Trit trit &&
                   this.trit == trit.trit;
        }

        public Trit(sbyte value)
        {
            this.trit = (TritVal)value;
        }

        public Trit(int value)
        {
            this.trit = (TritVal)value;
        }

        public Trit(TritVal trit)
        {
            this.trit = trit;
        }

        public Trit(char charValue)
        {
            this.trit = charValue switch
            {
                '0' => TritVal.z,
                '+' => TritVal.p,
                '-' => TritVal.n,
                _ => throw new ArgumentException("Trit SetValue not given a valid ternary character. Should be 0, +, or -", "charValue"),
            };
        }

        public void SetValue(TritVal trit)
        {
            this.trit = trit;
        }

        public void SetValue(sbyte value)
        {
            this.trit = (TritVal)value;
        }

        public void SetValue(int value)
        {
            this.trit = (TritVal)value;
        }

        public void SetValue(char charValue)
        {
            this.trit = charValue switch
            {
                '0' => TritVal.z,
                '+' => TritVal.p,
                '-' => TritVal.n,
                _ => throw new ArgumentException("Trit SetValue not given a valid ternary character. Should be 0, +, or -", "charValue"),
            };
        }

        public char ConvertToChar()
        {
            return this.trit switch
            {
                TritVal.n => '-',
                TritVal.p => '+',
                _ => '0',
            };
        }

        public Trit NEG()
        {
            return new Trit((sbyte)(((sbyte)trit) * -1));
        }

        public Trit XOR(Trit t)
        {
            if (this.trit == TritVal.z || t.trit == TritVal.z)
            {
                return new Trit(0);
            }
            else if (this.trit == t.trit)
            {
                return new Trit(-1);
            }
            else
            {
                return new Trit(1);
            }
        }

        public Trit SUM(Trit t)
        {
            if (this.trit == t.Value)
            {
                return this.NEG();
            }
            else if ((sbyte)this.trit == -(sbyte)t.Value)
            {
                return new Trit(0);
            }
            else if (this.trit == 0)
            {
                return new Trit(t.Value);
            }
            else if (t.Value == 0)
            {
                return new Trit(this.trit);
            }
            else
            {
                throw new ArgumentException("Unknown state of trit in SUM operation.", nameof(t));
            }
        }

        public Trit CONS(Trit t)
        {
            if (this.trit == t.Value)
            {
                return new Trit(this.trit);
            }
            else
            {
                return new Trit(0);
            }
        }

        public Trit XNOR(Trit t)
        {
            return new Trit((sbyte)((sbyte)this.trit * (sbyte)t.Value));
        }

        public Trit MULT(Trit t)
        {
            return this.XNOR(t);
        }

        public Trit MIN(Trit t)
        {
            if (this.trit == TritVal.n || t.Value == TritVal.n)
            {
                return new Trit(-1);
            }
            else if (this.trit == TritVal.z || t.Value == TritVal.z)
            {
                return new Trit(0);
            }
            else
            {
                return new Trit(1);
            }
        }

        public Trit AND(Trit t)
        {
            return this.MIN(t);
        }

        public Trit MAX(Trit t)
        {
            if (this.trit == TritVal.p || t.Value == TritVal.p)
            {
                return new Trit(1);
            }
            if (this.trit == TritVal.z || t.Value == TritVal.z)
            {
                return new Trit(0);
            }
            else
            {
                return new Trit(-1);
            }
        }

        public Trit OR(Trit t)
        {
            return this.MAX(t);
        }

        public Trit EQUAL(Trit t)
        {
            if (trit == t.Value)
            {
                return new Trit(1);
            }
            else
            {
                return new Trit(-1);
            }
        }

        public Trit NOTEQUAL(Trit t)
        {
            if (trit != t.Value)
            {
                return new Trit(1);
            }
            else
            {
                return new Trit(-1);
            }
        }

        public static Trit COMPARET(Trit a, Trit b)
        {
            if (a.Value < b.Value)
            {
                return new Trit(-1);
            }
            else if (a.Value > b.Value)
            {
                return new Trit(1);
            }
            else
            {
                return new Trit(0);
            }
        }
    }


    /// <summary>
    /// A customizable Balanced Ternary tryte data type/struct with a range of 2 - 10 trits per tryte. Bytewise operators have been overridden
    /// for everything. There is a COMPARISON (COMPARET) method which is for the <=> spaceship operator - a ternary comparison - but C# doesn't 
    /// allow custom operators.
    /// </summary>
    public struct Tryte
    {
        /// <summary>
        /// Static member defining the number of Trits in a Tryte. Can be between 2 and 10 trits.
        /// </summary>
        public static byte N_TRITS_PER_TRYTE = 6;
        /// <summary>
        /// Maximum value representable by a Tryte of length N_TRITS_PER_TRYTE
        /// </summary>
        public static short MaxValue = (short)((Math.Pow(3, N_TRITS_PER_TRYTE) - 1) / 2);
        /// <summary>
        /// Minimum (negative) value representable by a Tryte of length N_TRITS_PER_TRYTE
        /// </summary>
        public static short MinValue = (short)-MaxValue;
        /// <summary>
        /// The Tryte value represented by a packed uint
        /// </summary>
        private uint packedTritsTryte;

        public uint PackedTrits => packedTritsTryte;

        public Trit[] Value
        {
            get => TritPacker.UnpackTrits(packedTritsTryte, N_TRITS_PER_TRYTE);
            set => packedTritsTryte = TritPacker.PackTrits(value);
        }
        public string TryteString { get => ConvertToStringRepresentation(); }
        public short ShortValue { get => ConvertBalancedTritsToInteger(Value); set => SetValue(value); }

        public const TritVal Equal = TritVal.z;
        public const TritVal Smaller = TritVal.n;
        public const TritVal Larger = TritVal.p;

        public static Tryte operator &(Tryte left, Tryte right) => left.AND(right);
        public static Tryte operator |(Tryte left, Tryte right) => left.OR(right);
        public static Tryte operator ^(Tryte left, Tryte right) => left.XOR(right);
        public static Tryte operator ~(Tryte tryte) => tryte.INVERT();
        public static bool operator ==(Tryte left, Tryte right) => COMPARET(left, right).Value == Equal;
        public static bool operator ==(Tryte left, int right) => left.ShortValue == right;
        public static bool operator !=(Tryte left, Tryte right) => COMPARET(left, right).Value != Equal;
        public static bool operator !=(Tryte left, int right) => left.ShortValue != right;
        public static bool operator >(Tryte left, Tryte right) => COMPARET(left, right).Value == Larger;
        public static bool operator <(Tryte left, Tryte right) => COMPARET(left, right).Value == Smaller;
        public static bool operator >=(Tryte left, Tryte right) => COMPARET(left, right).Value != Smaller;
        public static bool operator <=(Tryte left, Tryte right) => COMPARET(left, right).Value != Larger;
        public static bool operator >(Tryte left, int right) => left.ShortValue > right;
        public static bool operator <(Tryte left, int right) => left.ShortValue < right;
        public static bool operator >=(Tryte left, int right) => left.ShortValue >= right;
        public static bool operator <=(Tryte left, int right) => left.ShortValue <= right;
        public static Tryte operator +(Tryte left, Tryte right) => left.ADD(right);
        public static Tryte operator *(Tryte left, Tryte right) => left.MULT(right);
        public static Tryte operator -(Tryte left, Tryte right) => left.SUB(right);
        public static Tryte operator /(Tryte left, Tryte right) => left.DIV(right);
        public static Tryte operator %(Tryte left, Tryte right) => left.MOD(right);
        public static Tryte operator <<(Tryte tryte, int nTrits) => tryte.SHIFTLEFT(nTrits);
        public static Tryte operator >>(Tryte tryte, int nTrits) => tryte.SHIFTRIGHT(nTrits);
        public static Tryte operator ++(Tryte tryte) => tryte + 1;
        public static Tryte operator --(Tryte tryte) => tryte - 1;
        public static Tryte operator +(Tryte tryte) => MathT.Abs(tryte);
        public static Tryte operator -(Tryte tryte) => ~tryte;
        public static implicit operator short(Tryte tryte) => tryte.ShortValue;
        public static implicit operator Tryte(short shortValue) => (shortValue <= MaxValue && shortValue >= MinValue) ? new Tryte(shortValue) : throw new ArithmeticException("Tried to assign to a tryte a short that has a magnitude too big for a tryte of " + N_TRITS_PER_TRYTE + " trits");
        public static implicit operator int(Tryte tryte) => tryte.ShortValue;
        public static implicit operator Tryte(int intValue) => (intValue <= MaxValue && intValue >= MinValue) ? new Tryte((short)intValue) : throw new ArithmeticException("Tried to assign to a tryte an int that has a magnitude too big for a tryte of " + N_TRITS_PER_TRYTE + " trits");
        public static explicit operator string(Tryte tryte) => tryte.TryteString;
        public static explicit operator Tryte(string str) => new Tryte(str);
        public static implicit operator double(Tryte tryte) => tryte.ShortValue;

        public Trit this[int index]
        {
            get => TritPacker.UnpackTrits(packedTritsTryte, N_TRITS_PER_TRYTE)[index];
        }

        public Tryte(Trit[] value)
        {
            if (value.Length != N_TRITS_PER_TRYTE)
            {
                throw new ArgumentException($"Expected {N_TRITS_PER_TRYTE} trits", nameof(value));
            }
            else
            {
                this.packedTritsTryte = TritPacker.PackTrits(value);
            }
        }

        public static short ConvertBalancedTritsToInteger(Trit[] value)
        {
            short sum = 0;
            short exponent = (short)(N_TRITS_PER_TRYTE - 1);
            foreach (var trit in value)
            {
                sum += (short)((sbyte)trit.Value * Math.Pow(3, exponent));   //exponent increases as the invisible index increases, and adds to the sum if the trit is -1 or 1
                exponent--;
            }
            return sum;
        }

        public Tryte(short value)
        {
            if (value > MaxValue || value < MinValue)
            {
                throw new ArgumentOutOfRangeException("The short value passed to SetValue in Tryte was too large to fit in a " + N_TRITS_PER_TRYTE + " trit tryte", "value");
            }
            else
            {
                this.packedTritsTryte = TritPacker.PackTrits(ConvertIntegerToBalancedTrits(value));
            }
        }

        public Tryte(uint packedTrits)
        {
            if (TritPacker.UnpackTrits(packedTrits, N_TRITS_PER_TRYTE).Length != N_TRITS_PER_TRYTE)
            {
                throw new ArgumentException($"Packed trits must contain exactly {N_TRITS_PER_TRYTE} trits");
            }
            this.packedTritsTryte = packedTrits;
        }

        public void SetTrit(int index, Trit t)
        {
            var trits = Value;
            trits[index] = t;
            packedTritsTryte = TritPacker.PackTrits(trits);
        }

        public Tryte(string value)
        {
            if (value.Length == N_TRITS_PER_TRYTE)
            {
                var trits = new Trit[N_TRITS_PER_TRYTE];
                for (int i = 0; i < value.Length; i++)
                {
                    trits[i] = value[i] switch
                    {
                        '+' => new Trit(1),
                        '-' => new Trit(-1),
                        '0' => new Trit(0),
                        _ => throw new ArgumentException("Invalid character encountered in a balanced ternary char array. Please stick to +, -, 0's", "value"),
                    };
                }
                packedTritsTryte = TritPacker.PackTrits(trits);
            }
            else
            {
                throw new ArgumentException("The character array passed to Tryte was not the expected size, should be " + N_TRITS_PER_TRYTE + " chars in length", "value");
            }
        }

        public void SetValue(Trit[] value)
        {
            if (value.Length == N_TRITS_PER_TRYTE)
            {
                this.packedTritsTryte = TritPacker.PackTrits(value);
                // short sum = 0;
                // short exponent = (short)(N_TRITS_PER_TRYTE - 1);
                // foreach (var trit in value)
                // {
                //     sum += (short)((sbyte)trit.Value * Math.Pow(3, exponent));   //exponent increases as the invisible index increases, and adds to the sum if the trit is -1 or 1
                //     exponent--;
                // }
                // shortValue = sum;
            }
            else
            {
                throw new ArgumentException("The trit array passed to Tryte was not the expected size, should be " + N_TRITS_PER_TRYTE + " trits in length", "value");
            }
        }

        public void SetValue(string value)
        {
            if (value.Length == N_TRITS_PER_TRYTE)
            {
                var trits = new Trit[N_TRITS_PER_TRYTE];
                for (int i = 0; i < value.Length; i++)
                {
                    trits[i] = value[i] switch
                    {
                        '+' => new Trit(1),
                        '-' => new Trit(-1),
                        '0' => new Trit(0),
                        _ => throw new ArgumentException("Invalid character encountered in a balanced ternary char array. Please stick to +, -, 0's", "value"),
                    };
                }
                packedTritsTryte = TritPacker.PackTrits(trits);
            }
            else
            {
                throw new ArgumentException("The character array passed to Tryte was not the expected size, should be " + N_TRITS_PER_TRYTE + " chars in length", "value");
            }
        }

        public void SetValue(short value)
        {
            if (value <= MaxValue && value >= MinValue)
            {
                //shortValue = value;
                this.Value = ConvertIntegerToBalancedTrits(value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("The short value passed to SetValue in Tryte was too large to fit in a " + N_TRITS_PER_TRYTE + " trit tryte", "value");
            }
        }

        public static Trit[] ConvertIntegerToBalancedTrits(int intValue)
        {
            Trit[] trits = new Trit[N_TRITS_PER_TRYTE];
            int workValue = Math.Abs(intValue);     //easier to work with a positive number...
            byte i = 0;     //easier to start with index 0 - greater numbers on the right
            while (workValue != 0)
            {
                switch (workValue % 3)
                {
                    case 0:
                        trits[i] = new Trit(0);
                        break;
                    case 1:
                        trits[i] = new Trit(1);
                        break;
                    case 2:
                        trits[i] = new Trit(-1);
                        break;
                }
                workValue = (workValue + 1) / 3;
                i++;
            }
            for (int j = i; j < N_TRITS_PER_TRYTE; j++)
            {
                trits[j] = new Trit(0);   //...add zeros to fill in to make the length match the N_TRITS_PER_INT static value
            }
            if (intValue < 0)   //...and invert if it was negative
            {
                for (int n = 0; n < N_TRITS_PER_TRYTE; n++)
                {
                    trits[n] = !trits[n];
                }
            }
            Array.Reverse(trits);    //...and reverse the trit order so greater numbers are on the left
            return trits;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Tryte tryte &&
                   EqualityComparer<Trit[]>.Default.Equals(this.Value, tryte.Value);
        }

        public override string ToString()
        {
            return new string(TryteString) + " = " + ShortValue;
        }

        public string ConvertToStringRepresentation()
        {
            var temp = "";
            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                switch (Value[i].Value)
                {
                    case TritVal.n:
                        temp += "-";
                        break;
                    case TritVal.p:
                        temp += "+";
                        break;
                    case TritVal.z:
                        temp += "0";
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }

        public Tryte SHIFTLEFT(int nTrits)
        {
            if (nTrits <= N_TRITS_PER_TRYTE)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[N_TRITS_PER_TRYTE];
                    for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
                    {
                        temp[i] = i >= N_TRITS_PER_TRYTE - nTrits ? new Trit(0) : Value[i + nTrits];
                    }
                    return new Tryte(temp);
                }
                else
                {
                    return this;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Number of trits to shift left is too large for a tryte of " + N_TRITS_PER_TRYTE + " trits", "nTrits");
            }
        }

        public Tryte SHIFTRIGHT(int nTrits)
        {
            if (nTrits <= N_TRITS_PER_TRYTE)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[N_TRITS_PER_TRYTE];
                    for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
                    {
                        temp[i] = i < nTrits ? new Trit(0) : Value[i - nTrits];
                    }
                    return new Tryte(temp);
                }
                else
                {
                    return this;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Number of trits to shift right is too large for a tryte of " + N_TRITS_PER_TRYTE + " trits", "nTrits");
            }
        }

        public Tryte MOD(Tryte divisor)
        {
            (var quotmod, var rem) = this.DIVREM(divisor);
            if (quotmod < 0 && rem != 0)
            {
                quotmod--;
            }
            var mod = this - (divisor * quotmod);
            return mod;
        }

        public (Tryte Quotient, Tryte Remainder) DIVREM(Tryte divisor)
        {
            if (COMPARET(divisor, 0).Value == TritVal.z)
                throw new DivideByZeroException("Divide by zero in Tryte division.");

            Tryte aAbs = MathT.Abs(this);
            Tryte bAbs = MathT.Abs(divisor);

            if (COMPARET(aAbs, bAbs).Value == TritVal.p)
            {
                Tryte rem = new Tryte(aAbs.PackedTrits);
                Tryte quot = new Tryte(0);

                while (COMPARET(rem, bAbs).Value != TritVal.n)
                {
                    rem = rem.SUB(bAbs);
                    quot = quot.ADD(new Tryte(1));
                }

                // Fix sign of quotient
                bool oppositeSigns = (COMPARET(this, 0).Value == TritVal.n)
                                ^ (COMPARET(divisor, 0).Value == TritVal.n);

                if (oppositeSigns)
                    quot = ~quot;

                return (quot, rem);
            }
            else if (COMPARET(this, divisor).Value == TritVal.z)
            {
                return (new Tryte(1), new Tryte(0));
            }
            else
            {
                return (new Tryte(0), new Tryte(this.PackedTrits));
            }
        }


        public Tryte DIV(Tryte divisor)
        {
            return this.DIVREM(divisor).Quotient;
        }


        public Tryte SUB(Tryte t)
        {
            return this.ADD(t.INVERT());
        }

        public Tryte MULT(Tryte t)
        {
            Trit[] a = TritPacker.UnpackTrits(this.packedTritsTryte, N_TRITS_PER_TRYTE);
            Array.Reverse(a); // multiplier reversed (least sig to most)
            
            uint packedT = t.packedTritsTryte;
            uint packedTInverted = t.INVERT().packedTritsTryte;

            var product = new Tryte(0);

            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                Trit trit = a[i];
                Tryte shiftedPartial;

                switch (trit.Value)
                {
                    case TritVal.p:
                        shiftedPartial = new Tryte(packedT).SHIFTLEFT(i);
                        product = product.ADD(shiftedPartial);
                        break;

                    case TritVal.n:
                        shiftedPartial = new Tryte(packedTInverted).SHIFTLEFT(i);
                        product = product.ADD(shiftedPartial);
                        break;

                    // TritVal.z → do nothing
                }
            }

            return product;
        }


        public Tryte ADD(Tryte t)
        {
            Trit[] a = TritPacker.UnpackTrits(this.packedTritsTryte, N_TRITS_PER_TRYTE);
            Trit[] b = TritPacker.UnpackTrits(t.packedTritsTryte, N_TRITS_PER_TRYTE);
            Trit[] sum = new Trit[N_TRITS_PER_TRYTE];

            sbyte carry = 0;

            for (int i = N_TRITS_PER_TRYTE - 1; i >= 0; i--)
            {
                sbyte aVal = (sbyte)a[i].Value;
                sbyte bVal = (sbyte)b[i].Value;
                sbyte total = (sbyte)(aVal + bVal + carry);

                switch (total)
                {
                    case 2:
                        carry = 1;
                        sum[i] = new Trit(-1);
                        break;
                    case -2:
                        carry = -1;
                        sum[i] = new Trit(1);
                        break;
                    case 3:
                    case -3:
                        carry = (sbyte)Math.Sign(total); // preserve direction
                        sum[i] = new Trit(0);
                        break;
                    default:
                        carry = 0;
                        sum[i] = new Trit(total);
                        break;
                }
            }

            if (carry != 0)
                throw new OverflowException($"Tryte ADD overflow! Carry = {carry}");

            return new Tryte(sum);
        }


        public Tryte AND(Tryte t)
        {
            var a = this.Value;
            var b = t.Value;
            var result = new Trit[N_TRITS_PER_TRYTE];
            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                result[i] = a[i] & b[i];
            }
            return new Tryte(result);
        }

        public Tryte OR(Tryte t)
        {
            var a = this.Value;
            var b = t.Value;
            var result = new Trit[N_TRITS_PER_TRYTE];
            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                result[i] = a[i] | b[i];
            }
            return new Tryte(result);
        }

        public Tryte XOR(Tryte t)
        {
            var a = this.Value;
            var b = t.Value;
            var result = new Trit[N_TRITS_PER_TRYTE];
            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                result[i] = a[i] ^ b[i];
            }
            return new Tryte(result);
        }

        /// <summary>
        /// Balanced Ternary Comparison operator method - returns 1 if tryte1 is larger, -1 if it is smaller, and 0 if the trytes are equal.
        /// Supposed to be used with the spaceship operator <=> but custom operators are not allowed in C#
        /// </summary>
        /// <param name="a">First Tryte to be compared</param>
        /// <param name="b">Second Tryte to be compared</param>
        /// <returns>Returns a Trit of value 1, 0, or -1</returns>
        public static Trit COMPARET(Tryte a, Tryte b)
        {
            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                if (a.Value[i] > b.Value[i])
                {
                    return new Trit(1);
                }
                else if (a.Value[i] < b.Value[i])
                {
                    return new Trit(-1);
                }
            }
            return new Trit(0);
        }

        public Tryte INVERT()
        {
            Trit[] inverted = new Trit[N_TRITS_PER_TRYTE];
            var trits = TritPacker.UnpackTrits(this.packedTritsTryte, N_TRITS_PER_TRYTE);

            for (int i = 0; i < N_TRITS_PER_TRYTE; i++)
            {
                inverted[i] = trits[i].NEG();
            }

            return new Tryte(inverted);
        }


        public void InvertSelf()
        {
            SetValue(INVERT().Value);
        }
    }


    /// <summary>
    /// The IntT struct is a modifiable general purpose Balanced Ternary integer, with an array of trits and a long for the values it holds.
    /// All the math is done ternary and Trit-shifting is done in Ternary. Can be explicitly cast to a string of +, -, 0's
    /// </summary>
    public struct IntT
    {
        /// <summary>
        /// Static member defining the number of trits that make up the IntT
        /// </summary>
        public static byte N_TRITS_PER_INT = 24;
        /// <summary>
        /// Maximum representable number given a particular N_TRITS_PER_INT
        /// </summary>
        public static long MaxValue = (long)(Math.Pow(3, N_TRITS_PER_INT) - 1) / 2;
        /// <summary>
        /// Minimum (negative) representable number given a particular N_TRITS_PER_INT
        /// </summary>
        public static long MinValue = -MaxValue;
        /// <summary>
        /// The ternary representation of the value of the IntT as an array of Trits
        /// </summary>
        private Trit[] intt;
        /// <summary>
        /// The binary representation of the value of hte IntT as a long integer
        /// </summary>
        //private long longValue;

        public Trit[] Value { get => intt; set => SetValue(value); }
        public string IntTString { get => ConvertToStringRepresentation(); }
        public long LongValue { get => ConvertBalancedTritsToInteger(intt); set => SetValue(value); }

        public const TritVal Equal = TritVal.z;
        public const TritVal Smaller = TritVal.n;
        public const TritVal Larger = TritVal.p;

        public static bool operator ==(IntT left, IntT right) => COMPARET(left, right).Value == Equal;
        public static bool operator !=(IntT left, IntT right) => COMPARET(left, right).Value != Equal;
        public static bool operator ==(IntT left, int right) => left.LongValue == right;
        public static bool operator !=(IntT left, int right) => left.LongValue != right;
        public static bool operator >(IntT left, IntT right) => COMPARET(left, right).Value == Larger;
        public static bool operator <(IntT left, IntT right) => COMPARET(left, right).Value == Smaller;
        public static bool operator >=(IntT left, IntT right) => COMPARET(left, right).Value != Smaller;
        public static bool operator <=(IntT left, IntT right) => COMPARET(left, right).Value != Larger;
        public static bool operator >(IntT left, int right) => left.LongValue > right;
        public static bool operator <(IntT left, int right) => left.LongValue < right;
        public static bool operator >=(IntT left, int right) => left.LongValue >= right;
        public static bool operator <=(IntT left, int right) => left.LongValue <= right;
        public static IntT operator +(IntT left, IntT right) => left.ADD(right);
        public static IntT operator -(IntT left, IntT right) => left.SUB(right);
        public static IntT operator *(IntT left, IntT right) => left.MULT(right);
        public static IntT operator /(IntT left, IntT right) => left.DIV(right);
        public static IntT operator %(IntT left, IntT right) => left.MOD(right);
        public static IntT operator <<(IntT intt, int nTrits) => intt.SHIFTLEFT(nTrits);
        public static IntT operator >>(IntT intt, int nTrits) => intt.SHIFTRIGHT(nTrits);
        public static IntT operator ++(IntT intt) => intt + 1;
        public static IntT operator --(IntT intt) => intt - 1;
        public static IntT operator +(IntT intt) => MathT.Abs(intt);
        public static IntT operator -(IntT intt) => intt.INVERT();
        public static IntT operator ~(IntT intt) => intt.INVERT();
        public static implicit operator long(IntT intt) => (intt <= long.MaxValue && intt >= long.MinValue) ? intt.LongValue : throw new ArithmeticException("Converting a IntT to a long value failed because it was outside the range of the long max/min values");
        public static implicit operator IntT(long @int) => (@int <= MaxValue && @int >= MinValue) ? new IntT(@int) : throw new ArithmeticException("Converting a long value to a IntT failed because it was outside the range of the IntT implementation: " + @int.ToString());
        public static implicit operator int(IntT intt) => (intt <= int.MaxValue && intt >= int.MinValue) ? (int)intt.LongValue : throw new ArithmeticException("Converting a IntT to an int failed because the value was outside the range of the int max/min values");
        public static implicit operator IntT(int @int) => (@int <= MaxValue && @int >= MinValue) ? new IntT(@int) : throw new ArithmeticException("Converting an int value to a IntT failed because it was outside the range of the IntT implementation");
        public static explicit operator IntT(Tryte tryte) => new IntT(tryte.ShortValue);
        ///public static implicit operator short(IntT intt) => (intt <= short.MaxValue && intt >= short.MinValue) ? (short)intt.longValue : throw new ArithmeticException("Converting a IntT to a short failed because the value was outside the range of the short max/min values");
        //public static implicit operator IntT(short @int) => (@int <= MaxValue && @int >= MinValue) ? new IntT(@int) : throw new ArithmeticException("Converting a short value to a IntT failed because it was outside the range of the IntT implementation");
        public static explicit operator string(IntT intt) => intt.IntTString;
        public static explicit operator IntT(string str) => new IntT(str);
        public static explicit operator double(IntT intt) => intt.LongValue;

        /// <summary>
        /// Constructor for the IntT struct, taking an array of Trits
        /// </summary>
        /// <param name="value">The value passed in as an array of Trits</param>
        public IntT(Trit[] value)
        {
            if (value.Length == N_TRITS_PER_INT)
            {
                intt = value;
                //longValue = ConvertBalancedTritsToInteger(value);
            }
            else if (value.Length < N_TRITS_PER_INT)
            {
                intt = new Trit[N_TRITS_PER_INT];
                for (int i = 0; i < N_TRITS_PER_INT - value.Length; i++)
                {
                    intt[i] = new Trit(0);
                }
                for (int i = N_TRITS_PER_INT - value.Length; i < N_TRITS_PER_INT; i++)
                {
                    intt[i] = value[i - (N_TRITS_PER_INT - value.Length)];
                }
                //longValue = ConvertBalancedTritsToInteger(intt);
            }
            else
            {
                throw new ArgumentException("Trit array passed to IntT SetValue method was not the expected size - should be " + N_TRITS_PER_INT + " trits in length or less", "value");
            }
        }

        /// <summary>
        /// Constructor for the IntT struct, taking a string of chars (0, +, -)
        /// </summary>
        /// <param name="value">The value passed in as a string of chars (0, +, -)</param>
        public IntT(string value)
        {
            if (value.Length == N_TRITS_PER_INT)
            {
                intt = new Trit[N_TRITS_PER_INT];
                for (int i = 0; i < N_TRITS_PER_INT; i++)
                {
                    intt[i] = value[i] switch
                    {
                        '+' => new Trit(1),
                        '-' => new Trit(-1),
                        '0' => new Trit(0),
                        _ => throw new ArgumentException("Invalid character encountered in a balanced ternary char array. Please stick to +, -, 0's", "value"),
                    };
                }
                //longValue = ConvertBalancedTritsToInteger(intt);
            }
            else
            {
                throw new ArgumentException("Char array passed to IntT SetValue method was not the expected size - should be " + N_TRITS_PER_INT + " chars in length", "value");
            }
        }

        /// <summary>
        /// Constructor for the IntT struct, taking a long binary integer
        /// </summary>
        /// <param name="value">The value passed in, as a long binary integer</param>
        public IntT(long value)
        {
            if (value > MaxValue || value < MinValue)
            {
                throw new OverflowException("Long integer value passed to IntT constructor too large for an IntT of size " + N_TRITS_PER_INT + " trits.");
            }

            intt = ConvertIntegerToBalancedTrits(value);
        }

        public string ConvertToStringRepresentation()
        {
            var temp = "";
            for (int i = 0; i < N_TRITS_PER_INT; i++)
            {
                switch (this.intt[i].Value)
                {
                    case TritVal.n:
                        temp += "-";
                        break;
                    case TritVal.p:
                        temp += "+";
                        break;
                    case TritVal.z:
                        temp += "0";
                        break;
                }
            }
            return temp;
        }

        public override string ToString()
        {
            return IntTString + " = " + LongValue;
        }

        public override bool Equals(object obj)
        {
            return obj is IntT @int &&
                   EqualityComparer<Trit[]>.Default.Equals(intt, @int.intt);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(intt);
        }

        public void SetValue(Trit[] value)
        {
            if (value.Length == N_TRITS_PER_INT)
            {
                intt = value;
                //longValue = ConvertBalancedTritsToInteger(value);
            }
            else if (value.Length < N_TRITS_PER_INT)
            {
                intt = new Trit[N_TRITS_PER_INT];
                for (int i = 0; i < N_TRITS_PER_INT - value.Length; i++)
                {
                    intt[i] = new Trit(0);
                }
                for (int i = N_TRITS_PER_INT - value.Length; i < N_TRITS_PER_INT; i++)
                {
                    intt[i] = value[i - (N_TRITS_PER_INT - value.Length)];
                }
                //longValue = ConvertBalancedTritsToInteger(intt);
            }
            else
            {
                throw new ArgumentException("Trit array passed to IntT SetValue method was not the expected size - should be " + N_TRITS_PER_INT + " trits in length or less", "value");
            }
        }

        public void SetValue(long value)
        {
            if (value <= MaxValue && value >= MinValue)
            {
                //longValue = value;
                intt = ConvertIntegerToBalancedTrits(value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("value", "Long integer value passed to IntT SetValue too large for a IntT of size " + N_TRITS_PER_INT + " trits.");
            }
        }

        public void SetValue(string value)
        {
            if (value.Length == N_TRITS_PER_INT)
            {
                for (int i = 0; i < N_TRITS_PER_INT; i++)
                {
                    intt[i] = value[i] switch
                    {
                        '+' => new Trit(1),
                        '-' => new Trit(-1),
                        '0' => new Trit(0),
                        _ => throw new ArgumentException("Invalid character encountered in a balanced ternary char array. Please stick to +, -, 0's", "value"),
                    };
                }
                //longValue = ConvertBalancedTritsToInteger(intt);
            }
            else
            {
                throw new ArgumentException("Char array passed to IntT SetValue method was not the expected size - should be " + N_TRITS_PER_INT + " chars in length", "value");
            }
        }

        public static long ConvertBalancedTritsToInteger(Trit[] trits)
        {
            long sum = 0;
            short exponent = (short)(N_TRITS_PER_INT - 1);
            foreach (var trit in trits)
            {
                sum += (long)((sbyte)trit.Value * Math.Pow(3, exponent));   //exponent increases as the invisible index increases, and adds to the sum if the trit is -1 or 1
                exponent--;
            }
            return sum;
        }

        public static Trit[] ConvertIntegerToBalancedTrits(long intValue)
        {
            Trit[] trits = new Trit[N_TRITS_PER_INT];
            long workValue = Math.Abs(intValue);     //easier to work with a positive number...
            byte i = 0;     //easier to start with index 0 - greater numbers on the right
            while (workValue != 0)
            {
                switch (workValue % 3)
                {
                    case 0:
                        trits[i] = new Trit(0);
                        break;
                    case 1:
                        trits[i] = new Trit(1);
                        break;
                    case 2:
                        trits[i] = new Trit(-1);
                        break;
                }
                workValue = (workValue + 1) / 3;
                i++;
            }
            for (int j = i; j < N_TRITS_PER_INT; j++)
            {
                trits[j] = new Trit(0);   //...add zeros to fill in to make the length match the N_TRITS_PER_INT static value
            }
            if (intValue < 0)   //...and invert if it was negative
            {
                for (int n = 0; n < N_TRITS_PER_INT; n++)
                {
                    trits[n] = !trits[n];
                }
            }
            Array.Reverse(trits);    //...and reverse the trit order so greater numbers are on the left
            return trits;
        }

        /// <summary>
        /// A Balanced Ternary Comparison Operator Method that returns 1 if a is larger, -1 if a is smaller, and 0 if the ints are equal.
        /// Supposed to be used with the spaceship operator <=> but C# does not support custom operators.
        /// </summary>
        /// <param name="a">The first IntT to be compared</param>
        /// <param name="b">The second IntT to be compared</param>
        /// <returns>The Trit containing a value of 1, -1, or 0 depending on the comparison</returns>
        public static Trit COMPARET(IntT a, IntT b)
        {
            for (byte i = 0; i < N_TRITS_PER_INT; i++)
            {
                if (a.intt[i] > b.intt[i])
                {
                    return new Trit(1);
                }
                else if (a.intt[i] < b.intt[i])
                {
                    return new Trit(-1);
                }
            }
            return new Trit(0);
        }

        public IntT INVERT()
        {
            var trits = new Trit[N_TRITS_PER_INT];
            for (int n = 0; n < N_TRITS_PER_INT; n++)
            {
                trits[n] = !this.intt[n];
            }
            return new IntT(trits);
        }

        public IntT SHIFTLEFT(int nTrits)
        {
            if (nTrits <= N_TRITS_PER_INT)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[N_TRITS_PER_INT];
                    for (byte i = 0; i < N_TRITS_PER_INT; i++)
                    {
                        temp[i] = i >= N_TRITS_PER_INT - nTrits ? new Trit(0) : intt[i + nTrits];
                    }
                    return new IntT(temp);
                }
                else
                {
                    return this;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Number of trits to shift left is too large for a IntT of " + N_TRITS_PER_INT + " trits", "nTrits");
            }
        }

        public IntT SHIFTRIGHT(int nTrits)
        {
            if (nTrits <= N_TRITS_PER_INT)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[N_TRITS_PER_INT];
                    for (byte i = 0; i < N_TRITS_PER_INT; i++)
                    {
                        temp[i] = i < nTrits ? new Trit(0) : intt[i - nTrits];
                    }
                    return new IntT(temp);
                }
                else
                {
                    return this;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("Number of trits to shift right is too large for a IntT of " + N_TRITS_PER_INT + " trits", "nTrits");
            }
        }

        public IntT MOD(IntT divisor)
        {
            (var quotmod, var rem) = this.DIVREM(divisor);
            if (quotmod < 0 && rem != 0)
            {
                quotmod--;
            }
            var mod = this - (divisor * quotmod);
            return mod;
        }

        public IntT DIV(IntT divisor)
        {
            return DIVREM(divisor).Item1;
        }

        public (IntT, IntT) DIVREM(IntT divisor)
        {
            if (COMPARET(divisor, 0).Value == Equal)
            {
                throw new DivideByZeroException("Attempt to divide by zero in IntT division operation.");
            }

            long dividend = this.LongValue;
            long divisorValue = divisor.LongValue;

            long quotient = dividend / divisorValue;
            long remainder = dividend % divisorValue;

            return (new IntT(quotient), new IntT(remainder));
        }

        public IntT MULT(IntT multIntT)
        {
            Trit[] temp = new Trit[N_TRITS_PER_INT];
            this.intt.CopyTo(temp, 0);
            Array.Reverse(temp);
            var product = new IntT(0);
            var invertedMultIntT = multIntT.INVERT();
            for (int i = 0; i < N_TRITS_PER_INT; i++)
            {
                var trit = temp[i];
                var temp2 = new IntT(0);
                switch (trit.Value)
                {
                    case TritVal.n:
                        temp2 = new IntT(invertedMultIntT.Value);
                        break;
                    case TritVal.p:
                        temp2 = new IntT(multIntT.Value);
                        break;
                }
                temp2 <<= i;
                product += temp2;
            }
            return product;
        }

        public IntT SUB(IntT subtractIntT)
        {
            return this.ADD(subtractIntT.INVERT());
        }

        public IntT ADD(IntT addIntT)
        {
            return new IntT(this.LongValue + addIntT.LongValue);
            // Trit[] sumTrits = new Trit[N_TRITS_PER_INT];
            // sbyte carry = 0;
            // for (int i = N_TRITS_PER_INT - 1; i >= 0; i--)
            // {
            //     sbyte n = (sbyte)((sbyte)this.intt[i].Value + (sbyte)addIntT.Value[i].Value + carry);
            //     switch (n)
            //     {
            //         case 2:
            //             carry = 1;
            //             sumTrits[i] = -1;
            //             break;
            //         case -2:
            //             carry = -1;
            //             sumTrits[i] = 1;
            //             break;
            //         case 3:
            //             carry = 1;
            //             sumTrits[i] = 0;
            //             break;
            //         case -3:
            //             carry = -1;
            //             sumTrits[i] = 0;
            //             break;
            //         default:
            //             carry = 0;
            //             sumTrits[i] = n;
            //             break;
            //     }
            // }
            // return carry switch
            // {
            //     0 => new IntT(sumTrits),
            //     1 => throw new OverflowException("IntT Integer Add Positive Overflow! You've gone beyond what an IntT of size " + N_TRITS_PER_INT + " trits can hold"),
            //     -1 => throw new OverflowException("IntT Integer Add Negative Overflow! You've gone beyond what an IntT of size " + N_TRITS_PER_INT + " trits can hold"),
            //     _ => new IntT(sumTrits),
            // };
        }

    }



    /// <summary>
    /// The FloatT struct is a modifiable general purpose Balanced Ternary floating point number with trits and a double. Can be customized
    /// by changing the N_TRITS_TOTAL static parameter, and the fractions in the N_TRITS_SIGNIFICAND and N_TRITS_EXPONENT static parameters.
    /// </summary>
    public struct FloatT
    {
        /// <summary>
        /// The total number of trits used by the FloatT - 27 is a nice number
        /// </summary>
        public static byte N_TRITS_TOTAL_FLOAT = 24;
        /// <summary>
        /// The number of trits used for the significand in the FloatT - the precision. Currently set to 3/4 of the total trits rounded up.
        /// </summary>
        public static byte N_TRITS_SIGNIFICAND = (byte)Math.Ceiling((double)N_TRITS_TOTAL_FLOAT * 3 / 4);
        /// <summary>
        /// The number of trits used for the exponent in the FloatT - the magnitude. Currently set to 1/4 of the total trits rounded down.
        /// </summary>
        public static byte N_TRITS_EXPONENT = (byte)Math.Floor((double)N_TRITS_TOTAL_FLOAT / 4);
        /// <summary>
        /// The smallest representable value for the FloatT
        /// </summary>
        public static double Epsilon = Math.Pow(3, -(Math.Pow(3, N_TRITS_EXPONENT) - 1) / 2) / Math.Pow(3, N_TRITS_SIGNIFICAND);
        /// <summary>
        /// The maximum representable value for the FloatT
        /// </summary>
        public static double MaxValue = Math.Pow(3, (Math.Pow(3, N_TRITS_EXPONENT) - 1) / 2 - 1) * 0.5;
        /// <summary>
        /// The minimum (negative) value representable for the FloatT
        /// </summary>
        public static double MinValue = Math.Pow(3, (Math.Pow(3, N_TRITS_EXPONENT) - 1) / 2) * -0.5;
        /// <summary>
        /// The maximum positive value of the exponent
        /// </summary>
        public static int MaxExponentValue = (int)(Math.Pow(3, N_TRITS_EXPONENT) - 1) / 2 - 1;
        /// <summary>
        /// The minimum (negative) value of the exponent
        /// </summary>
        public static int MinExponentValue = (int)-(Math.Pow(3, N_TRITS_EXPONENT) - 1) / 2;

        public static FloatT PositiveInfinity = Double.PositiveInfinity;
        public static FloatT NegativeInfinity = Double.NegativeInfinity;

        /// <summary>
        /// The exponent component of the FloatT as an array of Trits
        /// </summary>
        private Trit[] exponent;
        /// <summary>
        /// The significand component of the FloatT as an array of Trits
        /// </summary>
        private Trit[] significand;
        /// <summary>
        /// The whole FloatT value as an array of Trits
        /// </summary>
        private Trit[] floatt;
        /// <summary>
        /// Packed exponent trits (two bits per trit).
        /// </summary>
        private uint packedExponent;
        /// <summary>
        /// Packed significand trits (two bits per trit).
        /// </summary>
        private ulong packedSignificand;
        /// <summary>
        /// The FloatT value represented as a binary double
        /// </summary>
        //private double doubleValue;

        public double DoubleValue { get => ConvertToDouble(); set => SetValue(value); }   //when we modify the three value types, they are calling the appropriate SetValue function for that type
        public Trit[] Value { get => floatt; set => SetValue(value); }
        public string FloatTString { get => ConvertToStringRepresentation(); }

        public const TritVal Equal = TritVal.z;
        public const TritVal Smaller = TritVal.n;
        public const TritVal Larger = TritVal.p;

        public static bool operator ==(FloatT left, FloatT right) => COMPARET(left, right).Value == Equal;
        public static bool operator !=(FloatT left, FloatT right) => COMPARET(left, right).Value != Equal;
        public static bool operator ==(FloatT left, double right) => left.DoubleValue == right;
        public static bool operator !=(FloatT left, double right) => left.DoubleValue != right;
        public static bool operator >(FloatT left, FloatT right) => COMPARET(left, right).Value == Larger;
        public static bool operator <(FloatT left, FloatT right) => COMPARET(left, right).Value == Smaller;
        public static bool operator >=(FloatT left, FloatT right) => COMPARET(left, right).Value != Smaller;
        public static bool operator <=(FloatT left, FloatT right) => COMPARET(left, right).Value != Larger;
        public static bool operator >(FloatT left, double right) => left.DoubleValue > right;
        public static bool operator <(FloatT left, double right) => left.DoubleValue < right;
        public static bool operator >=(FloatT left, double right) => left.DoubleValue >= right;
        public static bool operator <=(FloatT left, double right) => left.DoubleValue <= right;
        public static FloatT operator +(FloatT left, FloatT right) => left.ADD(right);
        public static FloatT operator -(FloatT left, FloatT right) => left.SUB(right);
        public static FloatT operator *(FloatT left, FloatT right) => left.MULT(right);
        public static FloatT operator /(FloatT left, FloatT right) => left.DIV(right);
        //public static FloatT operator %(FloatT left, FloatT right) => new FloatT(left.doubleValue % right.doubleValue);
        public static FloatT operator %(FloatT left, FloatT right) => left.MOD(right);
        //public static FloatT operator ++(FloatT left) => new FloatT(left.doubleValue += 1); 
        //public static FloatT operator --(FloatT left) => new FloatT(left.doubleValue -= 1); //not sure if ++ or -- are appropriate for a floating point number
        public static FloatT operator +(FloatT floatt) => MathT.Abs(floatt);
        public static FloatT operator -(FloatT floatt) => floatt.INVERTSIG();

        public static explicit operator IntT(FloatT floatt) => (Math.Round(floatt.DoubleValue, 0) <= IntT.MaxValue && Math.Round(floatt.DoubleValue, 0) >= IntT.MinValue) ? new IntT((long)Math.Round(floatt.DoubleValue, 0)) : throw new ArithmeticException("Double Value of FloatT is too big for an IntT of size " + IntT.N_TRITS_PER_INT + " trits");
        public static explicit operator FloatT(IntT intt) => ((double)intt <= FloatT.MaxValue && (double)intt >= FloatT.MinValue) ? new FloatT(intt.LongValue) : throw new ArithmeticException("IntT value too big for a FloatT with an exponent of size " + N_TRITS_EXPONENT + " trits");
        public static implicit operator double(FloatT floatt) => floatt.DoubleValue;
        public static implicit operator FloatT(double doubleVal) => new FloatT(doubleVal);
        public static explicit operator string(FloatT floatt) => floatt.FloatTString;
        public static explicit operator FloatT(string str) => new FloatT(str);

        private void SyncPackedFromArrays()
        {
            exponent ??= new Trit[N_TRITS_EXPONENT];
            significand ??= new Trit[N_TRITS_SIGNIFICAND];

            if (floatt == null)
            {
                floatt = new Trit[N_TRITS_TOTAL_FLOAT];
                Array.Copy(exponent, 0, floatt, 0, N_TRITS_EXPONENT);
                Array.Copy(significand, 0, floatt, N_TRITS_EXPONENT, N_TRITS_SIGNIFICAND);
            }

            packedExponent = TritPacker.PackTrits(exponent);
            packedSignificand = TritPacker.PackTrits64(significand);
        }

        private void SyncArraysFromPacked()
        {
            exponent = TritPacker.UnpackTrits(packedExponent, N_TRITS_EXPONENT);
            significand = TritPacker.UnpackTrits64(packedSignificand, N_TRITS_SIGNIFICAND);
            floatt = new Trit[N_TRITS_TOTAL_FLOAT];
            Array.Copy(exponent, 0, floatt, 0, N_TRITS_EXPONENT);
            Array.Copy(significand, 0, floatt, N_TRITS_EXPONENT, N_TRITS_SIGNIFICAND);
        }

        public override bool Equals(object obj) //just making sure I'm following all the rules and implementing an Equals function
        {
            return obj is FloatT @float &&
                   EqualityComparer<Trit[]>.Default.Equals(floatt, @float.floatt);
        }

        public override int GetHashCode()   //same with the hash code
        {
            return HashCode.Combine(floatt);
        }

        public int ExpectedNDigitsOfPrecision()
        {
            Tryte exp = new Tryte(this.exponent);
            if (exp == 0)
            {
                exp = 1;
            }
            return (int)Math.Abs(Math.Ceiling(Math.Log10(MathT.Abs(exp) / Math.Pow(3, FloatT.N_TRITS_SIGNIFICAND))));
        }

        public static int ExpectedNDigitsOfPrecision(Tryte exp)
        {
            if (exp == 0)
            {
                exp = 1;
            }
            return (int)Math.Abs(Math.Ceiling(Math.Log10(MathT.Abs(exp) / Math.Pow(3, FloatT.N_TRITS_SIGNIFICAND))));
        }

        /// <summary>
        /// Constructor for FloatT struct, passing in a double as the value to be converted to Ternary
        /// </summary>
        /// <param name="value">The double value to be converted to Ternary</param>
        public FloatT(double value)
        {
            floatt = new Trit[N_TRITS_TOTAL_FLOAT];
            exponent = new Trit[N_TRITS_EXPONENT];
            significand = new Trit[N_TRITS_SIGNIFICAND];
            if (value == 0 || Math.Abs(value) < FloatT.Epsilon)
            {
                //doubleValue = 0;
                SetAllExponentTrits(-1);
                SetAllSignificandTrits(0);
            }
            else if (double.IsPositiveInfinity(value) || value > FloatT.MaxValue)  //special cases like infinity, neg infinity, NaN/undefined
            {
                //doubleValue = double.PositiveInfinity;
                SetAllExponentTrits(1);
                SetAllSignificandTrits(1);
            }
            else if (double.IsNegativeInfinity(value) || value < FloatT.MinValue)
            {
                //doubleValue = double.NegativeInfinity;
                SetAllExponentTrits(1);
                SetAllSignificandTrits(-1);
            }
            else if (double.IsNaN(value))
            {
                //doubleValue = double.NaN;
                SetAllExponentTrits(1);
                SetAllSignificandTrits(0);
            }
            else     //real, nonzero numbers are calculated here
            {
                //double doubleValue = value;
                int exponentValueBase3 = (int)Math.Ceiling((Math.Log(Math.Abs(value)) - Math.Log(0.5)) / Math.Log(3));  //calculate the exponent of 3 (magnitude)
                double significandValue = value / Math.Pow(3, exponentValueBase3);    //calculate the significand double value
                double remainder;   //remainder is how much is left after coverting to balanced ternary
                (significand, remainder) = ConvertDoubleToBalancedTritsWithRemainder(significandValue, N_TRITS_SIGNIFICAND); //significand in ternary floating point
                exponent = ConvertIntegerToBalancedTrits(exponentValueBase3, N_TRITS_EXPONENT);    //exponent in ternary integer
                floatt = new Trit[N_TRITS_TOTAL_FLOAT];
                for (byte i = 0; i < N_TRITS_EXPONENT; i++) //takes the exponent and significand (below) and puts them together into the whole balanced float, including chars +/-/0
                {
                    floatt[i] = exponent[i];
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)  // continues here with significand
                {
                    floatt[i] = significand[i - N_TRITS_EXPONENT];
                }
                //value = RoundToNearestDigitOfPrecision(value, new Tryte(exponent));

            }

            SyncPackedFromArrays();
        }

        public FloatT(Trit[] exp, Trit[] sig)
        {
            if (exp.Length == N_TRITS_EXPONENT && sig.Length == N_TRITS_SIGNIFICAND)
            {
                exponent = exp;
                significand = sig;
                floatt = new Trit[N_TRITS_TOTAL_FLOAT];
                for (byte i = 0; i < N_TRITS_EXPONENT; i++)
                {
                    floatt[i] = exponent[i];
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
                {
                    floatt[i] = significand[i - N_TRITS_EXPONENT];
                }

                SyncPackedFromArrays();
            }
            else
            {
                throw new ArgumentException("Exponent and/or significand length not the expected size, should be " + N_TRITS_EXPONENT + " trits and " + N_TRITS_SIGNIFICAND + " trits long, respectively.");
            }
        }

        public double ConvertToDouble()
        {
            double doubleValue;
            if (exponent.All(t => (sbyte)t.Value == 1))    //infinities and NaNs here
                {
                    if (significand.All(t => (sbyte)t.Value == 1))
                    {
                        doubleValue = double.PositiveInfinity;
                    }
                    else if (significand.All(t => (sbyte)t.Value == -1))
                    {
                        doubleValue = double.NegativeInfinity;
                    }
                    else if (significand.All(t => t.Value == 0))
                    {
                        doubleValue = double.NaN;
                    }
                    else
                    {
                        doubleValue = 0;
                    }
                }
                else if (significand.All(t => t.Value == 0) && exponent.All(t => (sbyte)t.Value == -1))    //zero case
                {
                    doubleValue = 0;
                }
                else      //real nonzero numbers calculated here
                {
                    double exponentValue = (double)Math.Pow(3, ConvertBalancedTritsToInteger(exponent));  //double for the exponent
                    double significandValue = ConvertBalancedTritsToDouble(significand);    //and double for the significand
                    doubleValue = significandValue * exponentValue; // multiply them together to get the final value
                    doubleValue = RoundToNearestDigitOfPrecision(doubleValue, new Tryte(exponent));  //round to nearest digit of precision
                }
            return doubleValue;
        }

        private bool IsZero()
        {
            for (int i = 0; i < N_TRITS_EXPONENT; i++)
            {
                if ((sbyte)exponent[i].Value != -1)
                {
                    return false;
                }
            }

            for (int i = 0; i < N_TRITS_SIGNIFICAND; i++)
            {
                if (significand[i].Value != TritVal.z)
                {
                    return false;
                }
            }

            return true;
        }

        private static Trit[] ShiftSignificandRight(Trit[] source, int shift)
        {
            if (shift <= 0)
            {
                var clone = new Trit[source.Length];
                Array.Copy(source, clone, source.Length);
                return clone;
            }

            if (shift >= source.Length)
            {
                return new Trit[source.Length];
            }

            var result = new Trit[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = i < shift ? new Trit(0) : source[i - shift];
            }
            return result;
        }

        /// <summary>
        /// Constructor for the FloatT taking an array of Trits to be converted to binary
        /// </summary>
        /// <param name="value">The array of Trits to pass into the constructor</param>
        public FloatT(Trit[] value)
        {
            if (value.Length == N_TRITS_TOTAL_FLOAT)
            {
                floatt = value;
                exponent = new Trit[N_TRITS_EXPONENT];
                significand = new Trit[N_TRITS_SIGNIFICAND];
                for (byte i = 0; i < N_TRITS_EXPONENT; i++)
                {
                    exponent[i] = value[i];
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
                {
                    significand[i - N_TRITS_EXPONENT] = value[i];
                }

                SyncPackedFromArrays();

                // if (exponent.All(t => (sbyte)t.Value == 1))    //infinities and NaNs here
                // {
                //     if (significand.All(t => (sbyte)t.Value == 1))
                //     {
                //         doubleValue = double.PositiveInfinity;
                //     }
                //     else if (significand.All(t => (sbyte)t.Value == -1))
                //     {
                //         doubleValue = double.NegativeInfinity;
                //     }
                //     else if (significand.All(t => t.Value == 0))
                //     {
                //         doubleValue = double.NaN;
                //     }
                //     else
                //     {
                //         doubleValue = 0;
                //     }
                // }
                // else if (significand.All(t => t.Value == 0) && exponent.All(t => (sbyte)t.Value == -1))    //zero case
                // {
                //     doubleValue = 0;
                // }
                // else      //real nonzero numbers calculated here
                // {
                //     double exponentValue = (double)Math.Pow(3, ConvertBalancedTritsToInteger(exponent));  //double for the exponent
                //     double significandValue = ConvertBalancedTritsToDouble(significand);    //and double for the significand
                //     doubleValue = significandValue * exponentValue; // multiply them together to get the final value
                //     doubleValue = RoundToNearestDigitOfPrecision(doubleValue, new Tryte(exponent));  //round to nearest digit of precision
                // }

            }
            else
            {
                throw new ArgumentException("Trit array passed to FloatT constructor method was not the expected size - should be " + N_TRITS_TOTAL_FLOAT + " trits in length", "value");
            }
        }

        /// <summary>
        /// A constructor passing in a string of +, - , and 0's and converting to trits and a double
        /// </summary>
        /// <param name="value">Character Array representing Balanced Ternary</param>
        public FloatT(string value)
        {
            if (value.Length == N_TRITS_TOTAL_FLOAT)
            {
                floatt = new Trit[N_TRITS_TOTAL_FLOAT];
                exponent = new Trit[N_TRITS_EXPONENT];
                significand = new Trit[N_TRITS_SIGNIFICAND];
                for (byte i = 0; i < N_TRITS_EXPONENT; i++)
                {
                    switch (value[i])
                    {
                        case '+':
                            exponent[i] = new Trit(1);
                            floatt[i] = new Trit(1);
                            break;
                        case '-':
                            exponent[i] = new Trit(-1);
                            floatt[i] = new Trit(-1);
                            break;
                        case '0':
                            exponent[i] = new Trit(0);
                            floatt[i] = new Trit(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid character in ternary char array passed to SetValue. Please stick to +, -, 0", "value");
                    }
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
                {
                    switch (value[i])
                    {
                        case '+':
                            significand[i - N_TRITS_EXPONENT] = new Trit(1);
                            floatt[i] = new Trit(1);
                            break;
                        case '-':
                            significand[i - N_TRITS_EXPONENT] = new Trit(-1);
                            floatt[i] = new Trit(-1);
                            break;
                        case '0':
                            significand[i - N_TRITS_EXPONENT] = new Trit(0);
                            floatt[i] = new Trit(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid character in ternary char array passed to SetValue. Please stick to +, -, 0", "value");
                    }
                }

                // if (exponent.All(t => (sbyte)t.Value == 1))    //infinities and NaNs here
                // {
                //     if (significand.All(t => (sbyte)t.Value == 1))
                //     {
                //         doubleValue = double.PositiveInfinity;
                //     }
                //     else if (significand.All(t => (sbyte)t.Value == -1))
                //     {
                //         doubleValue = double.NegativeInfinity;
                //     }
                //     else if (significand.All(t => (sbyte)t.Value == 0))
                //     {
                //         doubleValue = double.NaN;
                //     }
                //     else
                //     {
                //         doubleValue = 0;
                //     }
                // }
                // else if (significand.All(t => t.Value == 0) && exponent.All(t => (sbyte)t.Value == -1))    //zero case
                // {
                //     doubleValue = 0;
                // }
                // else      //real nonzero numbers calculated here
                // {
                //     double exponentValue = (double)Math.Pow(3, ConvertBalancedTritsToInteger(exponent));  //integer for the exponent
                //     double significandValue = ConvertBalancedTritsToDouble(significand);    //and double for the significand
                //     doubleValue = significandValue * exponentValue; // multiply them together to get the final value
                //     doubleValue = RoundToNearestDigitOfPrecision(doubleValue, new Tryte(exponent));
                // }

            }
            else
            {
                throw new ArgumentException("Char array passed to FloatT SetValue method was not the expected size - should be " + N_TRITS_TOTAL_FLOAT + " chars in length", "value");
            }
        }

        public string ConvertToStringRepresentation()
        {
            var temp = "";
            for (byte i = 0; i < N_TRITS_TOTAL_FLOAT; i++)
            {
                switch (floatt[i].Value)
                {
                    case TritVal.n:
                        temp += "-";
                        break;
                    case TritVal.p:
                        temp += "+";
                        break;
                    case TritVal.z:
                        temp += "0";
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }

        /// <summary>
        /// Rounds to the nearest digit of precision as calculated by the formula in ExpectedNDigitsOfPrecision.
        /// Takes a double and a Tryte for calculation.
        /// </summary>
        /// <param name="doubleValue">The double value passed in to be rounded</param>
        /// <param name="exp">The exponent to be used in the calculation</param>
        /// <returns>The double value of the rounded floating point number</returns>
        public static double RoundToNearestDigitOfPrecision(double doubleValue, Tryte exp)
        {
            var doubleString = doubleValue.ToString("E");
            var sigsplit = doubleString.Split('E');
            var significand = double.Parse(sigsplit[0]);
            significand = Math.Round(significand, ExpectedNDigitsOfPrecision(exp));
            return significand * Math.Pow(10, int.Parse(sigsplit[1]));
        }

        /// <summary>
        /// Balanced Ternary Comparison Operator - supposed to be used with the spaceship operator <=> but C# doesn't support custom operators at this time.
        /// Returns 0 if the two floats are equal, 1 if a is larger, and -1 if a float is smaller.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>A Trit with value of 1, -1, or 0 based on the comparison</returns>
        public static Trit COMPARET(FloatT a, FloatT b)
        {
            for (Tryte i = 0; i < (Tryte)N_TRITS_TOTAL_FLOAT; i++)
            {
                if (a.floatt[i] > b.floatt[i])
                {
                    return new Trit(1);
                }
                else if (a.floatt[i] < b.floatt[i])
                {
                    return new Trit(-1);
                }
            }
            return new Trit(0);
        }

        public FloatT INVERTSIG()
        {
            var trits = new Trit[N_TRITS_TOTAL_FLOAT];
            for (int i = 0; i < N_TRITS_EXPONENT; i++)
            {
                trits[i] = floatt[i];
            }
            for (int i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
            {
                trits[i] = !floatt[i];
            }
            return new FloatT(trits);
        }

        public FloatT MOD(FloatT divisor)
        {
            (_, var rem) = DIVREM(divisor);
            return rem;
        }

        public (FloatT, FloatT) DIVREM(FloatT divisor)
        {
            switch (COMPARET(divisor, 0).Value)
            {
                case TritVal.z:
                    throw new DivideByZeroException("Attempt to divide by zero in IntT division operation.");
                default:
                    if (COMPARET(MathT.Abs(this), MathT.Abs(divisor)).Value == TritVal.p)
                    {
                        var divd = MathT.Abs(this);
                        var divsr = MathT.Abs(divisor);
                        var quot = new FloatT(0);
                        var rem = new FloatT(divd.Value);

                        while (COMPARET(rem, divsr).Value != TritVal.n)
                        {
                            rem -= divsr;
                            rem = RoundToNearestDigitOfPrecision(rem, new Tryte(divisor.exponent));
                            quot += 1;
                        }
                        if (COMPARET(this, 0).Value == TritVal.n ^ COMPARET(divisor, 0).Value == TritVal.n)
                        {
                            quot = quot.INVERTSIG();
                        }
                        return (quot, rem);
                    }
                    else if (COMPARET(this, divisor).Value == TritVal.z)
                    {
                        return (new FloatT(1), new FloatT(0));
                    }
                    else
                    {
                        return (new FloatT(0), new FloatT(this.floatt));
                    }
            }
        }

        public FloatT DIV(FloatT divisor)
        {
            (var quotient, var remainder) = this.DIVREM(divisor);
            if (COMPARET(remainder, 0).Value != TritVal.z)
            {
                //remainder *= MathT.Pow(10, (FloatT)this.ExpectedNDigitsOfPrecision() + 3);
                do
                {
                    remainder *= 10;
                } while (remainder < (FloatT)(IntT.MaxValue / 100));
                IntT rem = (IntT)remainder;
                var remdiv = rem.DIV((IntT)(divisor * 10000));
                var remdivstr = remdiv.LongValue.ToString();
                remdivstr = remdivstr[0] == '-' ? "-0." + remdivstr[1..] : "0." + remdivstr;
                //remdivstr = "0." + remdivstr;
                FloatT remdivfloat = double.Parse(remdivstr);
                quotient += remdivfloat;
            }
            return quotient;
        }

        public FloatT MULT(FloatT f)
        {
            int expA = ConvertBalancedTritsToInteger(exponent);
            int expB = ConvertBalancedTritsToInteger(f.exponent);
            int newExp = expA + expB;

            var multipliedSigs = MultiplySignificands(this.significand, f.significand);
            Tryte normalizeCarry;
            (multipliedSigs, normalizeCarry) = NormalizeSignificand(multipliedSigs);
            newExp -= (int)normalizeCarry;

            if (newExp <= MaxExponentValue)
            {
                var exponentTrits = ConvertIntegerToBalancedTrits(newExp, N_TRITS_EXPONENT);
                return new FloatT(exponentTrits, multipliedSigs);
            }

            var sign = IntT.COMPARET(new IntT(multipliedSigs), 0).Value;
            return sign switch
            {
                TritVal.p => PositiveInfinity,
                TritVal.n => NegativeInfinity,
                _ => new FloatT(0),
            };
        }

        public FloatT SUB(FloatT f)
        {
            return this.ADD(f.INVERTSIG());
        }

        public FloatT ADD(FloatT f)
        {
            if (IsZero())
            {
                return new FloatT(f);
            }

            if (f.IsZero())
            {
                return new FloatT(this);
            }

            int expA = ConvertBalancedTritsToInteger(exponent);
            int expB = ConvertBalancedTritsToInteger(f.exponent);

            FloatT primary = this;
            FloatT secondary = f;
            int targetExp = expA;
            int diff = expA - expB;

            if (expB > expA)
            {
                primary = f;
                secondary = this;
                targetExp = expB;
                diff = expB - expA;
            }

            Trit[] shiftedSecondary = diff == 0
                ? secondary.significand
                : ShiftSignificandRight(secondary.significand, diff);

            (var addedSigs, var carry) = AddSignificands(primary.significand, shiftedSecondary);
            int newExp = targetExp + (int)carry.Value;

            Tryte normalizeCarry;
            (addedSigs, normalizeCarry) = NormalizeSignificand(addedSigs);
            newExp -= (int)normalizeCarry;

            if (newExp <= MaxExponentValue)
            {
                var exponentTrits = ConvertIntegerToBalancedTrits(newExp, N_TRITS_EXPONENT);
                return new FloatT(exponentTrits, addedSigs);
            }

            // Overflow path – mirror previous behaviour using sign of significand
            var sign = IntT.COMPARET(new IntT(addedSigs), 0).Value;
            return sign switch
            {
                TritVal.p => PositiveInfinity,
                TritVal.n => NegativeInfinity,
                _ => new FloatT(0),
            };
        }

        public override string ToString()
        {
            return FloatTString + " = " + DoubleValue;
        }

        private void SetAllExponentTrits(sbyte t)
        {
            exponent ??= new Trit[N_TRITS_EXPONENT];
            floatt ??= new Trit[N_TRITS_TOTAL_FLOAT];

            for (byte i = 0; i < N_TRITS_EXPONENT; i++)
            {
                floatt[i] = new Trit(t);
                exponent[i] = new Trit(t);
            }
            SyncPackedFromArrays();
        }

        private void SetAllSignificandTrits(sbyte t)
        {
            significand ??= new Trit[N_TRITS_SIGNIFICAND];
            floatt ??= new Trit[N_TRITS_TOTAL_FLOAT];

            for (int i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
            {
                this.floatt[i] = new Trit(t);
                this.significand[i - N_TRITS_EXPONENT] = new Trit(t);
            }
            SyncPackedFromArrays();
        }

        public void SetAllExponentTritsTo(Trit trit) //set all the exponent trits to a particular trit value
        {
            SetAllExponentTrits((sbyte)trit.Value);
        }

        public void SetAllSignificandTritsTo(Trit trit)      //set all the signficand trits to a particular trit value
        {
            SetAllSignificandTrits((sbyte)trit.Value);
        }

        public void SetValue(double value)
        {
            if (value == 0 || Math.Abs(value) < FloatT.Epsilon)
            {
                //doubleValue = 0;
                SetAllExponentTritsTo(-1);
                SetAllSignificandTritsTo(0);
            }
            else if (double.IsPositiveInfinity(value) || value > FloatT.MaxValue)  //special cases like infinity, neg infinity, NaN/undefined
            {
                //doubleValue = double.PositiveInfinity;
                SetAllExponentTritsTo(1);
                SetAllSignificandTritsTo(1);
            }
            else if (double.IsNegativeInfinity(value) || value < FloatT.MinValue)
            {
                //doubleValue = double.NegativeInfinity;
                SetAllExponentTritsTo(1);
                SetAllSignificandTritsTo(-1);
            }
            else if (double.IsNaN(value))
            {
                //doubleValue = double.NaN;
                SetAllExponentTritsTo(1);
                SetAllSignificandTritsTo(0);
            }
            else     //real, nonzero numbers are calculated here
            {
                double doubleValue = RoundToNearestDigitOfPrecision(value, new Tryte(exponent));
                int exponentValueBase3 = (int)Math.Ceiling((Math.Log(Math.Abs(doubleValue)) - Math.Log(0.5)) / Math.Log(3));  //calculate the exponent of 3 (magnitude)
                double significandValue = doubleValue / Math.Pow(3, exponentValueBase3);    //calculate the significand double value
                double remainder;   //remainder is how much is left after coverting to balanced ternary
                (significand, remainder) = ConvertDoubleToBalancedTritsWithRemainder(significandValue, N_TRITS_SIGNIFICAND); //significand in ternary floating point
                exponent = ConvertIntegerToBalancedTrits(exponentValueBase3, N_TRITS_EXPONENT);    //exponent in ternary integer
                for (byte i = 0; i < N_TRITS_EXPONENT; i++) //takes the exponent and significand (below) and puts them together into the whole balanced float, including chars +/-/0
                {
                    floatt[i] = exponent[i];
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)  // continues here with significand
                {
                    floatt[i] = significand[i - N_TRITS_EXPONENT];
                }
            }

            SyncPackedFromArrays();
        }

        public void SetValue(Trit[] value)   //separates the whole balanced float into exponent and significand...
        {
            if (value.Length == N_TRITS_TOTAL_FLOAT)
            {
                floatt = value;
                for (byte i = 0; i < N_TRITS_EXPONENT; i++)
                {
                    exponent[i] = value[i];
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
                {
                    significand[i - N_TRITS_EXPONENT] = value[i];
                }
                //CreateDoubleValueIncludingSpecialCases();   //...and converts them into a double including infinities and NaNs
                SyncPackedFromArrays();
            }
            else
            {
                throw new ArgumentException("Trit array passed to FloatT SetValue method was not the expected size - should be " + N_TRITS_TOTAL_FLOAT + " trits in length", "value");
            }
        }

        public void SetValue(string value)  // in case we're dealing with strings of +/-/0's this sets the trits to their appropriate values and...
        {
            if (value.Length == N_TRITS_TOTAL_FLOAT)
            {
                for (byte i = 0; i < N_TRITS_EXPONENT; i++)
                {
                    switch (value[i])
                    {
                        case '+':
                            exponent[i] = new Trit(1);
                            floatt[i] = new Trit(1);
                            break;
                        case '-':
                            exponent[i] = new Trit(-1);
                            floatt[i] = new Trit(-1);
                            break;
                        case '0':
                            exponent[i] = new Trit(0);
                            floatt[i] = new Trit(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid character in ternary char array passed to SetValue.", "value");
                    }
                }
                for (byte i = N_TRITS_EXPONENT; i < N_TRITS_TOTAL_FLOAT; i++)
                {
                    switch (value[i])
                    {
                        case '+':
                            significand[i - N_TRITS_EXPONENT] = new Trit(1);
                            floatt[i] = new Trit(1);
                            break;
                        case '-':
                            significand[i - N_TRITS_EXPONENT] = new Trit(-1);
                            floatt[i] = new Trit(-1);
                            break;
                        case '0':
                            significand[i - N_TRITS_EXPONENT] = new Trit(0);
                            floatt[i] = new Trit(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid character in ternary char array passed to SetValue.", "value");
                    }
                }
                //CreateDoubleValueIncludingSpecialCases();   //...and does the same conversion to double with special cases
                SyncPackedFromArrays();
            }
            else
            {
                throw new ArgumentException("Char array passed to FloatT SetValue method was not the expected size - should be " + N_TRITS_TOTAL_FLOAT + " chars in length", "value");
            }
        }

        /// <summary>
        /// Creates a double value based on the exponent and significand and puts it in the doubleValue struct member.
        /// Takes into account special values like infinities and NaNs, and does so without needing to know how many
        /// total trits are involved - uses LINQ's All function.
        /// </summary>
        // private void CreateDoubleValueIncludingSpecialCases()
        // {
        //     if (exponent.All(t => (sbyte)t.Value == 1))    //infinities and NaNs here
        //     {
        //         if (significand.All(t => (sbyte)t.Value == 1))
        //         {
        //             doubleValue = double.PositiveInfinity;
        //         }
        //         else if (significand.All(t => (sbyte)t.Value == -1))
        //         {
        //             doubleValue = double.NegativeInfinity;
        //         }
        //         else if (significand.All(t => (sbyte)t.Value == 0))
        //         {
        //             doubleValue = double.NaN;
        //         }
        //     }
        //     else if (significand.All(t => t.Value == 0) && exponent.All(t => (sbyte)t.Value == -1))    //zero case
        //     {
        //         doubleValue = 0;
        //     }
        //     else      //real nonzero numbers calculated here
        //     {
        //         double exponentValue = (double)Math.Pow(3, ConvertBalancedTritsToInteger(exponent));  //double for the exponent (incluing negative exponents)
        //         double significandValue = ConvertBalancedTritsToDouble(significand);    //and double for the significand
        //         doubleValue = significandValue * exponentValue; // multiply them together to get the final value
        //         doubleValue = RoundToNearestDigitOfPrecision(doubleValue, new Tryte(exponent));
        //     }
        // }


        public static Trit[] ConvertIntegerToBalancedTrits(int integerValue, byte nTrits)
        {
            Trit[] trits = new Trit[nTrits];
            int workValue = Math.Abs(integerValue);     //easier to work with a positive number...
            if (workValue <= (int)((Math.Pow(3, nTrits) - 1) / 2))  //make sure the value passed in is within the nTrits passed maximum value
            {
                byte i = 0;
                while (workValue != 0)      //easier to start with index 0 - greater numbers on the right
                {
                    switch (workValue % 3)
                    {
                        case 0:
                            trits[i] = new Trit(0);
                            break;
                        case 1:
                            trits[i] = new Trit(1);
                            break;
                        case 2:
                            trits[i] = new Trit(-1);
                            break;
                    }
                    workValue = (workValue + 1) / 3;
                    i++;
                }
                for (byte j = i; j < nTrits; j++)
                {
                    trits[j] = new Trit(0);   //...add zeros to fill in to make the length match the nTrits passed in
                }
                if (integerValue < 0)   //...and invert if it was negative
                {
                    for (byte n = 0; n < nTrits; n++)
                    {
                        trits[n] = !trits[n];
                    }
                }
                Array.Reverse(trits);    //...and reverse the trit order so greater numbers are on the left
                return trits;
            }
            else
            {
                throw new ArgumentException("The integer value passed to ConvertIntegerToBalancedTrits is too large for the number of trits passed.", "integerValue");
            }
        }

        public static int ConvertBalancedTritsToInteger(Trit[] trits)
        {
            int sum = 0;
            short exponent = (short)(trits.Length - 1);
            foreach (var trit in trits)
            {
                sum += (int)((sbyte)trit.Value * Math.Pow(3, exponent));
                exponent--;
            }
            return sum;
        }

        public static double ConvertBalancedTritsToDouble(Trit[] trits)   //same idea here, except we're counting exponents from -1 to the smallest exponent allowed by the implementation
        {
            double sum = 0;
            int exponent = -1;
            foreach (var trit in trits)
            {
                sum += (sbyte)trit.Value * Math.Pow(3, exponent);  //exponents are negative, so it's like doing 1/3^n and adding them up
                exponent--;
            }
            return sum;
        }

        public static (Trit[], double) ConvertDoubleToBalancedTritsWithRemainder(double doubleValue, int nTrits) //converts double to Trits[]
        {
            var trits = new Trit[nTrits];
            var sum = doubleValue;  //start with the sum total
            for (int exponent = 1; exponent <= nTrits; exponent++)  //start with 1/3
            {
                var place = Math.Pow(3, -exponent);
                var diffNegative = Math.Abs(sum - -place);
                var diffPositive = Math.Abs(sum - +place);
                if ((diffNegative < diffPositive) && diffNegative < Math.Abs(sum))  //is the negative difference smaller? smaller than if it's a zero?
                {
                    sum -= -place;   //subtract negative from sum if negative was smaller
                    trits[exponent - 1] = new Trit(-1);
                }
                else if ((diffPositive < diffNegative) && diffPositive < Math.Abs(sum)) //is the positive difference smaller? smaller than if it's a zero?
                {
                    sum -= +place;    //subtract positive from sum if positive was smaller
                    trits[exponent - 1] = new Trit(1);
                }
                else
                {
                    trits[exponent - 1] = new Trit(0);    //if zero was smaller, fill in a zero
                }
            }
            return (trits, sum); //return the array of trits, as well as the remainder (how much of the double value was not considered by the ternary conversion)
        }

        public static Trit[] TritShiftRight(Trit[] trits, Tryte nTrits)
        {
            if (nTrits <= trits.Length)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[trits.Length];
                    for (Tryte i = 0; i < (Tryte)trits.Length; i++)
                    {
                        temp[i] = i < nTrits ? new Trit(0) : trits[i - nTrits];
                    }
                    return temp;
                }
                else
                {
                    return trits;
                }
            }
            else
            {
                throw new ArgumentException("Trit Shift Right passed nTrits value larger than the size of the passed Trit[] array", nameof(nTrits));
            }
        }

        public static Trit[] TritShiftLeft(Trit[] trits, Tryte nTrits)
        {
            if (nTrits <= trits.Length)
            {
                if (nTrits > 0)
                {
                    var temp = new Trit[trits.Length];
                    for (Tryte i = 0; i < (Tryte)trits.Length; i++)
                    {
                        temp[i] = i >= trits.Length - nTrits ? new Trit(0) : trits[i + nTrits];
                    }
                    return temp;
                }
                else
                {
                    return trits;
                }
            }
            else
            {
                throw new ArgumentException("Trit Shift Left passed nTrits value larger than the size of hte passed Trit[] array", nameof(nTrits));
            }
        }


        public static (Trit[], Tryte) NormalizeSignificand(Trit[] trits)
        {
            Trit[] temp = new Trit[trits.Length];
            trits.CopyTo(temp, 0);
            Tryte count = 0;
            for (Tryte n = 0; n < trits.Length; n++)
            {
                if (trits[n].Value == TritVal.z)
                {
                    temp = TritShiftLeft(temp, 1);
                    count++;
                }
                else
                {
                    break;
                }
            }
            Trit[] normalized = new Trit[N_TRITS_SIGNIFICAND];
            Array.Copy(temp, 0, normalized, 0, N_TRITS_SIGNIFICAND);
            return (normalized, count);
        }

        public static Trit[] MultiplySignificands(Trit[] sigA, Trit[] sigB)
        {
            Trit[] product = new Trit[N_TRITS_SIGNIFICAND * 2];
            for (Tryte n = 0; n < (Tryte)N_TRITS_SIGNIFICAND * 2; n++)
            {
                product[n] = new Trit(0);
            }
            //Tryte nShiftsLeft = 0;
            for (Tryte i = N_TRITS_SIGNIFICAND - 1; i >= 0; i--)
            {
                Trit[] tempB = new Trit[N_TRITS_SIGNIFICAND * 2];
                switch (sigA[i].Value)
                {
                    case TritVal.n:
                        for (Tryte j = N_TRITS_SIGNIFICAND; j < (Tryte)N_TRITS_SIGNIFICAND * 2; j++)
                        {
                            tempB[j] = ~sigB[j - N_TRITS_SIGNIFICAND];
                        }
                        break;
                    case TritVal.p:
                        sigB.CopyTo(tempB, N_TRITS_SIGNIFICAND);
                        break;
                    default:
                        for (Tryte j = N_TRITS_SIGNIFICAND; j < (Tryte)N_TRITS_SIGNIFICAND * 2; j++)
                        {
                            tempB[j] = new Trit(0);
                        }
                        break;
                }
                Trit carry;
                tempB = TritShiftLeft(tempB, N_TRITS_SIGNIFICAND - i - 1);
                (product, carry) = AddSignificands(product, tempB);
            }
            return product;
        }

        public static string ConvertTritArrayToString(Trit[] tritArray)
        {
            string temp = "";
            foreach (Trit trit in tritArray) temp += trit.ConvertToChar();
            return temp;
        }

        public static (Trit[], Trit) AddSignificands(Trit[] sigA, Trit[] sigB)
        {
            Trit[] temp = new Trit[sigA.Length];
            Trit carry = 0;
            for (Tryte i = sigA.Length - 1; i >= 0; i--)
            {
                Tryte a = (sbyte)sigA[i].Value;
                Tryte b = (sbyte)sigB[i].Value;
                Tryte c = a + b + carry;
                Tryte sum;
                if (c == 2)
                {
                    carry = 1;
                    sum = -1;
                }
                else if (c == -2)
                {
                    carry = -1;
                    sum = 1;
                }
                else if (c == 3)
                {
                    carry = 1;
                    sum = 0;
                }
                else if (c == -3)
                {
                    carry = -1;
                    sum = 0;
                }
                else
                {
                    carry = 0;
                    sum = c;
                }
                temp[i] = new Trit(sum);
                if (i == 0 && carry == 1 || i == 0 && carry == -1)
                {
                    temp = TritShiftRight(temp, 1);
                    temp[0] = carry;
                    return (temp, 1);
                }
            }
            return (temp, 0);
        }

    }

    /// <summary>
    /// The CharT struct is a general purpose ternary character conforming to the UTF-16 standard used in C#. It uses an array of Trits and a char 
    /// for values it holds. The Trits represent an integer value which in turn represents a UTF-16 character.
    /// </summary>
    public struct CharT
    {
        /// <summary>
        /// Number of trits the CharT uses
        /// </summary>
        public static byte N_TRITS_PER_CHAR = 12;

        /// <summary>
        /// The trit array that represents the value the CharT holds
        /// </summary>
        private Trit[] chart = new Trit[N_TRITS_PER_CHAR];

        /// <summary>
        /// The UTF-16 character the value represents in the CharT
        /// </summary>
        private char charValue;

        public Trit[] Value { get => chart; set => SetValue(value); }
        public string CharTString { get => ConvertToStringRepresentation(); }
        public char CharValue { get => charValue; set => SetValue(value); }

        public const TritVal Equal = TritVal.z;
        public const TritVal Smaller = TritVal.n;
        public const TritVal Larger = TritVal.p;

        public static bool operator ==(CharT left, CharT right) => COMPARET(left, right).Value == Equal;
        public static bool operator !=(CharT left, CharT right) => COMPARET(left, right).Value != Equal;
        public static bool operator ==(CharT left, char right) => left.charValue == right;
        public static bool operator !=(CharT left, char right) => left.charValue != right;
        public static implicit operator char(CharT chart) => chart.CharValue;
        public static implicit operator CharT(char @char) => new CharT(@char);
        public static explicit operator string(CharT chart) => chart.CharTString;
        public static explicit operator CharT(string str) => new CharT(str);

        /// <summary>
        /// Constructor for the CharT struct, taking an array of Trits
        /// </summary>
        /// <param name="value">The value passed in as an array of Trits</param>
        public CharT(Trit[] value)
        {
            if (value.Length == N_TRITS_PER_CHAR)
            {
                chart = value;
                charValue = (char)ConvertBalancedTritsToInteger(value);
            }
            else if (value.Length < N_TRITS_PER_CHAR)
            {
                chart = new Trit[N_TRITS_PER_CHAR];
                for (int i = 0; i < N_TRITS_PER_CHAR - value.Length; i++)
                {
                    chart[i] = new Trit(0);
                }
                for (int i = N_TRITS_PER_CHAR - value.Length; i < N_TRITS_PER_CHAR; i++)
                {
                    chart[i] = value[i - (N_TRITS_PER_CHAR - value.Length)];
                }
                charValue = (char)ConvertBalancedTritsToInteger(chart);
            }
            else
            {
                throw new ArgumentException("Trit array passed to CharT SetValue method was not the expected size - should be " + N_TRITS_PER_CHAR + " trits in length or less", "value");
            }
        }

        /// <summary>
        /// Constructor for the CharT struct, taking a string of chars (0, +, -)
        /// </summary>
        /// <param name="value">The value passed in as a string of chars (0, +, -)</param>
        public CharT(string value)
        {
            if (value.Length == N_TRITS_PER_CHAR)
            {
                chart = new Trit[N_TRITS_PER_CHAR];
                for (int i = 0; i < N_TRITS_PER_CHAR; i++)
                {
                    chart[i] = value[i] switch
                    {
                        '+' => new Trit(1),
                        '-' => new Trit(-1),
                        '0' => new Trit(0),
                        _ => throw new ArgumentException("Invalid character encountered in a balanced ternary char array. Please stick to +, -, 0's", "value"),
                    };
                }
                charValue = (char)ConvertBalancedTritsToInteger(chart);
            }
            else
            {
                throw new ArgumentException("Char array passed to CharT SetValue method was not the expected size - should be " + N_TRITS_PER_CHAR + " chars in length", "value");
            }
        }

        /// <summary>
        /// Constructor for the CharT struct, taking a long binary integer
        /// </summary>
        /// <param name="value">The value passed in, as a char</param>
        public CharT(char value)
        {
            charValue = value;
            chart = ConvertIntegerToBalancedTrits((int)value);
        }

        public void SetValue(char value)
        {
            charValue = value;
            chart = ConvertIntegerToBalancedTrits((int)value);
        }

        public void SetValue(Trit[] value)
        {
            if (value.Length == N_TRITS_PER_CHAR)
            {
                chart = value;
                charValue = (char)ConvertBalancedTritsToInteger(value);
            }
            else if (value.Length < N_TRITS_PER_CHAR)
            {
                chart = new Trit[N_TRITS_PER_CHAR];
                for (int i = 0; i < N_TRITS_PER_CHAR - value.Length; i++)
                {
                    chart[i] = new Trit(0);
                }
                for (int i = N_TRITS_PER_CHAR - value.Length; i < N_TRITS_PER_CHAR; i++)
                {
                    chart[i] = value[i - (N_TRITS_PER_CHAR - value.Length)];
                }
                charValue = (char)ConvertBalancedTritsToInteger(chart);
            }
            else
            {
                throw new ArgumentException("Trit array passed to CharT SetValue method was not the expected size - should be " + N_TRITS_PER_CHAR + " trits in length or less", "value");
            }
        }

        public override string ToString()
        {
            return CharTString + " which signifies the character " + charValue;
        }

        public string ConvertToStringRepresentation()
        {
            var temp = "";
            for (int i = 0; i < N_TRITS_PER_CHAR; i++)
            {
                switch (this.chart[i].Value)
                {
                    case TritVal.n:
                        temp += "-";
                        break;
                    case TritVal.p:
                        temp += "+";
                        break;
                    case TritVal.z:
                        temp += "0";
                        break;
                }
            }
            return temp;
        }

        public static int ConvertBalancedTritsToInteger(Trit[] trits)
        {
            int sum = 0;
            short exponent = (short)(N_TRITS_PER_CHAR - 1);
            foreach (var trit in trits)
            {
                sum += (int)((sbyte)trit.Value * Math.Pow(3, exponent));   //exponent increases as the invisible index increases, and adds to the sum if the trit is -1 or 1
                exponent--;
            }
            return sum;
        }

        public static Trit[] ConvertIntegerToBalancedTrits(int intValue)
        {
            Trit[] trits = new Trit[N_TRITS_PER_CHAR];
            int workValue = Math.Abs(intValue);     //easier to work with a positive number...
            byte i = 0;     //easier to start with index 0 - greater numbers on the right
            while (workValue != 0)
            {
                switch (workValue % 3)
                {
                    case 0:
                        trits[i] = new Trit(0);
                        break;
                    case 1:
                        trits[i] = new Trit(1);
                        break;
                    case 2:
                        trits[i] = new Trit(-1);
                        break;
                }
                workValue = (workValue + 1) / 3;
                i++;
            }
            for (int j = i; j < N_TRITS_PER_CHAR; j++)
            {
                trits[j] = new Trit(0);   //...add zeros to fill in to make the length match the N_TRITS_PER_INT static value
            }
            if (intValue < 0)   //...and invert if it was negative
            {
                for (int n = 0; n < N_TRITS_PER_CHAR; n++)
                {
                    trits[n] = !trits[n];
                }
            }
            Array.Reverse(trits);    //...and reverse the trit order so greater numbers are on the left
            return trits;
        }

        public static Trit COMPARET(CharT a, CharT b)
        {
            for (byte i = 0; i < N_TRITS_PER_CHAR; i++)
            {
                if (a.chart[i] > b.chart[i])
                {
                    return new Trit(1);
                }
                else if (a.chart[i] < b.chart[i])
                {
                    return new Trit(-1);
                }
            }
            return new Trit(0);
        }

        public override bool Equals(object obj)
        {
            return obj is CharT charT &&
                   EqualityComparer<Trit[]>.Default.Equals(chart, charT.chart);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    // FloatT/IntT/Tryte versions of most Math functions, also including a log3, ILogT, and trit increment/decrement
    public static class MathT
    {
        public static FloatT PI = Math.PI;

        public static FloatT E = Math.E;

        public static IntT Pow(IntT a, IntT b)
        {
            var zero = new IntT(0);
            var pow = new IntT(1);
            switch (IntT.COMPARET(b, zero).Value)
            {
                case TritVal.z:
                    return pow;
                case TritVal.p:
                    {
                        for (int n = 0; n < b.LongValue; n++)
                        {
                            pow *= a;
                        }
                        return pow;
                    }

                default:
                    return zero;
            }
        }

        public static FloatT Pow(FloatT a, FloatT b)
        {
            return new FloatT(Math.Pow(a.DoubleValue, b.DoubleValue));
        }

        public static FloatT Sqrt(FloatT f)
        {
            return new FloatT(Math.Sqrt(f.DoubleValue));
        }

        public static FloatT Sin(FloatT f)
        {
            return new FloatT(Math.Sin(f.DoubleValue));
        }

        public static FloatT Cos(FloatT f)
        {
            return new FloatT(Math.Cos(f.DoubleValue));
        }

        public static FloatT Tan(FloatT f)
        {
            return new FloatT(Math.Tan(f.DoubleValue));
        }

        public static FloatT Sinh(FloatT f)
        {
            return new FloatT(Math.Sinh(f.DoubleValue));
        }

        public static FloatT Cosh(FloatT f)
        {
            return new FloatT(Math.Cosh(f.DoubleValue));
        }

        public static FloatT Tanh(FloatT f)
        {
            return new FloatT(Math.Tanh(f.DoubleValue));
        }

        public static FloatT Asin(FloatT f)
        {
            return new FloatT(Math.Sin(f.DoubleValue));
        }

        public static FloatT Acos(FloatT f)
        {
            return new FloatT(Math.Cos(f.DoubleValue));
        }

        public static FloatT Atan(FloatT f)
        {
            return new FloatT(Math.Tan(f.DoubleValue));
        }

        public static FloatT Asinh(FloatT f)
        {
            return new FloatT(Math.Sinh(f.DoubleValue));
        }

        public static FloatT Acosh(FloatT f)
        {
            return new FloatT(Math.Cosh(f.DoubleValue));
        }

        public static FloatT Atanh(FloatT f)
        {
            return new FloatT(Math.Atanh(f.DoubleValue));
        }

        public static FloatT Log(FloatT f)
        {
            return new FloatT(Math.Log(f.DoubleValue));
        }

        public static FloatT Log10(FloatT f)
        {
            return new FloatT(Math.Log10(f.DoubleValue));
        }

        public static FloatT Log2(FloatT f)
        {
            return new FloatT(Math.Log2(f.DoubleValue));
        }

        public static FloatT Log3(FloatT f)
        {
            return new FloatT(Math.Log(f.DoubleValue) / Math.Log(3));
        }

        public static FloatT Floor(FloatT f)
        {
            return new FloatT(Math.Floor(f.DoubleValue));
        }

        public static FloatT Ceiling(FloatT f)
        {
            return new FloatT(Math.Ceiling(f.DoubleValue));
        }

        public static FloatT Round(FloatT f, int nDigits = 0)
        {
            return new FloatT(Math.Round(f.DoubleValue, nDigits));
        }

        public static IntT Truncate(FloatT f)
        {
            return new IntT((long)Math.Truncate(f.DoubleValue));
        }

        /// <summary>
        /// Calculates the integer (IntT) equivalent of the ternary logarithm (base 3) of a FloatT
        /// </summary>
        /// <param name="f"></param>
        /// <returns>IntT base 3 logarithm of f</returns>
        public static IntT ILogT(FloatT f)
        {
            return new IntT((long)Math.Floor(Math.Log(f.DoubleValue) / Math.Log(3)));
        }

        public static FloatT Atan2(FloatT a, FloatT b)
        {
            return new FloatT(Math.Atan2(a.DoubleValue, b.DoubleValue));
        }

        public static FloatT Abs(FloatT f)
        {
            for (int i = FloatT.N_TRITS_EXPONENT; i < FloatT.N_TRITS_TOTAL_FLOAT; i++)
            {
                switch (f.Value[i].Value)
                {
                    case TritVal.n:
                        return f.INVERTSIG();
                    case TritVal.p:
                        return f;
                }
            }
            return f;
        }

        public static IntT Abs(IntT intT)
        {
            for (int i = 0; i < IntT.N_TRITS_PER_INT; i++)
            {
                switch (intT.Value[i].Value)
                {
                    case TritVal.n:
                        return intT.INVERT();
                    case TritVal.p:
                        return intT;
                }
            }
            return intT;
        }

        public static Tryte Abs(Tryte tryte)
        {
            for (int i = 0; i < Tryte.N_TRITS_PER_TRYTE; i++)
            {
                switch (tryte.Value[i].Value)
                {
                    case TritVal.n:
                        return tryte.INVERT();
                    case TritVal.p:
                        return tryte;
                }
            }
            return tryte;
        }

        public static FloatT Cbrt(FloatT f)
        {
            return new FloatT(Math.Cbrt(f.DoubleValue));
        }

        /// <summary>
        /// Increments a FloatT by one trit, returning the result
        /// </summary>
        /// <param name="f"></param>
        /// <returns>A new, incremented FloatT</returns>
        public static FloatT TritIncrement(FloatT f)
        {
            sbyte carry = 1;
            Trit[] trits = new Trit[FloatT.N_TRITS_TOTAL_FLOAT];
            for (int i = FloatT.N_TRITS_TOTAL_FLOAT - 1; i >= 0; i--)
            {
                sbyte n = (sbyte)(f.Value[i].Value + carry);
                switch (n)
                {
                    case 0:
                        trits[i] = new Trit(0);
                        carry = 0;
                        break;
                    case 1:
                        trits[i] = new Trit(1);
                        carry = 0;
                        break;
                    case 2:
                        trits[i] = new Trit(-1);
                        carry = 1;
                        break;
                    case -1:
                        trits[i] = new Trit(-1);
                        carry = 0;
                        break;
                    case -2:
                        trits[i] = new Trit(1);
                        carry = -1;
                        break;
                }
            }
            return new FloatT(trits);
        }

        /// <summary>
        /// Decrements a FloatT by one trit, returning the result
        /// </summary>
        /// <param name="f"></param>
        /// <returns>A new, incremented FloatT</returns>
        public static FloatT TritDecrement(FloatT f)
        {
            sbyte carry = -1;
            Trit[] trits = new Trit[FloatT.N_TRITS_TOTAL_FLOAT];
            for (int i = FloatT.N_TRITS_TOTAL_FLOAT - 1; i >= 0; i--)
            {
                sbyte n = (sbyte)(f.Value[i].Value + carry);
                switch (n)
                {
                    case 0:
                        trits[i] = 0;
                        carry = 0;
                        break;
                    case 1:
                        trits[i] = new Trit(1);
                        carry = 0;
                        break;
                    case 2:
                        trits[i] = new Trit(-1);
                        carry = 1;
                        break;
                    case -1:
                        trits[i] = new Trit(-1);
                        carry = 0;
                        break;
                    case -2:
                        trits[i] = new Trit(1);
                        carry = -1;
                        break;
                }
            }
            return new FloatT(trits);
        }

        public static FloatT FusedMultiplyAdd(FloatT a, FloatT b, FloatT c)
        {
            return new FloatT(Math.FusedMultiplyAdd(a.DoubleValue, b.DoubleValue, c.DoubleValue));
        }

    }

}
