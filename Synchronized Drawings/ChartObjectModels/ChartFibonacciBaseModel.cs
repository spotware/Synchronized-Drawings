using cAlgo.API;
using System;

namespace cAlgo.ChartObjectModels
{
    public abstract class ChartFibonacciBaseModel : ChartObjectBaseModel
    {
        public FibonacciLevelModel[] FibonacciLevels { get; set; }

        public bool DisplayPrices { get; set; }

        public int Thickness { get; set; }

        public LineStyle LineStyle { get; set; }

        public Color Color { get; set; }
    }
}