// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Unreal.Marshalling;
using Unreal.Metadata;
using Unreal.Util;

namespace Unreal.Generation
{
    public class FunctionMarshalling
    {
        public MarshalledParameter[] Parameters { get; }
        public MarshalledParameter Return { get; }

        public bool HasModifiedParameters, HasThis, HasModifiedReturn;

        public bool HasOutArguments;

        private readonly CodespaceFlags MarshalledCodespaces;

        public FunctionMarshalling(FunctionDefinition function)
        {
            var count = function.Parameters.Length;

            HasThis = !function.Attributes.HasAttribute(SymbolAttribute.Static);
            if (HasThis)
                count++;

            HasModifiedReturn = function.Return.Marshaller?.NeedsReturnValueInversion == true;
            if (HasModifiedReturn)
                count++;

            // Record if we had to modify the parameters.
            HasModifiedParameters = count != function.Parameters.Length;

            Parameters = new MarshalledParameter[count];

            int index = 0;

            if (HasThis)
                Parameters[index++] = GetThis(function);

            foreach (var param in function.Parameters)
                Parameters[index++] = GetMarshalled(param);

            if (HasModifiedReturn)
            {
                // Force parameter as out so it processes correctly.
                Parameters[index] = GetMarshalled(function.Return.Type,
                    "__return", function.Return.Marshaller, null);
                Parameters[index].MarshalOut = true; // Force marshal out even without out transfer type.
                Return = GetReturn(TransferableDefinition.Void);
            }
            else
            {
                Return = GetReturn(function.Return);
            }

            for (int i = 0; i < Parameters.Length; i++)
            {
                var typeMarshaller = Parameters[i].Marshaller;
                if (typeMarshaller != null)
                {
                    MarshalledCodespaces |= typeMarshaller.NeedsActiveMarshalling;

                    // Only set these for marshalled arguments.
                    HasOutArguments |= Parameters[i].MarshalOut;
                }
            }
        }

        private static MarshalledParameter GetThis(FunctionDefinition function)
        {
            return new("__self", new QualifiedTypeReference(function.EnclosingType),
                function.EnclosingType.DefaultMarshaller, null);
        }

        private static MarshalledParameter GetMarshalled(ParameterDefinition p)
        {
            return GetMarshalled(p.Type, p.Name, p.Marshaller, p.DefaultValue);
        }

        private static MarshalledParameter GetMarshalled(QualifiedTypeReference type, string name,
            ITypeMarshaller? marshaller, string? defaultValue)
        {
            return new(name, type, marshaller, defaultValue);
        }

        private static MarshalledParameter GetReturn(TransferableDefinition type)
        {
            return new("__return", type.Type, type.Marshaller, null);
        }

        /// <summary>
        /// Get the parameter used for the return value.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ref MarshalledParameter GetReturnParameter()
        {
            if (!HasModifiedReturn)
                throw new InvalidOperationException("This function dos not have a modified return value.");
            var lastIdx = Parameters.Length - 1;
            return ref Parameters[lastIdx];
        }

        public bool HasAny(Codespace space) => MarshalledCodespaces.HasSpace(space);

        [DebuggerDisplay("{OriginalType} {Name}")]
        public struct MarshalledParameter
        {
            public string Name;

            public string MarshalledName;

            public QualifiedTypeReference OriginalType;

            public ITypeInfo IntermediateType;

            public ITypeMarshaller? Marshaller;

            public string? DefaultValue;

            public bool MarshalOut;

            public bool IsVoid => IntermediateType.IsVoid();

            public MarshalledParameter(string name,
                QualifiedTypeReference originalType, ITypeMarshaller? marshaller, string? defaultValue)
            {
                Name = name;
                MarshalledName = $"{name}__marshalled";
                OriginalType = originalType;
                Marshaller = marshaller;
                IntermediateType = marshaller?.GetIntermediateType(originalType) ?? originalType.TypeInfo;
                DefaultValue = defaultValue;
                MarshalOut = originalType.TransferType.IsOut();
            }

            public QualifiedTypeReference GetType(MarshalOrder order)
            {
                if (Marshaller != null)
                    return order == MarshalOrder.Marshalled ? new QualifiedTypeReference(IntermediateType) : OriginalType;

                return OriginalType;
            }

            public string GetName(Codespace space, MarshalOrder order)
            {
                if (Marshaller?.NeedsActiveMarshalling.HasSpace(space) == true)
                {
                    return order switch
                    {
                        MarshalOrder.Before => Name,
                        MarshalOrder.Marshalled => MarshalledName,
                        MarshalOrder.After => Name,
                        _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
                    };
                }

                return Name;
            }

            public bool IsMarshalled(Codespace sourceSpace)
                => Marshaller?.NeedsActiveMarshalling.HasSpace(sourceSpace) == true;
        }

        /// <summary>
        /// Create a native function type signature for this function.
        /// </summary>
        /// <param name="name">The name of the type, if provided.</param>
        /// <returns></returns>
        public string MakeNativeIntermediateTypeSignature(string name = "")
        {
            var parameters = string.Join(", ",
                Parameters.Select(x => x.IntermediateType.FormatName(Codespace.Native)));
            var @return = Return.IntermediateType.FormatName(Codespace.Native);

            return $"{@return} (*{name}) ({parameters})";
        }

        /// <summary>
        /// Create a managed function pointer signature for this function.
        /// </summary>
        /// <returns></returns>
        public string MakeIntermediateFunctionPointerSignature()
        {
            var parameters = string.Join(", ",
                Parameters.Select(x => x.IntermediateType.FormatName(Codespace.Managed)));
            var @return = Return.IntermediateType.FormatName(Codespace.Managed);

            if (parameters.Length > 0)
                return $"delegate * unmanaged<{parameters}, {@return}>";
            else
                return $"delegate * unmanaged<{@return}>";
        }
    }
}