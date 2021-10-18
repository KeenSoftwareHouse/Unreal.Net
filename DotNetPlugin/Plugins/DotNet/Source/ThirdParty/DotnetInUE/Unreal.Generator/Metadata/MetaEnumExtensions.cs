// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Unreal.Marshalling;

namespace Unreal.Metadata
{
    public static class MetaEnumExtensions
    {
        #region Code Component

        public static MemberCodeComponent[] Components =
            Enum.GetValues(typeof(MemberCodeComponent)).Cast<MemberCodeComponent>().ToArray();

        public static MemberCodeComponentFlags With(this MemberCodeComponentFlags self, MemberCodeComponent component)
            => self | component.ToFlags();

        public static MemberCodeComponentFlags Without(this MemberCodeComponentFlags self,
            MemberCodeComponent component)
            => self & ~component.ToFlags();

        public static bool HasComponent(this MemberCodeComponentFlags self, MemberCodeComponent component)
            => (self & component.ToFlags()) != 0;

        public static bool HasAny(this MemberCodeComponentFlags self, MemberCodeComponentFlags toCompare)
            => (self & toCompare) != 0;

        public static bool HasAll(this MemberCodeComponentFlags self, MemberCodeComponentFlags toCompare)
            => (self & toCompare) == toCompare;

        public static MemberCodeComponentFlags ToFlags(this MemberCodeComponent component)
            => (MemberCodeComponentFlags) (1 << (int) component);

        public static Codespace GetSpace(this MemberCodeComponent self)
            => self == MemberCodeComponent.ManagedPart ? Codespace.Managed : Codespace.Native;

        public static IEnumerator<MemberCodeComponent> GetEnumerator(this MemberCodeComponentFlags self)
        {
            for (int i = 0; i < Components.Length; ++i)
            {
                if (self.HasComponent(Components[i]))
                    yield return Components[i];
            }
        }

        #endregion

        #region Symbol Visibility

        /// <summary>
        /// Get the best fit native symbol visibility.
        /// </summary>
        /// <remarks>Native methods cannot be internal, those types get promoted to public.</remarks>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SymbolVisibility GetNative(this SymbolVisibility self)
        {
            return self switch
            {
                SymbolVisibility.Default => SymbolVisibility.Private,
                SymbolVisibility.Public => SymbolVisibility.Public,
                SymbolVisibility.Protected => SymbolVisibility.Protected,
                SymbolVisibility.ProtectedInternal => SymbolVisibility.Public,
                SymbolVisibility.Internal => SymbolVisibility.Public,
                SymbolVisibility.Private => SymbolVisibility.Private,
                _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };
        }

        public static string Format(this SymbolVisibility self)
        {
            return self switch
            {
                SymbolVisibility.Default => "",
                SymbolVisibility.Public => "public",
                SymbolVisibility.Protected => "protected",
                SymbolVisibility.ProtectedInternal => "protected internal",
                SymbolVisibility.Internal => "internal",
                SymbolVisibility.Private => "private",
                _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };
        }

        #endregion

        #region Symbol Attributes

        public static bool HasAttribute(this SymbolAttributeFlags self, SymbolAttribute attribute)
            => (self & attribute.ToFlags()) != 0;

        public static SymbolAttributeFlags ToFlags(this SymbolAttribute self)
            => (SymbolAttributeFlags) (1 << (int) self);

        #endregion

        #region Codespace

        public static Codespace GetOther(this Codespace self) => (Codespace) (1 - (int) self);

        public static bool IsManaged(this Codespace self) => self == Codespace.Managed;

        public static bool IsNative(this Codespace self) => self == Codespace.Native;

        public static string GetAutoKeyword(this Codespace self)
            => self switch
            {
                Codespace.Managed => "var",
                Codespace.Native => "auto",
                _ => throw new ArgumentOutOfRangeException(nameof(self))
            };

        public static CodespaceFlags ToFlags(this Codespace self) => (CodespaceFlags) (1 << (int) self);

        public static bool HasSpace(this CodespaceFlags self, Codespace space) => (self & space.ToFlags()) != 0;

        #endregion

        #region Order

        public static Order GetOpposite(this Order self) => (Order) (1 - (int) self);

        /// <summary>
        /// Get the next step in the marshalling order.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static MarshalOrder GetNext(this MarshalOrder self)
            => self switch
            {
                MarshalOrder.Before => MarshalOrder.Marshalled,
                MarshalOrder.Marshalled => MarshalOrder.After,
                MarshalOrder.After or _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };

        #endregion

        #region Transfer Types

        public static string FormatType(this NativeTransferType self, string typeName)
        {
            string prefix = self.IsConst() ? "const " : "";
            switch (self.MakeNotConst())
            {
                case NativeTransferType.ByValue:
                    return prefix + typeName;
                case NativeTransferType.ByReference:
                    return "UPARAM(ref) " + typeName + "&";
                case NativeTransferType.ByPointer:
                    return prefix + typeName + "*";

                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            }
        }

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public static bool IsConst(this NativeTransferType self) => (self & NativeTransferType.Const) != 0;

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public static NativeTransferType MakeConst(this NativeTransferType self) => self | NativeTransferType.Const;

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public static NativeTransferType MakeNotConst(this NativeTransferType self) => self & ~NativeTransferType.Const;

        /// <summary>
        /// Whether this transfer type represents an incoming value.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsIn(this ManagedTransferType self) => self != ManagedTransferType.Out;

        /// <summary>
        /// Whether this transfer type represents an outgoing value.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsOut(this ManagedTransferType self)
            => self == ManagedTransferType.Ref || self == ManagedTransferType.Out;

        #endregion
        
        #region TypeKindExtensions
        
        public static TypeKind GetKind(this INamedTypeSymbol symbol)
        {
            return symbol.TypeKind switch
            {
                Microsoft.CodeAnalysis.TypeKind.Class => TypeKind.Class,
                Microsoft.CodeAnalysis.TypeKind.Enum => TypeKind.Enum,
                Microsoft.CodeAnalysis.TypeKind.Interface => TypeKind.Interface,
                Microsoft.CodeAnalysis.TypeKind.Struct => TypeKind.Struct,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static bool IsValueType(this TypeKind kind)
        {
            return kind switch
            {
                TypeKind.Class => false,
                TypeKind.Struct => true,
                TypeKind.Interface => false,
                TypeKind.Enum => true,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
        
        public static bool IsReferenceType(this TypeKind kind)
        {
            return !IsValueType(kind);
        }
        
        #endregion
    }
}