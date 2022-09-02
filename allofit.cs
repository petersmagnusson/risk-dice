/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Risk.Dice.RNG;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public sealed class BattleSimulator
    {
        public enum RoundMethod
        {
            DiceRoll,
            OddsBased
        }

        public enum BlitzMethod
        {
            DiceRoll,
            OddsBasedRound,
            OddsBasedBattle,
        }

        public enum StatusType
        {
            Unresolved,
            AttackerWin,
            DefenderWin
        }

        private IRNG _rng;

        [SerializeField] private RoundConfig _roundConfig;
        [SerializeField] private BattleConfig _battleConfig;
        [SerializeField] private BalanceConfig _balanceConfig;
        [SerializeField] private int _remainingAttackCount;
        [SerializeField] private int _remainingDefendCount;
        [SerializeField] private int _lastAttackLossCount;
        [SerializeField] private int _lastDefendLossCount;
        [SerializeField] private int[] _attackDiceRollTally;
        [SerializeField] private int[] _defendDiceRollTally;
        [SerializeField] private List<int> _attackDiceRolls;
        [SerializeField] private List<int> _defendDiceRolls;
        [SerializeField] private List<int> _lastAttackDiceRolls;
        [SerializeField] private List<int> _lastDefendDiceRolls;
        [SerializeField] private int _simulatedAttackDiceRollCount;
        [SerializeField] private int _simulatedDefendDiceRollCount;

        public RoundConfig RoundConfig => _roundConfig;
        public BattleConfig BattleConfig => _battleConfig;
        public BalanceConfig BalanceConfig => _balanceConfig;
        public int RemainingAttackCount => _remainingAttackCount;
        public int RemainingDefendCount => _remainingDefendCount;
        public int LastAttackLossCount => _lastAttackLossCount;
        public int LastDefendLossCount => _lastDefendLossCount;
        public int AttackLossCount => _battleConfig.AttackUnitCount - _remainingAttackCount;
        public int DefendLossCount => _battleConfig.DefendUnitCount - _remainingDefendCount;
        public int[] AttackDiceRollTally => _attackDiceRollTally;
        public int[] DefendDiceRollTally => _defendDiceRollTally;
        public List<int> AttackDiceRolls => _attackDiceRolls;
        public List<int> DefendDiceRolls => _defendDiceRolls;
        public List<int> LastAttackDiceRolls => _lastAttackDiceRolls;
        public List<int> LastDefendDiceRolls => _lastDefendDiceRolls;
        public int SimulatedAttackDiceRollCount => _simulatedAttackDiceRollCount;
        public int SimulatedDefendDiceRollCount => _simulatedDefendDiceRollCount;

        [ShowInInspector] public bool IsComplete => _remainingAttackCount <= _battleConfig.StopUntil || _remainingDefendCount == 0;
        [ShowInInspector] public bool IsAttackerWin => IsComplete && _remainingDefendCount == 0;

        public BattleSimulator (int attackUnitCount, int defendUnitCount) : this(new BattleConfig(attackUnitCount, defendUnitCount, 0))
        {
        }

        public BattleSimulator (BattleConfig battleConfig) : this(battleConfig, RoundConfig.Default)
        {
        }

        public BattleSimulator (BattleConfig battleConfig, RoundConfig roundConfig) : this(battleConfig, roundConfig, RNGConfig.Default)
        {
        }

        public BattleSimulator (BattleConfig battleConfig, RoundConfig roundConfig, RNGConfig rngConfig) : this(battleConfig, roundConfig, null, rngConfig)
        {
        }

        public BattleSimulator (BattleConfig battleConfig, RoundConfig roundConfig, BalanceConfig balanceConfig) : this(battleConfig, roundConfig, balanceConfig, RNGConfig.Default)
        {
        }

        public BattleSimulator (BattleConfig battleConfig, RoundConfig roundConfig, BalanceConfig balanceConfig, RNGConfig rngConfig) : this(battleConfig, roundConfig, balanceConfig, rngConfig.GetRNG())
        {
        }

        public BattleSimulator (BattleConfig battleConfig, RoundConfig roundConfig, BalanceConfig balanceConfig, IRNG rng)
        {
            _rng = rng;
            _roundConfig = roundConfig;
            _battleConfig = battleConfig;
            _balanceConfig = balanceConfig;

            _remainingAttackCount = battleConfig.AttackUnitCount;
            _remainingDefendCount = battleConfig.DefendUnitCount;

            _attackDiceRollTally = new int[roundConfig.DiceFaceCount];
            _defendDiceRollTally = new int[roundConfig.DiceFaceCount];

            _attackDiceRolls = new List<int>();
            _defendDiceRolls = new List<int>();

            _lastAttackDiceRolls = new List<int>(roundConfig.AttackDiceCount);
            _lastDefendDiceRolls = new List<int>(roundConfig.DefendDiceCount);
        }

        public void SetRNG (RNGConfig rngConfig)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(rngConfig.IsValid);
#endif

            _rng = rngConfig.GetRNG();
        }

        public StatusType GetStatus ()
        {
            if (_remainingDefendCount <= 0)
            {
                return StatusType.AttackerWin;
            }
            else if (_remainingAttackCount <= 0)
            {
                return StatusType.DefenderWin;
            }
            else
            {
                return StatusType.Unresolved;
            }
        }

        public void NextRound (RoundMethod method)
        {
            switch (method)
            {
                case RoundMethod.DiceRoll:
                    RoundDiceRoll();
                    break;
                case RoundMethod.OddsBased:
                    RoundOddsBased();
                    break;
            }
        }

        public void Blitz (BlitzMethod method)
        {
            if (IsComplete)
            {
                return;
            }

            switch (method)
            {
                case BlitzMethod.DiceRoll:
                    {
                        while (!IsComplete)
                        {
                            RoundDiceRoll();
                        }

                        break;
                    }

                case BlitzMethod.OddsBasedRound:
                    {
                        while (!IsComplete)
                        {
                            RoundOddsBased();
                        }

                        break;
                    }

                case BlitzMethod.OddsBasedBattle:
                    {
                        BlitzOddsBased();
                        break;
                    }
            }
        }

        private void RoundDiceRoll ()
        {
            if (IsComplete)
            {
                return;
            }

            BattleConfig currentBattleConfig = _battleConfig.WithNewUnits(_remainingAttackCount, _remainingDefendCount);
            RoundConfig currentRoundConfig = _roundConfig.WithBattle(currentBattleConfig);

            // Roll the dice

            _lastAttackDiceRolls.Clear();
            _lastDefendDiceRolls.Clear();

            for (int i = 0; i < currentRoundConfig.AttackDiceCount; i++)
            {
                int diceRoll = _rng.NextInt(0, currentRoundConfig.DiceFaceCount);

                _attackDiceRolls.Add(diceRoll);
                _lastAttackDiceRolls.Add(diceRoll);
                _attackDiceRollTally[diceRoll]++;
            }

            for (int i = 0; i < currentRoundConfig.DefendDiceCount; i++)
            {
                int diceRoll = _rng.NextInt(0, currentRoundConfig.DiceFaceCount);

                _defendDiceRolls.Add(diceRoll);
                _lastDefendDiceRolls.Add(diceRoll);
                _defendDiceRollTally[diceRoll]++;
            }

            // Sort the dice

            DescendingIntComparer descendingIntComparer = new DescendingIntComparer();

            _lastAttackDiceRolls.Sort(descendingIntComparer);
            _lastDefendDiceRolls.Sort(descendingIntComparer);

            // Compare the dice

            int attackLossCount = 0;
            int challengeCount = currentRoundConfig.ChallengeCount;

            for (int i = 0; i < challengeCount; i++)
            {
                if (_lastAttackDiceRolls[i] == _lastDefendDiceRolls[i])
                {
                    if (currentRoundConfig.FavourDefenderOnDraw)
                    {
                        attackLossCount++;
                    }
                }
                else if (_lastAttackDiceRolls[i] < _lastDefendDiceRolls[i])
                {
                    attackLossCount++;
                }
            }

            int defendLossCount = challengeCount - attackLossCount;

#if UNITY_ASSERTIONS
            Assert.IsFalse(attackLossCount == 0 && defendLossCount == 0);
            Assert.IsFalse(attackLossCount > _remainingAttackCount);
            Assert.IsFalse(defendLossCount > _remainingDefendCount);
#endif

            _lastAttackLossCount = attackLossCount;
            _lastDefendLossCount = defendLossCount;

            _remainingAttackCount -= attackLossCount;
            _remainingDefendCount -= defendLossCount;
        }

        private void RoundOddsBased ()
        {
            if (IsComplete)
            {
                return;
            }

            BattleConfig currentBattleConfig = _battleConfig.WithNewUnits(_remainingAttackCount, _remainingDefendCount);
            RoundConfig currentRoundConfig = _roundConfig.WithBattle(currentBattleConfig);
            RoundInfo roundInfo = RoundCache.Get(currentRoundConfig);
            roundInfo.Calculate();

            ApplyOddsBasedRound(roundInfo);
        }

        private void BlitzOddsBased ()
        {
            if (IsComplete)
            {
                return;
            }

            BattleConfig currentBattleConfig = _battleConfig.WithNewUnits(_remainingAttackCount, _remainingDefendCount).WithoutStopUntil();
            BattleInfo battleInfo = BattleCache.Get(_roundConfig, currentBattleConfig);
            battleInfo.Calculate();

            if (_balanceConfig != null)
            {
                BalancedBattleInfo balancedBattleInfo = new BalancedBattleInfo(battleInfo, _balanceConfig);
                balancedBattleInfo.ApplyBalance();

                battleInfo = balancedBattleInfo;
            }

            ApplyOddsBasedBattle(battleInfo);
        }

        private void ApplyOddsBasedRound (RoundInfo roundInfo)
        {
            RoundConfig roundConfig = roundInfo.Config;

            // Determine outcome

            int attackLossCount = 0;
            int defendLossCount = 0;

            double random = MathUtil.Clamp01(_rng.NextDouble());
            double current = 0.0;

            for (int i = 0; i < roundInfo.AttackLossChances.Length; i++)
            {
                if (roundInfo.AttackLossChances[i] <= 0)
                {
                    continue;
                }

                current += roundInfo.AttackLossChances[i];

                if (current >= random)
                {
                    attackLossCount = i;
                    defendLossCount = roundConfig.ChallengeCount - i;
                    break;
                }
            }

#if UNITY_ASSERTIONS
            Assert.IsFalse(attackLossCount == 0 && defendLossCount == 0);
            Assert.IsFalse(attackLossCount > _remainingAttackCount);
            Assert.IsFalse(defendLossCount > _remainingDefendCount);
#endif

            _simulatedAttackDiceRollCount += roundConfig.AttackDiceCount;
            _simulatedDefendDiceRollCount += roundConfig.DefendDiceCount;

            _lastAttackDiceRolls.Clear();
            _lastDefendDiceRolls.Clear();

            _lastAttackLossCount = attackLossCount;
            _lastDefendLossCount = defendLossCount;

            _remainingAttackCount -= attackLossCount;
            _remainingDefendCount -= defendLossCount;
        }

        private void ApplyOddsBasedBattle (BattleInfo battleInfo)
        {
            BattleConfig battleConfig = battleInfo.BattleConfig;

            // Determine outcome

            int attackLossCount = -1;
            int defendLossCount = -1;

            double random = MathUtil.Clamp01Exclusive(_rng.NextDouble());
            double current = 0.0;

            if (random <= battleInfo.AttackWinChance)
            {
                // Attacker win

                defendLossCount = battleConfig.DefendUnitCount;

                for (int i = 0; i < battleInfo.AttackLossChances.Length - 1; i++)
                {
                    if (battleInfo.AttackLossChances[i] <= 0)
                    {
                        continue;
                    }

                    current += battleInfo.AttackLossChances[i];

                    if (current >= random)
                    {
                        attackLossCount = i;
                        break;
                    }
                }

                if (attackLossCount == -1)
                {
                    battleInfo.AttackLossChances.Max(out attackLossCount);
                }
            }
            else
            {
                random -= battleInfo.AttackWinChance;

                // Defender win

                attackLossCount = battleConfig.AttackUnitCount - battleConfig.StopUntil;

                for (int i = 0; i < battleInfo.DefendLossChances.Length - 1; i++)
                {
                    if (battleInfo.DefendLossChances[i] <= 0)
                    {
                        continue;
                    }

                    current += battleInfo.DefendLossChances[i];

                    if (current >= random)
                    {
                        defendLossCount = i;
                        break;
                    }
                }

                if (defendLossCount == -1)
                {
                    battleInfo.DefendLossChances.Max(out defendLossCount);
                }
            }

#if UNITY_ASSERTIONS
            Assert.IsFalse(attackLossCount == -1 || defendLossCount == -1);
            Assert.IsFalse(attackLossCount == 0 && defendLossCount == 0);
            Assert.IsFalse(attackLossCount > _remainingAttackCount);
            Assert.IsFalse(defendLossCount > _remainingDefendCount);
#endif

            _simulatedAttackDiceRollCount += attackLossCount + defendLossCount;
            _simulatedDefendDiceRollCount += attackLossCount + defendLossCount;

            _lastAttackDiceRolls.Clear();
            _lastDefendDiceRolls.Clear();

            _lastAttackLossCount = attackLossCount;
            _lastDefendLossCount = defendLossCount;

            _remainingAttackCount -= attackLossCount;
            _remainingDefendCount -= defendLossCount;
        }

        public void Reset ()
        {
            _remainingAttackCount = _battleConfig.AttackUnitCount;
            _remainingDefendCount = _battleConfig.DefendUnitCount;

            for (int i = 0; i < _roundConfig.DiceFaceCount; i++)
            {
                _attackDiceRollTally[i] = 0;
                _defendDiceRollTally[i] = 0;
            }

            _attackDiceRolls.Clear();
            _defendDiceRolls.Clear();

            _lastAttackDiceRolls.Clear();
            _lastDefendDiceRolls.Clear();

            _simulatedAttackDiceRollCount = 0;
            _simulatedDefendDiceRollCount = 0;
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System.Collections.Generic;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using Risk.Dice.Utility;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System.Collections.Generic;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public struct RoundConfig : IEquatable<RoundConfig>
    {
        [SerializeField] private int _diceFaceCount;
        [SerializeField] private int _attackDiceCount;
        [SerializeField] private int _defendDiceCount;
        [SerializeField] private bool _favourDefenderOnDraw;

        public int DiceFaceCount => _diceFaceCount;
        public int AttackDiceCount => _attackDiceCount;
        public int DefendDiceCount => _defendDiceCount;
        public bool FavourDefenderOnDraw => _favourDefenderOnDraw;

        public int ChallengeCount => Math.Min(_attackDiceCount, _defendDiceCount);

        public static RoundConfig Default => new RoundConfig(6, 3, 2, true);

        public RoundConfig (int diceFaceCount, int attackDiceCount, int defendDiceCount, bool favourDefenderOnDraw)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(diceFaceCount >= 2);
            Assert.IsTrue(attackDiceCount > 0);
            Assert.IsTrue(defendDiceCount > 0);
#endif

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using UnityEngine;

namespace Risk.Dice
{
    [Serializable]
    public sealed class BalanceConfig : IEquatable<BalanceConfig>
    {
        [SerializeField] private double _winChanceCutoff;
        [SerializeField] private double _winChancePower;
        [SerializeField] private double _outcomeCutoff;
        [SerializeField] private double _outcomePower;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using Sirenix.OdinInspector;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public struct BattleConfig : IEquatable<BattleConfig>
    {
        [PropertyTooltip("This does NOT include the extra attack unit that stays behind")]
        [SerializeField] [LabelText("Attack Unit Count (?)")] private int _attackUnitCount;
        [SerializeField] private int _defendUnitCount;
        [SerializeField] private int _stopUntil;

        public int AttackUnitCount => _attackUnitCount;
        public int DefendUnitCount => _defendUnitCount;
        public int StopUntil => _stopUntil;

        public bool IsEarlyStop => _stopUntil > 0;

        public BattleConfig (int attackUnitCount, int defendUnitCount, int stopUntil)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(attackUnitCount > 0);
            Assert.IsTrue(defendUnitCount > 0);
            Assert.IsTrue(stopUntil >= 0);
            Assert.IsTrue(attackUnitCount > stopUntil);
#endif

            _attackUnitCount = attackUnitCount;
            _defendUnitCount = defendUnitCount;
            _stopUntil = stopUntil;
        }

        public BattleConfig WithNewUnits (int attackUnitCount, int defendUnitCount)
        {
#if UNITY_ASSERTIONS
            Assert.IsTrue(attackUnitCount > 0);
            Assert.IsTrue(defendUnitCount > 0);
#endif

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using Risk.Dice.RNG;
using Risk.Dice.Utility;

namespace Risk.Dice
{
    public static class SimulationHelper
    {
        public static float CalculateWinChance (BattleConfig battleConfig, RoundConfig roundConfig, BalanceConfig balanceConfig = null)
        {
            int requiredSize = Math.Max(battleConfig.AttackUnitCount - battleConfig.StopUntil, battleConfig.DefendUnitCount);
            WinChanceInfo winChanceInfo = WinChanceCache.Get(requiredSize, roundConfig, balanceConfig);

            if (!winChanceInfo.IsReady)
            {
                winChanceInfo.Calculate();
            }

            return winChanceInfo.WinChances[battleConfig.AttackUnitCount - battleConfig.StopUntil, battleConfig.DefendUnitCount];
        }

        public static int CalculateIdealUnits (int defendUnits, float winChanceThreshold, RoundConfig roundConfig, BalanceConfig balanceConfig = null)
        {
            winChanceThreshold = MathUtil.Clamp(winChanceThreshold, 0.01f, 0.99f);

            if (defendUnits <= 0)
            {
                return 1;
            }

            int idealAttackUnits = MathUtil.RoundToInt(MathUtil.Normalize(winChanceThreshold, 0, 1, 0, defendUnits * 2f));
            BattleConfig battleConfig = new BattleConfig(idealAttackUnits, defendUnits, 0);
            float winChance = CalculateWinChance(battleConfig, roundConfig, balanceConfig);

            if (winChance >= winChanceThreshold)
            {
                // Search by incrementing attack units down

                bool foundTurningPoint;
                int currentAttackUnits = idealAttackUnits;

                do
                {
                    currentAttackUnits--;

                    if (currentAttackUnits <= 1)
                    {
                        break;
                    }

                    battleConfig = battleConfig.WithNewUnits(currentAttackUnits, defendUnits);

                    winChance = CalculateWinChance(battleConfig, roundConfig, balanceConfig);
                    foundTurningPoint = winChance <= winChanceThreshold;

                    if (!foundTurningPoint)
                    {
                        idealAttackUnits = currentAttackUnits;
                    }
                }
                while (!foundTurningPoint);
            }
            else
            {
                // Search by incrementing attack units up

                bool foundTurningPoint;
                int currentAttackUnits = idealAttackUnits;

                do
                {
                    currentAttackUnits++;

                    battleConfig = battleConfig.WithNewUnits(currentAttackUnits, defendUnits);

                    winChance = CalculateWinChance(battleConfig, roundConfig, balanceConfig);
                    foundTurningPoint = winChance >= winChanceThreshold;

                    if (!foundTurningPoint)
                    {
                        idealAttackUnits = currentAttackUnits;
                    }
                }
                while (!foundTurningPoint);
            }

            return idealAttackUnits;
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public class BattleInfo
    {
        [SerializeField] protected BattleConfig _battleConfig;
        [SerializeField] protected RoundConfig _roundConfig;
        [SerializeField] protected double[] _attackLossChances;
        [SerializeField] protected double[] _defendLossChances;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using Risk.Dice.Utility;
using UnityEngine;

namespace Risk.Dice
{
    [Serializable]
    public sealed class RoundInfo
    {
        [SerializeField] private RoundConfig _config;
        [SerializeField] private double[] _attackLossChances;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public class WinChanceInfo
    {
        [SerializeField] private int _size;
        [SerializeField] private RoundConfig _roundConfig;
        [SerializeField] private BalanceConfig _balanceConfig;
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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections;
using System.Linq;
using Risk.Dice.Utility;
using UnityEngine;

#if UNITY_ASSERTIONS
using UnityEngine.Assertions;
#endif

namespace Risk.Dice
{
    [Serializable]
    public sealed class BalancedBattleInfo : BattleInfo
    {
        [SerializeField] private BalanceConfig _balanceConfig;
        [SerializeField] private bool _balanceApplied;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Security.Cryptography;
using Risk.Dice.Utility;

namespace Risk.Dice.RNG
{
    public class CryptoRNG : IRNG
    {
        private readonly byte[] _buffer = new byte[8];

        private RNGCryptoServiceProvider _rng;

        public CryptoRNG ()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        public double NextDouble ()
        {
            return (double) (NextULong() / (decimal) ulong.MaxValue);
        }

        public int NextInt (int min, int max)
        {
            int diff = max - min;
            int mask = diff >> 31;
            int range = (mask ^ diff) - mask;

            if (range == 0)
            {
                return min;
            }

            _rng.GetBytes(_buffer, 0, sizeof(int));

            int value = BitConverter.ToInt32(_buffer, 0);
            mask = 1 << 31;

            if (diff < 0)
            {
                value |= mask;
            }
            else
            {
                value &= ~mask;
            }

            return (value % range) + min;
        }

        public uint NextUInt ()
        {
            _rng.GetBytes(_buffer, 0, sizeof(uint));
            return BitConverter.ToUInt32(_buffer, 0);
        }

        public ulong NextULong ()
        {
            _rng.GetBytes(_buffer, 0, sizeof(ulong));
            return BitConverter.ToUInt64(_buffer, 0);
        }

        public void NextBytes (byte[] data)
        {
            _rng.GetBytes(data);
        }
    }
}// Modified XorShift C# implementation based on XorShiftPlus
// Source: http://codingha.us/2018/12/17/xorshift-fast-csharp-random-number-generator/
// License:

/*
===============================[ XorShiftPlus ]==============================
==-------------[ (c) 2018 R. Wildenhaus - Licensed under MIT ]-------------==
=============================================================================
*/

namespace Risk.Dice.RNG
{
    public sealed class XorShiftRNG : IRNG
    {
        private ulong _stateX;
        private ulong _stateY;

        public XorShiftRNG (ulong seedX, ulong seedY)
        {
            _stateX = seedX;
            _stateY = seedY;
        }

        public double NextDouble ()
        {
            double value;
            ulong tempX, tempY, tempZ;

            tempX = _stateY;
            _stateX ^= _stateX << 23; tempY = _stateX ^ _stateY ^ (_stateX >> 17) ^ (_stateY >> 26);

            tempZ = tempY + _stateY;
            value = 4.6566128730773926E-10 * (0x7FFFFFFF & tempZ);

            _stateX = tempX;
            _stateY = tempY;

            return value;
        }

        public int NextInt (int min, int max)
        {
            uint uMax = unchecked((uint) (max - min));
            uint threshold = (uint) (-uMax) % uMax;

            while (true)
            {
                uint result = NextUInt();

                if (result >= threshold)
                {
                    return (int) (unchecked((result % uMax) + min));
                }
            }
        }

        public uint NextUInt ()
        {
            uint value;
            ulong tempX, tempY;

            tempX = _stateY;
            _stateX ^= _stateX << 23; tempY = _stateX ^ _stateY ^ (_stateX >> 17) ^ (_stateY >> 26);

            value = (uint) (tempY + _stateY);

            _stateX = tempX;
            _stateY = tempY;

            return value;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public unsafe void NextBytes (byte[] buffer)
        {
            ulong x = _stateX, y = _stateY, tempX, tempY, z;

            fixed (byte* pBuffer = buffer)
            {
                ulong* pIndex = (ulong*) pBuffer;
                ulong* pEnd = (ulong*) (pBuffer + buffer.Length);

                while (pIndex <= pEnd - 1)
                {
                    tempX = y;
                    x ^= x << 23; tempY = x ^ y ^ (x >> 17) ^ (y >> 26);

                    *(pIndex++) = tempY + y;

                    x = tempX;
                    y = tempY;
                }

                if (pIndex < pEnd)
                {
                    tempX = y;
                    x ^= x << 23; tempY = x ^ y ^ (x >> 17) ^ (y >> 26);
                    z = tempY + y;

                    byte* pByte = (byte*) pIndex;
                    while (pByte < pEnd) *(pByte++) = (byte) (z >>= 8);
                }
            }

            _stateX = x;
            _stateY = y;
        }
    }
}// Modified PCG C# implementation based on PCGSharp
// Source: https://github.com/igiagkiozis/PCGSharp/blob/master/PCGSharp/Source/Pcg.cs
// License:

// MIT License
// 
// Copyright (c) 2016 Bismur Studios Ltd.
// Copyright (c) 2016 Ioannis Giagkiozis
//
// This file is based on PCG, the original has the following license: 
/*
 * PCG Random Number Generation for C.
 *
 * Copyright 2014 Melissa O'Neill <oneill@pcg-random.org>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * For additional information about the PCG random number generation scheme,
 * including its license and other licensing options, visit
 *
 *      http://www.pcg-random.org
 */

namespace Risk.Dice.RNG
{
    public sealed class PCGRNG : IRNG
    {
        private ulong _state;
        private ulong _increment = 1442695040888963407ul;

        public PCGRNG (ulong seed) : this(seed, 721347520444481703ul)
        {
        }

        public PCGRNG (ulong seed, ulong sequence)
        {
            Initialize(seed, sequence);
        }

        private void Initialize (ulong seed, ulong sequence)
        {
            _state = 0ul;
            _increment = (sequence << 1) | 1;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        public double NextDouble ()
        {
            return NextUInt() * 2.3283064365386963E-10;
        }

        public int NextInt (int min, int max)
        {
            uint uMax = unchecked((uint) (max - min));
            uint threshold = (uint) (-uMax) % uMax;

            while (true)
            {
                uint result = NextUInt();

                if (result >= threshold)
                {
                    return (int) (unchecked((result % uMax) + min));
                }
            }
        }

        public uint NextUInt ()
        {
            ulong prevState = _state;
            _state = unchecked(prevState * 6364136223846793005ul + _increment);
            uint xorShifted = (uint) (((prevState >> 18) ^ prevState) >> 27);
            int rot = (int) (prevState >> 59);
            uint result = (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            return result;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public byte NextByte ()
        {
            uint result = NextUInt();
            return (byte) (result % 256);
        }

        public void NextBytes (byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = NextByte();
            }
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

#if UNITY_EDITOR || UNITY_64

using UnityEngine;

namespace Risk.Dice.RNG
{
    public class StatelessUnityRNG : IRNG
    {
        public double NextDouble ()
        {
            return Random.value;
        }

        public int NextInt (int min, int max)
        {
            return Random.Range(min, max);
        }

        public uint NextUInt ()
        {
            uint loRange = (uint) Random.Range(0, 1 << 30);
            uint hiRange = (uint) Random.Range(0, 1 << 2);

            return (loRange << 2) | hiRange;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public unsafe void NextBytes (byte[] data)
        {
            uint value = 0;

            for (int i = 0; i < data.Length; i++)
            {
                int part = i % sizeof(uint);

                if (part == 0)
                {
                    value = NextUInt();
                }

                byte* valuePtr = (byte*) &value;
                data[i] = valuePtr[part];
            }
        }
    }
}
#endif/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System.Collections.Generic;

namespace Risk.Dice.RNG
{
    public interface IRNG
    {
        /// <summary>
        /// Returns a random double between 0 and 1.
        /// </summary>
        double NextDouble ();

        /// <summary>
        /// Returns a random int between min [inclusive] and max [exclusive unless min == max].
        /// </summary>
        int NextInt (int min, int max);

        /// <summary>
        /// Returns a random uint full range.
        /// </summary>
        uint NextUInt ();

        /// <summary>
        /// Returns a random ulong full range.
        /// </summary>
        ulong NextULong ();

        /// <summary>
        /// Randomizes the bytes in the passed in byte array.
        /// </summary>
        void NextBytes (byte[] data);
    }

    public static class RNGUtil
    {
        public static IRNG DefaultSeeder => new CryptoRNG();
        public static IRNG Default => new PCGRNG(DefaultSeeder.NextULong());

        private static IRNG _seeder = DefaultSeeder;
        public static IRNG Seeder => _seeder;

        public static int GetDiceRoll (this IRNG rng)
        {
            return rng.NextInt(0, 6);
        }

        public static T GetElement<T> (this IRNG rng, IList<T> list, T defaultValue = default)
        {
            if (rng == null || list == null || list.Count == 0)
            {
                return defaultValue;
            }

            return list[rng.NextInt(0, list.Count)];
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using Sirenix.OdinInspector;
using Risk.Dice.Utility;
using UnityEngine;

namespace Risk.Dice.RNG
{
    public enum RNGType
    {
        Crypto = 0,
        System = 1,
#if UNITY_EDITOR || UNITY_64
        Unity = 2,
        UnityStateless = 3,
#endif
        MersenneTwister = 4,
        XorShift = 5,
        PCG = 6
    }

    public enum SeedMode
    {
        Auto,
        None,
        Int,
        Ulong,
        Ulong2,
        UInts
    }

    [Serializable]
    public sealed class RNGConfig : IEquatable<RNGConfig>
    {
        [SerializeField] [LabelText("RNG Type")] private RNGType _rngType;
        [SerializeField] [InlineButton(nameof(RandomizeSeeds), "Randomize")] [ValidateInput(nameof(ValidateSeedMode))] private SeedMode _seedMode;
        [SerializeField] [ShowIf(nameof(_seedMode), SeedMode.Int)] private int _intSeed;
        [SerializeField] [ShowIf(nameof(ShowUlong))] private ulong _ulongSeed1;
        [SerializeField] [ShowIf(nameof(_seedMode), SeedMode.Ulong2)] private ulong _ulongSeed2;
        [SerializeField] [ShowIf(nameof(_seedMode), SeedMode.UInts)] private uint[] _uintsSeed;

        public bool IsValid => ValidateSeedMode();

        public static RNGConfig Default => new RNGConfig();

        public RNGConfig () : this(RNGType.PCG, SeedMode.Auto)
        {
        }

        public RNGConfig (RNGType type) : this(type, SeedMode.Auto)
        {
        }

        public RNGConfig (RNGType type, SeedMode seedMode)
        {
            _rngType = type;
            _seedMode = seedMode;
        }

        public void SetSeed ()
        {
            _seedMode = SeedMode.None;
        }

        public void SetSeed (int intSeed)
        {
            _seedMode = SeedMode.Int;
            _intSeed = intSeed;
        }

        public void SetSeed (ulong ulongSeed)
        {
            _seedMode = SeedMode.Ulong;
            _ulongSeed1 = ulongSeed;
        }

        public void SetSeed (ulong ulongSeed1, ulong ulongSeed2)
        {
            _seedMode = SeedMode.Ulong2;
            _ulongSeed1 = ulongSeed1;
            _ulongSeed2 = ulongSeed2;
        }

        public void SetSeed (uint[] uintsSeed)
        {
            _seedMode = SeedMode.UInts;
            _uintsSeed = uintsSeed;
        }

        public IRNG GetRNG ()
        {
            if (!IsValid)
            {
                return null;
            }

            switch (_rngType)
            {
                case RNGType.Crypto:
                    {
                        return new CryptoRNG();
                    }

                case RNGType.System:
                    {
                        switch (_seedMode)
                        {
                            case SeedMode.Auto:
                            case SeedMode.None:
                                return new SystemRNG();
                            case SeedMode.Int:
                                return new SystemRNG(_intSeed);
                        }

                        break;
                    }

#if UNITY_EDITOR || UNITY_64

                case RNGType.Unity:
                    {
                        switch (_seedMode)
                        {
                            case SeedMode.Auto:
                            case SeedMode.None:
                                return new UnityRNG();
                            case SeedMode.Int:
                                return new UnityRNG(_intSeed);
                        }

                        break;
                    }

                case RNGType.UnityStateless:
                    {
                        return new StatelessUnityRNG();
                    }

#endif

                case RNGType.MersenneTwister:
                    {
                        switch (_seedMode)
                        {
                            case SeedMode.Auto:
                            case SeedMode.None:
                                return new MersenneTwisterRNG();
                            case SeedMode.Int:
                                return new MersenneTwisterRNG(_intSeed);
                            case SeedMode.UInts:
                                return new MersenneTwisterRNG(_uintsSeed);
                        }

                        break;
                    }

                case RNGType.XorShift:
                    {
                        switch (_seedMode)
                        {
                            case SeedMode.Auto:
                                {
                                    byte[] data = new byte[16];
                                    RNGUtil.Seeder.NextBytes(data);
                                    ByteBuffer buffer = new ByteBuffer(data);

                                    ulong seed1 = buffer.ReadUlong();
                                    ulong seed2 = buffer.ReadUlong();

                                    return new XorShiftRNG(seed1, seed2);
                                }
                            case SeedMode.Ulong2:
                                return new XorShiftRNG(_ulongSeed1, _ulongSeed2);
                        }

                        break;
                    }

                case RNGType.PCG:
                    {
                        switch (_seedMode)
                        {
                            case SeedMode.Auto:
                                {
                                    byte[] data = new byte[8];
                                    RNGUtil.Seeder.NextBytes(data);
                                    ByteBuffer buffer = new ByteBuffer(data);

                                    ulong seed = buffer.ReadUlong();

                                    return new PCGRNG(seed);
                                }
                            case SeedMode.Ulong:
                                return new PCGRNG(_ulongSeed1);
                            case SeedMode.Ulong2:
                                return new PCGRNG(_ulongSeed1, _ulongSeed2);
                        }

                        break;
                    }
            }

            return default;
        }

        public bool Equals (RNGConfig other)
        {
            if (_rngType != other._rngType
                    || _seedMode != other._seedMode
                    || _intSeed != other._intSeed
                    || _ulongSeed1 != other._ulongSeed1
                    || _ulongSeed2 != other._ulongSeed2)
            {
                return false;
            }

            return true;
        }

        #region ODIN

        private bool ShowUlong => _seedMode == SeedMode.Ulong || _seedMode == SeedMode.Ulong2;

        private bool ValidateSeedMode ()
        {
            string unused = default;
            return ValidateSeedMode(_seedMode, ref unused);
        }

        private bool ValidateSeedMode (SeedMode value, ref string error)
        {
            if (value == SeedMode.Auto)
            {
                return true;
            }

            bool isError = false;

            switch (_rngType)
            {
                case RNGType.Crypto:
                    isError = value != SeedMode.None;
                    break;
                case RNGType.System:
                    isError = value != SeedMode.None && value != SeedMode.Int;
                    break;
#if UNITY_EDITOR || UNITY_64
                case RNGType.Unity:
                    isError = value != SeedMode.None && value != SeedMode.Int;
                    break;
                case RNGType.UnityStateless:
                    isError = value != SeedMode.None;
                    break;
#endif
                case RNGType.MersenneTwister:
                    isError = value != SeedMode.None && value != SeedMode.Int && value != SeedMode.UInts;
                    break;
                case RNGType.XorShift:
                    isError = value != SeedMode.Ulong2;
                    break;
                case RNGType.PCG:
                    isError = value != SeedMode.Ulong && value != SeedMode.Ulong2;
                    break;
            }

            if (isError && string.IsNullOrEmpty(error))
            {
                error = $"Selected RNG does not support seed mode | RNG: {_rngType} | Seed Mode: {value}";
            }

            return !isError;
        }

        private void RandomizeSeeds ()
        {
            byte[] data = new byte[16];
            RNGUtil.Seeder.NextBytes(data);
            ByteBuffer buffer = new ByteBuffer(data);

            _intSeed = buffer.ReadInt();
            buffer.Reset();

            _ulongSeed1 = buffer.ReadUlong();
            _ulongSeed2 = buffer.ReadUlong();
            buffer.Reset();

            _uintsSeed = new uint[4];

            for (int i = 0; i < _uintsSeed.Length; i++)
            {
                _uintsSeed[i] = buffer.ReadUint();
            }
        }

        #endregion
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

#if UNITY_EDITOR || UNITY_64

using System;
using Random = UnityEngine.Random;

namespace Risk.Dice.RNG
{
    public class UnityRNG : IRNG
    {
        protected Random.State _state;

        public UnityRNG () : this(Environment.TickCount)
        {
        }

        public UnityRNG (int seed)
        {
            Random.State prevState = Random.state;
            Random.InitState(seed);
            _state = Random.state;
            Random.state = prevState;
        }

        public double NextDouble ()
        {
            Random.State prevState = Random.state;
            Random.state = _state;

            double value = Random.value;

            _state = Random.state;
            Random.state = prevState;

            return value;
        }

        public int NextInt (int min, int max)
        {
            Random.State prevState = Random.state;
            Random.state = _state;

            int value = Random.Range(min, max);

            _state = Random.state;
            Random.state = prevState;

            return value;
        }

        public uint NextUInt ()
        {
            Random.State prevState = Random.state;
            Random.state = _state;

            uint loRange = (uint) Random.Range(0, 1 << 30);
            uint hiRange = (uint) Random.Range(0, 1 << 2);

            _state = Random.state;
            Random.state = prevState;

            return (loRange << 2) | hiRange;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public unsafe void NextBytes (byte[] data)
        {
            uint value = 0;

            for (int i = 0; i < data.Length; i++)
            {
                int part = i % sizeof(uint);

                if (part == 0)
                {
                    value = NextUInt();
                }

                byte* valuePtr = (byte*) &value;
                data[i] = valuePtr[part];
            }
        }
    }
}
#endif/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using Random = System.Random;
using MersenneTwister;

namespace Risk.Dice.RNG
{
    public sealed class MersenneTwisterRNG : IRNG
    {
        private Random _random;
        private readonly byte[] _buffer = new byte[8];

        public MersenneTwisterRNG ()
        {
            _random = MTRandom.Create();
        }

        public MersenneTwisterRNG (int seed)
        {
            _random = MTRandom.Create(seed);
        }

        public MersenneTwisterRNG (uint[] seed)
        {
            _random = MTRandom.Create(seed);
        }

        public double NextDouble ()
        {
            return _random.NextDouble();
        }

        public int NextInt (int min, int max)
        {
            return _random.Next(min, max);
        }

        public uint NextUInt ()
        {
            uint loRange = (uint) _random.Next(1 << 30);
            uint hiRange = (uint) _random.Next(1 << 2);

            return (loRange << 2) | hiRange;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public void NextBytes (byte[] data)
        {
            _random.NextBytes(data);
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using Random = System.Random;

namespace Risk.Dice.RNG
{
    public sealed class SystemRNG : IRNG
    {
        private Random _random;

        public SystemRNG () : this(Environment.TickCount)
        {
            
        }

        public SystemRNG (int seed)
        {
            _random = new Random(seed);
        }

        public double NextDouble ()
        {
            return _random.NextDouble();
        }

        public int NextInt (int min, int max)
        {
            return _random.Next(min, max);
        }

        public uint NextUInt ()
        {
            uint loRange = (uint) _random.Next(1 << 30);
            uint hiRange = (uint) _random.Next(1 << 2);

            return (loRange << 2) | hiRange;
        }

        public unsafe ulong NextULong ()
        {
            ulong value = 0;
            uint* valuePtr = (uint*) &value;
            valuePtr[0] = NextUInt();
            valuePtr[1] = NextUInt();
            return value;
        }

        public unsafe void NextBytes (byte[] data)
        {
            uint value = 0;

            for (int i = 0; i < data.Length; i++)
            {
                int part = i % sizeof(uint);

                if (part == 0)
                {
                    value = NextUInt();
                }

                byte* valuePtr = (byte*) &value;
                data[i] = valuePtr[part];
            }
        }
    }
}using System;
using System.Collections.Generic;

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
        [SerializeField] private RoundConfig _config;
        [SerializeField] private double[] _attackLossChances;

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
    public struct RoundConfig : IEquatable<RoundConfig>
    {
        [SerializeField] private int _diceFaceCount;
        [SerializeField] private int _attackDiceCount;
        [SerializeField] private int _defendDiceCount;
        [SerializeField] private bool _favourDefenderOnDraw;

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


class Hello
{
  static void Main(string[] args)
  {
      Console.WriteLine("Hello World 2");
      var roundInfo = RoundCache.Get(RoundConfig.Default);
      roundInfo.Calculate();
      Console.WriteLine(roundInfo.AttackLossChances[2]);
      Console.WriteLine("... did it print anything?");
  }
}
/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;

namespace Risk.Dice.Utility
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T> ()
                where T : struct, Enum
        {
            foreach (object value in Enum.GetValues(typeof(T)))
            {
                yield return (T) value;
            }
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System.Collections.Generic;

namespace Risk.Dice.Utility
{
    public struct DescendingIntComparer : IComparer<int>
    {
        int IComparer<int>.Compare (int x, int y)
        {
            return y - x;
        }
    }
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;

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
}/*
 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

#if !UNITY
#endif

/*

 *
 * Copyright 2021 SMG Studio.
 *
 * RISK is a trademark of Hasbro. ©2020 Hasbro.All Rights Reserved.Used under licence.
 *
 * You are hereby granted a non-exclusive, limited right to use and to install, one (1) copy of the
 * software for internal evaluation purposes only and in accordance with the provisions below.You
 * may not reproduce, redistribute or publish the software, or any part of it, in any form.
 * 
 * SMG may withdraw this licence without notice and/or request you delete any copies of the software
 * (including backups).
 *
 * The Agreement does not involve any transfer of any intellectual property rights for the
 * Software. SMG Studio reserves all rights to the Software not expressly granted in writing to
 * you.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using UnityEngine;

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