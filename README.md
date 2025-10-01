# stdTernary #
## A standard library for Ternary operations in C# ##
![build-and-test](https://github.com/actionjdjackson/stdTernary/actions/workflows/build-and-test.yml/badge.svg?event=push)

(2do) what this library is and what it enables: an elegant ternary first high level programming experience. The library abstract the low level implementation such that in the future the same c# source code can be recompiled on a ternary computer architecture. 

The library promotes the usage of mixed radix programming, using binary for binary concepts and ternary for ternary concepts. This means that boolean is great to model concepts with only 2 logical choices (True, False) and kleenean is great to model concepts with 3 choices (True, False, Unknown). Kleene logic is clear in what happens with the unknown state when using implications.    

### Type Conversion Library

The library now includes a comprehensive **TernaryConverter** class that provides seamless conversion between balanced ternary types and common .NET binary types:

- **IntT** ↔ int, uint, short, ushort, byte, sbyte, long, ulong, decimal
- **FloatT** ↔ float, double, decimal  
- **Tryte** ↔ byte, sbyte, short, ushort, uint, ulong, decimal
- **Trit** ↔ bool

All conversions include proper overflow checking and error handling. See [CONVERTER_README.md](CONVERTER_README.md) for detailed documentation and examples.

### Installation via NuGet
(2do) 

### Installation via Github Codespaces
- Click on the green Code button and then on the tab Codespaces. Choose "create new codespace on main".
- To build the .dll library type "task build"
- To build and run the unit tests type "task test" 
- To run the benchmark suite type "task run"

### Limitations
- The library uses binary coded ternary (BCT) using 2 bits to represent 1 trit. This is obviously inefficient as 1 of the 4 states is ignored. We define 6 trits as 1 tryte such that all binary states of a single byte can fit in a single tryte which simplifies conversions and add binary compatibility. A tryte (6 trits implemented as 12 bits) is packed as binary uint. Note that balanced ternary is always signed so the usage of `uint` is just a choice of implementation. 

Since the current compilation targets are all binary hardware platforms, this inefficiency is unavoidable. However, we expect that this is temporary as more and more balanced ternary instructions set architectures are being developed such as REBEL-6 and ART-9 which feature ternary assembly with ternary logic, memory and branching. With the new .net 10 featuring RISCV support, a possible compilation flow from c# to REBEL-6 (32-trit balanced ternary, binary compatible ISA) is technically possible through R2R. This is work in progress. The R2R framework allows low level binary vs ternary comparison to compute fairly the amount of memory and bitflips that are needed for both radixes to run the same code. 

### Dev Notes sep 2025 (by Jackson)
I've completely overhauled stdTernary - no longer using an enum for storing +, -, and 0 values. Instead, I'm trit-packing 2-bit-trits (0b10 is -1, 0b01 is 1, and 0b00 is zero, while 0b11 is reserved/unused) into binary unsigned integers (`uint`s or `ulong`s) but all the math is still done in Ternary.

All bitwise and bytewise operators have been overriden for trits and trytes. I am currently using the `*` operator on trits for XNOR/MULTIPLY. The structs are `Trit` and `Tryte` - and the `Tryte` can be modified easily to be any number of trits you want, up to 16 trits with the current implementation. Each `Tryte` holds a bitpacked array of trits in a `uint` - using 6 by default, up to 16. All math is done in Ternary.

Includes a customizable `IntT` struct that can have any number of total trits (up to 32 - bitpacking into a 64-bit `ulong`) in its implementation. It follows the same convention as the `Tryte`, but is able to work with non-multiples of trytes - like 21-trit or 32-trit integers. All math is done in Ternary.

Also includes a customizable `FloatT` struct that can have any number of total trits - up to 32, separated into a exponent and a significand. The significand can be any number of trits - typically 26 so that the exponent has 6 to work with. It doesn't have to be a multiple of 3. The `FloatT` struct holds a combination the exponent and the significand as bitpacked trits. All math is done in Ternary.

Also includes most of the `Math` functions specifically for use with these `FloatT`s and some for use with `IntT`s in a static class called `MathT`. I also added a `Log3` function and an `ILogT` function and trit increment/decrement functions for `FloatT`s.

The `string` conversion is for interoperability with my "Action Ternary Simulator" which runs on strings of `+, -, and 0` characters and does all the math in Ternary. Also for quick visualization of the ternary values as symbols and checking the outputs of functions like `SHIFTRIGHT` or `SHIFTLEFT` or trit increment/decrement.

Will possibly create an unbalanced ternary version of all of this.

`Tryte` and `FloatT` and `IntT` and have modifiable static integer values which is where you can "customize" them to certain sizes.


### License ###
The MIT License (MIT)
Copyright © 2024 Jacob Jackson <jacob@actionjdjackson.online> and Dr. ir. Steven Bos <steven.bos@usn.no>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
