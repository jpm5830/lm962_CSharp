using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Tests
{
    class RNGTest
    {
        const bool DEBUG_LINE_LEVEL = false; // enable to see each line
        const int REELSIZE = 47;
        const int NUMTESTS = 47000000; // 47M
        const double EXPECTED_PROBABILITY = 1.0 / 47.0;
        readonly int[,] PAYOUTS =
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 100, 40, 25, 25, 10, 10, 5, 5, 5, 331, 5 },
            { 500, 200, 100, 100, 50, 50, 30, 30, 30, 0,25 },
            { 10000, 1000, 500, 500, 500, 250, 200, 200, 150, 0, 200 }
        };

        readonly byte[] SYMBOLS_PER_REEL = new byte[5] { 47, 46, 48, 50, 50 };

        public static readonly Random rand = new Random();
        readonly int[] slots = new int[REELSIZE];
        readonly double[] prob = new double[REELSIZE];
        double maxDifferenceFromExpected = 0;

        int hitCount = 0;
        int hitSpins = 0;
        double hitFreq = 0.0; // line hit frequency. 1.00 = 100%

        int paybackSpins = 0;
        int paybackCredits = 0; // Line + Scatter winnings in credits (Line wins includes bonus wins)
        double paybackPercentage = 0.0; // Payback %, 1.00 = 100%

        int lineNum = 0;

        int scatterWinCredits = 0;
        int scatterWinCount = 0;
        double scatterWinPercentage = 0.0; // Scatter wins as a % or all spins (paybackSpins),  % 1.00 = 100%

        int bonusWinCount = 0;
        int bonusWinSum = 0;


        public void RunRNGTest()
        {
            for (int i = 0; i < NUMTESTS; i++)
            {
                int r = rand.Next(REELSIZE); // get the random index into slots[]
                slots[r]++;
            }

            for (int i = 0; i < REELSIZE; i++)
            {
                // Perentage difference from expectehitCountd probability
                prob[i] = EXPECTED_PROBABILITY - (double)slots[i] / (double)NUMTESTS;
                if (Math.Abs(prob[i]) > maxDifferenceFromExpected)
                    maxDifferenceFromExpected = prob[i];
                Console.WriteLine("{0,-5}{1,-18:P15}", i, prob[i]);
            }

            Console.WriteLine("\nMax difference from expected = {0:P15}\n", maxDifferenceFromExpected);
        } // End method RunRNGTest

        public int GetGameboardReelCombos()
        {
            string symbols = "ABCDEFGHIJK";
            int combos = 0;

            foreach (char s1 in symbols)
                foreach (char s2 in symbols)
                    foreach (char s3 in symbols)
                    {
                        if (!((s1.Equals(s2) && s2.Equals(s3)) && (s1.Equals('A') || s1.Equals('B') || s1.Equals('C'))))
                            combos++;
                        else
                            Console.WriteLine("{0}, {1}, {2}", s1, s2, s3);
                    }
            return combos;
        }

        public void HitFreq()
        {
            string symbols123 = "ABCDEFGHIJK"; // 11 distinct symbols for reels 1 thru 3
            string symbols45 = "ABCDEFGHIK"; // 10 distinct symbols for reels 4 and 5 (missing J or Bonus symbol)

            char[] line = new char[5];
            hitSpins = 0;
            hitCount = 0;
            hitFreq = 0.0;

            foreach (char s1 in symbols123) // reel 1
                foreach (char s2 in symbols123) // reel 2
                    foreach (char s3 in symbols123) // reel 3
                        foreach (char s4 in symbols45) // reel 4
                            foreach (char s5 in symbols45) // reel 5
                            {
                                hitSpins++;
                                if (s1.Equals(s2) && s2.Equals(s3) && s1.Equals('J')) // Bonus win
                                {
                                    hitCount++;
                                    continue;
                                }
                                line[0] = s1; line[1] = s2; line[2] = s3; line[3] = s4; line[4] = s5;

                                byte count = 1; // count of consecutive matching symbols, left to right 

                                for (byte i = 1; i < 5; i++)
                                    // Only count Wilds going left to right
                                    if (line[i].Equals(line[0]) || line[i].Equals('A'))
                                        count++;
                                    else
                                        break;

                                // count variable now set for number of consecutive line[0] symbols (1 based)

                                if (count < 2) continue; // no win
                                if (count == 2 && (line[0].Equals('A') || line[0].Equals('B'))) { hitCount++; continue; } // WS2 or LM2
                                if (count > 2) hitCount++;
                            } // End foreach s5
            hitFreq = (double)hitCount / (double)hitSpins;

        } // End method HitFreq

        public void PaybackPercentage()
        {
            string symbols123 = "ABCDEFGHIJK"; // 11 distinct symbols for reels 1 thru 3
            string symbols45 = "ABCDEFGHIK"; // 10 distinct symbols for reels 4 and 5 (missing J or Bonus symbol)

            char[] line = new char[5];
            paybackSpins = 0;
            paybackCredits = 0;
            paybackPercentage = 0.0;
            char[] reel1 = new char[47];
            byte[] r1Counts = { 2, 4, 4, 6, 5, 6, 6, 5, 5, 2, 2 };
            char[] reel2 = new char[46];
            byte[] r2Counts = { 2, 4, 4, 4, 4, 4, 6, 6, 5, 5, 2 };
            char[] reel3 = new char[48];
            byte[] r3Counts = { 1, 3, 5, 4, 6, 5, 5, 5, 6, 6, 2 };
            char[] reel4 = new char[50];
            byte[] r4Counts = { 4, 4, 4, 4, 6, 6, 6, 6, 8, 2 };
            char[] reel5 = new char[50];
            byte[] r5Counts = { 2, 4, 5, 4, 7, 7, 6, 6, 7, 2 };

            // Build the reels
            int idx = 0;
            for (byte i = 0; i < 11; i++) // index into symbols123
                for (byte j = 0; j < r1Counts[i]; j++) // index into reel rxCounts
                    reel1[idx++] = symbols123[i];

            idx = 0;
            for (byte i = 0; i < 11; i++) // index into symbols123
                for (byte j = 0; j < r2Counts[i]; j++) // index into reel rxCounts
                    reel2[idx++] = symbols123[i];

            idx = 0;
            for (byte i = 0; i < 11; i++) // index into symbols123
                for (byte j = 0; j < r3Counts[i]; j++) // index into reel rxCounts
                    reel3[idx++] = symbols123[i];

            idx = 0;
            for (byte i = 0; i < 10; i++) // index into symbols45
                for (byte j = 0; j < r4Counts[i]; j++) // index into reel rxCounts
                    reel4[idx++] = symbols45[i];

            idx = 0;
            for (byte i = 0; i < 10; i++) // index into symbols45
                for (byte j = 0; j < r5Counts[i]; j++) // index into reel rxCounts
                    reel5[idx++] = symbols45[i];

            // Randomize the reels
            ArrayShuffle.Shuffle(reel1);
            ArrayShuffle.Shuffle(reel2);
            ArrayShuffle.Shuffle(reel3);
            ArrayShuffle.Shuffle(reel4);
            ArrayShuffle.Shuffle(reel5);

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

        public void TestPaybackPercentage()
        {
            string symbols123 = "ABCJK"; // 11 distinct symbols for reels 1 thru 3
            string symbols45 = "ABCK"; // 10 distinct symbols for reels 4 and 5 (missing J or Bonus symbol)

            char[] line = new char[5];
            paybackSpins = 0;
            paybackCredits = 0;
            paybackPercentage = 0.0;
            hitCount = 0;
            hitSpins = 0;
            hitFreq = 0.0; // line hit frequency. 1.00 = 100%
            
            // set in GetLinePayout()
            bonusWinCount = 0; 
            bonusWinSum = 0;

            // 2,000 combinations 

            foreach (char s1 in symbols123)
                foreach (char s2 in symbols123)
                    foreach (char s3 in symbols123)
                        foreach (char s4 in symbols45)
                            foreach (char s5 in symbols45)
                            {
                                line[0] = s1; line[1] = s2; line[2] = s3; line[3] = s4; line[4] = s5;
                                paybackCredits += GetLinePayout(line);
                                hitSpins++;
                                paybackSpins++;
                                if (paybackCredits > 0)
                                    hitCount++;
                            }


            paybackPercentage = (double)paybackCredits / (double)paybackSpins; // total wins divided by credits spent (1 credit per line bet)
            hitFreq = (double)hitCount / (double)hitSpins;


        } // End method TestPaybackPercentage
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
                PrintLine(line, sym, count, lineWin);
            
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



        public static void Main()
        {
            RNGTest rng = new RNGTest();
            //rng.RunRNGTest();
            DateTime start_t = DateTime.Now;

            rng.HitFreq();
            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).\n",
                rng.hitCount, rng.hitSpins, rng.hitFreq);
           
            rng.PaybackPercentage();
            
            Console.WriteLine("\nThere are {0:N0} winning line combinations out of {1:N0} spins ({2:P1} hit frequency).",
                rng.hitCount, rng.paybackSpins, rng.hitFreq);

            Console.WriteLine("\n{0:N0} credits spent, {1:N0} credits won, {2:P1} payback percentage.",
                rng.paybackSpins, rng.paybackCredits, rng.paybackPercentage);
            
            Console.WriteLine("\nThere were {0:N0} bonus wins out of {1:N0} spins. (Average win was {2:N0} credits per bonus.)",
                rng.bonusWinCount,rng.paybackSpins,(double)rng.bonusWinSum / (double)rng.bonusWinCount);
            
            Console.WriteLine("\nThere were {0:N0} scatter wins out of {1:N0} spins. (Average win was {2:N0} credits per scatter.)\n",
                rng.scatterWinCount, rng.paybackSpins, (double)rng.scatterWinCredits / (double)rng.scatterWinCount);

            Console.WriteLine("Average number of spins to bonus:   {0:N0}", (double)rng.paybackSpins / (double)rng.bonusWinCount);
            Console.WriteLine("Average number of spins to scatter: {0:N0}\n", (double)rng.paybackSpins / (double)rng.scatterWinCount);

            DateTime end_t = DateTime.Now;
            TimeSpan runtime = end_t - start_t;
            Console.WriteLine("Run completed in {0:t}\n", runtime);
        } // End method Main

    } // End class RNGTest

}
