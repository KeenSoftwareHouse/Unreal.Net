// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Unreal.Core
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class ReflectionDataBase
    {
        public IntPtr NativeUClass { get; }

        public Type ManagedType { get; }

        /// <summary>
        /// whether the implementation of this type is managed.
        /// </summary>
        public TypeImplementation Implementation { get; }

        /// <summary>
        /// Whether this reflection mapping is a best fit choice for a native
        /// type that was not previously known to the metadata system.
        /// </summary>
        public bool IsBestFit { get; }

        private readonly Lazy<ReflectionDataBase?> m_parent;

        /// <summary>
        /// The reflection data for the parent type.
        /// </summary>
        public ReflectionDataBase? Parent => m_parent.Value;

        private string DebuggerDisplay
            => IsBestFit ? $"{ManagedType}/{NativeUClass:X16} BestFit" : ManagedType.ToString();

        protected internal ReflectionDataBase(IntPtr nativeUClass, Type managedType, TypeImplementation implementation,
            bool isBestFit = false)
        {
            if (nativeUClass == IntPtr.Zero)
                throw new InvalidOperationException("Cannot instantiate reflection data for a null class.");
            
            NativeUClass = nativeUClass;
            ManagedType = managedType;
            Implementation = implementation;
            IsBestFit = isBestFit;
            m_parent = new Lazy<ReflectionDataBase?>(GetParentReflectionData);
        }

        private ReflectionDataBase? GetParentReflectionData()
        {
            var parentClass = UObjectUtil.GetSuperClass(NativeUClass);
            return parentClass == IntPtr.Zero ? null : UObjectReflection.Instance.GetTypeData(parentClass);
        }
    }
}