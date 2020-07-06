using System;

namespace Lobstermania
{
    static class Bonus
    {
        // Constants
        private const int NUM_PRIZE_SLOTS = 322;
        private const int MAX_PRIZES = 12; // 4 buoys picked x 3 prizes per buoy
        
        // Fields
        private readonly static int[] PrizeLookupTable = new int[NUM_PRIZE_SLOTS];
        private static int BouysPicked = 0; // will be either 2, 3, or 4
        private static int PrizesPerBuoy = 0; // will be either 2 or 3
        public static int[] Prizes = new int[MAX_PRIZES]; // each individual prize amount, array will be right sized
        private static int BonusWin = 0;
        private static bool IsInitialized = false;

        // Properties

        // Methods
        public static void Display()
        {
            if (!IsInitialized)
                Initialize(); // Build the PrizeLookupTable

            Console.Write("\n           Bonus Prizes: [");
            for (int i = 0; i < MAX_PRIZES - 1; i++)
                Console.Write("{0}, ", Prizes[i]);
            Console.Write("{0}]", Prizes[MAX_PRIZES-1]);
        }
        private static void Initialize() 
        {
            // Set each PrizeLookupTable element to prize value in credits
            for (int i = 0; i <= 9; i++) PrizeLookupTable[i] = 10;
            for (int i = 10; i <= 14; i++) PrizeLookupTable[i] = 5;
            for (int i = 15; i <= 19; i++) PrizeLookupTable[i] = 6;
            for (int i = 20; i <= 24; i++) PrizeLookupTable[i] = 7;
            for (int i = 25; i <= 29; i++) PrizeLookupTable[i] = 8;
            for (int i = 30; i <= 39; i++) PrizeLookupTable[i] = 10;
            for (int i = 40; i <= 49; i++) PrizeLookupTable[i] = 12;
            for (int i = 50; i <= 59; i++) PrizeLookupTable[i] = 15;
            for (int i = 60; i <= 79; i++) PrizeLookupTable[i] = 20;
            for (int i = 80; i <= 99; i++) PrizeLookupTable[i] = 22;
            for (int i = 100; i <= 119; i++) PrizeLookupTable[i] = 25;
            for (int i = 120; i <= 139; i++) PrizeLookupTable[i] = 27;
            for (int i = 140; i <= 158; i++) PrizeLookupTable[i] = 30;
            for (int i = 159; i <= 180; i++) PrizeLookupTable[i] = 35;
            for (int i = 181; i <= 204; i++) PrizeLookupTable[i] = 45;
            for (int i = 205; i <= 223; i++) PrizeLookupTable[i] = 50;
            for (int i = 224; i <= 238; i++) PrizeLookupTable[i] = 55;
            for (int i = 239; i <= 253; i++) PrizeLookupTable[i] = 60;
            for (int i = 254; i <= 268; i++) PrizeLookupTable[i] = 65;
            for (int i = 269; i <= 283; i++) PrizeLookupTable[i] = 70;
            for (int i = 284; i <= 298; i++) PrizeLookupTable[i] = 75;
            for (int i = 299; i <= 308; i++) PrizeLookupTable[i] = 100;
            for (int i = 309; i <= 316; i++) PrizeLookupTable[i] = 150;
            for (int i = 317; i <= 321; i++) PrizeLookupTable[i] = 250;

            IsInitialized = true;
        }

        public static int GetPrizes()
        {
            if (!IsInitialized)
                Initialize(); // Build the PrizeLookupTable

            BouysPicked = LM962.rand.Next(2, 5); // 2, 3, or 4
            PrizesPerBuoy = LM962.rand.Next(2, 4); // 2 or 3
            int numPrizes = BouysPicked * PrizesPerBuoy;

            // Set all elements of the Prizes array to 0
            Array.Clear(Prizes, 0, MAX_PRIZES); 

            // Get each prize and save it to the Prizes array, add each prize to BonusWin
            BonusWin = 0;
            for(int i=0; i<numPrizes; i++)
            {
                int idx = LM962.rand.Next(322); // indexes between 0 and 321 inclusive
                Prizes[i] = PrizeLookupTable[idx];
                BonusWin += Prizes[i];
            }

            return BonusWin;

        } // End method GetPrizes

    } // End class Bonus

} // End namespace Lobstermania
