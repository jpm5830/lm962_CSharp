using System;

namespace Lobstermania
{
    static class Bonus
    {
        // CONSTANTS
        private const int NUM_PRIZE_SLOTS = 322;
        private const int MAX_PRIZES = 12; // 4 buoys picked x 3 prizes per buoy

        // FIELDS
        private readonly static int[] _prizeLookupTable = new int[NUM_PRIZE_SLOTS];
        private static int _bouysPicked = 0; // will be either 2, 3, or 4
        private static int _prizesPerBuoy = 0; // will be either 2 or 3
        private readonly static int[] _prizes = new int[MAX_PRIZES]; // each individual prize amount
        private static int _bonusWin = 0;
        private static bool _isInitialized = false;

        // METHODS
        private static void Initialize()
        {
            // Set each _prizeLookupTable element to prize value in credits
            for (int i = 0; i <= 9; i++) _prizeLookupTable[i] = 10;
            for (int i = 10; i <= 14; i++) _prizeLookupTable[i] = 5;
            for (int i = 15; i <= 19; i++) _prizeLookupTable[i] = 6;
            for (int i = 20; i <= 24; i++) _prizeLookupTable[i] = 7;
            for (int i = 25; i <= 29; i++) _prizeLookupTable[i] = 8;
            for (int i = 30; i <= 39; i++) _prizeLookupTable[i] = 10;
            for (int i = 40; i <= 49; i++) _prizeLookupTable[i] = 12;
            for (int i = 50; i <= 59; i++) _prizeLookupTable[i] = 15;
            for (int i = 60; i <= 79; i++) _prizeLookupTable[i] = 20;
            for (int i = 80; i <= 99; i++) _prizeLookupTable[i] = 22;
            for (int i = 100; i <= 119; i++) _prizeLookupTable[i] = 25;
            for (int i = 120; i <= 139; i++) _prizeLookupTable[i] = 27;
            for (int i = 140; i <= 158; i++) _prizeLookupTable[i] = 30;
            for (int i = 159; i <= 180; i++) _prizeLookupTable[i] = 35;
            for (int i = 181; i <= 204; i++) _prizeLookupTable[i] = 45;
            for (int i = 205; i <= 223; i++) _prizeLookupTable[i] = 50;
            for (int i = 224; i <= 238; i++) _prizeLookupTable[i] = 55;
            for (int i = 239; i <= 253; i++) _prizeLookupTable[i] = 60;
            for (int i = 254; i <= 268; i++) _prizeLookupTable[i] = 65;
            for (int i = 269; i <= 283; i++) _prizeLookupTable[i] = 70;
            for (int i = 284; i <= 298; i++) _prizeLookupTable[i] = 75;
            for (int i = 299; i <= 308; i++) _prizeLookupTable[i] = 100;
            for (int i = 309; i <= 316; i++) _prizeLookupTable[i] = 150;
            for (int i = 317; i <= 321; i++) _prizeLookupTable[i] = 250;

            _isInitialized = true;
        }

        public static int GetPrizes(Random rng) // Random Number Generator from LM962 object
        {
            if (!_isInitialized)
                Initialize(); // Build the _prizeLookupTable

            _bouysPicked = rng.Next(2, 5); // 2, 3, or 4
            _prizesPerBuoy = rng.Next(2, 4); // 2 or 3
            int numPrizes = _bouysPicked * _prizesPerBuoy;

            // Set all elements of the _prizes array to 0
            Array.Clear(_prizes, 0, MAX_PRIZES);

            // Get each prize and save it to the _prizes array, add each prize to _bonusWin
            _bonusWin = 0;
            for (int i = 0; i < numPrizes; i++)
            {
                int idx = rng.Next(322); // indexes between 0 and 321 inclusive
                _prizes[i] = _prizeLookupTable[idx];
                _bonusWin += _prizes[i];
            }

            return _bonusWin;

        } // End method GetPrizes

    } // End class Bonus

} // End namespace Lobstermania
