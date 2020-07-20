using System;

namespace Lobstermania
{

    public class Stats
    {
        // CUMULATIVE SESSION COUNTERS -- Fields
        private long _numJackpots = 0L;
        private long _spins = 0L;
        private double _hitFreq = 0.0; // line hit frequency. (1.00 = 100%)
        private double _jackpotFreq = 0.0; // Number of jackpots / (number of _spins * activeLines)

        private long _paybackCredits = 0L; // Line + Scatter winnings in credits (Line wins includes bonus wins)
        private double _paybackPercentage = 0.0; // Payback %, (1.00 = 100%)

        private long _scatterWinCount = 0L;
        private long _scatterWinCredits = 0L;

        private long _bonusWinCount = 0L;
        private long _bonusWinCredits = 0L;

        // INDIVIDUAL GAME COUNTERS -- Fields
        private int _igWin = 0; // ig = individual game, win (all paylines)
        private int _igScatterWin = 0;
        private int _igBonusWin = 0;

        // PROPERTIES
        public long NumJackpots { get => _numJackpots; set => _numJackpots = value; }
        public long HitCount { get; set; } = 0L;
        public long Spins { get => _spins; set => _spins = value; }
        public long PaybackCredits { get => _paybackCredits; set => _paybackCredits = value; }
        private double PaybackPercentage { get => _paybackPercentage; set => _paybackPercentage = value; } 
        public long ScatterWinCount { get => _scatterWinCount; set => _scatterWinCount = value; }
        public long ScatterWinCredits { get => _scatterWinCredits; set => _scatterWinCredits = value; }
        public long BonusWinCount { get => _bonusWinCount; set => _bonusWinCount = value; }
        public long BonusWinCredits { get => _bonusWinCredits; set => _bonusWinCredits = value; }
        public int GameWin { get => _igWin; set => _igWin = value; }
        public int GameScatterWin { get => _igScatterWin; set => _igScatterWin = value; }
        public int GameBonusWin { get => _igBonusWin; set => _igBonusWin = value; }
        private double HitFreq { get => _hitFreq; set => _hitFreq = value; } 
        private double JackpotFreq { get => _jackpotFreq; set => _jackpotFreq = value; } 

        public void ResetGameStats()
        {
            GameWin = 0;
            GameScatterWin = 0;
            GameBonusWin = 0;

        } // End method ResetGameStats

        public void ResetSessionStats()
        {
            NumJackpots = 0L;
            HitCount = 0L; // Count of winning _spins
            Spins = 0L;
            HitFreq = 0.0; // line hit frequency. (1.00 = 100%)
            JackpotFreq = 0.0;

            PaybackCredits = 0L; // Line + Scatter winnings in credits (Line wins includes bonus wins)
            PaybackPercentage = 0.0; // Payback %, (1.00 = 100%)

            ScatterWinCount = 0L;
            ScatterWinCredits = 0L;

            BonusWinCount = 0L;
            BonusWinCredits = 0L;

        } // End method ResetSessionStats
        public void DisplaySessionStats(int activePaylines)
        {
            HitFreq = (double)HitCount / ((double)Spins * (double)activePaylines);
            PaybackPercentage = (double)PaybackCredits / ((double)Spins * (double)activePaylines);
            JackpotFreq = (double)NumJackpots / ((double)Spins * (double)activePaylines);

            Console.WriteLine("\nMEASURED SESSION STATISTICS:");
            Console.WriteLine("----------------------------");

            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).",
                HitCount, Spins, HitFreq);

            Console.WriteLine("\n{0:N0} credits spent, {1:N0} credits won, {2:P1} payback percentage.",
                Spins * activePaylines, PaybackCredits, PaybackPercentage);

            Console.WriteLine("\nThere were {0:N0} bonus wins out of {1:N0} spins. (Average win was {2:N0} credits per bonus.)",
                BonusWinCount, Spins, (double)BonusWinCredits / (double)BonusWinCount);

            Console.WriteLine("\nThere were {0:N0} scatter wins out of {1:N0} spins. (Average win was {2:N0} credits per scatter.)",
                ScatterWinCount, Spins, (double)ScatterWinCredits / (double)ScatterWinCount);

            Console.WriteLine("\nThere were {0:N0} JACKPOT wins out of {1:N0} spins (Jackpot Hit Freq of {2:P6}).\n",
                NumJackpots, Spins, JackpotFreq);

            Console.WriteLine("Average number of spins to JACKPOT:   {0:N0}", (double)Spins / (double)NumJackpots);
            Console.WriteLine("Average number of spins to Bonus  :   {0:N0}", (double)Spins / (double)BonusWinCount);
            Console.WriteLine("Average number of spins to Scatter:   {0:N0}\n", (double)Spins / (double)ScatterWinCount);

        } // End method DisplaySessionStats

        public void DisplayGameStats()
        {
            Console.WriteLine("\nTOTAL CREDITS WON: {0:N0}  SCATTER WIN: {1:N0}  BONUS WIN: {2:N0}\n",
                GameWin, GameScatterWin, GameBonusWin);

        } // End method DisplayGameStats

    } // end class Stats

} // end namespace
