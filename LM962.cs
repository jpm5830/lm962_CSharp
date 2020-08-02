using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Lobstermania
{
    public class LM962
    {
        // CONSTANTS
        private const int NUM_REELS = 5;
        private const int GAMEBOARD_ROWS = 3;
        private const int MAX_PAYLINES = 15;
        private const int DISTINCT_SYMBOLS = 11;

        public enum Symbol { WS, LM, BU, BO, LH, TU, CL, SG, SF, LO, LT }; // All 11 game symbols

        // FIELDS
        private readonly int[][] _symbolCounts = new int[][]
        {
            new int[] { 2, 4, 4, 6, 5, 6, 6, 5, 5, 2, 2 },
            new int[] { 2, 4, 4, 4, 4, 4, 6, 6, 5, 5, 2 },
            new int[] { 1, 3, 5, 4, 6, 5, 5, 5, 6, 6, 2 },
            new int[] { 4, 4, 4, 4, 6, 6, 6, 6, 8, 0, 2 },
            new int[] { 2, 4, 5, 4, 7, 7, 6, 6, 7, 0, 2 }
        };

        private readonly int[,] _payouts =
        {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 100, 40, 25, 25, 10, 10, 5, 5, 5, 331, 5 },
            { 500, 200, 100, 100, 50, 50, 30, 30, 30, 0,25 },
            { 10000, 1000, 500, 500, 500, 250, 200, 200, 150, 0, 200 }
        };

        // The following 2 fields are set in the constructor
        private readonly int _seed;
        private static Random _rand; // Random Number Generator (RNG) object

        private readonly Symbol[][] _reels = new Symbol[NUM_REELS][] // 5 _reels in this game
        {
            new Symbol[47],
            new Symbol[46],
            new Symbol[48],
            new Symbol[50],
            new Symbol[50]
        };
        private readonly Symbol[,] _gameBoard = new Symbol[GAMEBOARD_ROWS, NUM_REELS];
        private readonly Symbol[][] _payLines = new Symbol[MAX_PAYLINES][];

        //PROPERTIES
        public int ActivePaylines { get; set; } = MAX_PAYLINES;
        public bool PrintReels { get; set; } = false;
        public bool PrintGameboard { get; set; } = false;
        public bool PrintPaylines { get; set; } = false;
        public Stats Stats { get; private set; } = new Stats();
        public double PaybackPercent { get; private set; } = 0.0;
        public double HitFreq { get; private set; } = 0.0;
        public bool UseRandomReelLayout { get; set; } = false;

        // METHODS
        public LM962()
        {

            // This will initialize all reels with the correct number of symbols,
            // and then randomize them on each reel (change each reel layout randomly)
            BuildReels();

            _seed = Environment.TickCount;
            _rand = new Random(_seed);

            // Randomize the _reels (1st use of Random()
            for (int i = 0; i < NUM_REELS; i++) // randomize each of the 5 _reels
                Shuffle(_reels[i]);

        } // End constructor

        private void BuildReels()
        {
            for (int i = 0; i < NUM_REELS; i++) // For each reel 
            {
                int idx = 0; // reel slot index
                for (int j = 0; j < DISTINCT_SYMBOLS; j++) // For each Symbol
                {
                    if ((j == 9) && (i > 2)) continue; // No bonus (LO) symbols on _reels 4 and 5
                    for (int k = 0; k < _symbolCounts[i][j]; k++) // number of times to repeat each symbol
                        _reels[i][idx++] = (Symbol)j; // assign symbol to slot
                }
            }

        } // End method BuildReels

        private void SetFixedReels()
        {
            // Spins: 259,440,000   Credits Won: 249,620,601   Hits: 13,418,964   Payback: 96.25%   Hit Frequency: 5.15%

            // The ordering of the symbols on the reel WILL affect the payback% and hit frequency %.
            // The symbol counts are according to the PAR sheet for each reel.
            // This is one of MANY reel set orderings that will give the payback and hit frequency above.

            _reels[0] = new Symbol[] { Symbol.SG, Symbol.TU, Symbol.LM, Symbol.SG, Symbol.BO, Symbol.LM, Symbol.LO, Symbol.BO, Symbol.TU, Symbol.TU, Symbol.LM, Symbol.BO, Symbol.SF, Symbol.BU, Symbol.BU, Symbol.LH, Symbol.TU, Symbol.SG, Symbol.CL, Symbol.TU, Symbol.BO, Symbol.WS, Symbol.TU, Symbol.LO, Symbol.CL, Symbol.LT, Symbol.LH, Symbol.SF, Symbol.SF, Symbol.WS, Symbol.SF, Symbol.CL, Symbol.LT, Symbol.BU, Symbol.CL, Symbol.SG, Symbol.BO, Symbol.SG, Symbol.LH, Symbol.LH, Symbol.CL, Symbol.LH, Symbol.SF, Symbol.BU, Symbol.LM, Symbol.BO, Symbol.CL };

            _reels[1] = new Symbol[] { Symbol.BO, Symbol.WS, Symbol.CL, Symbol.LH, Symbol.LT, Symbol.WS, Symbol.SF, Symbol.TU, Symbol.LH, Symbol.SF, Symbol.CL, Symbol.LM, Symbol.SG, Symbol.TU, Symbol.BU, Symbol.BO, Symbol.SG, Symbol.LO, Symbol.BO, Symbol.BU, Symbol.CL, Symbol.CL, Symbol.LO, Symbol.SG, Symbol.SF, Symbol.BU, Symbol.TU, Symbol.LT, Symbol.SG, Symbol.SF, Symbol.CL, Symbol.TU, Symbol.LO, Symbol.LO, Symbol.SG, Symbol.BO, Symbol.LM, Symbol.SG, Symbol.LH, Symbol.SF, Symbol.BU, Symbol.LM, Symbol.CL, Symbol.LM, Symbol.LH, Symbol.LO };

            _reels[2] = new Symbol[] { Symbol.LH, Symbol.BO, Symbol.CL, Symbol.LO, Symbol.SG, Symbol.LH, Symbol.LH, Symbol.CL, Symbol.WS, Symbol.SG, Symbol.BU, Symbol.BU, Symbol.TU, Symbol.SF, Symbol.LM, Symbol.BO, Symbol.LO, Symbol.TU, Symbol.SF, Symbol.LO, Symbol.LH, Symbol.BU, Symbol.LT, Symbol.SF, Symbol.SG, Symbol.SG, Symbol.CL, Symbol.TU, Symbol.BO, Symbol.LO, Symbol.LH, Symbol.BU, Symbol.SF, Symbol.LO, Symbol.CL, Symbol.CL, Symbol.BO, Symbol.LM, Symbol.TU, Symbol.SF, Symbol.LH, Symbol.LM, Symbol.BU, Symbol.TU, Symbol.LT, Symbol.SF, Symbol.SG, Symbol.LO };

            _reels[3] = new Symbol[] { Symbol.CL, Symbol.WS, Symbol.LT, Symbol.CL, Symbol.CL, Symbol.SF, Symbol.SG, Symbol.SF, Symbol.LM, Symbol.LH, Symbol.LT, Symbol.CL, Symbol.CL, Symbol.BO, Symbol.SF, Symbol.SF, Symbol.WS, Symbol.TU, Symbol.SG, Symbol.BO, Symbol.BU, Symbol.LH, Symbol.WS, Symbol.TU, Symbol.SF, Symbol.SG, Symbol.SG, Symbol.BU, Symbol.LM, Symbol.LM, Symbol.CL, Symbol.LH, Symbol.LH, Symbol.BO, Symbol.TU, Symbol.SF, Symbol.SF, Symbol.TU, Symbol.BU, Symbol.SF, Symbol.BU, Symbol.WS, Symbol.LM, Symbol.LH, Symbol.SG, Symbol.SG, Symbol.TU, Symbol.BO, Symbol.LH, Symbol.TU };

            _reels[4] = new Symbol[] { Symbol.LH, Symbol.BU, Symbol.SG, Symbol.SF, Symbol.CL, Symbol.LH, Symbol.TU, Symbol.BU, Symbol.LT, Symbol.SF, Symbol.BO, Symbol.WS, Symbol.LM, Symbol.TU, Symbol.LT, Symbol.LH, Symbol.SF, Symbol.LH, Symbol.LH, Symbol.CL, Symbol.BU, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.WS, Symbol.BU, Symbol.SF, Symbol.BU, Symbol.TU, Symbol.TU, Symbol.LH, Symbol.SG, Symbol.SG, Symbol.TU, Symbol.SG, Symbol.LH, Symbol.SG, Symbol.LM, Symbol.TU, Symbol.BO, Symbol.SF, Symbol.TU, Symbol.SG, Symbol.CL, Symbol.BO, Symbol.SF, Symbol.BO, Symbol.LM, Symbol.SF, Symbol.LM };

        } // End method SetFixedReels

        public static void Shuffle(Symbol[] array)
        {
            // Fisher - Yates shuffle implementation

            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + _rand.Next(n - i);

                Symbol t = array[r];
                array[r] = array[i];
                array[i] = t;
            }

        } // End method Shuffle


        private void PrintAllReels()
        {
            for (int num = 0; num < NUM_REELS; num++) // all 5 _reels
            {
                Console.Write("\nReel[{0}]: [  ", num);
                for (int s = 0; s < _reels[num].Length - 1; s++)
                    Console.Write("{0}, ", _reels[num][s]);
                Console.WriteLine("{0}  ];", (_reels[num][_reels[num].Length - 1]));
            }
            Console.WriteLine();
        } // End method PrintAllReels

        public void Spin()
        {
            Stats.Spins++;
            if (PrintReels)
                PrintAllReels();
            UpdateGameboard();
            if (PrintGameboard)
                PrintCurrentGameboard();
            UpdatePaylines();

            if (PrintPaylines)
            {
                Console.WriteLine("WINNING PAY LINES");
                Console.WriteLine("-----------------");
            }

            for (int i = 0; i < ActivePaylines; i++) // for each payline
            {
                int linePayout = GetLinePayout(_payLines[i]); // will include any bonus win
                if (linePayout > 0)
                {
                    Stats.GameWin += linePayout;
                    Stats.HitCount++;
                    Stats.PaybackCredits += linePayout;
                    if (PrintPaylines)
                        PrintPayline(i + 1, _payLines[i], linePayout);
                }
            } // end for (each payLine)

            int scatterWin = GetScatterWin();

            if (scatterWin > 0)
            {
                // _stats._paybackCredits are updated in GetScatterWin()
                Stats.HitCount++;
                Stats.GameScatterWin = scatterWin; // only 1 scatter win allowed per game
                Stats.GameWin += scatterWin; // add to total game win
            }

        } // End method Spin

        private int GetLinePayout(Symbol[] line, bool useFixedBonus = false)
        {
            int count = 1; // count of consecutive matching symbols, left to right 
            Symbol sym = line[0];
            int bonusWin = 0;

            switch (sym)
            {
                case Symbol.LO: // Bonus
                    for (int i = 1; i < 3; i++)
                    {
                        if (line[i] == Symbol.LO)
                            count++;
                        else
                            break;
                    }

                    if (count == 3)
                    {
                        if (useFixedBonus)
                            bonusWin = 331; // Average bonus win
                        else
                        {
                            // Use random bonus win
                            bonusWin = Bonus.GetPrizes(_rand);
                            Stats.BonusWinCount++;
                            Stats.BonusWinCredits += bonusWin;
                            Stats.GameBonusWin += bonusWin;
                        }
                    }
                    else
                        bonusWin = 0;

                    break; // case Symbol.LO

                case Symbol.LT: // Scatter
                    count = 1; // Scatter handled at gameboard level, no win here
                    break; // case "LT"

                case Symbol.WS: // Wild
                    Symbol altSym = Symbol.WS;

                    for (int i = 1; i < NUM_REELS; i++)
                        if ((line[i] == sym) || (line[i] == altSym))
                            count++;
                        else
                        {
                            if (!line[i].Equals(Symbol.LO) && !line[i].Equals(Symbol.LT) && altSym.Equals(Symbol.WS))
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
                    if (line[1].Equals(Symbol.WS) && line[2].Equals(Symbol.WS) && line[3].Equals(Symbol.WS) && !line[4].Equals(Symbol.LM) && !line[4].Equals(Symbol.WS))
                    {
                        sym = Symbol.WS;
                        count = 4;
                    }

                    // Leading 3 wilds
                    if (line[1].Equals(Symbol.WS) && line[2].Equals(Symbol.WS) && !line[3].Equals(Symbol.LM) && !line[3].Equals(Symbol.WS) && !line[4].Equals(line[3]) && !line[4].Equals(Symbol.WS))
                    {
                        sym = Symbol.WS;
                        count = 3;
                    }

                    break; // case "WS"

                default: // Handle all other 1st symbols not handled in cases above
                    sym = line[0];
                    for (int i = 1; i < NUM_REELS; i++)
                        if (line[i].Equals(sym) || line[i].Equals(Symbol.WS))
                            count++;
                        else
                            break;
                    break; // case default
            } // end switch

            if (sym.Equals(Symbol.WS) && count == 5)
                Stats.NumJackpots++;

            // count variable now set for number of consecutive line[0] symbols (1 based)
            count--; // adjust for zero based indexing

            if (bonusWin > 0) return bonusWin;

            int lineWin = _payouts[count, GetSymIndex(sym)];

            return lineWin;

        } // End method GetLinePayout

        private int GetSymIndex(Symbol sym)
        {
            int symidx = 0;
            switch (sym)
            {
                case Symbol.WS:
                    symidx = 0;
                    break;
                case Symbol.LM:
                    symidx = 1;
                    break;
                case Symbol.BU:
                    symidx = 2;
                    break;
                case Symbol.BO:
                    symidx = 3;
                    break;
                case Symbol.LH:
                    symidx = 4;
                    break;
                case Symbol.TU:
                    symidx = 5;
                    break;
                case Symbol.CL:
                    symidx = 6;
                    break;
                case Symbol.SG:
                    symidx = 7;
                    break;
                case Symbol.SF:
                    symidx = 8;
                    break;
                case Symbol.LO:
                    symidx = 9;
                    break;
                case Symbol.LT:
                    symidx = 10;
                    break;
            } // end switch

            return symidx;
        } // End method GetSymIndex

        private void UpdateGameboard()
        {
            int[] lineIdxs = new int[5]; // Random starting slots for each reel
            for (int i = 0; i < NUM_REELS; i++)
                lineIdxs[i] = _rand.Next(_reels[i].Length);

            int i1, i2, i3;
            int r = 0; // reel number 1-5 zero adjusted

            foreach (int reelIdx in lineIdxs)
            {
                // set i1,i2, i3 to consecutive slot numbers on this reel, wrap if needed
                i1 = reelIdx;

                // set i2, correct if past last slot in reel
                if ((i1 + 1) == _reels[r].Length)
                    i2 = 0;
                else
                    i2 = i1 + 1;

                // set i3, correct if past last slot in reel
                if ((i2 + 1) == _reels[r].Length)
                    i3 = 0;
                else
                    i3 = i2 + 1;

                // i1, i2, i3 are now set to consecutive slot indexes on this reel (r)
                // Populate Random Gameboard
                _gameBoard[0, r] = _reels[r][i1];
                _gameBoard[1, r] = _reels[r][i2];
                _gameBoard[2, r] = _reels[r][i3];

                r++; // increment to next reel
            } // end foreach

        } // End method UpdateGameboard

        private void PrintCurrentGameboard()
        {
            Console.WriteLine("GAMEBOARD:");
            Console.WriteLine("------------------");
            for (int r = 0; r < GAMEBOARD_ROWS; r++)
            {
                for (int c = 0; c < NUM_REELS; c++)
                    Console.Write("{0}  ", _gameBoard[r, c].ToString());
                Console.WriteLine();
            }
            Console.WriteLine();

        } // End method PrintCurrentGameboard


        private void UpdatePaylines()
        {
            _payLines[0] = new Symbol[] { _gameBoard[1, 0], _gameBoard[1, 1], _gameBoard[1, 2], _gameBoard[1, 3], _gameBoard[1, 4] };
            _payLines[1] = new Symbol[] { _gameBoard[0, 0], _gameBoard[0, 1], _gameBoard[0, 2], _gameBoard[0, 3], _gameBoard[0, 4] };
            _payLines[2] = new Symbol[] { _gameBoard[2, 0], _gameBoard[2, 1], _gameBoard[2, 2], _gameBoard[2, 3], _gameBoard[2, 4] };
            _payLines[3] = new Symbol[] { _gameBoard[0, 0], _gameBoard[1, 1], _gameBoard[2, 2], _gameBoard[1, 3], _gameBoard[0, 4] };
            _payLines[4] = new Symbol[] { _gameBoard[2, 0], _gameBoard[1, 1], _gameBoard[0, 2], _gameBoard[1, 3], _gameBoard[2, 4] };

            _payLines[5] = new Symbol[] { _gameBoard[2, 0], _gameBoard[2, 1], _gameBoard[1, 2], _gameBoard[0, 3], _gameBoard[0, 4] };
            _payLines[6] = new Symbol[] { _gameBoard[0, 0], _gameBoard[0, 1], _gameBoard[1, 2], _gameBoard[2, 3], _gameBoard[2, 4] };

            _payLines[7] = new Symbol[] { _gameBoard[1, 0], _gameBoard[2, 1], _gameBoard[1, 2], _gameBoard[0, 3], _gameBoard[1, 4] };
            _payLines[8] = new Symbol[] { _gameBoard[1, 0], _gameBoard[0, 1], _gameBoard[1, 2], _gameBoard[2, 3], _gameBoard[1, 4] };

            _payLines[9] = new Symbol[] { _gameBoard[2, 0], _gameBoard[1, 1], _gameBoard[1, 2], _gameBoard[1, 3], _gameBoard[0, 4] };
            _payLines[10] = new Symbol[] { _gameBoard[0, 0], _gameBoard[1, 1], _gameBoard[1, 2], _gameBoard[1, 3], _gameBoard[2, 4] };

            _payLines[11] = new Symbol[] { _gameBoard[1, 0], _gameBoard[2, 1], _gameBoard[2, 2], _gameBoard[1, 3], _gameBoard[0, 4] };
            _payLines[12] = new Symbol[] { _gameBoard[1, 0], _gameBoard[0, 1], _gameBoard[0, 2], _gameBoard[1, 3], _gameBoard[2, 4] };

            _payLines[13] = new Symbol[] { _gameBoard[1, 0], _gameBoard[1, 1], _gameBoard[2, 2], _gameBoard[1, 3], _gameBoard[0, 4] };
            _payLines[14] = new Symbol[] { _gameBoard[1, 0], _gameBoard[1, 1], _gameBoard[0, 2], _gameBoard[1, 3], _gameBoard[2, 4] };

        } // End method UpdatePaylines

        private void PrintPayline(int payLineNum, Symbol[] line, int payout)
        {
            if (payLineNum > 9) // for formatting purposes
                Console.Write("Payline[{0}]: [  ", payLineNum);
            else
                Console.Write("Payline[{0}]:  [  ", payLineNum);

            foreach (Symbol sym in line)
                Console.Write("{0}  ", sym.ToString());

            Console.WriteLine("]  PAYS {0} credits.", payout);

        } // End method PrintPayline

        private int GetScatterWin() // in credits 
        {
            // Only count 1 scatter per reel
            int count = 0;

            // Check each column (reel) in GameBoard for Scatters
            // Scatter wins only count 1 scatter per column
            for (int c = 0; c < NUM_REELS; c++)
            {
                for (int r = 0; r < GAMEBOARD_ROWS; r++)
                {
                    if (_gameBoard[r, c].Equals(Symbol.LT))
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
                    Stats.ScatterWinCredits += 0;
                    Stats.PaybackCredits += 0;
                    break;
                case 3:
                    win = 5;
                    Stats.ScatterWinCredits += 5;
                    Stats.PaybackCredits += 5;
                    break;
                case 4:
                    win = 25;
                    Stats.ScatterWinCredits += 25;
                    Stats.PaybackCredits += 25;
                    break;
                case 5:
                    win = 200;
                    Stats.ScatterWinCredits += 200;
                    Stats.PaybackCredits += 200;
                    break;
            } // end switch (count)

            if (count > 2)
                Stats.ScatterWinCount++;

            return win;

        } // end GetScatterWin()

        public void TestMetrics()
        {
            long payBack = 0L;
            long win = 0L;
            long hits = 0L; ;
            long creditsSpent = 0L;
            int scatterCount; // count of scatter on a gameboard or a payline
            Symbol[] payline = new Symbol[5];
            int[] slotIdxs = new int[5];
            float percentComplete = 0.0f;
            StreamWriter writer = null; // Used only if property PrintPaylines is true

            DateTime start_t = DateTime.Now;
            if (PrintPaylines)
                writer = new StreamWriter("paylines.txt");

            if (!UseRandomReelLayout)
                SetFixedReels();

            // Payback %
            // 259,440,000 combinations (47x46x48x50x50)
            Console.Write("Progress:   0%   ");

            for (int s1Idx = 0; s1Idx < _reels[0].Length; s1Idx++) // reel 1
                for (int s2Idx = 0; s2Idx < _reels[1].Length; s2Idx++) // reel 2
                    for (int s3Idx = 0; s3Idx < _reels[2].Length; s3Idx++) // reel 3
                        for (int s4Idx = 0; s4Idx < _reels[3].Length; s4Idx++) // reel 4
                            for (int s5Idx = 0; s5Idx < _reels[4].Length; s5Idx++) // reel 5
                            {
                                // payline[] holds Symbol elements
                                payline[0] = _reels[0][s1Idx];
                                payline[1] = _reels[1][s2Idx];
                                payline[2] = _reels[2][s3Idx];
                                payline[3] = _reels[3][s4Idx];
                                payline[4] = _reels[4][s5Idx];
                                // slotIdxs[] holds reel slots numbers for each reel (needed to check for scatter wins)
                                slotIdxs[0] = s1Idx;
                                slotIdxs[1] = s2Idx;
                                slotIdxs[2] = s3Idx;
                                slotIdxs[3] = s4Idx;
                                slotIdxs[4] = s5Idx;

                                creditsSpent++; // 1 spin costs 1 credit 
                                if ((creditsSpent % 25944000) == 0) // 1/10th of total spins
                                {
                                    percentComplete += 0.1f;
                                    Console.Write("{0:P0}   ", percentComplete);
                                    Console.Out.Flush();
                                }


                                // Scatter wins are a board level win, and the gameboard will change with each new line.
                                // Account for Scatter Wins at gameboard level
                                // The code below will check for scatter wins using a virtual gameboard
                                {
                                    scatterCount = 0;
                                    int i1, i2, i3;
                                    int r = 0; // reel number 1-5 zero adjusted

                                    foreach (byte slotIdx in slotIdxs)
                                    {
                                        // set i1,i2, i3 to consecutive slot numbers on this reel, wrap if needed
                                        i1 = slotIdx;

                                        // set i2, correct if past last slot in reel
                                        if ((i1 + 1) < _reels[r].Length)
                                            i2 = i1 + 1;
                                        else
                                            i2 = 0;

                                        // set i3, correct if past last slot in reel
                                        if ((i2 + 1) < _reels[r].Length)
                                            i3 = (i2 + 1);
                                        else
                                            i3 = 0;


                                        // i1, i2, i3 are now set to consecutive slots (reel indexes) on this reel (r)
                                        // Count 1 scatter symbol per reel
                                        switch (r)
                                        {
                                            case 0:
                                                if (_reels[0][i1].Equals(Symbol.LT) || _reels[0][i2].Equals(Symbol.LT) || _reels[0][i3].Equals(Symbol.LT)) scatterCount++;
                                                break;
                                            case 1:
                                                if (_reels[1][i1].Equals(Symbol.LT) || _reels[1][i2].Equals(Symbol.LT) || _reels[1][i3].Equals(Symbol.LT)) scatterCount++;
                                                break;
                                            case 2:
                                                if (_reels[2][i1].Equals(Symbol.LT) || _reels[2][i2].Equals(Symbol.LT) || _reels[2][i3].Equals(Symbol.LT)) scatterCount++;
                                                break;
                                            case 3:
                                                if (_reels[3][i1].Equals(Symbol.LT) || _reels[3][i2].Equals(Symbol.LT) || _reels[3][i3].Equals(Symbol.LT)) scatterCount++;
                                                break;
                                            case 4:
                                                if (_reels[4][i1].Equals(Symbol.LT) || _reels[4][i2].Equals(Symbol.LT) || _reels[4][i3].Equals(Symbol.LT)) scatterCount++;
                                                break;
                                        } // end switch (r)

                                        r++; // next reel
                                    } // end foreach (byte slotIdx in slotIdxs)

                                    // Add in scatter win payouts 
                                    switch (scatterCount)
                                    {
                                        case 0:
                                        case 1:
                                        case 2:
                                            win = 0;
                                            break;
                                        case 3:
                                            win = 5;
                                            break;
                                        case 4:
                                            win = 25;
                                            break;
                                        case 5:
                                            win = 200;
                                            break;
                                    }

                                    win += GetLinePayout(payline, true); // true use a fixed average bonus of 331

                                    // GetLinePayout() does not return anything for scatters 
                                    // Bonus wins are accounted for in GetLinePayout()
                                    if (win > 0)
                                    {
                                        hits++;
                                        payBack += win;

                                        if (PrintPaylines)
                                        {
                                            // Write each winning line (including scatters not on a line) to text file
                                            writer.Write("Payline[{0}] = [ ", hits);
                                            foreach (Symbol s in payline)
                                                writer.Write("{0}  ", s);
                                            writer.WriteLine(" ]  PAYOUT => {0}", win);
                                        }
                                    }

                                }
                            } // End s5Idx block
            if (PrintPaylines)
                writer.Close();

            PaybackPercent = (double)payBack / (double)creditsSpent;
            HitFreq = (double)hits / (double)creditsSpent;

            if (PrintReels)
                PrintAllReels();

            DateTime end_t = DateTime.Now;
            TimeSpan runtime = end_t - start_t;

            Console.WriteLine("\nPayback%: {0:P2}  Hit Freq: {1:P2}  Hits: {2:N0}  Credits Won: {3:N0},  Spins: {4:N0}  Runtime: {5}",
                PaybackPercent, HitFreq, hits, payBack, creditsSpent, runtime.ToString(@"hh\:mm\:ss"));

        } // End method TestMetrics

    } // End class LM962

} // end namespace 


