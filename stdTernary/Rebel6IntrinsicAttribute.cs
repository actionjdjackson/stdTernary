using System;

namespace stdTernary;

/// <summary>
/// Marks a method whose semantics map 1:1 onto a REBEL-6 instruction. The
/// <see cref="Mnemonic"/> must be an entry in the REBEL-6 assembler's pattern
/// table (TernaryWorkbench.RebelAssembler, InstructionSet6.Patterns); a unit
/// test in stdTernary.Tests cross-checks every annotated mnemonic against that
/// table so the library and the ISA cannot drift apart silently.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class Rebel6IntrinsicAttribute : Attribute
{
    public Rebel6IntrinsicAttribute(string mnemonic)
    {
        Mnemonic = mnemonic ?? throw new ArgumentNullException(nameof(mnemonic));
    }

    /// <summary>The REBEL-6 assembler mnemonic, e.g. "ADD.T".</summary>
    public string Mnemonic { get; }
}
