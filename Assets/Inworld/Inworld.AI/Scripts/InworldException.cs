using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld
{
    public class InworldException : Exception
    {
        public InworldException(string errorMessage) : base(errorMessage) {}
    } 
}

