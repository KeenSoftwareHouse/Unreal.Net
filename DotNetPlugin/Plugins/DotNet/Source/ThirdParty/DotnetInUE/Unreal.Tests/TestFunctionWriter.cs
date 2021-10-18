// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.IO;
using Unreal.Generation;
using Unreal.Marshalling;
using Unreal.Metadata;
using Xunit;
using Xunit.Abstractions;
using ITypeInfo = Unreal.Metadata.ITypeInfo;

namespace Unreal.Tests
{
    public class TestFunctionWriter
    {
        private readonly ITestOutputHelper m_output;

        private static Module m_module = new("Test");

        public TestFunctionWriter(ITestOutputHelper output)
        {
            m_output = output;
        }

        struct Foo
        { }
        
        struct Intermediate
        { }

        private FunctionDefinition CreateTestMarshalled(TypeDefinition? enclosingType = null)
        {
            var marshaller = new CustomTypeMarshaller(
                new MarshalFormats(fromManagedToIntermediate: "M2I({0})", fromIntermediateToManaged: "I2M({0})",
                    fromNativeToIntermediate: "N2I({0})", fromIntermediateToNative: "I2N({0})"),
                ManagedTypeInfo.GetType<Intermediate>());

            enclosingType ??= TypeDefinition.CreateBuilder(m_module, "Test")
                .WithTypicalArgumentType(NativeTransferType.ByPointer)
                .Build();

            return FunctionDefinition.CreateBuilder(enclosingType, "Test")
                .WithAttribute(SymbolAttribute.Static)
                .WithParameter<Foo>("foo", customMarshaller: marshaller)
                .Build();
        }

        private void GetCodeWriter(out StringWriter sw, out CodeWriter writer)
        {
            sw = new StringWriter();
            sw.NewLine = "\n";
            writer = new CodeWriter(sw);
        }

        [Fact]
        public void TestManagedToNative()
        {
            var binder = new NativeFunctionBinder(CreateTestMarshalled());

            GetCodeWriter(out var str, out var writer);

            binder.Write(writer, MemberCodeComponent.ManagedPart);
            binder.Write(writer, MemberCodeComponent.NativeImplementation);

            m_output.WriteLine(str.ToString());
        }

        [Fact]
        public void TestNativeToManaged()
        {
            var binder = new ManagedFunctionBinder(CreateTestMarshalled());

            GetCodeWriter(out var str, out var writer);

            binder.Write(writer, MemberCodeComponent.NativeClassDeclaration);
            binder.Write(writer, MemberCodeComponent.ManagedPart);

            m_output.WriteLine(str.ToString());
        }
    }
}