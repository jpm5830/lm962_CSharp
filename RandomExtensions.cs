using System;

namespace Lobstermania
{
        static class RandomExtensions
        {
            public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                //Console.Write("Before: {0} n = {1,2} | ", array[n], n.ToString().PadLeft(2, '0'));
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
                //Console.Write("After: {0} k = {1,2} \n", array[n], k.ToString().PadLeft(2, '0'));
            }

        } // End method Shuffle

        private enum Symbol { WS, LM, BU, BO, LH, TU, CL, SG, SF, LO, LT }; // All 11 game symbols    
        //static Symbol[] ary = new Symbol[] { Symbol.WS, Symbol.WS, Symbol.LM, Symbol.LM, Symbol.LM, Symbol.LM, Symbol.BU, Symbol.BU, Symbol.BU, Symbol.BU, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.LO, Symbol.LO, Symbol.LT, Symbol.LT };
        static Symbol[] ary = new Symbol[] { Symbol.WS, Symbol.WS, Symbol.LM, Symbol.LM, Symbol.LM, Symbol.LM, Symbol.BU, Symbol.BU, Symbol.BU, Symbol.BU, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.BO, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.LH, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.TU, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.CL, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SG, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.SF, Symbol.LO, Symbol.LO, Symbol.LT, Symbol.LT };
        public static void Main()
        {

            Random rnd = new Random(931375296);
            Console.Write("Before shuffle: ");
            foreach (Symbol i in ary)
                Console.Write("\"{0}\", ", i);

            rnd.Shuffle(ary);
            Console.Write("\n\nAfter shuffle: ");
            foreach (Symbol i in ary)
                Console.Write("\"{0}\", ", i);
            Console.WriteLine();
        }

    } // End class RandomExtensions

} // End namespace
