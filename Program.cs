using System;

namespace Lobstermania
{
	public class Program
	{
        public static void Main()
        {
            LM962 game = new LM962();
            Stats stats = new Stats();

            DateTime start_t = DateTime.Now;

            DateTime end_t = DateTime.Now;
            TimeSpan runtime = end_t - start_t;
            Console.WriteLine("\nRun completed in {0:t}\n", runtime);
        } // End method Main
    }

    /*
     *             rng.HitFreq();
            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).\n",
                rng.hitCount, rng.hitSpins, rng.hitFreq);

            rng.PaybackPercentage();

            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).",
                rng.hitCount, rng.paybackSpins, rng.hitFreq);

            Console.WriteLine("\n{0:N0} credits spent, {1:N0} credits won, {2:P1} payback percentage.",
                rng.paybackSpins, rng.paybackCredits, rng.paybackPercentage);

            Console.WriteLine("\nThere were {0:N0} bonus wins out of {1:N0} spins. (Average win was {2:N0} credits per bonus.)",
                rng.bonusWinCount, rng.paybackSpins, (double)rng.bonusWinSum / (double)rng.bonusWinCount);

            Console.WriteLine("\nThere were {0:N0} scatter wins out of {1:N0} spins. (Average win was {2:N0} credits per scatter.)\n",
                rng.scatterWinCount, rng.paybackSpins, (double)rng.scatterWinCredits / (double)rng.scatterWinCount);

            Console.WriteLine("Average number of spins to bonus:   {0:N0}", (double)rng.paybackSpins / (double)rng.bonusWinCount);
            Console.WriteLine("Average number of spins to scatter: {0:N0}\n", (double)rng.paybackSpins / (double)rng.scatterWinCount);


     */

} // end namespace
