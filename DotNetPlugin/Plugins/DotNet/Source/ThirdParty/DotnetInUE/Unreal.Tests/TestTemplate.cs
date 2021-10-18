// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using Unreal.Generation;
using Xunit;

namespace Unreal.Tests
{
    public class TestTemplate
    {
        class Model
        {
            #pragma warning disable 414
            private string Hi = "Hi!";

            private int FortyTwo = 42;

            private float Pi = 3.14f;
            #pragma warning restore 414
        }

        [Fact]
        public void TestReplacement()
        {
            string template = "Hi: {Hi}, FortyTwo: {FortyTwo}, Pi: {Pi}";

            var result = TemplateWriter.WriteTemplate(template, new Model());

            var expected = "Hi: Hi!, FortyTwo: 42, Pi: 3.14";

            Assert.Equal(expected, result);
        }
    }
}