// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Unreal.Core;
using Xunit;

namespace Unreal.Tests
{
    public class TestRuntime
    {
        [Fact]
        public void TestMethod()
        {
            var method = typeof(TestRuntime).GetMethod("Test", BindingFlags.Static | BindingFlags.NonPublic)!;

            var ftn = Runtime.GetFunction(method);

            var ftn2 = GetTest();

            Assert.Equal(ftn2, ftn);
        }

        private static unsafe IntPtr GetTest()
        {
            delegate* unmanaged<void> method = &Test;
            return new IntPtr(method);
        }

        [UnmanagedCallersOnly]
        private static void Test()
        { }

        private unsafe int TestRef_Native(int* i, int* j)
        {
            *j = *i * 3;
            return *i * 2;
        }

        private unsafe int TestRef(ref int i, out int j)
        {
            fixed (int* i_m = &i)
            fixed (int* j_m = &j)
                return TestRef_Native(i_m, j_m);
        }
    }
}