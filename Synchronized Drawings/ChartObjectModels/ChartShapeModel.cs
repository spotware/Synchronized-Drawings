using cAlgo.API;

namespace cAlgo.ChartObjectModels
{
    public abstract class ChartShapeModel : ChartObjectBaseModel
    {
        public int Thickness { get; set; }

        public LineStyle LineStyle { get; set; }

        public Color Color { get; set; }

        public bool IsFilled { get; set; }
    }
}