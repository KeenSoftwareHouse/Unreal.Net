// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.IO;
using Unreal.Generation;
using Xunit;

namespace Unreal.Tests
{
    public class TestCodeWriter
    {
        [Fact]
        public void TestBlock()
        {
            var str = new StringWriter();
            str.NewLine = "\n";
            var writer = new CodeWriter(str, CodeWriter.NativeIndent);

            using (writer.OpenBlock())
                writer.WriteLine("Test");

            writer.Flush();

            var result = str.ToString();
            var expected = "{\n\tTest\n}\n";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestUnIndent()
        {
            var str = new StringWriter();
            str.NewLine = "\n";
            var writer = new CodeWriter(str, CodeWriter.NativeIndent);

            using (writer.OpenBlock())
            {
                using (writer.UnIndent())
                    writer.WriteLine("Prefix:");

                writer.WriteLine("Test");
            }

            writer.Flush();

            var result = str.ToString();
            var expected = "{\nPrefix:\n\tTest\n}\n";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TestMultiline()
        {
            var str = new StringWriter {NewLine = "\n"};
            var writer = new CodeWriter(str, CodeWriter.NativeIndent);

            writer.PushIndent(".");
            writer.WriteLine(@".
.
.".Replace("\r\n", "\n"));
            var expected = @"..
..
..
".Replace("\r\n", "\n");
            Assert.Equal(expected, str.ToString());
        }
    }
}