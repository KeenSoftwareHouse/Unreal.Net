// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    /// <summary>
    /// Defines an object that is capable of marshalling a type between it's managed and unmanaged representations.
    /// </summary>
    /// <remarks>Type marshallers are responsible for handling different type qualifications as well, making sure to
    /// transfer arguments in the way specified in the <see cref="QualifiedTypeReference"/>.</remarks>
    public interface ITypeMarshaller
    {
        /// <summary>
        /// Codespaces that require active code transformations by this marshaller.
        /// </summary>
        /// <remarks>The marshaller's specified transition type will still be respected, but will be assumed to implicitly convert in the given codespace.</remarks>
        /// <returns></returns>
        CodespaceFlags NeedsActiveMarshalling { get; }

        /// <summary>
        /// Whether functions returning types handled by this marshaller must invert the return clause to a passed in return address.
        /// </summary>
        bool NeedsReturnValueInversion { get; }

        /// <summary>
        /// Additional header file that may be required when types ara handled by this marshaller.
        /// </summary>
        public string? AdditionalHeader { get; }

        /// <summary>
        /// Additional namespace that may be required when types ara handled by this marshaller.
        /// </summary>
        public string? AdditionalNamespace { get; }

        /// <summary>
        /// Write to a file an expression or series of lines that marshal a variable.
        /// The result of this call should be the definition of a variable <paramref name="outputName"/>.
        /// No other variables should be defined as a result of the expression.
        /// </summary>
        /// <remarks>
        /// The combinations of space and order define the direction of conversion and where the code is defined.
        /// <list type="table">
        /// <listheader><term>Space</term><term>Order</term><term>Description</term></listheader>
        /// <item><term>Native</term><term>Before</term><term>Marshal variable in native code to intermediate.</term></item>
        /// <item><term>Native</term><term>After</term><term>Marshal variable in native code from intermediate.</term></item>
        /// <item><term>Managed</term><term>Before</term><term>Marshal variable in managed code to intermediate.</term></item>
        /// <item><term>Managed</term><term>After</term><term>Marshal variable in managed code from intermediate.</term></item>
        /// </list>
        /// </remarks>
        /// <param name="writer">Writer for the expression or code block.</param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="outputName">The name of the output variable.</param>
        /// <param name="space">The codespace where the variable is defined.</param>
        /// <param name="order">Whether to marshal the variable to <see cref="Order.Before"/> or from (<see cref="Order.After"/>)
        ///     the opposite code space.</param>
        /// <param name="afterCall"></param>
        /// <returns></returns>
        public void MarshalVariable(CodeWriter writer, QualifiedTypeReference type, string name, string outputName,
            Codespace space, Order order, bool afterCall);

        /// <summary>
        /// Get the intermediate type reference used for a marshalled argument. This intermediate representation
        /// must be a blittable type.
        /// </summary>
        /// <remarks>This is the type used when the argument is received or transferred to native code.
        /// Essentially the type of the expression produced by <see cref="MarshalVariable"/>.</remarks>
        /// <param name="type">The managed type.</param>
        /// <returns>The type of the marshalled value.</returns>
        public ITypeInfo GetIntermediateType(QualifiedTypeReference type);
    }
}