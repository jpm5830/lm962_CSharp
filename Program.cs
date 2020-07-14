using System;
using System.Threading;

namespace Lobstermania
{
	public static class Program
	{

        // IMPORTANT NUMBERS
        // -----------------
        // Theoretical spins per JACKPOT 8,107,500
        // Number of all slot combinations (47x46x48x50x50 -- 1 slot per reel) = 259,440,000
        // Number of all possible symbol combinations on a line (11x11x11x10x10) = 133,100

        public static void Main()
        {
        mainMenu:
            Console.Clear();
            Console.WriteLine("PRESS:\n");
            Console.WriteLine("\t1 for Bulk Game Spins\n\t2 for Individual Games\n\t3 to quit\n");
            Console.Write("Your choice: ");
            string res = Console.ReadLine();
            int num;
            try
            {
                num = Convert.ToInt32(res);
            } 
            catch (Exception)
            {
                Console.WriteLine("\n***ERROR: Please enter number 1, 2, or 3 !!!");
                Thread.Sleep(2000); // sleep for 2 seconds
                goto mainMenu;
            }
            switch(num)
            {
                case 1:
                    long numSpins;
                    int paylines; // number of paylines to play
                    
                    labelNumSpins:
                    Console.Write("\nEnter the number of spins:  ");
                    try
                    {
                        numSpins = Convert.ToInt64(Console.ReadLine()); // convert to a long
                        if (numSpins <= 0)
                            throw new ArgumentOutOfRangeException();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\n***ERROR: Please enter a positive number greater than 0 !!!");
                        goto labelNumSpins;
                    }

                    labelActivePaylines:
                    Console.Write("Enter the number of active paylines (1 through 15):  ");

                    try
                    {
                        paylines = Convert.ToInt32(Console.ReadLine()); // convert to an int

                        if (paylines < 1 || paylines > 15)
                            throw new ArgumentOutOfRangeException();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\n***ERROR: Please enter a positive number between 1 and 15 !!!");
                        goto labelActivePaylines;
                    }

                    Console.Clear();
                    BulkGameSpins(numSpins,paylines);
                    Console.WriteLine("Press any key to continue to Main Menu ...");
                    Console.ReadKey();
                    goto mainMenu;
                case 2:
                    Console.Clear();
                    IndividualGames();
                    goto mainMenu;
                case 3:
                    Environment.Exit(0); // planned exit
                    break;
                default:
                    goto mainMenu;
            }

        } // End method Main

        private static void BulkGameSpins(long numSpins, int numPayLines)
        {
            LM962 game = new LM962()
            {
                activePaylines = numPayLines
            };

            DateTime start_t = DateTime.Now;

            Console.WriteLine("Progress Bar ({0:N0} spins)\n",numSpins);
            Console.WriteLine("0%       100%");
            Console.WriteLine("|--------|");


            if (numSpins <= 10)
            {
                Console.WriteLine("**********"); // 10 markers
                for (long i = 1; i <= numSpins; i++)
                    game.Spin();
            }
            else
            {
                int marks = 1; // Number of printed marks
                long markerEvery = (long)Math.Ceiling((double)numSpins / (double)10); // print progression marker at every 1/10th of total spins.

                for (long i = 1; i <= numSpins; i++)
                {
                    game.Spin();
                    if ((i % markerEvery == 0))
                    {
                        Console.Write("*");
                        marks++;
                    }
                }

                for (int i = marks; i <= 10; i++)
                    Console.Write("*");
            }

            Console.WriteLine();
            game.stats.DisplaySessionStats(numPayLines);

            DateTime end_t = DateTime.Now;
            TimeSpan runtime = end_t - start_t;
            Console.WriteLine("\nRun completed in {0:t}\n", runtime);

        } // End method BulkGameSpins

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
                Console.WriteLine("\nPlaying {0} active paylines.\n", game.activePaylines);

                game.Spin();
                game.stats.DisplayGameStats ();
                game.stats.ResetGameStats();

                Console.WriteLine("Press the P key to change the number of pay lines");
                Console.WriteLine("Press the Escape key to return to the Main Menu");
                Console.WriteLine("\nPress any other key to continue playing.");
                ConsoleKeyInfo cki = Console.ReadKey(true);

                if (cki.KeyChar == 'p')
                {
                getPayLines:
                    Console.Write("\nEnter the new number of active paylines (1 through 15):  ");
                    int paylines;
                    try
                    {
                        paylines = Convert.ToInt32(Console.ReadLine()); // convert to an int

                        if (paylines < 1 || paylines > 15)
                            throw new ArgumentOutOfRangeException();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\n***ERROR: Please enter a positive number between 1 and 15 !!!");
                        goto getPayLines;
                    }
                    
                    game.activePaylines = paylines;

                } // end if cki.KeyChar == 'p'

                if (cki.Key == ConsoleKey.Escape) // quit when you hit the escape key
                    break;
            }  // end for ever

        } // End method IndividualGames

    } // End class Program

} // end namespace
