using System;

namespace FixMath.NET
{
	public class Fix64Random
    {
        private Random random;

        public Fix64Random(int seed)
        {
            random = new Random(seed);
        }

        public FP Next()
        {
            FP result = new FP();
            result.RawValue = (uint)random.Next(int.MinValue, int.MaxValue);
            return result;
        }

        public FP NextInt(int maxValue)
        {
            return random.Next(maxValue);
        }
    }
}
