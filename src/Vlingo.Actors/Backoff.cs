﻿// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Actors
{
    public sealed class Backoff
    {
        private const long BACKOFF_CAP = 4096;
        private const long BACKOFF_RESET = 0L;
        private const long BACKOFF_START = 1L;

        private long backoff;
        private readonly bool isFixed;

        public Backoff()
        {
            backoff = BACKOFF_RESET;
            isFixed = false;
        }

        public Backoff(long fixedBackoff)
        {
            backoff = fixedBackoff;
            isFixed = true;
        }

        public void Now()
        {
            if (!isFixed)
            {
                if (backoff == BACKOFF_RESET)
                {
                    backoff = BACKOFF_START;
                }
                else if (backoff < BACKOFF_CAP)
                {
                    backoff = backoff * 2;
                }
            }
            YieldFor(backoff);
        }

        public void Reset()
        {
            backoff = BACKOFF_RESET;
        }

        private void YieldFor(long aMillis)
        {
            try
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(aMillis));
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
