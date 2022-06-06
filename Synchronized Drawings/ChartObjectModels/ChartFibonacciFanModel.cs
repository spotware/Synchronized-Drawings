using System;

namespace cAlgo.ChartObjectModels
{
    public class ChartFibonacciFanModel : ChartFibonacciBaseModel
    {
        public DateTime Time1 { get; set; }

        public DateTime Time2 { get; set; }

        public double Y1 { get; set; }

        public double Y2 { get; set; }
    }
}