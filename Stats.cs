using System;

namespace Lobstermania
{

    public class Stats
    {
        long hitCount = 0; // Count of winning spins
        long spins = 0;
        double hitFreq = 0.0; // line hit frequency. 1.00 = 100%

        long paybackCredits = 0; // Line + Scatter winnings in credits (Line wins includes bonus wins)
        double paybackPercentage = 0.0; // Payback %, 1.00 = 100%

        int lineNum = 0;

        int scatterWinCount = 0;
        int scatterWinCredits = 0;

        int bonusWinCount = 0;
        int bonusWinCredits = 0;

    } // end clas Stats
} // end namespace
