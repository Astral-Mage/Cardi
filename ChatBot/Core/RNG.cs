﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Core
{
    public static class RNG
    {
        public static Random Seed;

        static RNG()
        {
            Seed = new Random();
        }

    }
}
