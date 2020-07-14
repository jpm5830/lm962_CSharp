using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace Lobstermania
{
    class LM962
    {
        const int NUM_REELS = 5;
        const int GAMEBOARD_ROWS = 3;
        const int MAX_PAYLINES = 15; 

        readonly string[] SYMBOLS123 = { "WS", "LM", "BU", "BO", "LH", "TU", "CL", "SG", "SF", "LO", "LT" }; // All 11 game symbols
        readonly string[] SYMBOLS45 =  { "WS", "LM", "BU", "BO", "LH", "TU", "CL", "SG", "SF", "LT" }; // Reels 4 and 5 -- missing LO (bonus) symbol
        readonly int[][] SYMBOL_COUNTS = new int[][]
        {
            new int[] { 2, 4, 4, 6, 5, 6, 6, 5, 5, 2, 2 },
            new int[] { 2, 4, 4, 4, 4, 4, 6, 6, 5, 5, 2 },
            new int[] { 1, 3, 5, 4, 6, 5, 5, 5, 6, 6, 2 },
            new int[] { 4, 4, 4, 4, 6, 6, 6, 6, 8, 2 },
            new int[] { 2, 4, 5, 4, 7, 7, 6, 6, 7, 2 }
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

        readonly string[][] reels = new string[][] // 5 reels in this game
        {
            new string[47], // 47 slots in this reel
            new string[46],
            new string[48],
            new string[50],
            new string[50]
        };

        readonly string[,] gameBoard = new string[GAMEBOARD_ROWS, NUM_REELS];
        readonly string[][] payLines = new string[MAX_PAYLINES][];
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
            for (int i = 0; i < 3; i++) // 1st 3 reels
            {
                int idx = 0; // reel slot index

                for (int j = 0; j < SYMBOLS123.Length; j++)
                    for (int k = 0; k < SYMBOL_COUNTS[i][j]; k++) // number of times to repeat each symbol
                        reels[i][idx++] = SYMBOLS123[j]; // assign symbol to slot
            }

            for (int i = 3; i < 5; i++) // last 2 reels (they don't have the LO (bonus) symbol)
            {
                int idx = 0; // reel slot index

                for (int j = 0; j < SYMBOLS45.Length; j++)
                    for (int k = 0; k < SYMBOL_COUNTS[i][j]; k++) // number of times to repeat each symbol
                        reels[i][idx++] = SYMBOLS45[j]; // assign symbol to slot
            }

        } // End method BuildReels

        private void PrintReels()
        {
            for(int num=0; num<NUM_REELS; num++) // all 5 reels
            {
                Console.Write("\nReel[{0}]: [  ", num);
                for (int s = 0; s < reels[num].Length - 1; s++)
                    Console.Write("{0}, ", reels[num][s]);
                Console.WriteLine("{0} ]", reels[num][reels[num].Length-2]);
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

        private int GetLinePayout(string[] line)
        {
            int count = 1; // count of consecutive matching symbols, left to right 
            string sym = line[0];
            int bonusWin = 0;

            switch (sym)
            {
                case "LO": // Bonus
                    for (int i = 1; i < 3; i++)
                    {
                        if (line[i] == "LO")
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

                    break; // case "LO"

                case "LT": // Scatter
                    count = 1; // Scatter handled at gameboard level
                    break; // case "LT"

                case "WS": // Wild
                    string altSym = "WS";

                    for (int i = 1; i < NUM_REELS; i++)
                        if ((line[i] == sym) || (line[i] == altSym))
                            count++;
                        else
                        {
                            if (line[i] != "LO" && line[i] != "LT" && altSym == "WS")
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
                    if ((line[1] == "WS") && (line[2] == "WS") && (line[3] == "WS"))
                    {
                        if (line[4] == "LM")
                        {
                            sym = "LM";
                            count = 5;
                        }
                        else
                        if (line[4] != "WS")
                        {
                            sym = "WS";
                            count = 4;
                        }
                    }

                    // Leading 3 wilds
                    if ((line[1] == "WS") && (line[2] == "WS") && (line[3] == "LM") && (line[4] == "WS") && (line[4] != "LM"))
                    {
                        sym = "LM";
                        count = 4;
                        goto Done;
                    }
                    if ((line[1] == "WS") && (line[2] == "WS") && (line[3] != "LM") && (line[3] != "WS") && (line[4] != line[3]))
                    {
                        sym = "WS";
                        count = 3;
                    }

                Done:
                    break; // case "WS"

                default: // Handle all other 1st symbols not handled in cases above
                    sym = line[0];
                    for (int i = 1; i < NUM_REELS; i++)
                        if ((line[i] == sym) || (line[i] == "WS"))
                            count++;
                        else
                            break;
                    break; // case default
            } // end switch

            if ((sym == "WS") && (count == 5))
                stats.numJackpots++;

            // count variable now set for number of consecutive line[0] symbols (1 based)
            count--; // adjust for zero based indexing

            if (bonusWin > 0) return bonusWin;

            int lineWin = PAYOUTS[count, GetSymIndex(sym)];

            return lineWin;

        } // End method GetLinePayout

        private int GetSymIndex(string sym)
        {
            int symidx = 0;
            switch (sym)
            {
                case "WS":
                    symidx = 0;
                    break;
                case "LM":
                    symidx = 1;
                    break;
                case "BU":
                    symidx = 2;
                    break;
                case "BO":
                    symidx = 3;
                    break;
                case "LH":
                    symidx = 4;
                    break;
                case "TU":
                    symidx = 5;
                    break;
                case "CL":
                    symidx = 6;
                    break;
                case "SG":
                    symidx = 7;
                    break;
                case "SF":
                    symidx = 8;
                    break;
                case "LO":
                    symidx = 9;
                    break;
                case "LT":
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
                    Console.Write("{0}  ", gameBoard[r, c]);
                Console.WriteLine();
            }
            Console.WriteLine();

        } // End method PrintGameboard


        private void UpdatePaylines()
        {
            payLines[0] = new string[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[1, 4] };
            payLines[1] = new string[] { gameBoard[0, 0], gameBoard[0, 1], gameBoard[0, 2], gameBoard[0, 3], gameBoard[0, 4] };
            payLines[2] = new string[] { gameBoard[2, 0], gameBoard[2, 1], gameBoard[2, 2], gameBoard[2, 3], gameBoard[2, 4] };
            payLines[3] = new string[] { gameBoard[0, 0], gameBoard[1, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[4] = new string[] { gameBoard[2, 0], gameBoard[1, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[5] = new string[] { gameBoard[2, 0], gameBoard[2, 1], gameBoard[1, 2], gameBoard[0, 3], gameBoard[0, 4] };
            payLines[6] = new string[] { gameBoard[0, 0], gameBoard[0, 1], gameBoard[1, 2], gameBoard[2, 3], gameBoard[2, 4] };

            payLines[7] = new string[] { gameBoard[1, 0], gameBoard[2, 1], gameBoard[1, 2], gameBoard[0, 3], gameBoard[1, 4] };
            payLines[8] = new string[] { gameBoard[1, 0], gameBoard[0, 1], gameBoard[1, 2], gameBoard[2, 3], gameBoard[1, 4] };

            payLines[9] = new string[] { gameBoard[2, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[10] = new string[] { gameBoard[0, 0], gameBoard[1, 1], gameBoard[1, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[11] = new string[] { gameBoard[1, 0], gameBoard[2, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[12] = new string[] { gameBoard[1, 0], gameBoard[0, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

            payLines[13] = new string[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[2, 2], gameBoard[1, 3], gameBoard[0, 4] };
            payLines[14] = new string[] { gameBoard[1, 0], gameBoard[1, 1], gameBoard[0, 2], gameBoard[1, 3], gameBoard[2, 4] };

        } // End method UpdatePaylines

        private void PrintPayline(int payLineNum, string[] line, int payout)
        { 
            if (payLineNum > 9) // for formatting purposes
                Console.Write("Payline[{0}]: [  ", payLineNum);
            else
                Console.Write("Payline[{0}]:  [  ", payLineNum);

            foreach (string sym in line)
                    Console.Write("{0}  ", sym);

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
                    if (gameBoard[r, c] == "LT")
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





        /*
    public void PaybackPercentage()
    {


        byte[] lineIdxs = new byte[5];

        hitCount = 0;
        hitFreq = 0.0; // line hit frequency. 1.00 = 100%

        // 259,440,000 combinations (47x46x48x50x50)
        for (byte s1Idx = 0; s1Idx < reel1.Length; s1Idx++) // reel 1
            for (byte s2Idx = 0; s2Idx < reel2.Length; s2Idx++) // reel 2
                for (byte s3Idx = 0; s3Idx < reel3.Length; s3Idx++) // reel 3
                    for(byte s4Idx = 0; s4Idx < reel4.Length; s4Idx++) // reel 4
                        for(byte s5Idx = 0; s5Idx  <reel5.Length; s5Idx++) // reel 5
                        {
                            paybackSpins++;
                            if ((paybackSpins % 25000000) == 0)
                                Console.WriteLine("-> {0:N0}", paybackSpins);

                            line[0] = reel1[s1Idx]; line[1] = reel2[s2Idx]; line[2] = reel3[s3Idx]; 
                            line[3] = reel4[s4Idx]; line[4] = reel5[s5Idx];
                            lineIdxs[0] = s1Idx;
                            lineIdxs[1] = s2Idx;
                            lineIdxs[2] = s3Idx;
                            lineIdxs[3] = s4Idx;
                            lineIdxs[4] = s5Idx;

                            int lineWin = GetLinePayout(line);
                            paybackCredits += lineWin;

                            // Scatter win check MUST appear after line win check.
                            // Line check win is set to 0 for all scatters in GetLinePayout()
                            // Scatter wins are a board level win, and the gameboard will change with each new line
                            // in this test.
                            int scatterCount = 0;
                            byte i1, i2, i3;
                            byte r = 0; // reel number 1-5 zero adjusted

                            foreach (byte reelIdx in lineIdxs)
                            {
                                // set i1,i2, i3 to consecutive slot numbers on this reel, wrap if needed
                                i1 = reelIdx;

                                // set i2, correct if past last slot in reel
                                if ((byte)(i1 + 1) == SYMBOLS_PER_REEL[r])
                                    i2 = 0;
                                else
                                    i2 = (byte)(i1 + 1);

                                // set i3, correct if past last slot in reel
                                if ((byte)(i2 + 1) == SYMBOLS_PER_REEL[r])
                                    i3 = 0;
                                else
                                    i3 = (byte)(i2 + 1);


                                // i1, i2, i3 are now set to consecutive slots on this reel (r)
                                // Count 1 scatter symbol per reel
                                switch (r)
                                {
                                    case 0:
                                        if (reel1[i1] == 'K' || reel1[i2] == 'K' || reel1[i3] == 'K') scatterCount++;
                                        break;
                                    case 1:
                                        if (reel2[i1] == 'K' || reel2[i2] == 'K' || reel2[i3] == 'K') scatterCount++;
                                        break;
                                    case 2:
                                        if (reel3[i1] == 'K' || reel3[i2] == 'K' || reel3[i3] == 'K') scatterCount++;
                                        break;
                                    case 3:
                                        if (reel4[i1] == 'K' || reel4[i2] == 'K' || reel4[i3] == 'K') scatterCount++;
                                        break;
                                    case 4:
                                        if (reel5[i1] == 'K' || reel5[i2] == 'K' || reel5[i3] == 'K') scatterCount++;
                                        break;
                                } // end switch (r)

                                r++; // next reel
                            } // end foreach (byte reelIdx in lineIdxs)

                            // scatter now contains count of scatters on gameboard
                            switch (scatterCount)
                            {
                                case 3:
                                    scatterWinCount++;
                                    scatterWinCredits += 5;
                                    paybackCredits += 5;
                                    break;
                                case 4:
                                    scatterWinCount++;
                                    scatterWinCredits += 25;
                                    paybackCredits += 25;
                                    break;
                                case 5:
                                    scatterWinCount++;
                                    scatterWinCredits += 200;
                                    paybackCredits += 200;
                                    break;
                            } // end switch

                            if (lineWin > 0) hitCount++;
                            if (scatterCount > 2) hitCount++;

                        } // End for s5Idx

        paybackPercentage = (double)paybackCredits / (double)paybackSpins; // total wins divided by credits spent (1 credit per line bet)
        hitFreq = (double)hitCount / (double)paybackSpins;


    } // End method PaybackPercentage

    public int GetLinePayout(char[] line)
    {
        byte count = 1; // count of consecutive matching symbols, left to right 
        char sym = line[0];
        int bonusWin = 0;

        switch (sym)
        {
            case 'J': // Bonus
                for (byte i = 1; i < 3; i++)
                {
                    if (line[i].Equals('J'))
                        count++;
                    else
                        break;
                }

                if (count == 3)
                {
                    bonusWin = Bonus.GetPrizes();
                    bonusWinCount++;
                    bonusWinSum += bonusWin;
                }
                else
                    bonusWin = 0;

                break; // case 'J'

            case 'K': // Scatter
                count = 1; // Scatter handled at gameboard level
                break; // case 'K'

            case 'A': // Wild
                char altSym = 'A';

                for (byte i = 1; i < 5; i++)
                    if (line[i].Equals(sym) || line[i].Equals(altSym))
                        count++;
                    else
                    {
                        if (!line[i].Equals('J') && !line[i].Equals('K') && altSym.Equals('A'))
                        {
                            altSym = line[i];
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }

                sym = altSym;

                // 3 wilds pay more than 4 of anything but lobstermania
                // 4 wilds pay more than 5 of anything but bobstermania
                // Take greatest win possible

                // Leading 4 wilds
                if (line[1].Equals('A') && line[2].Equals('A') && line[3].Equals('A') )
                {
                    if (line[4].Equals('B'))
                    {
                        sym = 'B';
                        count = 5;
                    }
                    else
                    if (!line[4].Equals('A'))
                    {
                        sym = 'A';
                        count = 4;
                    }
                }

                // Leading 3 wilds
                if (line[1].Equals('A') && line[2].Equals('A') && line[3].Equals('B') && !line[4].Equals('A') && !line[4].Equals('B'))
                {
                    sym = 'B';
                    count = 4;
                    goto Done;
                }
                if (line[1].Equals('A') && line[2].Equals('A') && !line[3].Equals('B') && !line[3].Equals('A') && !line[4].Equals(line[3]))
                {
                    sym = 'A';
                    count = 3;
                }

                Done:
                break; // case 'A'

            default:
                sym = line[0];
                for (byte i = 1; i < 5; i++)
                    if (line[i].Equals(sym) || line[i].Equals('A'))
                        count++;
                    else
                        break;
                break; // case default
        } // end switch


        // count variable now set for number of consecutive line[0] symbols (1 based)
        count--; // adjust for zero based indexing

        if (bonusWin > 0) return bonusWin;

        int lineWin = PAYOUTS[count, GetSymIndex(sym)];

        if(DEBUG_LINE_LEVEL)
#pragma warning disable CS0162 // Unreachable code detected
            PrintLine(line, sym, count, lineWin);
#pragma warning restore CS0162 // Unreachable code detected

        return lineWin;

    } // End method GetLinePayout

    private byte GetSymIndex(char sym)
    {
        byte symidx = 0;
        switch (sym)
        {
            case 'A':
                symidx = 0;
                break;
            case 'B':
                symidx = 1;
                break;
            case 'C':
                symidx = 2;
                break;
            case 'D':
                symidx = 3;
                break;
            case 'E':
                symidx = 4;
                break;
            case 'F':
                symidx = 5;
                break;
            case 'G':
                symidx = 6;
                break;
            case 'H':
                symidx = 7;
                break;
            case 'I':
                symidx = 8;
                break;
            case 'J':
                symidx = 9;
                break;
            case 'K':
                symidx = 10;
                break;
        } // end switch

        return symidx;
    } // End method GetSymIndex

    private void PrintLine(char[] line, char sym, byte count, int lineWin)
    {
        Console.Write("Line {0}: [ ", ++lineNum);
        for (int i = 0; i < 5; i++)
            Console.Write("{0}", line[i]);
        Console.WriteLine(" ]  Symbol = {0}, count = {1}, Payout = {2}\n", sym, count + 1, lineWin);

    } // End method PrintLine

    */

    } // End class LM962

}
