// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Unreal.Core
{
    public class UObjectReflection
    {
        #region Instance

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly UObjectReflection Instance = new();

        private UObjectReflection()
        { }

        #endregion

        #region Factory

        public interface IReflectionDataFactory
        {
            public ReflectionDataBase Create(IntPtr nativeUClass, Type managedType, TypeImplementation implementation,
                bool isBestFit = false);
        }

        private IReflectionDataFactory m_factory = new BaseFactory();

        public void RegisterFactory(IReflectionDataFactory factory)
        {
            if (m_nativeIndex.Count > 0)
                throw new InvalidOperationException(
                    "The reflection data factory can onl;y be set before indexing starts.");

            m_factory = factory;
        }

        private class BaseFactory : IReflectionDataFactory
        {
            public ReflectionDataBase Create(IntPtr nativeUClass, Type managedType, TypeImplementation implementation,
                bool isBestFit = false)
                => new(nativeUClass, managedType, implementation, isBestFit);
        }

        #endregion

        #region Reflection Info

        private readonly Dictionary<IntPtr, ReflectionDataBase> m_nativeIndex = new();

        private readonly Dictionary<Type, ReflectionDataBase> m_managedIndex = new();

        public void RegisterType(IntPtr nativeUClass, Type managedType, TypeImplementation implementation)
        {
            var data = m_factory.Create(nativeUClass, managedType, implementation);

            m_nativeIndex.Add(nativeUClass, data);
            m_managedIndex.Add(managedType, data);
        }

        /// <summary>
        /// Get the uclass handle for a given type.
        /// </summary>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public IntPtr GetUClassHandle(Type managedType)
        {
            return m_managedIndex[managedType].NativeUClass;
        }

        /// <summary>
        /// Get reflection information for a managed type.
        /// </summary>
        /// <param name="managedType"></param>
        /// <returns></returns>
        public ReflectionDataBase GetTypeData(Type managedType)
        {
            return m_managedIndex[managedType];
        }

        /// <summary>
        /// Get the managed type for known native class handle.
        /// </summary>
        /// <param name="uClassHandle"></param>
        /// <returns></returns>
        public Type GetManagedType(IntPtr uClassHandle)
        {
            return m_nativeIndex[uClassHandle].ManagedType;
        }

        /// <summary>
        /// Get reflection information for a native class handle.
        /// </summary>
        /// <param name="uClassHandle"></param>
        /// <returns></returns>
        public ReflectionDataBase GetTypeData(IntPtr uClassHandle)
        {
            return m_nativeIndex[uClassHandle];
        }

        /// <summary>
        /// Get the best fit managed type for the given native uclass.
        /// </summary>
        /// <remarks>For registered types this behaves the same as <see cref="GetManagedType"/>.
        /// For unregistered types this performs a search of the type's inheritance tre in search
        /// of the first known parent type and returns that </remarks>
        /// <param name="uClassHandle"></param>
        /// <returns></returns>
        public unsafe ReflectionDataBase GetBestFitType(IntPtr uClassHandle)
        {
            ReflectionDataBase? typeData = null;
            var bestFitClass = uClassHandle;
            while (bestFitClass != IntPtr.Zero && !m_nativeIndex.TryGetValue(bestFitClass, out typeData))
                bestFitClass = UObjectUtil.GetSuperClass(bestFitClass);

            if (typeData == null)
                throw new TypeLoadException("No component of the type's hierarchy is known.");

            // Cache result of search for next use.
            if (bestFitClass != uClassHandle)
            {
                typeData = m_factory.Create(uClassHandle, typeData.ManagedType, typeData.Implementation, true);
                m_nativeIndex.Add(uClassHandle, typeData);
            }

            return typeData;
        }

        #endregion
    }
}