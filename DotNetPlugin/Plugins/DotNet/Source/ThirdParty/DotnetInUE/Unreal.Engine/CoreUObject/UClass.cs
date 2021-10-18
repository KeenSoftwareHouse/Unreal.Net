// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Reflection;
using Unreal.Core;

namespace Unreal.CoreUObject
{
    public partial class UClass
    {
        /// <summary>
        /// The implementation of the type represented by this UClass instance.
        /// </summary>
        public TypeImplementation Implementation { get; private set; }

        /// <summary>
        /// Reflection data for this class.
        /// </summary>
        public ReflectionData Reflection { get; private set; }
        
        /// <summary>
        /// Parent UClass.
        /// </summary>
        public UClass? ParentClass => Reflection.GetParent()?.Class;

        [Constructor]
        private void Construct()
        {
            Implementation = typeof(UClass).IsDefined<ManagedTypeAttribute>()
                ? TypeImplementation.Managed
                : TypeImplementation.Native;

            // Get best fit because this UClass could be coming from native code about some type we don't know.
            Reflection = UObjectReflection.Instance.GetBestFitType(this);
        }

        /// <summary>
        /// whether this class instance is assignable to a variable of type <typeparamref name="TClass"/>.
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <returns></returns>
        public bool IsAssignableTo<TClass>()
            where TClass : UObject
        {
            ReflectionData reflection = Reflection;

            var targetType = typeof(TClass);

            while (reflection.ManagedType != targetType)
            {
                if (reflection.GetParent() is not { } p)
                    return false;
                reflection = p;
            }

            return true;
        }

        /// <summary>
        /// Whether the current class is equal to or a base of <paramref name="derived"/>.
        /// </summary>
        /// <param name="derived"></param>
        /// <returns></returns>
        public bool IsBaseClassOf(UClass derived)
        {
            var cl = derived;
            while (cl != this)
            {
                if (cl.ParentClass == null)
                    return false;
                cl = cl.ParentClass;
            }

            return true;
        }
    }
}