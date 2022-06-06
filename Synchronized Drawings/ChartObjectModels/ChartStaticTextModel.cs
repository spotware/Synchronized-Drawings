using cAlgo.API;
using System;

namespace cAlgo.ChartObjectModels
{
    public class ChartStaticTextModel : ChartObjectBaseModel
    {
        public Color Color { get; set; }

        public string Text { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }
    }
}