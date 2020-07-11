using System;

namespace Lobstermania
{
	public static class Program
	{
        const long ALL_SLOT_COMBOS = 259440000L; // 259 million --- 259,440,000 is the number of all possible slot combinations on a line (47x46x48x50x50).
        const long ALL_SLOT_COMBOS_10x = 2594400000L; // 2.5 billion -- 10 times ALL_SLOT_COMBOS

        public static void Main()
        {
            startover:
            Console.Clear();
            Console.Write("Press 1 for Fixed Game Runs, 2 for Individual games: ");
            int num = Convert.ToInt32(Console.ReadLine());
            switch(num)
            {
                case 1:
                    long numSpins;
                    int paylines; // number of paylines to play

                    Console.Write("\nEnter the number of spins:  ");
                    numSpins = Convert.ToInt64(Console.ReadLine()); // convert to a long
                    Console.Write("Enter the number of active paylines (1 through 15):  ");
                    paylines = Convert.ToInt32(Console.ReadLine()); // convert to a long
                    Console.Clear();
                    TestStats(numSpins,paylines);
                    break;
                case 2:
                    Console.Clear();
                    IndividualGames();
                    break;
                default:
                    goto startover;
            }

        } // End method Main

        private static void TestStats(long numSpins, int numPayLines)
        {
            LM962 game = new LM962()
            {
                activePaylines = numPayLines
            };

            DateTime start_t = DateTime.Now;

            for (long i = 0; i < numSpins; i++) 
            {
                game.Spin();
                if ((i % (numSpins/10L)) == 0) // print progression marker at every 1/10th of total spins.
                    Console.WriteLine("-> {0:N0}", i);
            }
           
            game.DisplayStats();

            DateTime end_t = DateTime.Now;
            TimeSpan runtime = end_t - start_t;
            Console.WriteLine("\nRun completed in {0:t}\n", runtime);

        } // End method TestStats

        private static void IndividualGames()
        {
            LM962 game = new LM962
            {
                printGameboard = true,
                printPaylines = true
            };

            for (; ;) // ever
            {
                Console.Clear(); // clear the console screen
                game.Spin();
                game.stats.DisplayGameStats ();
                game.stats.ResetGameStats();

                Console.WriteLine("Press any key to spin again, or Escape to quit ...");
                if (Console.ReadKey(true).Key == ConsoleKey.Escape) // quit when you hit the escape key
                    break;
            }  

        } // End method IndividualGames

    } // End class Program

} // end namespace
