using System;
using System.IO;
using System.Linq;

namespace Lobstermania
{
    public static class ArrayShuffle
    {
        public static void Shuffle(LM962.Symbol[] array)
        {
            // Fisher - Yates shuffle implementation

            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + LM962.Rand.Next(n - i);

                LM962.Symbol t = array[r];
                array[r] = array[i];
                array[i] = t;

            } // End current swap

        } // End method Shuffle

    } // End class ArrayShuffle

} // End namespace
