using System;

namespace stdTernary;

/// <summary>
/// Marks a method whose semantics map 1:1 onto a REBEL-6 instruction. The
/// <see cref="Mnemonic"/> must be an entry in the REBEL-6 assembler's pattern
/// table (TernaryWorkbench.RebelAssembler, InstructionSet6.Patterns); a unit
/// test in stdTernary.Tests cross-checks every annotated mnemonic against that
/// table so the library and the ISA cannot drift apart silently.
///
/// A method may carry several annotations when one library operation covers
/// multiple encodings of the same semantics - e.g. <c>Rotate</c> maps to both
/// ROT.T (immediate amount) and ROTR.T (register amount), a distinction that
/// collapses in a library where every amount is a runtime value.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class Rebel6IntrinsicAttribute : Attribute
{
    public Rebel6IntrinsicAttribute(string mnemonic)
    {
        Mnemonic = mnemonic ?? throw new ArgumentNullException(nameof(mnemonic));
    }

    /// <summary>The REBEL-6 assembler mnemonic, e.g. "ADD.T".</summary>
    public string Mnemonic { get; }
}
