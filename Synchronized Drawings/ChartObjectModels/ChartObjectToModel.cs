using cAlgo.API;
using System.Linq;

namespace cAlgo.ChartObjectModels
{
    public static class ChartObjectToModel
    {
        public static IChartObjectModel GetObjectModel(this ChartObject chartObject, int areaIndex)
        {
            IChartObjectModel chartObjectModel;

            switch (chartObject.ObjectType)
            {
                case ChartObjectType.TrendLine:
                    var chartTrendLine = chartObject as ChartTrendLine;

                    chartObjectModel = new ChartTrendLineModel
                    {
                        Time1 = chartTrendLine.Time1,
                        Time2 = chartTrendLine.Time2,
                        Y1 = chartTrendLine.Y1,
                        Y2 = chartTrendLine.Y2,
                        ShowAngle = chartTrendLine.ShowAngle,
                        Color = chartTrendLine.Color,
                        ExtendToInfinity = chartTrendLine.ExtendToInfinity,
                        LineStyle = chartTrendLine.LineStyle,
                        Thickness = chartTrendLine.Thickness
                    };

                    break;

                case ChartObjectType.Text:
                    var chartText = chartObject as ChartText;

                    chartObjectModel = new ChartTextModel
                    {
                        Time = chartText.Time,
                        Y = chartText.Y,
                        Color = chartText.Color,
                        Text = chartText.Text,
                        HorizontalAlignment = chartText.HorizontalAlignment,
                        VerticalAlignment = chartText.VerticalAlignment,
                        FontSize = chartText.FontSize,
                        IsBold = chartText.IsBold,
                        IsItalic = chartText.IsItalic,
                        IsUnderlined = chartText.IsUnderlined
                    };

                    break;

                case ChartObjectType.Triangle:
                    var chartTriangle = chartObject as ChartTriangle;

                    chartObjectModel = new ChartTriangleModel
                    {
                        Time1 = chartTriangle.Time1,
                        Time2 = chartTriangle.Time2,
                        Time3 = chartTriangle.Time3,
                        Y1 = chartTriangle.Y1,
                        Y2 = chartTriangle.Y2,
                        Y3 = chartTriangle.Y3,
                    };

                    break;

                case ChartObjectType.Rectangle:
                    var chartRectangle = chartObject as ChartRectangle;

                    chartObjectModel = new ChartRectangleModel
                    {
                        Time1 = chartRectangle.Time1,
                        Time2 = chartRectangle.Time2,
                        Y1 = chartRectangle.Y1,
                        Y2 = chartRectangle.Y2,
                    };

                    break;

                case ChartObjectType.VerticalLine:
                    var chartVerticalLine = chartObject as ChartVerticalLine;

                    chartObjectModel = new ChartVerticalLineModel
                    {
                        Time = chartVerticalLine.Time,
                        Thickness = chartVerticalLine.Thickness,
                        Color = chartVerticalLine.Color,
                        LineStyle = chartVerticalLine.LineStyle
                    };

                    break;

                case ChartObjectType.HorizontalLine:
                    var chartHorizontalLine = chartObject as ChartHorizontalLine;

                    chartObjectModel = new ChartHorizontalLineModel
                    {
                        Y = chartHorizontalLine.Y,
                        Thickness = chartHorizontalLine.Thickness,
                        Color = chartHorizontalLine.Color,
                        LineStyle = chartHorizontalLine.LineStyle
                    };

                    break;

                case ChartObjectType.EquidistantChannel:
                    var chartEquidistantChannel = chartObject as ChartEquidistantChannel;

                    chartObjectModel = new ChartEquidistantChannelModel
                    {
                        Time1 = chartEquidistantChannel.Time1,
                        Time2 = chartEquidistantChannel.Time2,
                        Y1 = chartEquidistantChannel.Y1,
                        Y2 = chartEquidistantChannel.Y2,
                        ShowAngle = chartEquidistantChannel.ShowAngle,
                        ChannelHeight = chartEquidistantChannel.ChannelHeight,
                        ExtendToInfinity = chartEquidistantChannel.ExtendToInfinity,
                        LineStyle = chartEquidistantChannel.LineStyle,
                        Color = chartEquidistantChannel.Color,
                        Thickness = chartEquidistantChannel.Thickness
                    };

                    break;

                case ChartObjectType.AndrewsPitchfork:
                    var chartAndrewsPitchfork = chartObject as ChartAndrewsPitchfork;

                    chartObjectModel = new ChartAndrewsPitchforkModel
                    {
                        Time1 = chartAndrewsPitchfork.Time1,
                        Time2 = chartAndrewsPitchfork.Time2,
                        Time3 = chartAndrewsPitchfork.Time3,
                        Y1 = chartAndrewsPitchfork.Y1,
                        Y2 = chartAndrewsPitchfork.Y2,
                        Y3 = chartAndrewsPitchfork.Y3,
                        LineStyle = chartAndrewsPitchfork.LineStyle,
                        Color = chartAndrewsPitchfork.Color,
                        Thickness = chartAndrewsPitchfork.Thickness
                    };

                    break;

                case ChartObjectType.FibonacciRetracement:
                    var chartFibonacciRetracement = chartObject as ChartFibonacciRetracement;

                    chartObjectModel = new ChartFibonacciRetracementModel
                    {
                        Time1 = chartFibonacciRetracement.Time1,
                        Time2 = chartFibonacciRetracement.Time2,
                        Y1 = chartFibonacciRetracement.Y1,
                        Y2 = chartFibonacciRetracement.Y2
                    };

                    break;

                case ChartObjectType.FibonacciFan:
                    var chartFibonacciFan = chartObject as ChartFibonacciFan;

                    chartObjectModel = new ChartFibonacciFanModel
                    {
                        Time1 = chartFibonacciFan.Time1,
                        Time2 = chartFibonacciFan.Time2,
                        Y1 = chartFibonacciFan.Y1,
                        Y2 = chartFibonacciFan.Y2
                    };

                    break;

                case ChartObjectType.FibonacciExpansion:
                    var chartFibonacciExpansion = chartObject as ChartFibonacciExpansion;

                    chartObjectModel = new ChartFibonacciExpansionModel
                    {
                        Time1 = chartFibonacciExpansion.Time1,
                        Time2 = chartFibonacciExpansion.Time2,
                        Time3 = chartFibonacciExpansion.Time3,
                        Y1 = chartFibonacciExpansion.Y1,
                        Y2 = chartFibonacciExpansion.Y2,
                        Y3 = chartFibonacciExpansion.Y3
                    };

                    break;

                case ChartObjectType.Ellipse:
                    var chartEllipse = chartObject as ChartEllipse;

                    chartObjectModel = new ChartEllipseModel
                    {
                        Time1 = chartEllipse.Time1,
                        Time2 = chartEllipse.Time2,
                        Y1 = chartEllipse.Y1,
                        Y2 = chartEllipse.Y2,
                    };

                    break;

                case ChartObjectType.Icon:
                    var chartIcon = chartObject as ChartIcon;

                    chartObjectModel = new ChartIconModel
                    {
                        Time = chartIcon.Time,
                        Y = chartIcon.Y,
                        IconType = chartIcon.IconType,
                        Color = chartIcon.Color,
                    };

                    break;

                default:
                    return null;
            }

            if (chartObjectModel != null)
            {
                if (chartObject is ChartShape && chartObjectModel is ChartShapeModel)
                {
                    var chartShape = chartObject as ChartShape;
                    var chartShapeModel = chartObjectModel as ChartShapeModel;

                    chartShapeModel.IsFilled = chartShape.IsFilled;
                    chartShapeModel.LineStyle = chartShape.LineStyle;
                    chartShapeModel.Thickness = chartShape.Thickness;
                    chartShapeModel.Color = chartShape.Color;
                }

                if (chartObject is ChartFibonacciBase && chartObjectModel is ChartFibonacciBaseModel)
                {
                    var chartFibonacciBase = chartObject as ChartFibonacciBase;
                    var chartFibonacciBaseModel = chartObjectModel as ChartFibonacciBaseModel;

                    chartFibonacciBaseModel.DisplayPrices = chartFibonacciBase.DisplayPrices;
                    chartFibonacciBaseModel.FibonacciLevels = chartFibonacciBase.FibonacciLevels.Select(level => new FibonacciLevelModel
                    {
                        IsVisible = level.IsVisible,
                        PercentLevel = level.PercentLevel
                    }).ToArray();

                    chartFibonacciBaseModel.LineStyle = chartFibonacciBase.LineStyle;
                    chartFibonacciBaseModel.Color = chartFibonacciBase.Color;
                    chartFibonacciBaseModel.Thickness = chartFibonacciBase.Thickness;
                }

                chartObjectModel.Name = chartObject.Name;
                chartObjectModel.Comment = chartObject.Comment;
                chartObjectModel.IsInteractive = chartObject.IsInteractive;
                chartObjectModel.IsHidden = chartObject.IsHidden;
                chartObjectModel.IsLocked = chartObject.IsLocked;
                chartObjectModel.ZIndex = chartObject.ZIndex;
                chartObjectModel.ObjectType = chartObject.ObjectType;
                chartObjectModel.AreaIndex = areaIndex;
            }

            return chartObjectModel;
        }
    }
}