using cAlgo.API;
using System;

namespace cAlgo.ChartObjectModels
{
    public class ChartVerticalLineModel : ChartObjectBaseModel
    {
        public DateTime Time { get; set; }

        public Color Color { get; set; }

        public int Thickness { get; set; }

        public LineStyle LineStyle { get; set; }
    }
}