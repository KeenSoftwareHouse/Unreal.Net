// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unreal.CoreUObject;

namespace Unreal.Core
{
    public class ReflectionData : ReflectionDataBase
    {
        private readonly Lazy<UClass> m_class;

        public UClass Class => m_class.Value;

        private ReflectionData(IntPtr nativeUClass, Type managedType, TypeImplementation implementation,
            bool isBestFit = false)
            : base(nativeUClass, managedType, implementation, isBestFit)
        {
            // nativeClass cannot be null, our base validates this.
            m_class = new Lazy<UClass>(() => UObjectBase.GetOrCreateNative<UClass>(nativeUClass)!);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [ModuleInitializer]
        internal static void Register()
        {
            UObjectReflection.Instance.RegisterFactory(new ReflectionFactory());
        }
        
        internal class ReflectionFactory : UObjectReflection.IReflectionDataFactory
        {
            public ReflectionDataBase Create(IntPtr nativeUClass, Type managedType, TypeImplementation implementation,
                bool isBestFit = false)
            {
                return new ReflectionData(nativeUClass, managedType, implementation, isBestFit);
            }
        }
    }
}