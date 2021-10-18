// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using Unreal.Core;
using Unreal.CoreUObject;
using Unreal.Marshalling;

namespace Unreal
{
    /// <summary>
    /// Type safe wrapper for <see cref="UClass"/> that enforces the use of a subtype of a given class type. 
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    [NativeType("TSubclassOf", "ClassProperty", TypeMemoryKind.ValueType,
        typeof(UClass), TypeTransformation.OpaquePointer)]
    [MarshalFormats(fromManagedToIntermediate: "{0}.ClassHandle", // Pass handle. 
        fromIntermediateToManaged: "new ({0})", // Create from class handle.
        fromNativeToIntermediate: "{0}.Get()", // Get class
        fromIntermediateToNative: "{0}")] // Set from class (take advantage of overload resolution.
    public struct SubclassOf<TClass>
        where TClass : UObject
    {
        private IntPtr m_classHandle;

        public UClass? Class
        {
            get => UObjectBase.GetOrCreateNative<UClass>(m_classHandle);

            set
            {
                if (value == null)
                    m_classHandle = IntPtr.Zero;
                else
                {
                    if (!value.IsAssignableTo<TClass>())
                        throw new ArgumentException("Value is not assignable to the base type constraint.",
                            nameof(value));
                    m_classHandle = UObjectUtil.GetNativeInstance(value);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IntPtr ClassHandle => m_classHandle;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public SubclassOf(IntPtr classHandle)
        {
            m_classHandle = classHandle;
        }

        public static implicit operator UClass?(SubclassOf<TClass> instance) => instance.Class;

        public static implicit operator SubclassOf<TClass>(UClass @class) => new(UObjectUtil.GetNativeInstance(@class));
    }
}