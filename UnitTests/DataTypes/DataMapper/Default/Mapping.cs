﻿/*
Copyright (c) 2012 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System.Reflection;
using Utilities.DataTypes;
using Utilities.DataTypes.DataMapper.Default;
using Xunit;
using System.Linq;

namespace UnitTests.DataTypes.DataMapper.Default
{
    public class Mapping
    {
        [Fact]
        public void CreationTest()
        {
            Utilities.DataTypes.DataMapper.Default.Mapping<MappingA, MappingB> TempObject = null;
            Assert.DoesNotThrow(() => TempObject = new Mapping<MappingA, MappingB>(x => x.Item1, x => x.Item1));
            Assert.NotNull(TempObject);
        }

        [Fact]
        public void LeftToRight()
        {
            Utilities.DataTypes.DataMapper.Default.Mapping<MappingA, MappingB> TempObject = new Mapping<MappingA, MappingB>(x => x.Item1, x => x.Item1);
            MappingA A = new MappingA();
            A.Item1 = 12;
            A.Item2 = "ASDF";
            MappingB B = new MappingB();
            B.Item1 = 13;
            B.Item2 = "ZXCV";
            TempObject.CopyLeftToRight(A, B);
            Assert.Equal(B.Item1, 12);
            Assert.NotEqual(B.Item2, "ASDF");
        }

        [Fact]
        public void RightToLeft()
        {
            Utilities.DataTypes.DataMapper.Default.Mapping<MappingA, MappingB> TempObject = new Mapping<MappingA, MappingB>(x => x.Item1, x => x.Item1);
            MappingA A = new MappingA();
            A.Item1 = 12;
            A.Item2 = "ASDF";
            MappingB B = new MappingB();
            B.Item1 = 13;
            B.Item2 = "ZXCV";
            TempObject.CopyRightToLeft(B, A);
            Assert.Equal(A.Item1, 13);
            Assert.NotEqual(A.Item2, "ZXCV");
        }

        [Fact]
        public void NullLeftToRight()
        {
            Utilities.DataTypes.DataMapper.Default.Mapping<MappingA, MappingB> TempObject = new Mapping<MappingA, MappingB>(null, x => x.Item1);
            MappingA A = new MappingA();
            A.Item1 = 12;
            A.Item2 = "ASDF";
            MappingB B = new MappingB();
            B.Item1 = 13;
            B.Item2 = "ZXCV";
            TempObject.CopyLeftToRight(A, B);
            Assert.Equal(13, B.Item1);
            Assert.Equal("ZXCV", B.Item2);
        }

        [Fact]
        public void NullRightToLeft()
        {
            Utilities.DataTypes.DataMapper.Default.Mapping<MappingA, MappingB> TempObject = new Mapping<MappingA, MappingB>(x => x.Item1, null);
            MappingA A = new MappingA();
            A.Item1 = 12;
            A.Item2 = "ASDF";
            MappingB B = new MappingB();
            B.Item1 = 13;
            B.Item2 = "ZXCV";
            TempObject.CopyRightToLeft(B, A);
            Assert.Equal(12, A.Item1);
            Assert.Equal("ASDF", A.Item2);
        }
    }

    public class MappingA
    {
        public int Item1 { get; set; }
        public string Item2 { get; set; }
    }

    public class MappingB
    {
        public int Item1 { get; set; }
        public string Item2 { get; set; }
    }
}