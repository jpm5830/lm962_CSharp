using System;

namespace Lobstermania
{

    public class Stats
    {
        // CUMULATIVE SESSION COUNTERS
        public long numJackpots = 0L;
        public long hitCount = 0L; // Count of winning spins
        public long spins = 0L;
        double hitFreq = 0.0; // line hit frequency. (1.00 = 100%)
        double jackpotFreq = 0.0; // Number of jackpots / (number of spins * activeLines)

        public long paybackCredits = 0L; // Line + Scatter winnings in credits (Line wins includes bonus wins)
        double paybackPercentage = 0.0; // Payback %, (1.00 = 100%)

        public long scatterWinCount = 0L;
        public long scatterWinCredits = 0L;

        public long bonusWinCount = 0L;
        public long bonusWinCredits = 0L;

        // INDIVIDUAL GAME COUNTERS
        public int igWin = 0; // ig = individual game, win (all paylines)
        public int igScatterWin = 0;
        public int igBonusWin = 0;


        public void ResetGameStats()
        {
            igWin = 0;
            igScatterWin = 0;
            igBonusWin = 0;

        } // End method ResetGameStats

        public void ResetSessionStats()
        {
            numJackpots = 0L;
            hitCount = 0L; // Count of winning spins
            spins = 0L;
            hitFreq = 0.0; // line hit frequency. (1.00 = 100%)
            jackpotFreq = 0.0;

            paybackCredits = 0L; // Line + Scatter winnings in credits (Line wins includes bonus wins)
            paybackPercentage = 0.0; // Payback %, (1.00 = 100%)

            scatterWinCount = 0L;
            scatterWinCredits = 0L;

            bonusWinCount = 0L;
            bonusWinCredits = 0L;

    } // End method ResetSessionStats
    public void DisplaySessionStats(int activePaylines)
        {
            hitFreq = (double)hitCount / ((double)spins * (double)activePaylines);
            paybackPercentage = (double)paybackCredits / ((double)spins * (double)activePaylines);
            jackpotFreq = (double)numJackpots / ((double)spins * (double)activePaylines);

            Console.WriteLine("\nPlaying {0} active paylines.", activePaylines);

            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).",
                hitCount, spins, hitFreq);

            Console.WriteLine("\n{0:N0} credits spent, {1:N0} credits won, {2:P1} payback percentage.",
                spins * activePaylines, paybackCredits, paybackPercentage);

            Console.WriteLine("\nThere were {0:N0} bonus wins out of {1:N0} spins. (Average win was {2:N0} credits per bonus.)",
                bonusWinCount, spins, (double)bonusWinCredits / (double)bonusWinCount);

            Console.WriteLine("\nThere were {0:N0} scatter wins out of {1:N0} spins. (Average win was {2:N0} credits per scatter.)",
                scatterWinCount, spins, (double)scatterWinCredits / (double)scatterWinCount);

            Console.WriteLine("\nThere were {0:N0} JACKPOT wins out of {1:N0} spins (Jackpot Hit Freq of {2:P6}).\n",
                numJackpots, spins, jackpotFreq);

            Console.WriteLine("Average number of spins to JACKPOT:   {0:N0}", (double)spins / (double)numJackpots);
            Console.WriteLine("Average number of spins to Bonus  :   {0:N0}", (double)spins / (double)bonusWinCount);
            Console.WriteLine("Average number of spins to Scatter:   {0:N0}\n", (double)spins / (double)scatterWinCount);
        
        } // End method DisplaySessionStats

        public void DisplayGameStats()
        {
            Console.WriteLine("\nTOTAL CREDITS WON: {0:N0}  SCATTER WIN: {1:N0}  BONUS WIN: {2:N0}\n",
                igWin, igScatterWin, igBonusWin);

        } // End method DisplayGameStats

    } // end class Stats

} // end namespace
