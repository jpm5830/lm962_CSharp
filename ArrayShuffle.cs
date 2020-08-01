using System;
using System.IO;
using System.Linq;

namespace Lobstermania
{
    public static class ArrayShuffle
    {
        public static void Shuffle(LM962.Symbol[] array)
        {
            /// <summary>
            /// Shuffle the array.
            /// Fisher - Yates shuffle implementation
            /// </summary>
            /// <typeparam name="T">Array element type.</typeparam>
            /// <param name="array">Array to shuffle.</param>
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                // Use Next on random instance with an argument.
                // ... The argument is an exclusive bound.
                //     So we will not go past the end of the array.
                int r = i + LM962.Rand.Next(n - i);
                LM962.Symbol t = array[r];
                array[r] = array[i];
                array[i] = t;

                // Test code
                var q = array.GroupBy(x => x)
                            .Select(g => new { Value = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Value);
                
                bool err = false;
                foreach (var x in q)
                {
                    switch (x.Value)
                    {
                        case LM962.Symbol.WS:
                           if(x.Count != 2)
                                err = true;
                            break;
                        case LM962.Symbol.LM:
                            if (x.Count != 4)
                                err = true;
                            break;
                        case LM962.Symbol.BU:
                            if (x.Count != 4)
                                err = true;
                            break;
                        case LM962.Symbol.BO:
                            if (x.Count != 6)
                                err = true;
                            break;
                        case LM962.Symbol.LH:
                            if (x.Count != 5)
                                err = true;
                            break;
                        case LM962.Symbol.TU:
                            if (x.Count != 6)
                                err = true;
                            break;
                        case LM962.Symbol.CL:
                            if (x.Count != 6)
                                err = true;
                            break;
                        case LM962.Symbol.SG:
                            if (x.Count != 5)
                                err = true;
                            break;
                        case LM962.Symbol.SF:
                            if (x.Count != 5)
                                err = true;
                            break;
                        case LM962.Symbol.LO:
                            if (x.Count != 2)
                                err = true;
                            break;
                        case LM962.Symbol.LT:
                            if (x.Count != 2)
                                err = true;
                            break;
                    } // end switch

                } // End foreach

                if (err)
                {
                    Console.Write("ERROR: Swap: {0,-4}", i);
                    foreach (var x in q)
                        Console.Write("{0}:{1}  ", x.Value, x.Count);
                    Console.WriteLine();
                    Environment.Exit(0);
                }
            } // End current swap
            Console.WriteLine("After shuffle() => " + string.Join(", ", array));
        } // End method Shuffle

    } // End class ArrayShuffle

} // End namespace
