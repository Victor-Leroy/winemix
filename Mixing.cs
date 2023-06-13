using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blender
{
    public class Mix
    {
        public Mix(params double[] values)
            : this((IReadOnlyList<double>)values)
        { }

        public Mix(IReadOnlyList<double> values)
        {
            Values = values;
            Sum = Values.Sum();
            var sumSqrs = 0.0;
            for (var i = 0; i < Values.Count; i++)
            {
                sumSqrs += Values[i] * Values[i];
            }
            Length = Math.Sqrt(sumSqrs);
        }

        public IReadOnlyList<double> Values { get; }
        public int Count => Values.Count;
        public double Sum { get; }
        public double Length { get; }

        public double Distance(Mix? other)
            => other == null
                ? double.MaxValue
                : (other - this).Length;

        public static Mix Add(Mix mixA, Mix mixB)
        {
            var tmp = new double[mixA.Count];
            for (var i = 0; i < mixA.Count; ++i)
            {
                tmp[i] = mixA.Values[i] + mixB.Values[i];
            }

            return new Mix(tmp);
        }

        public static Mix Subtract(Mix mixA, Mix mixB)
        {
            var tmp = new double[mixA.Count];
            for (var i = 0; i < mixA.Count; ++i)
            {
                tmp[i] = mixA.Values[i] - mixB.Values[i];
            }

            return new Mix(tmp);
        }

        public static Mix operator +(Mix a, Mix b)
            => Add(a, b);

        public static Mix operator -(Mix a, Mix b)
            => Subtract(a, b);

        public static Mix operator *(Mix mix, double x)
            => Multiply(mix, x);

        public static Mix operator /(Mix mix, double x)
            => Multiply(mix, 1.0 / x);

        public Mix Normal
            => Length.AlmostEquals(0) ? this : this / Length;

        public static Mix Multiply(Mix mix, double x)
        {
            var tmp = new double[mix.Count];
            for (var i = 0; i < mix.Count; ++i)
            {
                tmp[i] = mix.Values[i] * x;
            }

            return new Mix(tmp);
        }

        public override string ToString()
        {
            return $"({string.Join(", ", Values.Select(x => $"{x:0.###}"))})";
        }

        public int CountUsedWines()
        {
            return Values.Count(x => x > 0);
        }

        public static Mix LoadFromFile(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            var r = new List<double>();
            foreach (var line in lines)
            {
                var amt = double.Parse(line);
                r.Add(amt);
            }
            return new Mix(r);
        }

        public static Mix CreateFromIndex(int index, int numWines)
        {
            var tmp = new double[numWines];
            tmp[index] = 1.0;
            return new Mix(tmp);
        }

        public Mix Lerp(Mix other, double amount = 0.5)
        {
            var tmp = new double[Count];
            for (var i = 0; i < Count; ++i)
            {
                tmp[i] = (Values[i] * 1.0 - amount) + (other.Values[i] * amount);
            }

            return new Mix(tmp);
        }

        public double DistanceOfNormals(Mix? other)
        {
            return Normal.Distance(other?.Normal);
        }
    }
}
