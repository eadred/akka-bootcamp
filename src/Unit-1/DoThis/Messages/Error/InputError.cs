﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail.Messages.Error
{
    public class InputError
    {
        public InputError(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}
