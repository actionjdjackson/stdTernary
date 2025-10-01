using System;
using stdTernary;

namespace stdTernary.Examples;

/// <summary>
/// Example program demonstrating the TernaryConverter library usage.
/// </summary>
public class ConverterExamples
{
    public static void RunExamples()
    {
        Console.WriteLine("=== Ternary Type Conversion Examples ===\n");

        IntTExamples();
        FloatTExamples();
        TryteExamples();
        TritExamples();
        RoundTripExamples();
    }

    static void IntTExamples()
    {
        Console.WriteLine("--- IntT Conversions ---");
        
        // Create IntT from various types
        IntT value = TernaryConverter.IntTFromInt32(12345);
        Console.WriteLine($"Created IntT from int: {value}");
        
        // Convert IntT to various types
        int i32 = TernaryConverter.IntTToInt32(value);
        short i16 = TernaryConverter.IntTToInt16(value);
        byte ui8 = TernaryConverter.IntTToUInt8(new IntT(200));
        decimal dec = TernaryConverter.IntTToDecimal(value);
        
        Console.WriteLine($"  To int32: {i32}");
        Console.WriteLine($"  To int16: {i16}");
        Console.WriteLine($"  To byte (200): {ui8}");
        Console.WriteLine($"  To decimal: {dec}");
        
        // Demonstrate overflow handling
        try
        {
            IntT largeValue = new IntT(100000);
            byte overflow = TernaryConverter.IntTToUInt8(largeValue);
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"  Overflow caught: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    static void FloatTExamples()
    {
        Console.WriteLine("--- FloatT Conversions ---");
        
        // Create FloatT from various types
        FloatT value = TernaryConverter.FloatTFromFloat(123.456f);
        Console.WriteLine($"Created FloatT from float: {value}");
        Console.WriteLine($"  As double: {value.ToDouble()}");
        
        // Convert FloatT to various types
        float f = TernaryConverter.FloatTToFloat(value);
        decimal dec = TernaryConverter.FloatTToDecimal(value);
        
        Console.WriteLine($"  To float: {f}");
        Console.WriteLine($"  To decimal: {dec}");
        
        // Demonstrate precision
        FloatT fromDecimal = TernaryConverter.FloatTFromDecimal(999.999m);
        Console.WriteLine($"From decimal 999.999: {fromDecimal.ToDouble()}");
        
        Console.WriteLine();
    }

    static void TryteExamples()
    {
        Console.WriteLine("--- Tryte Conversions ---");
        
        // Show current configuration
        Console.WriteLine($"Current Tryte size: {Tryte.N_TRITS_PER_TRYTE} trits");
        Console.WriteLine($"Tryte range: {Tryte.MinValue} to {Tryte.MaxValue}");
        
        // Create Tryte from various types
        Tryte value = TernaryConverter.TryteFromUInt8(200);
        Console.WriteLine($"Created Tryte from byte 200: {value}");
        
        // Convert Tryte to various types
        byte ui8 = TernaryConverter.TryteToUInt8(value);
        ushort ui16 = TernaryConverter.TryteToUInt16(value);
        uint ui32 = TernaryConverter.TryteToUInt32(value);
        
        Console.WriteLine($"  To byte: {ui8}");
        Console.WriteLine($"  To ushort: {ui16}");
        Console.WriteLine($"  To uint: {ui32}");
        
        // Demonstrate signed conversion
        Tryte negative = TernaryConverter.TryteFromInt8(-100);
        Console.WriteLine($"Negative Tryte from sbyte -100: {negative}");
        Console.WriteLine($"  Back to sbyte: {TernaryConverter.TryteToInt8(negative)}");
        
        Console.WriteLine();
    }

    static void TritExamples()
    {
        Console.WriteLine("--- Trit Conversions ---");
        
        // Trit to boolean
        Trit positive = new Trit(1);
        Trit zero = new Trit(0);
        Trit negative = new Trit(-1);
        
        Console.WriteLine($"Trit(1) to bool: {TernaryConverter.ToBoolean(positive)}");
        Console.WriteLine($"Trit(0) to bool: {TernaryConverter.ToBoolean(zero)}");
        Console.WriteLine($"Trit(-1) to bool: {TernaryConverter.ToBoolean(negative)}");
        
        // Boolean to Trit
        Trit fromTrue = TernaryConverter.FromBoolean(true);
        Trit fromFalse = TernaryConverter.FromBoolean(false);
        
        Console.WriteLine($"bool(true) to Trit: {(int)fromTrue}");
        Console.WriteLine($"bool(false) to Trit: {(int)fromFalse}");
        
        Console.WriteLine();
    }

    static void RoundTripExamples()
    {
        Console.WriteLine("--- Round-Trip Conversions ---");
        
        // IntT round-trip
        int originalInt = 54321;
        IntT intT = TernaryConverter.IntTFromInt32(originalInt);
        int resultInt = TernaryConverter.IntTToInt32(intT);
        Console.WriteLine($"int round-trip: {originalInt} -> IntT -> {resultInt} (match: {originalInt == resultInt})");
        
        // FloatT round-trip
        float originalFloat = 3.14159f;
        FloatT floatT = TernaryConverter.FloatTFromFloat(originalFloat);
        float resultFloat = TernaryConverter.FloatTToFloat(floatT);
        Console.WriteLine($"float round-trip: {originalFloat} -> FloatT -> {resultFloat} (match: {Math.Abs(originalFloat - resultFloat) < 0.0001})");
        
        // Tryte round-trip
        byte originalByte = 250;
        Tryte tryte = TernaryConverter.TryteFromUInt8(originalByte);
        byte resultByte = TernaryConverter.TryteToUInt8(tryte);
        Console.WriteLine($"byte round-trip: {originalByte} -> Tryte -> {resultByte} (match: {originalByte == resultByte})");
        
        // Cross-type conversion
        Console.WriteLine("\nCross-type conversions:");
        IntT cross1 = TernaryConverter.IntTFromDecimal(999.99m);
        Console.WriteLine($"decimal 999.99 -> IntT -> long: {cross1.ToInt64()}");
        
        FloatT cross2 = TernaryConverter.FloatTFromFloat(2.718f);
        decimal cross2Result = TernaryConverter.FloatTToDecimal(cross2);
        Console.WriteLine($"float 2.718 -> FloatT -> decimal: {cross2Result}");
        
        Console.WriteLine();
    }
}
