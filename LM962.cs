using System;

namespace Lobstermania
{
    class LM962
    {
        // CONSTANTS
        private const int NUM_REELS = 5;
        private const int GAMEBOARD_ROWS = 3;
        private const int MAX_PAYLINES = 15;
        private const int DISTINCT_SYMBOLS = 11;

        private enum Symbol : byte { WS, LM, BU, BO, LH, TU, CL, SG, SF, LO, LT }; // All 11 game symbols

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

        public static readonly Random rand = new Random(); // Random Number Generator (RNG) object

        private readonly Symbol[][] _reels = new Symbol[][] // 5 _reels in this game
        {
            new Symbol[47], // 47 slots in this reel
            new Symbol[46],
            new Symbol[48],
            new Symbol[50],
            new Symbol[50]
        };

        private readonly Symbol[,] _gameBoard = new Symbol[GAMEBOARD_ROWS, NUM_REELS];
        private readonly Symbol[][] _payLines = new Symbol[MAX_PAYLINES][];
        private int _activePaylines = MAX_PAYLINES; //number of active paylines, default of 15

        // Print flags
        private bool _printReels = false;
        private bool _printGameboard = false;
        private bool _printPaylines = false;

        private Stats _stats = new Stats(); // Game statistics object

        //PROPERTIES
        public int ActivePaylines { get => _activePaylines; set => _activePaylines = value; }
        public bool PrintReels { get => _printReels; set => _printReels = value; }
        public bool PrintGameboard { get => _printGameboard; set => _printGameboard = value; }
        public bool PrintPaylines { get => _printPaylines; set => _printPaylines = value; }
        public Stats Stats { get => _stats; private set => _stats = value; }

        public LM962()
        {
            // Build the _reels
            BuildReels();

            // Randomize the _reels
            for (int i = 0; i < NUM_REELS; i++) // randomize each of the 5 _reels
                ArrayShuffle.Shuffle(_reels[i]);

        } // End constructor

        private void BuildReels()
        {
            // Build _reels 1,2,3
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

        private void PrintAllReels()
        {
            for (int num = 0; num < NUM_REELS; num++) // all 5 _reels
            {
                Console.Write("\nReel[{0}]: [  ", num);
                for (int s = 0; s < _reels[num].Length - 1; s++)
                    Console.Write("{0}, ", _reels[num][s].ToString());
                Console.WriteLine("{0} ]", _reels[num][_reels[num].Length - 2].ToString());
            }
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

        private int GetLinePayout(Symbol[] line)
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
                        bonusWin = Bonus.GetPrizes();
                        Stats.BonusWinCount++;
                        Stats.BonusWinCredits += bonusWin;
                        Stats.GameBonusWin += bonusWin;
                    }
                    else
                        bonusWin = 0;

                    break; // case Symbol.LO

                case Symbol.LT: // Scatter
                    count = 1; // Scatter handled at gameboard level
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
                lineIdxs[i] = rand.Next(_reels[i].Length);

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
            long win;
            long hits = 0L;
            long creditsSpent = 0L;
            Symbol[] payline = new Symbol[5];

            Console.Clear();
            Console.WriteLine("Running game metrics, (this might take a while) ...\n");
            foreach (Symbol s1 in _reels[0])
                foreach (Symbol s2 in _reels[1])
                    foreach (Symbol s3 in _reels[2])
                        foreach (Symbol s4 in _reels[3])
                            foreach (Symbol s5 in _reels[4])
                            {
                                creditsSpent++;
                                payline[0] = s1; payline[1] = s2; payline[2] = s3; payline[3] = s4; payline[4] = s5;
                                win = GetLinePayout(payline); // Scatter wins are not accounted for here

                                if (win > 0)
                                {
                                    hits++; // won't account for any scatter wins
                                    payBack += win;
                                }

                                // Get scatter wins on a payline
                                int count = 0;
                                foreach (Symbol s in payline)
                                    if (s.Equals(Symbol.LT))
                                        count++;
                                switch (count)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                        break;
                                    case 3:
                                        hits++;
                                        payBack += 5;
                                        break;
                                    case 4:
                                        hits++;
                                        payBack += 25;
                                        break;
                                    case 5:
                                        hits++;
                                        payBack += 200;
                                        break;
                                }
                            }
            Console.WriteLine("Spins : {0}   Payback% : {1:P1}   Hit Freq% : {2:P1}\n", creditsSpent,
                (double)payBack / (double)creditsSpent, (double)hits / (double)creditsSpent);

        } // End method TestMetrics

    } // End class LM962


} // end namespace 


