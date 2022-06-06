using cAlgo.API;
using System;

namespace cAlgo.ChartObjectModels
{
    public class ChartAndrewsPitchforkModel : ChartObjectBaseModel
    {
        public DateTime Time1 { get; set; }

        public DateTime Time2 { get; set; }

        public DateTime Time3 { get; set; }

        public double Y1 { get; set; }

        public double Y2 { get; set; }

        public double Y3 { get; set; }

        public int Thickness { get; set; }

        public LineStyle LineStyle { get; set; }

        public Color Color { get; set; }
    }
}