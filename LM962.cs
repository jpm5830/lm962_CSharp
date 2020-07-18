using System;

namespace Lobstermania
{
    class LM962
    {
        const int NUM_REELS = 5;
        const int GAMEBOARD_ROWS = 3;
        const int MAX_PAYLINES = 15; 

        enum Symbols : byte { WS, LM, BU, BO, LH, TU, CL, SG, SF, LO, LT }; // All 11 game symbols
        const int DISTINCT_SYMBOLS = 11;

        readonly int[][] SYMBOL_COUNTS = new int[][]
        {
            new int[] { 2, 4, 4, 6, 5, 6, 6, 5, 5, 2, 2 },
            new int[] { 2, 4, 4, 4, 4, 4, 6, 6, 5, 5, 2 },
            new int[] { 1, 3, 5, 4, 6, 5, 5, 5, 6, 6, 2 },
            new int[] { 4, 4, 4, 4, 6, 6, 6, 6, 8, 0, 2 },
            new int[] { 2, 4, 5, 4, 7, 7, 6, 6, 7, 0, 2 }
        };

        readonly int[,] PAYOUTS =
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 100, 40, 25, 25, 10, 10, 5, 5, 5, 331, 5 },
            { 500, 200, 100, 100, 50, 50, 30, 30, 30, 0,25 },
            { 10000, 1000, 500, 500, 500, 250, 200, 200, 150, 0, 200 }
        };

        public static readonly Random rand = new Random(); // Random Number Generator (RNG) object

        readonly Symbols[][] reels  = new Symbols[][] // 5 reels in this game
        {
            new Symbols[47], // 47 slots in this reel
            new Symbols[46],
            new Symbols[48],
            new Symbols[50],
            new Symbols[50]
        };

        readonly Symbols[,] gameBoard = new Symbols[GAMEBOARD_ROWS, NUM_REELS];
        readonly Symbols[][] payLines = new Symbols[MAX_PAYLINES][];
        public int activePaylines = MAX_PAYLINES; //number of active paylines, default of 15

        // Print flags
        public bool printReels = false;
        public bool printGameboard = false;
        public bool printPaylines = false;

        public Stats stats = new Stats(); // Game statistics object

        public LM962()
        {
            // Build the reels
            BuildReels();

            // Randomize the reels
            for(int i=0;i<NUM_REELS;i++) // randomize each of the 5 reels
                ArrayShuffle.Shuffle(reels[i]);

        } // End constructor

        private void BuildReels()
        {
            // Build reels 1,2,3
            for (int i = 0; i < NUM_REELS; i++) // For each reel 
            {

                int idx = 0; // reel slot index
                for (int j = 0; j < DISTINCT_SYMBOLS; j++) // For each Symbol
                {
                    if ((j == 9) && (i > 2)) continue; // No bonus (LO) symbols on reels 4 and 5
                    for (int k = 0; k < SYMBOL_COUNTS[i][j]; k++) // number of times to repeat each symbol
                        reels[i][idx++] = (Symbols)j; // assign symbol to slot
                }
            }

        } // End method BuildReels

        private void PrintReels()
        {
            for(int num=0; num<NUM_REELS; num++) // all 5 reels
            {
                Console.Write("\nReel[{0}]: [  ", num);
                for (int s = 0; s < reels[num].Length - 1; s++)
                    Console.Write("{0}, ", reels[num][s].ToString());
                Console.WriteLine("{0} ]", reels[num][reels[num].Length-2].ToString());
            }
        } // End method PrintReels

        public void Spin()
        {
            stats.spins++;
            if (printReels)
                PrintReels();
            UpdateGameboard();
            if (printGameboard)
                PrintGameboard();
            UpdatePaylines();

            if (printPaylines)
            {
                Console.WriteLine("WINNING PAY LINES");
                Console.WriteLine("-----------------");
            }

            for (int i = 0; i < activePaylines; i++) // for each payline
            {                
                int linePayout = GetLinePayout(payLines[i]); // will include any bonus win
                if(linePayout > 0)
                {
                    stats.igWin += linePayout;
                    stats.hitCount++;
                    stats.paybackCredits += linePayout;
                    if (printPaylines)
                        PrintPayline(i+1, payLines[i], linePayout);
                }
            } // end for (each payLine)

            int scatterWin = GetScatterWin();

            if (scatterWin > 0)
            {
                // stats.paybackCredits are updated in GetScatterWin()
                stats.hitCount++;
                stats.igScatterWin = scatterWin; // only 1 scatter win allowed per game
                stats.igWin += scatterWin; // add to total game win
            }

        } // End method Spin

        private int GetLinePayout(Symbols[] line)
        {
            int count = 1; // count of consecutive matching symbols, left to right 
            Symbols sym = line[0];
            int bonusWin = 0;

            switch (sym)
            {
                case Symbols.LO: // Bonus
                    for (int i = 1; i < 3; i++)
                    {
                        if (line[i] == Symbols.LO)
                            count++;
                        else
                            break;
                    }

                    if (count == 3)
                    {
                        bonusWin = Bonus.GetPrizes();
                        stats.bonusWinCount++;
                        stats.bonusWinCredits += bonusWin;
                        stats.igBonusWin += bonusWin;
                    }
                    else
                        bonusWin = 0;

                    break; // case Symbols.LO

                case Symbols.LT: // Scatter
                    count = 1; // Scatter handled at gameboard level
                    break; // case "LT"

                case Symbols.WS: // Wild
                    Symbols altSym = Symbols.WS;

                    for (int i = 1; i < NUM_REELS; i++)
                        if ((line[i] == sym) || (line[i] == altSym))
                            count++;
                        else
                        {
                            if (!line[i].Equals(Symbols.LO) && !line[i].Equals(Symbols.LT) && altSym.Equals(Symbols.WS))
                            {
                                altSym = line[i];
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }

                    sym = altSym; // count and sym are now set correctly 

                    // ANOMOLY FIX
                    // 3 wilds pay more than 4 of anything but lobstermania
                    // 4 wilds pay more than 5 of anything but lobstermania
                    // Take greatest win possible

                     // Leading 4 wilds
                     if ( line[1].Equals(Symbols.WS) && line[2].Equals(Symbols.WS) && line[3].Equals(Symbols.WS) && !line[4].Equals(Symbols.LM) && !line[4].Equals(Symbols.WS) )
                     {
                        sym = Symbols.WS;
                        count = 4;
                     }

                     // Leading 3 wilds
                     if ( line[1].Equals(Symbols.WS) && line[2].Equals(Symbols.WS) && !line[3].Equals(Symbols.LM) && !line[3].Equals(Symbols.WS) && !line[4].Equals(line[3]) && !line[4].Equals(Symbols.WS) )
                     {
                         sym = Symbols.WS;
                         count = 3;
                     }

                    break; // case "WS"

                default: // Handle all other 1st symbols not handled in cases above
                    sym = line[0];
                    for (int i = 1; i < NUM_REELS; i++)
                        if (line[i].Equals(sym) || line[i].Equals(Symbols.WS))
                            count++;
                        else
                            break;
                    break; // case default
            } // end switch

            if (sym.Equals(Symbols.WS) && count == 5)
                stats.numJackpots++;

            // count variable now set for number of consecutive line[0] symbols (1 based)
            count--; // adjust for zero based indexing

            if (bonusWin > 0) return bonusWin;

            int lineWin = PAYOUTS[count, GetSymIndex(sym)];

            return lineWin;

        } // End method GetLinePayout

        private int GetSymIndex(Symbols sym)
        {
            int symidx = 0;
            switch (sym)
            {
                case Symbols.WS:
                    symidx = 0;
                    break;
                case Symbols.LM:
                    symidx = 1;
                    break;
                case Symbols.BU:
                    symidx = 2;
                    break;
                case Symbols.BO:
                    symidx = 3;
                    break;
                case Symbols.LH:
                    symidx = 4;
                    break;
                case Symbols.TU:
                    symidx = 5;
                    break;
                case Symbols.CL:
                    symidx = 6;
                    break;
                case Symbols.SG:
                    symidx = 7;
                    break;
                case Symbols.SF:
                    symidx = 8;
                    break;
                case Symbols.LO:
                    symidx = 9;
                    break;
                case Symbols.LT:
                    symidx = 10;
                    break;
            } // end switch

            return symidx;
        } // End method GetSymIndex

        private void UpdateGameboard()
        {
            int[] lineIdxs = new int[5]; // Random starting slots for each reel
            for (int i = 0; i < NUM_REELS; i++)
                lineIdxs[i] = rand.Next(reels[i].Length);

            int i1, i2, i3;
            int r = 0; // reel number 1-5 zero adjusted

            foreach (int reelIdx in lineIdxs)
            {
                // set i1,i2, i3 to consecutive slot numbers on this reel, wrap if needed
                i1 = reelIdx;

                // set i2, correct if past last slot in reel
                if ((i1 + 1) == reels[r].Length)
                    i2 = 0;
                else
                    i2 = i1 + 1;

                // set i3, correct if past last slot in reel
                if ((i2 + 1) == reels[r].Length)
                    i3 = 0;
                else
                    i3 = i2 + 1;

                // i1, i2, i3 are now set to consecutive slot indexes on this reel (r)
                // Populate Random Gameboard
                gameBoard[0, r] = reels[r][i1];
                gameBoard[1, r] = reels[r][i2];
                gameBoard[2, r] = reels[r][i3];

                r++; // increment to next reel
            } // end foreach

        } // End method UpdateGameboard

        private void PrintGameboard()
        {
            Console.WriteLine("GAMEBOARD:");
            Console.WriteLine("------------------");
            for (int r = 0; r < GAMEBOARD_ROWS; r++)
            {
                for (int c = 0; c < NUM_REELS; c++)
                    Console.Write("{0}  ", gameBoard[r, c].ToString());
                Console.WriteLine();
            }
            Console.WriteLine();

        } // End method PrintGameboard


        private void UpdatePaylines()
        {
            payLines[0] = new Symbols[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[1, 4] };
            payLines[1] = new Symbols[] { gameBoard[0, 0], gameBoard[0, 1], gameBoard[0, 2], gameBoard[0, 3], gameBoard[0, 4] };
            payLines[2] = new Symbols[] { gameBoard[2, 0], gameBoard[2, 1], gameBoard[2, 2], gameBoard[2, 3], gameBoard[2, 4] };
            payLines[3] = new Symbols[] { gameBoard[0, 0], gameBoard[1, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[4] = new Symbols[] { gameBoard[2, 0], gameBoard[1, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[5] = new Symbols[] { gameBoard[2, 0], gameBoard[2, 1], gameBoard[1, 2], gameBoard[0, 3], gameBoard[0, 4] };
            payLines[6] = new Symbols[] { gameBoard[0, 0], gameBoard[0, 1], gameBoard[1, 2], gameBoard[2, 3], gameBoard[2, 4] };

            payLines[7] = new Symbols[] { gameBoard[1, 0], gameBoard[2, 1], gameBoard[1, 2], gameBoard[0, 3], gameBoard[1, 4] };
            payLines[8] = new Symbols[] { gameBoard[1, 0], gameBoard[0, 1], gameBoard[1, 2], gameBoard[2, 3], gameBoard[1, 4] };

            payLines[9] = new Symbols[] { gameBoard[2, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[10] = new Symbols[] { gameBoard[0, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[11] = new Symbols[] { gameBoard[1, 0], gameBoard[2, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[12] = new Symbols[] { gameBoard[1, 0], gameBoard[0, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[13] = new Symbols[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[14] = new Symbols[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

        } // End method UpdatePaylines

        private void PrintPayline(int payLineNum, Symbols[] line, int payout)
        { 
            if (payLineNum > 9) // for formatting purposes
                Console.Write("Payline[{0}]: [  ", payLineNum);
            else
                Console.Write("Payline[{0}]:  [  ", payLineNum);

            foreach (Symbols sym in line)
                    Console.Write("{0}  ", sym.ToString());

            Console.WriteLine("]  PAYS {0} credits.", payout);

        } // End method PrintPayline

        // Only count 1 scatter per column
        private int GetScatterWin() // in credits 
        {
            int count = 0;

            // Check each column (reel) in GameBoard for Scatters
            // Scatter wins only count 1 scatter per column
            for (int c = 0; c < NUM_REELS; c++)
            {
                for (int r = 0; r < GAMEBOARD_ROWS; r++)
                {
                    if (gameBoard[r, c].Equals(Symbols.LT))
                    {
                        count++;
                        break; // already 1 scatter in this column. Move on to next column
                    }
                }
            }

            int win = 0;
            switch (count)
            {
                case 1:
                case 2:
                    win = 0;
                    stats.scatterWinCredits += 0;
                    stats.paybackCredits += 0;
                    break;
                case 3:
                    win = 5;
                    stats.scatterWinCredits += 5;
                    stats.paybackCredits += 5;
                    break;
                case 4:
                    win = 25;
                    stats.scatterWinCredits += 25;
                    stats.paybackCredits += 25;
                    break;
                case 5:
                    win = 200;
                    stats.scatterWinCredits += 200;
                    stats.paybackCredits += 200;
                    break;
            } // end switch (count)

            if (count > 2)
                stats.scatterWinCount++;

            return win;

        } // end GetScatterWin()

    } // End class LM962
        
} // end namespace 
