using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FixMath.NET
{
    public class Fix64Tests
    {
		private readonly ITestOutputHelper output;

		public Fix64Tests(ITestOutputHelper output)
		{
			if (output == null)
				output = new ConsoleTestOutputHelper();
			this.output = output;
		}

		long[] m_testCases = new[] {
            // Small numbers
            0L, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,
  
            // Integer numbers
            0x100000000, -0x100000000, 0x200000000, -0x200000000, 0x300000000, -0x300000000,
            0x400000000, -0x400000000, 0x500000000, -0x500000000, 0x600000000, -0x600000000,
  
            // Fractions (1/2, 1/4, 1/8)
            0x80000000, -0x80000000, 0x40000000, -0x40000000, 0x20000000, -0x20000000,
  
            // Problematic carry
            0xFFFFFFFF, -0xFFFFFFFF, 0x1FFFFFFFF, -0x1FFFFFFFF, 0x3FFFFFFFF, -0x3FFFFFFFF,
  
            // Smallest and largest values
            long.MaxValue, long.MinValue,
  
            // Large random numbers
            6791302811978701836, -8192141831180282065, 6222617001063736300, -7871200276881732034,
            8249382838880205112, -7679310892959748444, 7708113189940799513, -5281862979887936768,
            8220231180772321456, -5204203381295869580, 6860614387764479339, -9080626825133349457,
            6658610233456189347, -6558014273345705245, 6700571222183426493,
  
            // Small random numbers
            -436730658, -2259913246, 329347474, 2565801981, 3398143698, 137497017, 1060347500,
            -3457686027, 1923669753, 2891618613, 2418874813, 2899594950, 2265950765, -1962365447,
            3077934393

            // Tiny random numbers
            - 171,
            -359, 491, 844, 158, -413, -422, -737, -575, -330,
            -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993
        };

        [Fact]
        public void Precision()
        {
            Assert.Equal(FP.Precision, 0.00000000023283064365386962890625m);
        }

        [Fact]
        public void LongToFix64AndBack()
        {
            var sources = new[] { long.MinValue, int.MinValue - 1L, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MaxValue + 1L, long.MaxValue };
            var expecteds = new[] { 0L, int.MaxValue, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MinValue, -1L };
            for (int i = 0; i < sources.Length; ++i)
            {
                var expected = expecteds[i];
                var f = (FP)sources[i];
                var actual = (long)f;
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void DoubleToFix64AndBack()
        {
            var sources = new[] {
                (double)int.MinValue,
                -(double)Math.PI,
                -(double)Math.E,
                -1.0,
                -0.0,
                0.0,
                1.0,
                (double)Math.PI,
                (double)Math.E,
                (double)int.MaxValue
            };

            foreach (var value in sources)
            {
                AreEqualWithinPrecision(value, (double)(FP)value);
            }
        }

        static void AreEqualWithinPrecision(decimal value1, decimal value2)
        {
            Assert.True(Math.Abs(value2 - value1) < FP.Precision);
        }

        static void AreEqualWithinPrecision(double value1, double value2)
        {
            Assert.True(Math.Abs(value2 - value1) < (double)FP.Precision);
        }

        [Fact]
        public void DecimalToFix64AndBack()
        {

            Assert.Equal(FP.MaxValue, (FP)(decimal)FP.MaxValue);
            Assert.Equal(FP.MinValue, (FP)(decimal)FP.MinValue);

            var sources = new[] {
                int.MinValue,
                -(decimal)Math.PI,
                -(decimal)Math.E,
                -1.0m,
                -0.0m,
                0.0m,
                1.0m,
                (decimal)Math.PI,
                (decimal)Math.E,
                int.MaxValue
            };

            foreach (var value in sources)
            {
                AreEqualWithinPrecision(value, (decimal)(FP)value);
            }
        }

        [Fact]
        public void Addition()
        {
#if CHECKMATH
            var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { (Fix64)(-1), (Fix64)2, (Fix64)(-1.5m), (Fix64)(-2), Fix64.One };
			var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
#else
			var terms1 = new[] { (FP)(-1), FP.Zero, FP.One};
			var terms2 = new[] { (FP)2, (FP)(-1.5m), (FP)(-2)};
			var expecteds = new[] { FP.One, (FP)(-1.5m), (FP)(-1), FP.MaxValue };
#endif
			for (int i = 0; i < terms1.Length; ++i)
            {
                var actual = terms1[i] + terms2[i];
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Subtraction()
        {
#if CHECKMATH
			var terms1 = new[] { Fix64.MinValue, (Fix64)(-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
            var terms2 = new[] { Fix64.One, (Fix64)(-2), (Fix64)(1.5m), (Fix64)(2), (Fix64)(-1) };
			var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64)(-1.5m), (Fix64)(-1), Fix64.MaxValue };
#else
			var terms1 = new[] { (FP)(-1), FP.Zero, FP.One };
			var terms2 = new[] { (FP)(-2), (FP)(1.5m), (FP)(2) };
			var expecteds = new[] { FP.One, (FP)(-1.5m), (FP)(-1) };
#endif
            for (int i = 0; i < terms1.Length; ++i)
            {
                var actual = terms1[i] - terms2[i];
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void BasicMultiplication()
        {
            var term1s = new[] { 0m, 1m, -1m, 5m, -5m, 0.5m, -0.5m, -1.0m };
            var term2s = new[] { 16m, 16m, 16m, 16m, 16m, 16m, 16m, -1.0m };
            var expecteds = new[] { 0L, 16, -16, 80, -80, 8, -8, 1 };
            for (int i = 0; i < term1s.Length; ++i)
            {
                var expected = expecteds[i];
                var actual = (long)((FP)term1s[i] * (FP)term2s[i]);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void MultiplicationTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
				for (int j = 0; j < m_testCases.Length; ++j)
				{
					var x = FP.FromRaw(m_testCases[i]);
					var y = FP.FromRaw(m_testCases[j]);
					var xM = (decimal)x;
					var yM = (decimal)y;
					var expected = xM * yM;

					bool overflow = false;
					if (expected > (decimal)FP.MaxValue)
					{
						expected = (decimal)FP.MaxValue;
						overflow = true;
					}
					else if (expected < (decimal)FP.MinValue)
					{
						expected = (decimal)FP.MinValue;
						overflow = true;
					}
                    sw.Start();
					FP actual;
					if (overflow)
						actual = FP.SafeMul(x, y);
					else
						actual = x * y;
                    sw.Stop();
                    var actualM = (decimal)actual;
                    var maxDelta = (decimal)FP.FromRaw(1);
                    if (Math.Abs(actualM - expected) > maxDelta)
                    {
                        output.WriteLine("Failed for FromRaw({0}) * FromRaw({1}): expected {2} but got {3}",
                                          m_testCases[i],
                                          m_testCases[j],
                                          (FP)expected,
                                          actualM);
                        ++failures;
                    }
                }
            }
            output.WriteLine("{0} total, {1} per multiplication", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.True(failures < 1);
        }


        static void Ignore<T>(T value) { }

        [Fact]
        public void DivisionTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var x = FP.FromRaw(m_testCases[i]);
                    var y = FP.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;

                    if (m_testCases[j] == 0)
                    {
						var actual = x / y;
						Assert.Equal(actual, FP.MaxValue);
                    }
                    else
                    {
                        var expected = xM / yM;
                        expected =
                            expected > (decimal)FP.MaxValue
                                ? (decimal)FP.MaxValue
                                : expected < (decimal)FP.MinValue
                                      ? (decimal)FP.MinValue
                                      : expected;
                        sw.Start();
                        var actual = x / y;
                        sw.Stop();
                        var actualM = (decimal)actual;
                        var maxDelta = (decimal)FP.FromRaw(1);
                        if (Math.Abs(actualM - expected) > maxDelta)
                        {
                            output.WriteLine("Failed for FromRaw({0}) / FromRaw({1}): expected {2} but got {3}",
                                              m_testCases[i],
                                              m_testCases[j],
                                              (FP)expected,
                                              actualM);
                            ++failures;
                        }
                    }
                }
            }
            output.WriteLine("{0} total, {1} per division", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.True(failures < 1);
        }



        [Fact]
        public void Sign()
        {
            var sources = new[] { FP.MinValue, (FP)(-1), FP.Zero, FP.One, FP.MaxValue };
            var expecteds = new[] { -1, -1, 0, 1, 1 };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = FP.Sign(sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Abs()
        {
            Assert.Equal(FP.MaxValue, FP.Abs(FP.MinValue));
            var sources = new[] { -1, 0, 1, int.MaxValue };
            var expecteds = new[] { 1, 0, 1, int.MaxValue };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = FP.Abs((FP)sources[i]);
                var expected = (FP)expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Floor()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -6m, -1, 0, 1, 5m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)FP.Floor((FP)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void Ceiling()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -5m, -1, 0, 1, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)FP.Ceiling((FP)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
#if CHECKMATH
			Assert.Equal(Fix64.MaxValue, Fix64.Ceiling(Fix64.MaxValue));
#endif
        }

        [Fact]
        public void Round()
        {
            var sources = new[] { -5.5m, -5.1m, -4.5m, -4.4m, -1, 0, 1, 4.5m, 4.6m, 5.4m, 5.5m };
            var expecteds = new[] { -6m, -5m, -4m, -4m, -1, 0, 1, 4m, 5m, 5m, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)FP.Round((FP)sources[i]);
                var expected = expecteds[i];
                Assert.Equal(expected, actual);
            }
#if CHECKMATH
			// Rounding MaxValue fails in optimised builds without CHECKMATH
			Assert.Equal(Fix64.MaxValue, Fix64.Round(Fix64.MaxValue));
#endif
        }


        [Fact]
        public void Sqrt()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var f = FP.FromRaw(m_testCases[i]);
                if (FP.Sign(f) < 0)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => FP.Sqrt(f));
                }
                else
                {
                    var expected = Math.Sqrt((double)f);
                    var actual = (double)FP.Sqrt(f);
                    var delta = (decimal)Math.Abs(expected - actual);
                    Assert.True(delta <= FP.Precision);
                }
            }
        }

		[Fact]
		public void Log2()
		{
			double maxDelta = (double)(FP.Precision * 4);

			for (int j = 0; j < m_testCases.Length; ++j)
			{
				var b = FP.FromRaw(m_testCases[j]);

				if (b <= FP.Zero)
				{
					Assert.Throws<ArgumentOutOfRangeException>(() => FP.Log2(b));
				}
				else
				{
					var expected = Math.Log((double)b) / Math.Log(2);
					var actual = (double)FP.Log2(b);
					var delta = Math.Abs(expected - actual);

					Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
				}
			}
		}

		[Fact]
		public void Ln()
		{
			for (int j = 0; j < m_testCases.Length; ++j)
			{
				var b = FP.FromRaw(m_testCases[j]);

				if (b <= 0)
					continue;

				if (b > FP.MaxValue / 100)
					continue;

				// Reduced precision requirements for very small values
				double maxDelta = b < 0.000001m ? 0.1 : (double)(FP.Precision * 10);

				var expected = Math.Log((double)b);
				var actual = (double)FP.Ln(b);
				var delta = Math.Abs(expected - actual);

				Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
			}
		}

		[Fact]
		public void Pow2()
		{
			var test = (double)FP.Pow2((FP)3);
			double maxDelta = 0.0000001;
			for (int i = 0; i < m_testCases.Length; ++i)
			{
				var e = FP.FromRaw(m_testCases[i]);

				var expected = Math.Min(Math.Pow(2, (double)e), (double)FP.MaxValue);
				var actual = (double)FP.Pow2(e);
				var delta = Math.Abs(expected - actual);

				Assert.True(delta <= maxDelta, string.Format("Pow2({0}) = expected {1} but got {2}", e, expected, actual));
			}
		}

		[Fact]
		public void Pow()
		{
			for (int i = 0; i < m_testCases.Length; ++i)
			{
				var b = FP.FromRaw(m_testCases[i]);

				for (int j = 0; j < m_testCases.Length; ++j)
				{
					var e = FP.FromRaw(m_testCases[j]);

					if (b < FP.Zero && e != FP.Zero)
					{
						Assert.Throws<ArgumentOutOfRangeException>(() => FP.Pow(b, e));
					}
					else
					{
						var expected = e == FP.Zero ? 1 : b == FP.Zero ? 0 : Math.Min(Math.Pow((double)b, (double)e), (double)FP.MaxValue);

						double maxDelta = Math.Abs((double)e) > 100000000 ? 0.5 : expected > 100000000 ? 10 : expected > 1000 ? 0.5 : 0.00001;

						var actual = (double)FP.Pow(b, e);
						var delta = Math.Abs(expected - actual);

						Assert.True(delta <= maxDelta, string.Format("Pow({0}, {1}) = expected {2} but got {3}", b, e, expected, actual));
					}
				}
			}
		}

		[Fact]
        public void Modulus()
        {
            var deltas = new List<decimal>();
            foreach (var operand1 in m_testCases)
            {
                foreach (var operand2 in m_testCases)
                {
                    var f1 = FP.FromRaw(operand1);
                    var f2 = FP.FromRaw(operand2);

                    if (operand2 == 0)
                    {
						continue;
                    }
                    else
                    {
                        var d1 = (decimal)f1;
                        var d2 = (decimal)f2;
                        var actual = (decimal)(f1 % f2);
                        var expected = d1 % d2;
                        var delta = Math.Abs(expected - actual);
                        deltas.Add(delta);
                        Assert.True(delta <= 60 * FP.Precision, string.Format("{0} % {1} = expected {2} but got {3}", f1, f2, expected, actual));
                    }
                }
            }
            output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
            output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
            output.WriteLine("failed: {0}%", deltas.Count(d => d > FP.Precision) * 100.0 / deltas.Count);
        }

        //[Fact]
        //public void SinBenchmark()
        //{
        //    var deltas = new List<double>();

        //    var swf = new Stopwatch();
        //    var swd = new Stopwatch();

        //    // Restricting the range to from 0 to Pi/2
        //    for (var angle = 0.0; angle <= 2 * Math.PI; angle += 0.000004)
        //    {
        //        var f = (Fix64)angle;
        //        swf.Start();
        //        var actualF = Fix64.Sin(f);
        //        swf.Stop();
        //        var actual = (double)actualF;
        //        swd.Start();
        //        var expectedD = Math.Sin(angle);
        //        swd.Stop();
        //        var expected = (double)expectedD;
        //        var delta = Math.Abs(expected - actual);
        //        deltas.Add(delta);
        //    }
        //    output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / (double)Fix64.Precision);
        //    output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / (double)Fix64.Precision);
        //    output.WriteLine("Fix64.Sin time = {0}ms, Math.Sin time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        //}

        [Fact]
        public void Sin()
        {
            Assert.True(FP.Sin(FP.Zero) == FP.Zero);

            Assert.True(FP.Sin(FP.PiOver2) == FP.One);
            Assert.True(FP.Sin(FP.Pi) == FP.Zero);
            Assert.True(FP.Sin(FP.Pi + FP.PiOver2) == -FP.One);
            Assert.True(FP.Sin(FP.PiTimes2) == FP.Zero);

            Assert.True(FP.Sin(-FP.PiOver2) == -FP.One);
            Assert.True(FP.Sin(-FP.Pi) == FP.Zero);
            Assert.True(FP.Sin(-FP.Pi - FP.PiOver2) == FP.One);
            Assert.True(FP.Sin(-FP.PiTimes2) == FP.Zero);


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (FP)angle;
                var actualF = FP.Sin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 3 * FP.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = FP.FromRaw(val);
                var actualF = FP.Sin(f);
                var expected = (decimal)Math.Sin((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.003M, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }

            output.WriteLine("Max delta = {0}", m_testCases.Max(val => {
                var f = FP.FromRaw(val);
                var actualF = FP.Sin(f);
                var expected = (decimal)Math.Sin((double)f);
                return Math.Abs(expected - (decimal)actualF);
            }));
        }

        [Fact]
        public void FastSin()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (FP)angle;
                var actualF = FP.FastSin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * FP.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = FP.FromRaw(val);
                var actualF = FP.FastSin(f);
                var expected = (decimal)Math.Sin((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

		[Fact]
		public void Acos()
		{
			decimal maxDelta = FP.Precision * 20;
			var deltas = new List<decimal>();

			// Precision
			for (var x = -1.0; x < 1.0; x += 0.001)
			{
				var xf = (FP)x;
				var actual = (decimal)FP.Acos(xf);
				var expected = (decimal)Math.Acos((double)xf);
				var delta = Math.Abs(actual - expected);
				deltas.Add(delta);
				Assert.True(delta <= maxDelta, string.Format("Precision: Acos({0}): expected {1} but got {2}", xf, expected, actual));
			}

			for (int i = 0; i < m_testCases.Length; ++i)
			{
				var b = FP.FromRaw(m_testCases[i]);

				if (b < -FP.One)
					continue;
				if (b > FP.One)
					continue;

				var expected = (decimal)Math.Acos((double)b);
				var actual = (decimal)FP.Acos(b);
				var delta = Math.Abs(expected - actual);
				deltas.Add(delta);
				Assert.True(delta <= maxDelta, string.Format("Acos({0}) = expected {1} but got {2}", b, expected, actual));
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
		}

		[Fact]
        public void Cos()
        {
            Assert.True(FP.Cos(FP.Zero) == FP.One);

            Assert.True(FP.Cos(FP.PiOver2) == FP.Zero);
            Assert.True(FP.Cos(FP.Pi) == -FP.One);
            Assert.True(FP.Cos(FP.Pi + FP.PiOver2) == FP.Zero);
            Assert.True(FP.Cos(FP.PiTimes2) == FP.One);

            Assert.True(FP.Cos(-FP.PiOver2) == -FP.Zero);
            Assert.True(FP.Cos(-FP.Pi) == -FP.One);
            Assert.True(FP.Cos(-FP.Pi - FP.PiOver2) == FP.Zero);
            Assert.True(FP.Cos(-FP.PiTimes2) == FP.One);


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (FP)angle;
                var actualF = FP.Cos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 3 * FP.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = FP.FromRaw(val);
                var actualF = FP.Cos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.004M, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void FastCos()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (FP)angle;
                var actualF = FP.FastCos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * FP.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = FP.FromRaw(val);
                var actualF = FP.FastCos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Fact]
        public void Tan()
        {
            Assert.True(FP.Tan(FP.Zero) == FP.Zero);
            Assert.True(FP.Tan(FP.Pi) == FP.Zero);
            Assert.True(FP.Tan(-FP.Pi) == FP.Zero);

            Assert.True(FP.Tan(FP.PiOver2 - (FP)0.001) > FP.Zero);
            Assert.True(FP.Tan(FP.PiOver2 + (FP)0.001) < FP.Zero);
            Assert.True(FP.Tan(-FP.PiOver2 - (FP)0.001) > FP.Zero);
            Assert.True(FP.Tan(-FP.PiOver2 + (FP)0.001) < FP.Zero);

            for (double angle = 0;/*-2 * Math.PI;*/
		angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (FP)angle;
                var actualF = FP.Tan(f);
                var expected = (decimal)Math.Tan(angle);
                Assert.Equal(actualF > FP.Zero, expected > 0);
                //TODO figure out a real way to test this function
            }

            //foreach (var val in m_testCases) {
            //    var f = (Fix64)val;
            //    var actualF = Fix64.Tan(f);
            //    var expected = (decimal)Math.Tan((double)f);
            //    var delta = Math.Abs(expected - (decimal)actualF);
            //    Assert.True(delta <= 0.01, string.Format("Tan({0}): expected {1} but got {2}", f, expected, actualF));
            //}
        }

		[Fact]
		public void Atan()
		{
			var maxDelta = FP.Precision * 20;
			var deltas = new List<decimal>();

			// Precision
			for (var x = -1.0; x < 1.0; x += 0.0001)
			{
				var xf = (FP)x;
				var actual = (decimal)FP.Atan(xf);
				var expected = (decimal)Math.Atan((double)xf);
				var delta = Math.Abs(actual - expected);
				deltas.Add(delta);
				Assert.True(delta <= maxDelta, string.Format("Precision: Atan({0}): expected {1} but got {2}", xf, expected, actual));
			}

			// Scalability and edge cases
			foreach (var x in m_testCases)
			{
				var xf = (FP)x;
				var actual = (decimal)FP.Atan(xf);
				var expected = (decimal)Math.Atan((double)xf);
				var delta = Math.Abs(actual - expected);
				deltas.Add(delta);
				Assert.True(delta <= maxDelta, string.Format("Scalability: Atan({0}): expected {1} but got {2}", xf, expected, actual));
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
		}

		[Fact]
		public void AtanBenchmark()
		{
			var deltas = new List<decimal>();

			var swf = new Stopwatch();
			var swd = new Stopwatch();

			for (var x = -1.0; x < 1.0; x += 0.001)
			{
				for (int k = 0; k < 1000; ++k)
				{
					var xf = (FP)x;
					swf.Start();
					var actualF = FP.Atan(xf);
					swf.Stop();
					swd.Start();
					var expected = Math.Atan((double)xf);
					swd.Stop();
					deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
				}
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
			output.WriteLine("Fix64.Atan time = {0}ms, Math.Atan time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
		}		

		[Fact]
        public void FastAtan2()
        {
            var deltas = new List<decimal>();
            // Identities
            Assert.Equal(FP.FastAtan2(FP.Zero, -FP.One), FP.Pi);
            Assert.Equal(FP.FastAtan2(FP.Zero, FP.Zero), FP.Zero);
            Assert.Equal(FP.FastAtan2(FP.Zero, FP.One), FP.Zero);
            Assert.Equal(FP.FastAtan2(FP.One, FP.Zero), FP.PiOver2);
            Assert.Equal(FP.FastAtan2(-FP.One, FP.Zero), -FP.PiOver2);

            // Precision
            for (var y = -1.0; y < 1.0; y += 0.01)
            {
                for (var x = -1.0; x < 1.0; x += 0.01)
                {
                    var yf = (FP)y;
                    var xf = (FP)x;
                    var actual = (decimal)FP.FastAtan2(yf, xf);
                    var expected = (decimal)Math.Atan2((double)yf, (double)xf);
                    var delta = Math.Abs(actual - expected);
                    deltas.Add(delta);
                    Assert.True(delta <= 0.005M, string.Format("Precision: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }

            // Scalability and edge cases
            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    var yf = (FP)y;
                    var xf = (FP)x;
                    var actual = (decimal)FP.FastAtan2(yf, xf);
                    var expected = (decimal)Math.Atan2((double)yf, (double)xf);
                    var delta = Math.Abs(actual - expected);
                    deltas.Add(delta);
                    Assert.True(delta <= 0.005M, string.Format("Scalability: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }
            output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
            output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
        }
		
        //[Fact]
        public void FastAtan2Benchmark()
        {
            var deltas = new List<decimal>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    for (int k = 0; k < 1000; ++k)
                    {
                        var yf = (FP)y;
                        var xf = (FP)x;
                        swf.Start();
                        var actualF = FP.FastAtan2(yf, xf);
                        swf.Stop();
                        swd.Start();
                        var expected = Math.Atan2((double)yf, (double)xf);
                        swd.Stop();
                        deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
                    }
                }
            }
            output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
            output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
            output.WriteLine("Fix64.Atan2 time = {0}ms, Math.Atan2 time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }

		[Fact]
		public void Atan2()
		{
			var maxDelta = 0.005m;

			var deltas = new List<decimal>();
			// Identities
			Assert.Equal(FP.Atan2(FP.Zero, -FP.One), FP.Pi);
			Assert.Equal(FP.Atan2(FP.Zero, FP.Zero), FP.Zero);
			Assert.Equal(FP.Atan2(FP.Zero, FP.One), FP.Zero);
			Assert.Equal(FP.Atan2(FP.One, FP.Zero), FP.PiOver2);
			Assert.Equal(FP.Atan2(-FP.One, FP.Zero), -FP.PiOver2);

			// Precision
			for (var y = -1.0; y < 1.0; y += 0.01)
			{
				for (var x = -1.0; x < 1.0; x += 0.01)
				{
					var yf = (FP)y;
					var xf = (FP)x;
					var actual = (decimal)FP.Atan2(yf, xf);
					var expected = (decimal)Math.Atan2((double)yf, (double)xf);
					var delta = Math.Abs(actual - expected);
					deltas.Add(delta);
					Assert.True(delta <= maxDelta, string.Format("Precision: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
				}
			}

			// Scalability and edge cases
			foreach (var y in m_testCases)
			{
				foreach (var x in m_testCases)
				{
					var yf = (FP)y;
					var xf = (FP)x;
					var actual = (decimal)FP.Atan2(yf, xf);
					var expected = (decimal)Math.Atan2((double)yf, (double)xf);
					var delta = Math.Abs(actual - expected);
					deltas.Add(delta);
					Assert.True(delta <= maxDelta, string.Format("Scalability: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
				}
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
		}

		//[Fact]
		public void Atan2Benchmark()
		{
			var deltas = new List<decimal>();

			var swf = new Stopwatch();
			var swd = new Stopwatch();

			foreach (var y in m_testCases)
			{
				foreach (var x in m_testCases)
				{
					for (int k = 0; k < 1000; ++k)
					{
						var yf = (FP)y;
						var xf = (FP)x;
						swf.Start();
						var actualF = FP.Atan2(yf, xf);
						swf.Stop();
						swd.Start();
						var expected = Math.Atan2((double)yf, (double)xf);
						swd.Stop();
						deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
					}
				}
			}
			output.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / FP.Precision);
			output.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / FP.Precision);
			output.WriteLine("Fix64.Atan2 time = {0}ms, Math.Atan2 time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
		}

		[Fact]
        public void Negation()
        {
            foreach (var operand1 in m_testCases)
            {
                var f = FP.FromRaw(operand1);
                if (f == FP.MinValue)
                {
                    Assert.Equal(-f, FP.MaxValue);
                }
                else
                {
                    var expected = -((decimal)f);
                    var actual = (decimal)(-f);
                    Assert.Equal(expected, actual);
                }
            }
        }

        [Fact]
        public void Equals2()
        {
            foreach (var op1 in m_testCases)
            {
                foreach (var op2 in m_testCases)
                {
                    var d1 = (decimal)op1;
                    var d2 = (decimal)op2;
                    Assert.True(op1.Equals(op2) == d1.Equals(d2));
                }
            }
        }

        [Fact]
        public void EqualityAndInequalityOperators()
        {
            var sources = m_testCases.Select(FP.FromRaw).ToList();
            foreach (var op1 in sources)
            {
                foreach (var op2 in sources)
                {
                    var d1 = (double)op1;
                    var d2 = (double)op2;
                    Assert.True((op1 == op2) == (d1 == d2));
                    Assert.True((op1 != op2) == (d1 != d2));
                    Assert.False((op1 == op2) && (op1 != op2));
                }
            }
        }

        [Fact]
        public void CompareTo()
        {
            var nums = m_testCases.Select(FP.FromRaw).ToArray();
            var numsDecimal = nums.Select(t => (decimal)t).ToArray();
            Array.Sort(nums);
            Array.Sort(numsDecimal);
            Assert.True(nums.Select(t => (decimal)t).SequenceEqual(numsDecimal));
        }
    }
}
