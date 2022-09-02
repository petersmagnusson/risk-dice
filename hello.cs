/* run it:

   csc /unsafe hello.cs && mono hello.exe

   */

using System;
using System.Collections.Generic;
using Risk.Dice;
using Risk.Dice.Utility;
using UnityEngine;
using System.Linq;


namespace Risk.Dice.Utility
{
    public struct DescendingIntComparer : IComparer<int>
    {
        int IComparer<int>.Compare (int x, int y)
        {
            return y - x;
        }
    }
}

namespace UnityEngine
{
    public static class Mathf
    {
        public static int FloorToInt (float value)
        {
            return (int) value;
        }

        public static int RoundToInt (float value)
        {
            return (int) Math.Round(value);
        }

        public static float Pow (float value, float power)
        {
            return (float) Math.Pow(value, power);
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class SerializeFieldAttribute : Attribute
    {

    }
}


namespace Risk.Dice.Utility
{
    public static class MathUtil
    {
        public static unsafe double Increment (double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return value;
            }

            ulong intValue = *(ulong*) &value;

            if (value > 0)
            {
                intValue++;
            }
            else if (value < 0)
            {
                intValue--;
            }
            else if (value == 0)
            {
                return double.Epsilon;
            }

            return *(double*) &intValue;
        }

        public static unsafe double Decrement (double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return value;
            }

            ulong intValue = *(ulong*) &value;

            if (value > 0)
            {
                intValue--;
            }
            else if (value < 0)
            {
                intValue++;
            }
            else if (value == 0)
            {
                return -double.Epsilon;
            }

            return *(double*) &intValue;
        }

        public static double RoundToNearest (double source, double nearest)
        {
            double inverse = Math.Pow(nearest, -1);

            return Math.Round(source * inverse, MidpointRounding.AwayFromZero) / inverse;
        }

        public static double RoundToNearest (double source, double nearest, double zeroOffset)
        {
            double roundedSouce = RoundToNearest(source, nearest);
            double offset = zeroOffset % nearest;

            return roundedSouce + offset;
        }

        public static int NextPowerOfTwo (int value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }

        public static int FloorToInt (double value)
        {
            return (int) Math.Floor(value);
        }

        public static int RoundToInt (double value)
        {
            return (int) Math.Round(value);
        }

        public static int CeilToInt (double value)
        {
            return (int) Math.Ceiling(value);
        }

        public static bool IsApproximatelyEqual (double a, double b, double tolerance = 0.0001)
        {
            return Math.Abs(a - b) <= tolerance;
        }

        public static int Clamp (int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static float Clamp (float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static double Clamp (double value, double min, double max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static double Clamp01 (double value)
        {
            if (value >= 1.0)
            {
                return 1.0;
            }

            if (value <= 0.0)
            {
                return 0.0;
            }

            return value;
        }

        public static double Clamp01Exclusive (double value)
        {
            if (value >= 1.0)
            {
                return Decrement(1.0);
            }

            if (value <= 0.0)
            {
                return Increment(0.0);
            }

            return value;
        }

        public static double Lerp (double min, double max, double t, bool clamp = false)
        {
            return min + ((max - min) * (clamp ? Clamp01(t) : t));
        }

        public static double Normalize (double source, double curMin, double curMax, double newMin, double newMax, bool clamp = false)
        {
            double t = (source - curMin) / (curMax - curMin);

            return newMin + ((newMax - newMin) * (clamp ? Clamp01(t) : t));
        }

        public static double Normalize (double source, double curMin, double curMax, bool clamp = false)
        {
            return Normalize(source, curMin, curMax, 0, 1, clamp);
        }

        public static double Normalize (double source, double curMax, bool clamp = false)
        {
            return Normalize(source, 0, curMax, 0, 1, clamp);
        }

        public static double PercentDifference (double a, double b)
        {
            return Math.Abs((a - b) / ((a + b) * 0.5)) * 100.0;
        }

        public static double UniformDeviation (double min, double max)
        {
            return Math.Sqrt(Math.Pow(max - min, 2) / 12.0);
        }

        public static double UniformDeviation (int min, int max)
        {
            return Math.Sqrt(Math.Pow(max - min, 2) / 12.0);
        }

        public static double UniformDeviation (uint min, uint max)
        {
            return Math.Sqrt(Math.Pow(max - min, 2) / 12.0);
        }

        public static double UniformDeviation (ulong min, ulong max)
        {
            return Math.Sqrt(Math.Pow(max - min, 2) / 12.0);
        }

        public static double SumAsDouble (this IList<double> values)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumAsDouble (this IList<int> values)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumAsDouble (this IList<uint> values)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumAsDouble (this IList<ulong> values)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumAsDouble (this IList<byte> values)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumAsDouble (this IList<double> values, int offset, int length = -1)
        {
            if (values == null)
            {
                return default;
            }

            double sum = 0;
            int limit = length >= 0 ? Math.Min(values.Count, offset + length) : values.Count;

            for (int i = offset; i < limit; i++)
            {
                sum += values[i];
            }

            return sum;
        }

        public static double SumOfSquares (IList<int> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }

            double squareSum = 0f;

            for (int i = 0; i < values.Count; i++)
            {
                squareSum += values[i] * values[i];
            }

            double average = Mean(values);

            squareSum -= average * average * values.Count;

            return squareSum;
        }

        public static double SumOfSquares (IList<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }

            double squareSum = 0f;

            for (int i = 0; i < values.Count; i++)
            {
                squareSum += values[i] * values[i];
            }

            double average = Mean(values);

            squareSum -= average * average * values.Count;

            return squareSum;
        }

        public static double Mean (this IList<double> values)
        {
            if (values == null)
            {
                return default;
            }

            return SumAsDouble(values) / values.Count;
        }

        public static double Mean (this IList<int> values)
        {
            if (values == null)
            {
                return default;
            }

            return SumAsDouble(values) / values.Count;
        }

        public static double Mean (this IList<uint> values)
        {
            if (values == null)
            {
                return default;
            }

            return SumAsDouble(values) / values.Count;
        }

        public static double Mean (this IList<ulong> values)
        {
            if (values == null)
            {
                return default;
            }

            return SumAsDouble(values) / values.Count;
        }

        public static double Mean (this IList<byte> values)
        {
            if (values == null)
            {
                return default;
            }

            return SumAsDouble(values) / values.Count;
        }

        public static double Max (this IList<double> values, out int index)
        {
            index = -1;

            if (values == null)
            {
                return default;
            }

            int count = values.Count;
            double max = double.NegativeInfinity;

            for (int i = 0; i < count; i++)
            {
                double value = values[i];

                if (value > max)
                {
                    max = value;
                    index = i;
                }
            }

            return max;
        }

        public static double Median (this IList<double> sortedValues)
        {
            if (sortedValues == null)
            {
                return default;
            }

            int count = sortedValues.Count;

            if (count <= 0)
            {
                return default;
            }

            if (count == 1)
            {
                return sortedValues[0];
            }

            if (count % 2 == 0)
            {
                int index = Mathf.RoundToInt(count * 0.5f);

                double value1 = sortedValues[index];
                double value2 = sortedValues[index - 1];

                return (value1 + value2) * 0.5;
            }
            else
            {
                return sortedValues[Mathf.FloorToInt(count * 0.5f)];
            }
        }

        public static double Median (this IList<int> sortedValues)
        {
            if (sortedValues == null)
            {
                return default;
            }

            int count = sortedValues.Count;

            if (count <= 0)
            {
                return default;
            }

            if (count == 1)
            {
                return sortedValues[0];
            }

            if (count % 2 == 0)
            {
                int index = Mathf.RoundToInt(count * 0.5f);

                int value1 = sortedValues[index];
                int value2 = sortedValues[index - 1];

                return ((double) value1 + value2) * 0.5;
            }
            else
            {
                return sortedValues[Mathf.FloorToInt(count * 0.5f)];
            }
        }

        public static double Median (this IList<uint> sortedValues)
        {
            if (sortedValues == null)
            {
                return default;
            }

            int count = sortedValues.Count;

            if (count <= 0)
            {
                return default;
            }

            if (count == 1)
            {
                return sortedValues[0];
            }

            if (count % 2 == 0)
            {
                int index = Mathf.RoundToInt(count * 0.5f);

                uint value1 = sortedValues[index];
                uint value2 = sortedValues[index - 1];

                return ((double) value1 + value2) * 0.5;
            }
            else
            {
                return sortedValues[Mathf.FloorToInt(count * 0.5f)];
            }
        }

        public static double Median (this IList<ulong> sortedValues)
        {
            if (sortedValues == null)
            {
                return default;
            }

            int count = sortedValues.Count;

            if (count <= 0)
            {
                return default;
            }

            if (count == 1)
            {
                return sortedValues[0];
            }

            if (count % 2 == 0)
            {
                int index = Mathf.RoundToInt(count * 0.5f);

                ulong value1 = sortedValues[index];
                ulong value2 = sortedValues[index - 1];

                return ((double) value1 + value2) * 0.5;
            }
            else
            {
                return sortedValues[Mathf.FloorToInt(count * 0.5f)];
            }
        }

        public static double Median (this IList<byte> sortedValues)
        {
            if (sortedValues == null)
            {
                return default;
            }

            int count = sortedValues.Count;

            if (count <= 0)
            {
                return default;
            }

            if (count == 1)
            {
                return sortedValues[0];
            }

            if (count % 2 == 0)
            {
                int index = Mathf.RoundToInt(count * 0.5f);

                byte value1 = sortedValues[index];
                byte value2 = sortedValues[index - 1];

                return ((double) value1 + value2) * 0.5;
            }
            else
            {
                return sortedValues[Mathf.FloorToInt(count * 0.5f)];
            }
        }

        public static double StandardDeviation (this IList<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double relativeSquaredSum = 0f;
            double mean = Mean(values);

            for (int i = 0; i < values.Count; i++)
            {
                relativeSquaredSum += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(relativeSquaredSum / (values.Count - 1));
        }

        public static double StandardDeviation (this IList<int> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double relativeSquaredSum = 0f;
            double mean = Mean(values);

            for (int i = 0; i < values.Count; i++)
            {
                relativeSquaredSum += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(relativeSquaredSum / (values.Count - 1));
        }

        public static double StandardDeviation (this IList<uint> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double relativeSquaredSum = 0f;
            double mean = Mean(values);

            for (int i = 0; i < values.Count; i++)
            {
                relativeSquaredSum += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(relativeSquaredSum / (values.Count - 1));
        }

        public static double StandardDeviation (this IList<ulong> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double relativeSquaredSum = 0f;
            double mean = Mean(values);

            for (int i = 0; i < values.Count; i++)
            {
                relativeSquaredSum += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(relativeSquaredSum / (values.Count - 1));
        }

        public static double StandardDeviation (this IList<byte> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            double relativeSquaredSum = 0f;
            double mean = Mean(values);

            for (int i = 0; i < values.Count; i++)
            {
                relativeSquaredSum += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(relativeSquaredSum / (values.Count - 1));
        }

        public static void NormalizeSum (IList<double> values, double normalizedSum)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            if (normalizedSum <= 0f)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = 0f;
                }

                return;
            }

            double sum = SumAsDouble(values);

            if (sum <= 0f)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = normalizedSum / values.Count;
                }

                return;
            }

            double normalizationRatio = normalizedSum / sum;

            for (int i = 0; i < values.Count; i++)
            {
                values[i] *= normalizationRatio;
            }
        }

        public static void NormalizeSum (IList<double> values, double normalizedSum, int offset, int length = -1)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            int limit = length >= 0 ? Math.Min(values.Count, offset + length) : values.Count;

            if (normalizedSum <= 0f)
            {
                for (int i = offset; i < limit; i++)
                {
                    values[i] *= 0f;
                }

                return;
            }

            double sum = SumAsDouble(values, offset, length);

            if (sum <= 0f)
            {
                for (int i = offset; i < limit; i++)
                {
                    values[i] = normalizedSum / (limit - offset);
                }

                return;
            }

            double normalizationRatio = normalizedSum / sum;

            for (int i = offset; i < limit; i++)
            {
                values[i] *= normalizationRatio;
            }
        }
    }
}

namespace Risk.Dice
{
    public static class RoundCache
    {
        private static readonly object _lock = new object();
        private static RoundInfo _lastRoundInfo;
        private static List<RoundInfo> _cache = new List<RoundInfo>();

        public static List<RoundInfo> Cache => _cache;

        public static RoundInfo Get (RoundConfig config)
        {
            RoundInfo roundInfo = default;

            lock (_lock)
            {
                if (_lastRoundInfo != null && _lastRoundInfo.Config.Equals(config))
                {
                    roundInfo = _lastRoundInfo;
                }

                if (roundInfo == null)
                {
                    foreach (RoundInfo round in _cache)
                    {
                        if (round.Config.Equals(config))
                        {
                            roundInfo = round;
                            break;
                        }
                    }
                }

                if (roundInfo == null)
                {
                    roundInfo = new RoundInfo(config);
                    _cache.Add(roundInfo);
                }

                _lastRoundInfo = roundInfo;
            }

            return roundInfo;
        }

        public static void Clear ()
        {
            _lastRoundInfo = null;

            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }
}

namespace Risk.Dice
{
    [Serializable]
    public struct BattleConfig : IEquatable<BattleConfig>
    {
        private int _attackUnitCount;
        private int _defendUnitCount;
        private int _stopUntil;

        public int AttackUnitCount => _attackUnitCount;
        public int DefendUnitCount => _defendUnitCount;
        public int StopUntil => _stopUntil;

        public bool IsEarlyStop => _stopUntil > 0;

        public BattleConfig (int attackUnitCount, int defendUnitCount, int stopUntil)
        {
            _attackUnitCount = attackUnitCount;
            _defendUnitCount = defendUnitCount;
            _stopUntil = stopUntil;
        }

        public BattleConfig WithNewUnits (int attackUnitCount, int defendUnitCount)
        {
            BattleConfig config = this;
            config._attackUnitCount = attackUnitCount;
            config._defendUnitCount = defendUnitCount;
            return config;
        }

        public BattleConfig WithoutStopUntil ()
        {
            BattleConfig config = this;
            config._attackUnitCount -= _stopUntil;
            config._stopUntil = 0;
            return config;
        }

        public bool Equals (BattleConfig other)
        {
            if (_attackUnitCount != other._attackUnitCount
                    || _defendUnitCount != other._defendUnitCount
                    || _stopUntil != other._stopUntil)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            int hashCode = -1491950684;
            hashCode = hashCode * -1521134295 + _attackUnitCount.GetHashCode();
            hashCode = hashCode * -1521134295 + _defendUnitCount.GetHashCode();
            hashCode = hashCode * -1521134295 + _stopUntil.GetHashCode();
            return hashCode;
        }
    }
}


namespace Risk.Dice
{
    [Flags]
    public enum DiceAugment
    {
        None = 0,
        OnCapital = 1 << 1,
        IsBehindWall = 1 << 2,
        IsZombie = 1 << 3
    }
}

namespace Risk.Dice
{
    [Serializable]
    public sealed class RoundInfo
    {
        private RoundConfig _config;
        private double[] _attackLossChances;

        public RoundConfig Config => _config;
        public bool IsReady => _attackLossChances != null && _attackLossChances.Length > 0;
        public double[] AttackLossChances => _attackLossChances;

        internal RoundInfo ()
        {
        }

        public RoundInfo (RoundConfig config)
        {
            _config = config;
        }

        public void Calculate ()
        {
            if (IsReady)
            {
                return;
            }

            // Localize vars

            int diceFaceCount = _config.DiceFaceCount;
            int attackDiceCount = _config.AttackDiceCount;
            int defendDiceCount = _config.DefendDiceCount;
            int powersLength = attackDiceCount + defendDiceCount + 1;
            bool favourDefenderOnDraw = _config.FavourDefenderOnDraw;
            int challengeCount = _config.ChallengeCount;

            // Calculate powers

            double[] attackLossChances = new double[challengeCount + 1];

            int[] powers = new int[powersLength];

            for (int i = 0; i < powersLength; i++)
            {
                powers[i] = (int) Mathf.Pow(diceFaceCount, i);
            }

            // Calculate losses - Setup

            int totalPermutationCount = powers[powersLength - 1];
            int attackPermutationCount = powers[attackDiceCount];
            int defendPermutationCount = powers[defendDiceCount];

            DescendingIntComparer descendingIntComparer = new DescendingIntComparer();

            int[] attackDiceRolls = new int[attackDiceCount];
            int[] defendDiceRolls = new int[defendDiceCount];
            int[] orderedAttackDiceRolls = new int[attackDiceCount];
            int[] orderedDefendDiceRolls = new int[defendDiceCount];

            int[] attackLosses = new int[totalPermutationCount];

            // Calculate losses - Core

            int permutationIndex = 0;

            for (int a = 0; a < attackPermutationCount; a++)
            {
                // Iterate attack roll values

                for (int i = 0; i < attackDiceCount; i++)
                {
                    if (a % powers[i] == 0)
                    {
                        attackDiceRolls[i]++;
                    }

                    if (attackDiceRolls[i] > diceFaceCount)
                    {
                        attackDiceRolls[i] = 1;
                    }
                }

                for (int d = 0; d < defendPermutationCount; d++)
                {
                    // Iterate defend roll values

                    for (int i = 0; i < defendDiceCount; i++)
                    {
                        if (d % powers[i] == 0)
                        {
                            defendDiceRolls[i]++;
                        }

                        if (defendDiceRolls[i] > diceFaceCount)
                        {
                            defendDiceRolls[i] = 1;
                        }
                    }

                    // Sort rolls in descending order

                    Array.Copy(attackDiceRolls, orderedAttackDiceRolls, attackDiceCount);
                    Array.Copy(defendDiceRolls, orderedDefendDiceRolls, defendDiceCount);

                    Array.Sort(orderedAttackDiceRolls, 0, attackDiceCount, descendingIntComparer);
                    Array.Sort(orderedDefendDiceRolls, 0, defendDiceCount, descendingIntComparer);

                    // Determine losses

                    int attackLossCount = 0;

                    for (int i = 0; i < challengeCount; i++)
                    {
                        if (orderedAttackDiceRolls[i] == orderedDefendDiceRolls[i])
                        {
                            if (favourDefenderOnDraw)
                            {
                                attackLossCount++;
                            }
                        }
                        else if (orderedAttackDiceRolls[i] < orderedDefendDiceRolls[i])
                        {
                            attackLossCount++;
                        }
                    }

                    // Store loss counts

                    attackLosses[permutationIndex] = attackLossCount;

                    permutationIndex++;
                }
            }

            // Calculate chances

            for (int i = 0; i < challengeCount + 1; i++)
            {
                int unitLossCount = 0;

                for (int j = 0; j < totalPermutationCount; j++)
                {
                    if (attackLosses[j] == i)
                    {
                        unitLossCount++;
                    }
                }

                attackLossChances[i] = (double) unitLossCount / totalPermutationCount;
            }

            // Complete

            _attackLossChances = attackLossChances;
        }
    }
}


namespace Risk.Dice
{
    [Serializable]
    public struct RoundConfig : IEquatable<RoundConfig>
    {
        private int _diceFaceCount;
        private int _attackDiceCount;
        private int _defendDiceCount;
        private bool _favourDefenderOnDraw;

        public int DiceFaceCount => _diceFaceCount;
        public int AttackDiceCount => _attackDiceCount;
        public int DefendDiceCount => _defendDiceCount;
        public bool FavourDefenderOnDraw => _favourDefenderOnDraw;

        public int ChallengeCount => Math.Min(_attackDiceCount, _defendDiceCount);

        public static RoundConfig Default => new RoundConfig(6, 3, 2, true);

        public RoundConfig (int diceFaceCount, int attackDiceCount, int defendDiceCount, bool favourDefenderOnDraw)
        {
            _diceFaceCount = diceFaceCount;
            _attackDiceCount = attackDiceCount;
            _defendDiceCount = defendDiceCount;
            _favourDefenderOnDraw = favourDefenderOnDraw;
        }

        public void SetMaxAttackDice (int maxAttackDice)
        {
            if (maxAttackDice > 0)
            {
                _attackDiceCount = Math.Min(_attackDiceCount, maxAttackDice);
            }
        }

        public void ApplyAugments (DiceAugment attackAugment, DiceAugment defendAugment)
        {
            if ((attackAugment & DiceAugment.IsZombie) != 0)
            {
                _attackDiceCount = Math.Max(_attackDiceCount - 1, 1);
            }

            if ((defendAugment & DiceAugment.OnCapital) != 0)
            {
                _defendDiceCount += 1;
            }

            if ((defendAugment & DiceAugment.IsBehindWall) != 0)
            {
                _defendDiceCount += 1;
            }

            if ((defendAugment & DiceAugment.IsZombie) != 0)
            {
                _favourDefenderOnDraw = false;
            }
        }

        public RoundConfig WithBattle (BattleConfig battleConfig)
        {
            RoundConfig roundConfig = this;
            roundConfig._attackDiceCount = Math.Min(battleConfig.AttackUnitCount - battleConfig.StopUntil, _attackDiceCount);
            roundConfig._defendDiceCount = Math.Min(battleConfig.DefendUnitCount, _defendDiceCount);
            return roundConfig;
        }

        public RoundConfig WithDiceCounts (int attackDiceCount, int defendDiceCount)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(attackDiceCount > 0);
            Assert.IsTrue(defendDiceCount > 0);
#endif

            RoundConfig roundConfig = this;
            roundConfig._attackDiceCount = attackDiceCount;
            roundConfig._defendDiceCount = defendDiceCount;
            return roundConfig;
        }

        public RoundConfig WithAugments (DiceAugment attackAugment, DiceAugment defendAugment)
        {
            RoundConfig roundConfig = this;
            roundConfig.ApplyAugments(attackAugment, defendAugment);
            return roundConfig;
        }

        public bool Equals (RoundConfig other)
        {
            if (_diceFaceCount != other._diceFaceCount
                || _attackDiceCount != other._attackDiceCount
                || _defendDiceCount != other._defendDiceCount
                || _favourDefenderOnDraw != other._favourDefenderOnDraw)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            int hashCode = 1172639866;
            hashCode = hashCode * -1521134295 + _diceFaceCount.GetHashCode();
            hashCode = hashCode * -1521134295 + _attackDiceCount.GetHashCode();
            hashCode = hashCode * -1521134295 + _defendDiceCount.GetHashCode();
            hashCode = hashCode * -1521134295 + _favourDefenderOnDraw.GetHashCode();
            return hashCode;
        }
    }

    public struct RoundConfigComparer : IEqualityComparer<RoundConfig>
    {
        public bool Equals (RoundConfig x, RoundConfig y)
        {
            return x.Equals(y);
        }

        public int GetHashCode (RoundConfig obj)
        {
            return obj.GetHashCode();
        }
    }
}

namespace Risk.Dice
{
    public static class BattleCache
    {
        private static readonly object _lock = new object();
        private static Dictionary<RoundConfig, List<BattleInfo>> _cache = new Dictionary<RoundConfig, List<BattleInfo>>(new RoundConfigComparer());

        public static Dictionary<RoundConfig, List<BattleInfo>> Cache => _cache;

        public static BattleInfo Get (RoundConfig roundConfig, BattleConfig battleConfig)
        {
            if (battleConfig.IsEarlyStop)
            {
                return new BattleInfo(battleConfig, roundConfig);
            }

            BattleInfo battleInfo = default;

            lock (_lock)
            {
                if (!_cache.ContainsKey(roundConfig))
                {
                    _cache[roundConfig] = new List<BattleInfo>();
                }

                List<BattleInfo> battleInfoList = _cache[roundConfig];

                foreach (BattleInfo battle in battleInfoList)
                {
                    if (battle.BattleConfig.Equals(battleConfig))
                    {
                        battleInfo = battle;
                        break;
                    }
                }

                if (battleInfo == null)
                {
                    battleInfo = new BattleInfo(battleConfig, roundConfig);
                    battleInfoList.Add(battleInfo);
                }
            }

            return battleInfo;
        }

        public static void Clear ()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }
}

namespace Risk.Dice
{
    [Serializable]
    public sealed class BalanceConfig : IEquatable<BalanceConfig>
    {
        private double _winChanceCutoff;
        private double _winChancePower;
        private double _outcomeCutoff;
        private double _outcomePower;

        public double WinChanceCutoff => _winChanceCutoff;
        public double WinChancePower => _winChancePower;
        public double OutcomeCutoff => _outcomeCutoff;
        public double OutcomePower => _outcomePower;

        public static BalanceConfig Default => new BalanceConfig(0.05, 1.3, 0.1, 1.8);

        public BalanceConfig (double winChanceCutoff, double winChancePower, double outcomeCutoff, double outcomePower)
        {
            _winChanceCutoff = winChanceCutoff;
            _winChancePower = winChancePower;
            _outcomeCutoff = outcomeCutoff;
            _outcomePower = outcomePower;
        }

        public bool Equals (BalanceConfig other)
        {
            if (other == null
                || _winChanceCutoff != other._winChanceCutoff
                || _outcomePower != other._outcomePower)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            int hashCode = -762923008;
            hashCode = hashCode * -1521134295 + _winChanceCutoff.GetHashCode();
            hashCode = hashCode * -1521134295 + _winChancePower.GetHashCode();
            hashCode = hashCode * -1521134295 + _outcomeCutoff.GetHashCode();
            hashCode = hashCode * -1521134295 + _outcomePower.GetHashCode();
            return hashCode;
        }
    }
}

namespace Risk.Dice
{
    [Serializable]
    public sealed class BalancedBattleInfo : BattleInfo
    {
        private BalanceConfig _balanceConfig;
        private bool _balanceApplied;

        public override bool IsReady => _balanceApplied && base.IsReady;

        public BalancedBattleInfo (BattleInfo battleInfo, BalanceConfig balanceConfig) : base(battleInfo.BattleConfig, battleInfo.RoundConfig)
        {
            _balanceConfig = balanceConfig;

            if (battleInfo.IsReady)
            {
                _attackLossChances = battleInfo.AttackLossChances.ToArray();
                _defendLossChances = battleInfo.DefendLossChances.ToArray();
            }
        }

        public BalancedBattleInfo (BalanceConfig balanceConfig, BattleConfig battleConfig, RoundConfig roundConfig) : base(battleConfig, roundConfig)
        {
            _balanceConfig = balanceConfig;
        }

        public override void Calculate ()
        {
            base.Calculate();

            ApplyBalance();
        }

        public void ApplyBalance ()
        {
            if (_balanceApplied)
            {
                return;
            }

#if UNITY_ASSERTIONS
            Assert.IsTrue(base.IsReady);
#endif

            ApplyWinChanceCutoff();
            ApplyWinChancePower();
            ApplyOutcomeCutoff();
            ApplyOutcomePower();

#if UNITY_ASSERTIONS
            Assert.AreApproximatelyEqual((float) (AttackWinChance + DefendWinChance + UnresolvedChance), (float) 1.0);
            Assert.AreApproximatelyEqual((float) _attackLossChances.SumAsDouble() + (float) UnresolvedChance, (float) 1.0);
            Assert.AreApproximatelyEqual((float) _defendLossChances.SumAsDouble(), (float) 1.0);
#endif

            _balanceApplied = true;
        }

        private void ApplyWinChanceCutoff ()
        {
            // If the overall win OR lose chance is less than the cutoff value it will get rounded to 0% or 100%.
            // Example 1) 97% win chance with a 5% cutoff will turn into 100% win chance
            // Example 2) 3% win chance with a 5% cutoff will turn into 0% win chance
            // Example 3) 70% win chance with a 5% cutoff will remain a 70% win chance

            if (_balanceConfig.WinChanceCutoff <= 0)
            {
                return;
            }

            double[] loseChances = null;
            double[] winChances = null;

            if (AttackWinChance <= _balanceConfig.WinChanceCutoff)
            {
                loseChances = _attackLossChances;
                winChances = _defendLossChances;
            }

            if (_battleConfig.StopUntil > 0)
            {
                if (UnresolvedChance <= _balanceConfig.WinChanceCutoff)
                {
                    loseChances = _defendLossChances;
                    winChances = _attackLossChances;
                }
            }
            else
            {
                if (DefendWinChance <= _balanceConfig.WinChanceCutoff)
                {
                    loseChances = _defendLossChances;
                    winChances = _attackLossChances;
                }
            }

            if (winChances != null && loseChances != null)
            {
                for (int i = 0; i < loseChances.Length - 1; i++)
                {
                    loseChances[i] = 0.0;
                }

                if (loseChances == _attackLossChances && _battleConfig.StopUntil > 0)
                {
                    loseChances[loseChances.Length - 1] = 0.0;
                }
                else
                {
                    loseChances[loseChances.Length - 1] = 1.0;
                }

                winChances[winChances.Length - 1] = 0.0;
                MathUtil.NormalizeSum(winChances, 1.0, 0, winChances.Length - 1);
            }
        }

        private void ApplyWinChancePower ()
        {
            // Adjusts the overall win chance by improving the odds of the more likely outcome.
            // The more likely the outcome - the more this balance will apply.
            // Example 1) 56.8% win chance with a power of 1.4 will turn into a 59.4% win chance (2.6% difference)
            // Example 2) 43.2% win chance with a power of 1.4 will turn into a 40.6% win chance (2.6% difference)
            // Example 3) 86.1% win chance with a power of 1.4 will turn into a 92.8% win chance (6.7% difference)

            if (_balanceConfig.WinChancePower == 1.0)
            {
                return;
            }

            double targetWinChance, targetLoseChance;
            double[] winChances, loseChances;

            if (_battleConfig.StopUntil > 0)
            {
                if (AttackWinChance > UnresolvedChance)
                {
                    winChances = _attackLossChances;
                    loseChances = _defendLossChances;

                    targetWinChance = Math.Pow(AttackWinChance, _balanceConfig.WinChancePower);
                    targetLoseChance = Math.Pow(UnresolvedChance, _balanceConfig.WinChancePower);
                }
                else
                {
                    winChances = _defendLossChances;
                    loseChances = _attackLossChances;

                    targetWinChance = Math.Pow(UnresolvedChance, _balanceConfig.WinChancePower);
                    targetLoseChance = Math.Pow(AttackWinChance, _balanceConfig.WinChancePower);
                }
            }
            else
            {
                if (AttackWinChance > DefendWinChance)
                {
                    winChances = _attackLossChances;
                    loseChances = _defendLossChances;

                    targetWinChance = Math.Pow(AttackWinChance, _balanceConfig.WinChancePower);
                    targetLoseChance = Math.Pow(DefendWinChance, _balanceConfig.WinChancePower);
                }
                else
                {
                    winChances = _defendLossChances;
                    loseChances = _attackLossChances;

                    targetWinChance = Math.Pow(DefendWinChance, _balanceConfig.WinChancePower);
                    targetLoseChance = Math.Pow(AttackWinChance, _balanceConfig.WinChancePower);
                }
            }

            double normalizationRatio = 1.0 / (targetWinChance + targetLoseChance);
            targetWinChance *= normalizationRatio;
            targetLoseChance *= normalizationRatio;

            MathUtil.NormalizeSum(winChances, targetWinChance, 0, winChances.Length - 1);
            MathUtil.NormalizeSum(loseChances, targetLoseChance, 0, loseChances.Length - 1);

            if (_battleConfig.StopUntil > 0)
            {
                if (winChances == _attackLossChances)
                {
                    loseChances[loseChances.Length - 1] = targetWinChance;
                }
                else
                {
                    winChances[winChances.Length - 1] = targetLoseChance;
                }
            }
            else
            {
                winChances[winChances.Length - 1] = targetLoseChance;
                loseChances[loseChances.Length - 1] = targetWinChance;
            }
        }

        private void ApplyOutcomeCutoff ()
        {
            // Trims outcome chances at the high and low end equally then re-normalizes.
            // A cutoff of 20% will trim that much from the most favourable outcomes for both the attacker AND defender.
            // Overall win chance may or may not be affected.

            if (_balanceConfig.OutcomeCutoff <= 0)
            {
                return;
            }

            int outcomeCount = (_attackLossChances.Length - 1) + (_defendLossChances.Length - 1);
            double[] distribution = new double[outcomeCount];

            for (int i = 0; i < outcomeCount; i++)
            {
                if (i < _attackLossChances.Length - 1)
                {
                    distribution[i] = _attackLossChances[i];
                }
                else
                {
                    int flip = (i - _attackLossChances.Length) + 1;
                    int index = _defendLossChances.Length - 2 - flip;
                    distribution[i] = _defendLossChances[index];
                }
            }

            // Low cutoff
            {
                double cutSum = 0;

                for (int i = 0; i < outcomeCount; i++)
                {
                    cutSum += distribution[i];

                    if (cutSum > _balanceConfig.OutcomeCutoff)
                    {
                        distribution[i] = cutSum - _balanceConfig.OutcomeCutoff;
                        break;
                    }
                    else
                    {
                        distribution[i] = 0;
                    }
                }
            }

            // High cutoff
            {
                double cutSum = 0;

                for (int i = outcomeCount - 1; i >= 0; i--)
                {
                    cutSum += distribution[i];

                    if (cutSum > _balanceConfig.OutcomeCutoff)
                    {
                        distribution[i] = cutSum - _balanceConfig.OutcomeCutoff;
                        break;
                    }
                    else
                    {
                        distribution[i] = 0;
                    }
                }
            }

            // Copy back distribution

            for (int i = 0; i < outcomeCount; i++)
            {
                if (i < _attackLossChances.Length - 1)
                {
                    _attackLossChances[i] = distribution[i];
                }
                else
                {
                    int flip = (i - _attackLossChances.Length) + 1;
                    int index = _defendLossChances.Length - 2 - flip;
                    _defendLossChances[index] = distribution[i];
                }
            }

            // Re-normalization

            if (_battleConfig.StopUntil > 0)
            {
                double targetAttackChance = MathUtil.SumAsDouble(_attackLossChances, 0, _attackLossChances.Length - 1);
                double targetUnresolvedChance = MathUtil.SumAsDouble(_defendLossChances, 0, _defendLossChances.Length - 1);

                double normalizationRatio = 1.0 / (targetAttackChance + targetUnresolvedChance);
                targetAttackChance *= normalizationRatio;
                targetUnresolvedChance *= normalizationRatio;

                MathUtil.NormalizeSum(_attackLossChances, targetAttackChance, 0, _attackLossChances.Length - 1);
                MathUtil.NormalizeSum(_defendLossChances, targetUnresolvedChance, 0, _defendLossChances.Length - 1);

                _defendLossChances[_defendLossChances.Length - 1] = targetAttackChance;
            }
            else
            {
                double targetAttackChance = MathUtil.SumAsDouble(_attackLossChances, 0, _attackLossChances.Length - 1);
                double targetDefendChance = MathUtil.SumAsDouble(_defendLossChances, 0, _defendLossChances.Length - 1);

                double normalizationRatio = 1.0 / (targetAttackChance + targetDefendChance);
                targetAttackChance *= normalizationRatio;
                targetDefendChance *= normalizationRatio;

                MathUtil.NormalizeSum(_attackLossChances, targetAttackChance, 0, _attackLossChances.Length - 1);
                MathUtil.NormalizeSum(_defendLossChances, targetDefendChance, 0, _defendLossChances.Length - 1);

                _attackLossChances[_attackLossChances.Length - 1] = targetDefendChance;
                _defendLossChances[_defendLossChances.Length - 1] = targetAttackChance;
            }
        }

        private void ApplyOutcomePower ()
        {
            // Adjusts the individual outcome chances by boosting more likely outcomes and bringing down less likely outcomes.
            // Will NOT change the overall win chance or make any currently existing outcomes impossible or certain.

            if (_balanceConfig.OutcomePower == 1.0)
            {
                return;
            }

            for (int i = 0; i < _attackLossChances.Length - 1; i++)
            {
                _attackLossChances[i] = Math.Pow(_attackLossChances[i], _balanceConfig.OutcomePower);
            }

            for (int i = 0; i < _defendLossChances.Length - 1; i++)
            {
                _defendLossChances[i] = Math.Pow(_defendLossChances[i], _balanceConfig.OutcomePower);
            }

            if (_battleConfig.StopUntil > 0)
            {
                MathUtil.NormalizeSum(_attackLossChances, AttackWinChance, 0, _attackLossChances.Length - 1);
                MathUtil.NormalizeSum(_defendLossChances, UnresolvedChance, 0, _defendLossChances.Length - 1);
            }
            else
            {
                MathUtil.NormalizeSum(_attackLossChances, AttackWinChance, 0, _attackLossChances.Length - 1);
                MathUtil.NormalizeSum(_defendLossChances, DefendWinChance, 0, _defendLossChances.Length - 1);
            }
        }
    }
}

namespace Risk.Dice
{
    [Serializable]
    public class BattleInfo
    {
        protected BattleConfig _battleConfig;
        protected RoundConfig _roundConfig;
        protected double[] _attackLossChances;
        protected double[] _defendLossChances;

        public BattleConfig BattleConfig => _battleConfig;
        public RoundConfig RoundConfig => _roundConfig;
        public double[] AttackLossChances => _attackLossChances;
        public double[] DefendLossChances => _defendLossChances;
        public double AttackWinChance => _defendLossChances[_battleConfig.DefendUnitCount];
        public double DefendWinChance => _attackLossChances[_battleConfig.AttackUnitCount];
        public double UnresolvedChance => _battleConfig.StopUntil > 0 ? Math.Max(1.0 - AttackWinChance - DefendWinChance, 0.0) : 0.0;
        public virtual bool IsReady => _attackLossChances != null && _defendLossChances != null && _attackLossChances.Length > 0 && _defendLossChances.Length > 0;

        internal BattleInfo (RoundConfig roundConfig)
        {
            _roundConfig = roundConfig;
        }

        public BattleInfo (BattleConfig battleConfig, RoundConfig roundConfig)
        {
            _battleConfig = battleConfig;
            _roundConfig = roundConfig;
        }

        public virtual void Calculate ()
        {
            if (IsReady)
            {
                return;
            }

            int attackUnitCount = _battleConfig.AttackUnitCount;
            int defendUnitCount = _battleConfig.DefendUnitCount;
            int stopUntil = _battleConfig.StopUntil;

            double[] attackLossChances = new double[attackUnitCount + 1];
            double[] defendLossChances = new double[defendUnitCount + 1];

            RoundConfig roundConfig = _roundConfig.WithBattle(_battleConfig);
            RoundInfo roundInfo = RoundCache.Get(roundConfig);
            roundInfo.Calculate();

            if (stopUntil == 0)
            {
                // Only calculate if not using early stop

                for (int i = 0; i < roundInfo.AttackLossChances.Length; i++)
                {
                    double roundChance = roundInfo.AttackLossChances[i];

                    if (roundChance <= 0.0)
                    {
                        continue;
                    }

                    int remainingAttackUnitCount = attackUnitCount - i;
                    int remainingDefendUnitCount = defendUnitCount - (roundConfig.ChallengeCount - i);

                    if (remainingAttackUnitCount <= 0 || remainingDefendUnitCount <= 0)
                    {
                        // Battle chain is over: accumulate chance
                        attackLossChances[attackUnitCount - remainingAttackUnitCount] += roundChance;
                        defendLossChances[defendUnitCount - remainingDefendUnitCount] += roundChance;
                    }
                    else
                    {
                        // Battle chain continues
                        BattleConfig nextBattleConfig = _battleConfig.WithNewUnits(remainingAttackUnitCount, remainingDefendUnitCount);
                        BattleInfo battleInfo = BattleCache.Get(_roundConfig, nextBattleConfig);
                        battleInfo.Calculate();

                        for (int a = 0; a < battleInfo._attackLossChances.Length; a++)
                        {
                            attackLossChances[attackUnitCount - remainingAttackUnitCount + a] += roundChance * battleInfo._attackLossChances[a];
                        }

                        for (int d = 0; d < battleInfo._defendLossChances.Length; d++)
                        {
                            defendLossChances[defendUnitCount - remainingDefendUnitCount + d] += roundChance * battleInfo._defendLossChances[d];
                        }
                    }
                }
            }
            else
            {
                BattleConfig baseBattleConfig = _battleConfig.WithoutStopUntil();
                BattleInfo baseBattleInfo = BattleCache.Get(_roundConfig, baseBattleConfig);
                baseBattleInfo.Calculate();

                for (int i = 0; i < attackLossChances.Length; i++)
                {
                    if (i < baseBattleInfo._attackLossChances.Length - 1)
                    {
                        attackLossChances[i] = baseBattleInfo._attackLossChances[i];
                    }
                }

                for (int i = 0; i < defendLossChances.Length; i++)
                {
                    defendLossChances[i] = baseBattleInfo._defendLossChances[i];
                }
            }

            // Complete

            _attackLossChances = attackLossChances;
            _defendLossChances = defendLossChances;

#if UNITY_ASSERTIONS
            Assert.AreApproximatelyEqual((float) (AttackWinChance + DefendWinChance + UnresolvedChance), (float) 1.0);
            Assert.AreApproximatelyEqual((float) _attackLossChances.SumAsDouble() + (float) UnresolvedChance, (float) 1.0);
            Assert.AreApproximatelyEqual((float) _defendLossChances.SumAsDouble(), (float) 1.0);
#endif
        }

        public virtual double GetOutcomeChance (int lostAttackCount, int lostDefendCount)
        {
            if (lostAttackCount == _battleConfig.AttackUnitCount - _battleConfig.StopUntil)
            {
                return _attackLossChances[lostAttackCount];
            }
            else if (lostDefendCount == _battleConfig.DefendUnitCount)
            {
                return _defendLossChances[lostDefendCount];
            }
            else
            {
                return -1;
            }
        }
    }
}


namespace Risk.Dice.Utility
{
    public struct ByteBuffer
    {
        private readonly byte[] bytes;
        private int position;

        public byte[] Bytes => bytes;
        public bool IsComplete => position == bytes.Length;

        public ByteBuffer (int length)
        {
            bytes = new byte[length];
            position = 0;
        }

        public ByteBuffer (byte[] bytes)
        {
            this.bytes = bytes;
            position = 0;
        }

        public void Reset()
        {
            position = 0;
        }

        public unsafe void WriteInt (int value)
        {
            fixed (byte* bytesPtr = &bytes[position])
            {
                int* ptr = (int*) bytesPtr;
                *ptr = value;
            }

            position += sizeof(int);
        }

        public unsafe void WriteUint (uint value)
        {
            fixed (byte* bytesPtr = &bytes[position])
            {
                uint* ptr = (uint*) bytesPtr;
                *ptr = value;
            }

            position += sizeof(uint);
        }

        public unsafe void WriteFloat (float value)
        {
            fixed (byte* bytesPtr = &bytes[position])
            {
                float* ptr = (float*) bytesPtr;
                *ptr = value;
            }

            position += sizeof(float);
        }

        public unsafe void WriteDouble (double value)
        {
            fixed (byte* bytesPtr = &bytes[position])
            {
                double* ptr = (double*) bytesPtr;
                *ptr = value;
            }

            position += sizeof(double);
        }

        public unsafe void WriteBool (bool value)
        {
            fixed (byte* bytesPtr = &bytes[position])
            {
                bool* ptr = (bool*) bytesPtr;
                *ptr = value;
            }

            position += sizeof(bool);
        }

        public void WriteDoubleArray (double[] array)
        {
            WriteBool(array != null);

            if (array != null)
            {
                WriteInt(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    WriteDouble(array[i]);
                }
            }
        }

        public unsafe int ReadInt ()
        {
            int value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                int* ptr = (int*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(int);
            return value;
        }

        public unsafe uint ReadUint ()
        {
            uint value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                uint* ptr = (uint*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(uint);
            return value;
        }

        public unsafe float ReadFloat ()
        {
            float value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                float* ptr = (float*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(float);
            return value;
        }

        public unsafe double ReadDouble ()
        {
#if UNITY_64
            double value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                double* ptr = (double*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(double);
            return value;
#else
            double value = BitConverter.ToDouble(bytes, position);
            position += sizeof(double);
            return value;
#endif
        }

        public unsafe bool ReadBool ()
        {
            bool value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                bool* ptr = (bool*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(bool);
            return value;
        }

        public unsafe ulong ReadUlong ()
        {
#if UNITY_64
            ulong value;

            fixed (byte* bytesPtr = &bytes[position])
            {
                ulong* ptr = (ulong*) bytesPtr;
                value = *ptr;
            }

            position += sizeof(ulong);
            return value;
#else
            ulong value = BitConverter.ToUInt64(bytes, position);
            position += sizeof(ulong);
            return value;
#endif
        }

        public double[] ReadDoubleArray ()
        {
            bool hasArray = ReadBool();

            if (hasArray)
            {
                int length = ReadInt();

                double[] array = new double[length];

                for (int i = 0; i < length; i++)
                {
                    array[i] = ReadDouble();
                }

                return array;
            }
            else
            {
                return null;
            }
        }

        public static int GetByteLength (double[] array)
        {
            int length = sizeof(bool);

            if (array != null)
            {
                length += sizeof(int);
                length += sizeof(double) * array.Length;
            }

            return length;
        }
    }
}

namespace Risk.Dice
{
    [Serializable]
    public class WinChanceInfo
    {
        private int _size;
        private RoundConfig _roundConfig;
        private BalanceConfig _balanceConfig;
        private float[,] _winChances;

        public int Size => _size;
        public RoundConfig RoundConfig => _roundConfig;
        public BalanceConfig BalanceConfig => _balanceConfig;
        public float[,] WinChances => _winChances;
        public bool IsReady => _winChances != null;

        public WinChanceInfo (int size) : this(size, RoundConfig.Default, null)
        {
        }

        public WinChanceInfo (int size, RoundConfig roundConfig) : this (size, roundConfig, null)
        {
        }

        public WinChanceInfo (int size, RoundConfig roundConfig, BalanceConfig balanceConfig)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(size > 2);
#endif

            _size = size;
            _roundConfig = roundConfig;
            _balanceConfig = balanceConfig;
        }

        public void Calculate ()
        {
            if (IsReady)
            {
                return;
            }

            int maxA = _roundConfig.AttackDiceCount;
            int maxD = _roundConfig.DefendDiceCount;

            // Find round chances

            float[,][] roundChances = new float[maxA + 1, maxD + 1][];

            for (int a = 1; a <= maxA; a++)
            {
                for (int d = 1; d <= maxD; d++)
                {
                    RoundConfig roundConfig = _roundConfig.WithBattle(new BattleConfig(a, d, 0));
                    RoundInfo roundInfo = RoundCache.Get(roundConfig);
                    roundInfo.Calculate();

                    roundChances[a, d] = new float[roundConfig.ChallengeCount + 1];

                    for (int c = 0; c <= roundConfig.ChallengeCount; c++)
                    {
                        roundChances[a, d][c] = (float) roundInfo.AttackLossChances[c];
                    }
                }
            }

            // Find win chances

            float[,] winChances = new float[_size, _size];

            for (int a = 0; a < _size; a++)
            {
                for (int d = 0; d < _size; d++)
                {
                    if (d == 0 || a == 0)
                    {
                        winChances[a, d] = a > 0 ? 1f : 0f;
                    }
                    else
                    {
                        int roundA = Math.Min(a, maxA);
                        int roundD = Math.Min(d, maxD);
                        int challengeCount = Math.Min(roundA, roundD);

                        for (int o = 0; o < challengeCount + 1 && a - o > 0; o++)
                        {
                            winChances[a, d] += roundChances[roundA, roundD][o] * winChances[a - o, (d - challengeCount) + o];
                        }
                    }
                }
            }

            // Apply balanced blitz

            if (_balanceConfig != null)
            {
                for (int a = 0; a < _size; a++)
                {
                    for (int d = 0; d < _size; d++)
                    {
                        winChances[a, d] = ApplyBalance(winChances[a, d]);
                    }
                }
            }

            // Complete

            _winChances = winChances;
        }

        private float ApplyBalance (float winChance)
        {
            winChance = ApplyWinChanceCutoff(winChance);
            winChance = ApplyWinChancePower(winChance);
            winChance = ApplyOutcomeCutoff(winChance);

            return winChance;
        }

        private float ApplyWinChanceCutoff (float winChance)
        {
            if (_balanceConfig.WinChanceCutoff <= 0f)
            {
                return winChance;
            }
            else if (winChance < _balanceConfig.WinChanceCutoff)
            {
                return 0f;
            }
            else if (winChance > 1f - _balanceConfig.WinChanceCutoff)
            {
                return 1f;
            }

            return winChance;
        }

        private float ApplyWinChancePower (float winChance)
        {
            float a = Mathf.Pow(winChance, (float) _balanceConfig.WinChancePower);
            float d = Mathf.Pow(1f - winChance, (float) _balanceConfig.WinChancePower);

            float ratio = 1f / (a + d);

            winChance = a * ratio;

            return winChance;
        }

        private float ApplyOutcomeCutoff (float winChance)
        {
            float a = winChance - (float) _balanceConfig.OutcomeCutoff;
            float d = (1f - winChance) - (float) _balanceConfig.OutcomeCutoff;

            if (a < 0)
            {
                d += -a;
                a = 0f;
            }
            
            if (d < 0)
            {
                a += -d;
                d = 0f;
            }

            float ratio = 1f / (a + d);

            winChance = a * ratio;

            return winChance;
        }
    }
}

namespace Risk.Dice
{
    public static class WinChanceCache
    {
        private static readonly object _lock = new object();
        private static List<WinChanceInfo> _cache = new List<WinChanceInfo>();

        public static List<WinChanceInfo> Cache => _cache;

        public static WinChanceInfo Get (int requiredSize, RoundConfig roundConfig, BalanceConfig balanceConfig)
        {
            int nextSize = GetNextChacheSize(requiredSize);

            WinChanceInfo winChanceInfo = default;
            bool hasBalance = balanceConfig != null;

            lock (_lock)
            {
                foreach (WinChanceInfo winChance in _cache)
                {
                    if (hasBalance && winChance.BalanceConfig != null)
                    {
                        if (winChance.RoundConfig.Equals(roundConfig) && winChance.BalanceConfig.Equals(balanceConfig))
                        {
                            winChanceInfo = winChance;
                            break;
                        }
                    }
                    else if (!hasBalance && winChance.BalanceConfig == null)
                    {
                        if (winChance.RoundConfig.Equals(roundConfig))
                        {
                            winChanceInfo = winChance;
                            break;
                        }
                    }
                }

                if (winChanceInfo == null)
                {
                    winChanceInfo = new WinChanceInfo(nextSize + 1, roundConfig, balanceConfig);
                    _cache.Add(winChanceInfo);
                }
                else if (requiredSize >= winChanceInfo.Size)
                {
                    _cache.Remove(winChanceInfo);

                    // TODO: Use smaller win chance size to speed up next calculation

                    winChanceInfo = new WinChanceInfo(nextSize + 1, roundConfig, balanceConfig);
                    _cache.Add(winChanceInfo);
                }
            }

            return winChanceInfo;
        }

        public static void Clear ()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }

        private static int GetNextChacheSize (int requiredSize)
        {
            return Math.Max(MathUtil.NextPowerOfTwo(requiredSize + 1), 64);
        }
    }
}

class Hello
{
  static void Main(string[] args)
  {
      Console.WriteLine("Hello World 2");
      var roundInfo = RoundCache.Get(RoundConfig.Default);
      roundInfo.Calculate();
      Console.WriteLine("Should print 0.292566872427984");
      Console.WriteLine(roundInfo.AttackLossChances[2]);

      /* Compare true random and balanced blitz, with the chance of
      losing 12 out of 30 troops when attacking a capitol territory
      with 15 troops */
      RoundConfig roundConfig = RoundConfig.Default;
      roundConfig.ApplyAugments(DiceAugment.None, DiceAugment.OnCapital);
      var battleConfig = new BattleConfig(30, 15, 0);
      var battle = BattleCache.Get(roundConfig, battleConfig);
      battle.Calculate();
      var balancedBattle = new BalancedBattleInfo(battle, BalanceConfig.Default);
      balancedBattle.ApplyBalance();
      /* print($"TR: { | BB: {}"); */
      Console.WriteLine("Should print 0.0222128001707278");
      Console.WriteLine(battle.AttackLossChances[12]);
      Console.WriteLine("Should print 0.0100282888709122");
      Console.WriteLine(balancedBattle.AttackLossChances[12]);

      /* Compare true random and balanced blitz, with the chance of
      	 losing 37 out of 37 troops when attacking a territory with 8 troops */
      roundConfig = RoundConfig.Default;
      battleConfig = new BattleConfig(37, 8, 0);
      battle = BattleCache.Get(roundConfig, battleConfig);
      battle.Calculate();
      balancedBattle = new BalancedBattleInfo(battle, BalanceConfig.Default);
      balancedBattle.ApplyBalance();
      Console.WriteLine("True Dice (chance to lose 37 v 8)");
      Console.WriteLine(battle.AttackLossChances[37]);
      Console.WriteLine("... which is a 'one in X' chance or 1 in:");
      Console.WriteLine(1/battle.AttackLossChances[37]);
      Console.WriteLine("Balanced Dice");
      Console.WriteLine(balancedBattle.AttackLossChances[37]);


      /* from their support site comment on 38 vs 8 */
      var rc02 = RoundConfig.Default;
      var wc02 = WinChanceCache.Get(1000, rc02, null);
      wc02.Calculate();
      Console.WriteLine("Chance to win 38 v 8");
      Console.WriteLine("(by my xls should be 0.999997269276864)");
      Console.WriteLine((wc02.WinChances[38, 8]));


      /* Find the win chance for attacking 50 troops with 50 troops */
      roundConfig = RoundConfig.Default;
      var winChanceInfo = WinChanceCache.Get(1000, roundConfig, null);
      winChanceInfo.Calculate();
      Console.WriteLine("Chance to win 50 v 50");
      Console.WriteLine("(by my xls should be 0.73553513127029)");
      Console.WriteLine((winChanceInfo.WinChances[50, 50]));

      Console.WriteLine("Chance to win 5 v 50");
      Console.WriteLine("(by my xls should be 0.00000000562085068188557)");
      Console.WriteLine((winChanceInfo.WinChances[5, 50]));
      Console.WriteLine("(multiplied by 100000000 by my xls should be 0.562085068188557)");
      Console.WriteLine((winChanceInfo.WinChances[5, 50] * 100000000));

      roundConfig = RoundConfig.Default;
      winChanceInfo = WinChanceCache.Get(100, roundConfig, null);
      winChanceInfo.Calculate();
      Console.WriteLine("20 vs 30 chance to win");
      Console.WriteLine("Balanced (by my xls 0.190677373343385):");
      Console.WriteLine(winChanceInfo.WinChances[20, 30]);
  }
}
