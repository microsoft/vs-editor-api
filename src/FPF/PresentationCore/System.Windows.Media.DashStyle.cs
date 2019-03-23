using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media
{
    public class DashStyle
    {
        public double Offset { get; }
        public double [] Dashes { get; }

        public DashStyle (IEnumerable<double> dashes, Double offset)
        {
            Offset = offset;
            if (dashes is double [] arr)
                Dashes = arr;
            else
                Dashes = dashes.ToArray ();
        }
    }
}
