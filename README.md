# stdTernary #
## A standard library for Ternary operations in C# ##
![build-and-test](https://github.com/actionjdjackson/stdTernary/actions/workflows/build-and-test.yml/badge.svg?event=push)

Since the binary computer operates in bytes, even when dealing with boolean values, I decided to use the signed byte enum as the carrier for the Balanced Ternary values 1, -1, and 0. They are named n, p, and z for -1 (negative), 1 (positive), and 0 (zero). I could have used two booleans but I think the performance would be about the same. I might give it a test run in the near future and find out if a pair of booleans is actually faster than a `sbyte`, but the consensus seems to be that there is no performance advantage to a bool over a byte.

All bitwise and bytewise operators have been overriden for trits and trytes. I am currently using the `*` operator on trits for XNOR/MULTIPLY. The structs are `Trit` and `Tryte` - and the `Tryte` can be modified easily to be any number of trits you want, up to 10 trits with the current implementation. Each `Tryte` holds a combination of an array of `Trit`s and a `short` value for binary equivalent. All math is done in Ternary.

Includes a customizable `IntT` struct that can have any number of total trits in its implementation. It follows the same convention as the `Tryte`, but is able to work with non-multiples of trytes - like 21-trit or 32-trit integers. All math is done in Ternary.

Also includes a customizable `FloatT` struct that can have any number of total trits, separated into a exponent and a significand, 1/4 going to the exponent and 3/4 going to the significand (mantissa). It doesn't have to be a multiple of 3, though that is preferable (27 is a nice number, currently using 24). The `FloatT` struct holds a combination of an array of `Trit`s for the exponent and the signficand and the whole float combined, and a `double` for binary equivalent. All math is done in Ternary.

Also includes a `CharT` struct with 12 trits for a `char` representation. It's just an integer value stored as an array of `Trit`s that corresponds to a UTF-16 character.

Also includes most of the `Math` functions specifically for use with these `FloatT`s and some for use with `IntT`s and `Tryte`s in a static class called `MathT`. I also added a `Log3` function and an `ILogT` function and trit increment/decrement functions for `FloatT`s.

The `string` conversion is for interoperability with my "Action Ternary Simulator" which runs on strings of `+, -, and 0` characters and does all the math in Ternary. Also for quick visualization of the ternary values as symbols and checking the outputs of functions like `SHIFTRIGHT` or `SHIFTLEFT` or trit increment/decrement.

Will possibly create an unbalanced ternary version of all of this.

`Tryte` and `FloatT` and `IntT` and `CharT` have modifiable static integer values which is where you can "customize" them to certain sizes - `N_TRITS_PER_TRYTE`, and `N_TOTAL_TRITS_FLOAT` (`N_TRITS_SIGNIFICAND` and `N_TRITS_EXPONENT` too), and `N_TRITS_PER_INT`, and `N_TRITS_PER_CHAR` respectively.

I throw a lot of Exceptions when dealing with numbers outside the range of the Balanced Ternary stucts' acceptable values - which is annoying so I might change it to a "zero" value whenever the values are too large (either positive or negative) and type casting is involved.

### License ###
The MIT License (MIT)
Copyright © 2024 Jacob Jackson <jacob@actionjdjackson.online> and Dr. ir. Steven Bos <steven.bos@usn.no>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
