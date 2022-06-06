using cAlgo.API;
using System;

namespace cAlgo.ChartObjectModels
{
    public class ChartIconModel : ChartObjectBaseModel
    {
        public ChartIconType IconType { get; set; }

        public DateTime Time { get; set; }

        public double Y { get; set; }

        public Color Color { get; set; }
    }
}