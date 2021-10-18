// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Diagnostics;
using Unreal.Core;
using Unreal.CoreUObject;
using Unreal.Engine;
using Unreal.TestModule;

namespace Unreal
{
    [UClass, Blueprintable]
    public partial class UTestClass : UObject
    {
        private const string Category = ".Net Test";

        [UFunction, BlueprintCallable, Category(Category)]
        public void MemberFunctionTest(int i, float f)
        {
            UEngine.Instance.PrintMessage(-1, 5, Color.Yellow,
                $"Hello UE4 from {nameof(UTestClass)}.{nameof(MemberFunctionTest)}: {i}, {f}");
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public static void ArgumentTest(int i, float f)
        {
            UEngine.Instance.PrintMessage(-1, 5, Color.Yellow,
                $"Hello UE4 from {nameof(UTestClass)}.{nameof(ArgumentTest)}: {i}, {f}");
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public static int Add(int lhs, int rhs)
        {
            return lhs + rhs;
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public UTestClass CreateChild()
        {
            return NewObject<UTestClass>(this);
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public void InteractWithOtherClass()
        {
            var result = UTestNativeClass.AddNumbers(1, 3);
            UEngine.Instance.PrintMessage(-1, 5, Color.Yellow,
                $"UTestNativeClass told me that 1 + 3 = {result}");
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public static void PrintTypeHierarchy(UObject anyObject)
        {
            var type = anyObject.GetType();

            while (type != typeof(UObject))
            {
                UEngine.Instance.PrintMessage(-1, 5, Color.Yellow, $"{type.Name}: {type.BaseType!.Name}");

                type = type.BaseType;
            }
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public static void PrintReflectionData(UObject objectInstance)
        {
            var classReflectionData = UObjectReflection.Instance.GetClassData(objectInstance.GetClass());

            var reflectionData = UObjectReflection.Instance.GetTypeData(objectInstance);

            // Same reflection data both for class and type.
            Debug.Assert(classReflectionData == reflectionData);

            string message =
                $"ManagedInstance = {reflectionData.ManagedType}, NativeClass={reflectionData.NativeUClass}, Implementation={reflectionData.Implementation}, IsBestFit={reflectionData.IsBestFit}";

            UEngine.Instance.PrintMessage(-1, 5, Color.Yellow, message);
        }

        [UFunction, BlueprintCallable, Category(Category)]
        public static AActor SpawnActor(SubclassOf<AActor> actorClass, UObject worldContext)
        {
            // TODO: Make better transform
            var transform = new FTransform()
            {
                Rotation = new FQuat {W = 1},
                Scale3D = new FVector {X = 1, Y = 1, Z = 1}
            };
            var actor = UGameplayStatics.BeginSpawningActorFromClass(worldContext, actorClass, transform, true, null);
            actor = UGameplayStatics.FinishSpawningActor(actor, transform);

            return actor;
        }
    }
}