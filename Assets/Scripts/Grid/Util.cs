using System;
using System.Collections.Generic;

namespace Spatial
{
    public static class Util
    {
        // Transformaciones

        //Clamp básico
        public static int Clamp(int v, int min, int max) => v < min ? min : v > max ? max : v;

        //Generator genérico, lo vamos a ver más adelante.
        public static IEnumerable<Src> Generate<Src>(Src seed, Func<Src, Src> generator)
        {
            while (true)
            {
                yield return seed;
                seed = generator(seed);
            }
        }
    }
}