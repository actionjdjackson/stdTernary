# Ternary Type Conversion Library

This library provides comprehensive type conversion functionality between balanced ternary types (Trit, Tryte, IntT, FloatT) and common .NET binary types.

## Overview

The `TernaryConverter` static class provides methods to convert between:

- **IntT** (32-trit integer) ↔ int, uint, short, ushort, byte, sbyte, long, ulong, decimal
- **FloatT** (ternary floating-point) ↔ float, double, decimal
- **Tryte** (configurable-size ternary) ↔ byte, sbyte, short, ushort, uint, ulong, decimal
- **Trit** (single ternary digit) ↔ bool

## IntT Conversions

### From IntT to Binary Types

```csharp
IntT value = new IntT(12345);

int i32 = TernaryConverter.IntTToInt32(value);
uint ui32 = TernaryConverter.IntTToUInt32(value);
short i16 = TernaryConverter.IntTToInt16(value);
ushort ui16 = TernaryConverter.IntTToUInt16(value);
byte ui8 = TernaryConverter.IntTToUInt8(value);
sbyte i8 = TernaryConverter.IntTToInt8(value);
ulong ui64 = TernaryConverter.IntTToUInt64(value);
decimal dec = TernaryConverter.IntTToDecimal(value);
```

### From Binary Types to IntT

```csharp
IntT fromInt32 = TernaryConverter.IntTFromInt32(12345);
IntT fromUInt32 = TernaryConverter.IntTFromUInt32(12345u);
IntT fromInt16 = TernaryConverter.IntTFromInt16((short)1234);
IntT fromUInt16 = TernaryConverter.IntTFromUInt16((ushort)1234);
IntT fromUInt8 = TernaryConverter.IntTFromUInt8((byte)123);
IntT fromInt8 = TernaryConverter.IntTFromInt8((sbyte)-123);
IntT fromUInt64 = TernaryConverter.IntTFromUInt64(12345ul);
IntT fromDecimal = TernaryConverter.IntTFromDecimal(12345.6m);
```

## FloatT Conversions

### From FloatT to Binary Types

```csharp
FloatT value = FloatT.FromDouble(123.456);

float f = TernaryConverter.FloatTToFloat(value);
decimal dec = TernaryConverter.FloatTToDecimal(value);
// Note: double conversion is built-in via value.ToDouble()
```

### From Binary Types to FloatT

```csharp
FloatT fromFloat = TernaryConverter.FloatTFromFloat(123.456f);
FloatT fromDecimal = TernaryConverter.FloatTFromDecimal(123.456m);
// Note: double conversion is built-in via FloatT.FromDouble()
```

## Tryte Conversions

### From Tryte to Binary Types

```csharp
Tryte value = new Tryte(123);

byte ui8 = TernaryConverter.TryteToUInt8(value);
sbyte i8 = TernaryConverter.TryteToInt8(value);
ushort ui16 = TernaryConverter.TryteToUInt16(value);
uint ui32 = TernaryConverter.TryteToUInt32(value);
ulong ui64 = TernaryConverter.TryteToUInt64(value);
decimal dec = TernaryConverter.TryteToDecimal(value);
// Note: short and int conversions are built-in via value.ShortValue
```

### From Binary Types to Tryte

```csharp
Tryte fromUInt8 = TernaryConverter.TryteFromUInt8((byte)123);
Tryte fromInt8 = TernaryConverter.TryteFromInt8((sbyte)-123);
Tryte fromUInt16 = TernaryConverter.TryteFromUInt16((ushort)123);
Tryte fromUInt32 = TernaryConverter.TryteFromUInt32(123u);
Tryte fromUInt64 = TernaryConverter.TryteFromUInt64(123ul);
Tryte fromDecimal = TernaryConverter.TryteFromDecimal(123.6m);
// Note: short and int conversions are built-in via new Tryte(value)
```

## Trit Conversions

### Trit ↔ Boolean

```csharp
Trit positive = new Trit(1);
bool isTrue = TernaryConverter.ToBoolean(positive);  // true

Trit fromTrue = TernaryConverter.FromBoolean(true);   // Trit(1)
Trit fromFalse = TernaryConverter.FromBoolean(false); // Trit(-1)
```

Note: In ternary logic, positive (1) is true, while zero (0) and negative (-1) are both false.

## Error Handling

All conversion methods throw `OverflowException` when:
- The source value is outside the range of the target type
- Converting negative values to unsigned types
- Converting values that exceed the maximum representable value

## Examples

### Round-trip Conversion

```csharp
// IntT round-trip
int original = 12345;
IntT ternary = TernaryConverter.IntTFromInt32(original);
int result = TernaryConverter.IntTToInt32(ternary);
// original == result

// FloatT round-trip
float originalFloat = 123.456f;
FloatT ternaryFloat = TernaryConverter.FloatTFromFloat(originalFloat);
float resultFloat = TernaryConverter.FloatTToFloat(ternaryFloat);
// originalFloat ≈ resultFloat (within precision limits)
```

### Type Compatibility

```csharp
// IntT can represent values in range: ±(3^32 - 1) / 2
// Be mindful of overflow when converting to smaller types
IntT largeValue = new IntT(100000);
byte smallValue = TernaryConverter.IntTToUInt8(largeValue); // Throws OverflowException

// Tryte range depends on N_TRITS_PER_TRYTE (default 6 trits)
// Default range: ±364
Tryte.N_TRITS_PER_TRYTE = 6; // Default
Tryte tryteValue = new Tryte(364); // Maximum value
```

## Built-in Conversions

Some conversions are already built into the types themselves:

- **IntT**: `ToInt64()` and constructor from `long`
- **FloatT**: `ToDouble()` and `FromDouble()`
- **Tryte**: `ShortValue` property and constructor from `short`/`int`
- **Trit**: Implicit conversions to/from `sbyte` and `int`

The `TernaryConverter` library extends these by providing conversions to additional binary types with proper overflow checking.
