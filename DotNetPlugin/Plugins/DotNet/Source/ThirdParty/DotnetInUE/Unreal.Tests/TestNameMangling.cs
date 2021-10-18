// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Xunit;

namespace Unreal.Tests
{
    public class TestNameMangling
    {
        [Fact]
        public void TestSanitize()
        {
            // Note: As it turns out the C# compiler does not find characters
            // outside the BMP to be valid for identifier names.
            
            // This means the '𐐷' character bellow is not valid in C#, but it's good that we support it regardless.
            // It's unfortunate that C ABI only allows ascii alnum and underscore, so I can't really escape unicode in a reversible way. 
            var name = "_ação_de_nominho_𐐷";

            var mangled = NameMangler.SanitizeName(name);

            const string Expected = "_aue7ue3o_de_nominho_u10437";

            Assert.Equal(Expected, mangled);
        }
    }
}