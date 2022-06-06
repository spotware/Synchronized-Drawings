using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SynchronizedDrawings : Indicator
    {
        private static ConcurrentDictionary<string, IndicatorInstanceContainer<SynchronizedDrawings>> _indicatorInstances = new ConcurrentDictionary<string, IndicatorInstanceContainer<SynchronizedDrawings>>();

        private static int _numberOfChartsToDraw;

        private string _chartKey;

        [Parameter("Mode", DefaultValue = Mode.All, Group = "General")]
        public Mode Mode { get; set; }

        [Parameter("Y Axis Type", DefaultValue = YAxisType.Relative, Group = "General")]
        public YAxisType YAxisType { get; set; }

        [Parameter("Object Type", DefaultValue = ObjectType.Interactive, Group = "General")]
        public ObjectType ObjectType { get; set; }

        protected override void Initialize()
        {
            var chartObjectNamePrefix = string.Format("SynchronizedDrawings_{0}_{1}_{2}", SymbolName, TimeFrame, Chart.ChartType);
            _chartKey = string.Format("{0}_{1}", chartObjectNamePrefix, Server.Time.Ticks);

            foreach (var chartObject in Chart.Objects)
            {
                if (chartObject.Name.StartsWith(chartObjectNamePrefix, StringComparison.Ordinal))
                {
                    Chart.RemoveObject(chartObject.Name);
                }
            }

            _indicatorInstances.AddOrUpdate(_chartKey, new IndicatorInstanceContainer<SynchronizedDrawings>(this), (key, value) => new IndicatorInstanceContainer<SynchronizedDrawings>(this));

            Chart.ObjectsAdded += Chart_ObjectsAdded;
            Chart.ObjectsRemoved += Chart_ObjectsRemoved;
            Chart.ObjectsUpdated += Chart_ObjectsUpdated;
        }

        private void Chart_ObjectsUpdated(ChartObjectsUpdatedEventArgs obj)
        {
            var objects = obj.ChartObjects.Where(chartObject => IsObjectValid(chartObject)).ToArray();

            SyncObjects(objects, ChartObjectOperationType.Updated);
        }

        private void Chart_ObjectsRemoved(ChartObjectsRemovedEventArgs obj)
        {
            var objects = obj.ChartObjects.Where(chartObject => IsObjectValid(chartObject)).ToArray();

            SyncObjects(objects, ChartObjectOperationType.Removed);
        }

        private void Chart_ObjectsAdded(ChartObjectsAddedEventArgs obj)
        {
            var objects = obj.ChartObjects.Where(chartObject => IsObjectValid(chartObject)).ToArray();

            SyncObjects(objects, ChartObjectOperationType.Added);
        }

        private bool IsObjectValid(ChartObject chartObject)
        {
            switch (ObjectType)
            {
                case ObjectType.Interactive:
                    return chartObject.IsInteractive;

                case ObjectType.NonInteractive:
                    return !chartObject.IsInteractive;

                default:
                    return true;
            }
        }

        private void SyncObjects(ChartObject[] chartObjects, ChartObjectOperationType operationType)
        {
            if (_numberOfChartsToDraw > 0)
            {
                Interlocked.Decrement(ref _numberOfChartsToDraw);

                return;
            }

            var indicators = GetIndicators();
            var chartObjectsWithNames = chartObjects.ToDictionary(chartObject => string.IsNullOrWhiteSpace(chartObject.Name) ? string.Format("{0}_{1}", chartObject.GetHashCode(), _chartKey) : chartObject.Name);

            Interlocked.CompareExchange(ref _numberOfChartsToDraw, indicators.Count, _numberOfChartsToDraw);

            var chartInfo = new ChartInfo
            {
                TopY = Chart.TopY,
                BottomY = Chart.BottomY,
                SymbolName = Chart.SymbolName
            };

            foreach (var indicatorKeyValuePair in indicators)
            {
                try
                {
                    indicatorKeyValuePair.Value.BeginInvokeOnMainThread(() => indicatorKeyValuePair.Value.UpdateObject(chartObjectsWithNames, operationType, chartInfo));
                }
                catch (Exception)
                {
                    IndicatorInstanceContainer<SynchronizedDrawings> instanceContainer;

                    _indicatorInstances.TryRemove(indicatorKeyValuePair.Key, out instanceContainer);

                    Interlocked.Decrement(ref _numberOfChartsToDraw);
                }
            }
        }

        public void UpdateObject(Dictionary<string, ChartObject> chartObjectsWithNames, ChartObjectOperationType operationType, ChartInfo sourceChartInfo)
        {
            var currentChartObjects = Chart.Objects.ToArray();

            foreach (var chartObjectWithName in chartObjectsWithNames)
            {
                var currentObject = currentChartObjects.FirstOrDefault(chartObject => chartObject.Name.Equals(chartObjectWithName.Key, StringComparison.Ordinal));

                switch (operationType)
                {
                    case ChartObjectOperationType.Added:
                        if (currentObject != null)
                        {
                            UpdateObject(chartObjectWithName.Value, currentObject, sourceChartInfo);
                        }
                        else
                        {
                            AddObject(chartObjectWithName.Key, chartObjectWithName.Value, sourceChartInfo);
                        }

                        break;

                    case ChartObjectOperationType.Removed:
                        if (currentObject != null)
                        {
                            Chart.RemoveObject(currentObject.Name);
                        }

                        break;

                    case ChartObjectOperationType.Updated:
                        if (currentObject != null)
                        {
                            UpdateObject(chartObjectWithName.Value, currentObject, sourceChartInfo);
                        }
                        else
                        {
                            AddObject(chartObjectWithName.Key, chartObjectWithName.Value, sourceChartInfo);
                        }

                        break;
                }
            }
        }

        private void AddObject(string name, ChartObject chartObject, ChartInfo sourceChartInfo)
        {
            ChartObject result = null;

            switch (chartObject.ObjectType)
            {
                case ChartObjectType.AndrewsPitchfork:
                    var andrewsPitchfork = chartObject as ChartAndrewsPitchfork;

                    result = Chart.DrawAndrewsPitchfork(name, andrewsPitchfork.Time1, GetY(andrewsPitchfork.Y1, sourceChartInfo), andrewsPitchfork.Time2, GetY(andrewsPitchfork.Y2, sourceChartInfo),
                        andrewsPitchfork.Time3, GetY(andrewsPitchfork.Y3, sourceChartInfo), andrewsPitchfork.Color, andrewsPitchfork.Thickness, andrewsPitchfork.LineStyle);

                    break;

                case ChartObjectType.Ellipse:
                    var ellipse = chartObject as ChartEllipse;

                    result = Chart.DrawEllipse(name, ellipse.Time1, GetY(ellipse.Y1, sourceChartInfo), ellipse.Time2, GetY(ellipse.Y2, sourceChartInfo), ellipse.Color);

                    break;

                case ChartObjectType.EquidistantChannel:
                    var equidistantChannel = chartObject as ChartEquidistantChannel;

                    result = Chart.DrawEquidistantChannel(name, equidistantChannel.Time1, GetY(equidistantChannel.Y1, sourceChartInfo), equidistantChannel.Time2, GetY(equidistantChannel.Y2, sourceChartInfo),
                        GetY(equidistantChannel.ChannelHeight, sourceChartInfo), equidistantChannel.Color);

                    break;

                case ChartObjectType.FibonacciExpansion:
                    var fibonacciExpansion = chartObject as ChartFibonacciExpansion;

                    result = Chart.DrawFibonacciExpansion(name, fibonacciExpansion.Time1, GetY(fibonacciExpansion.Y1, sourceChartInfo), fibonacciExpansion.Time2, GetY(fibonacciExpansion.Y2, sourceChartInfo),
                        fibonacciExpansion.Time3, GetY(fibonacciExpansion.Y3, sourceChartInfo), fibonacciExpansion.Color);

                    break;

                case ChartObjectType.FibonacciFan:
                    var fibonacciFan = chartObject as ChartFibonacciFan;

                    result = Chart.DrawFibonacciFan(name, fibonacciFan.Time1, GetY(fibonacciFan.Y1, sourceChartInfo), fibonacciFan.Time2, GetY(fibonacciFan.Y2, sourceChartInfo), fibonacciFan.Color);

                    break;

                case ChartObjectType.FibonacciRetracement:
                    var fibonacciRetracement = chartObject as ChartFibonacciRetracement;

                    result = Chart.DrawFibonacciRetracement(name, fibonacciRetracement.Time1, GetY(fibonacciRetracement.Y1, sourceChartInfo), fibonacciRetracement.Time2, GetY(fibonacciRetracement.Y2, sourceChartInfo),
                        fibonacciRetracement.Color);

                    break;

                case ChartObjectType.HorizontalLine:
                    var horizontalLine = chartObject as ChartHorizontalLine;

                    result = Chart.DrawHorizontalLine(name, GetY(horizontalLine.Y, sourceChartInfo), horizontalLine.Color, horizontalLine.Thickness, horizontalLine.LineStyle);

                    break;

                case ChartObjectType.Icon:
                    var icon = chartObject as ChartIcon;

                    result = Chart.DrawIcon(name, icon.IconType, icon.Time, GetY(icon.Y, sourceChartInfo), icon.Color);

                    break;

                case ChartObjectType.Rectangle:
                    var rectangle = chartObject as ChartRectangle;

                    result = Chart.DrawRectangle(name, rectangle.Time1, GetY(rectangle.Y1, sourceChartInfo), rectangle.Time2, GetY(rectangle.Y2, sourceChartInfo), rectangle.Color);

                    break;

                case ChartObjectType.StaticText:
                    var staticText = chartObject as ChartStaticText;

                    result = Chart.DrawStaticText(name, staticText.Text, staticText.VerticalAlignment, staticText.HorizontalAlignment, staticText.Color);

                    break;

                case ChartObjectType.Text:
                    var text = chartObject as ChartText;

                    var currentChartText = Chart.DrawText(name, text.Text, text.Time, GetY(text.Y, sourceChartInfo), text.Color);

                    currentChartText.FontSize = text.FontSize;

                    result = currentChartText;

                    break;

                case ChartObjectType.TrendLine:
                    var trendLine = chartObject as ChartTrendLine;

                    var currentChartTrendLine = Chart.DrawTrendLine(name, trendLine.Time1, GetY(trendLine.Y1, sourceChartInfo), trendLine.Time2, GetY(trendLine.Y2, sourceChartInfo), trendLine.Color, trendLine.Thickness, trendLine.LineStyle);

                    currentChartTrendLine.ExtendToInfinity = trendLine.ExtendToInfinity;
                    currentChartTrendLine.ShowAngle = trendLine.ShowAngle;

                    result = currentChartTrendLine;

                    break;

                case ChartObjectType.Triangle:
                    var triangle = chartObject as ChartTriangle;

                    result = Chart.DrawTriangle(name, triangle.Time1, GetY(triangle.Y1, sourceChartInfo), triangle.Time2, GetY(triangle.Y2, sourceChartInfo), triangle.Time3, GetY(triangle.Y3, sourceChartInfo), triangle.Color);

                    break;

                case ChartObjectType.VerticalLine:
                    var verticalLine = chartObject as ChartVerticalLine;

                    result = Chart.DrawVerticalLine(name, verticalLine.Time, verticalLine.Color, verticalLine.Thickness, verticalLine.LineStyle);

                    break;
            }

            result.Comment = chartObject.Comment;
            result.IsLocked = chartObject.IsLocked;
            result.IsHidden = chartObject.IsHidden;

            if (chartObject.ObjectType != ChartObjectType.StaticText)
            {
                result.IsInteractive = chartObject.IsInteractive;
            }

            if (result is ChartShape)
            {
                var chartObjectShape = chartObject as ChartShape;
                var resultShape = result as ChartShape;

                resultShape.LineStyle = chartObjectShape.LineStyle;
                resultShape.Thickness = chartObjectShape.Thickness;
                resultShape.IsFilled = chartObjectShape.IsFilled;
            }

            if (result is ChartFibonacciBase)
            {
                var chartObjectFibonacciBase = chartObject as ChartFibonacciBase;
                var resultFibonacciBase = result as ChartFibonacciBase;

                resultFibonacciBase.DisplayPrices = chartObjectFibonacciBase.DisplayPrices;
            }
        }

        private void UpdateObject(ChartObject otherChartObject, ChartObject currentChartObject, ChartInfo sourceChartInfo)
        {
            switch (otherChartObject.ObjectType)
            {
                case ChartObjectType.AndrewsPitchfork:
                    var otherChartAndrewsPitchfork = otherChartObject as ChartAndrewsPitchfork;
                    var currentChartAndrewsPitchfork = currentChartObject as ChartAndrewsPitchfork;

                    currentChartAndrewsPitchfork.Time1 = otherChartAndrewsPitchfork.Time1;
                    currentChartAndrewsPitchfork.Time2 = otherChartAndrewsPitchfork.Time2;
                    currentChartAndrewsPitchfork.Time3 = otherChartAndrewsPitchfork.Time3;

                    currentChartAndrewsPitchfork.Y1 = GetY(otherChartAndrewsPitchfork.Y1, sourceChartInfo);
                    currentChartAndrewsPitchfork.Y2 = GetY(otherChartAndrewsPitchfork.Y2, sourceChartInfo);
                    currentChartAndrewsPitchfork.Y3 = GetY(otherChartAndrewsPitchfork.Y3, sourceChartInfo);

                    currentChartAndrewsPitchfork.Color = otherChartAndrewsPitchfork.Color;
                    currentChartAndrewsPitchfork.LineStyle = otherChartAndrewsPitchfork.LineStyle;
                    currentChartAndrewsPitchfork.Thickness = otherChartAndrewsPitchfork.Thickness;

                    break;

                case ChartObjectType.Ellipse:
                    var otherChartEllipse = otherChartObject as ChartEllipse;
                    var currentChartEllipse = currentChartObject as ChartEllipse;

                    currentChartEllipse.Time1 = otherChartEllipse.Time1;
                    currentChartEllipse.Time2 = otherChartEllipse.Time2;

                    currentChartEllipse.Y1 = GetY(otherChartEllipse.Y1, sourceChartInfo);
                    currentChartEllipse.Y2 = GetY(otherChartEllipse.Y2, sourceChartInfo);

                    currentChartEllipse.Color = otherChartEllipse.Color;

                    break;

                case ChartObjectType.EquidistantChannel:
                    var otherChartEquidistantChanne = otherChartObject as ChartEquidistantChannel;
                    var currentChartEquidistantChanne = currentChartObject as ChartEquidistantChannel;

                    currentChartEquidistantChanne.Time1 = otherChartEquidistantChanne.Time1;
                    currentChartEquidistantChanne.Time2 = otherChartEquidistantChanne.Time2;

                    currentChartEquidistantChanne.Y1 = GetY(otherChartEquidistantChanne.Y1, sourceChartInfo);
                    currentChartEquidistantChanne.Y2 = GetY(otherChartEquidistantChanne.Y2, sourceChartInfo);

                    currentChartEquidistantChanne.Color = otherChartEquidistantChanne.Color;

                    currentChartEquidistantChanne.ChannelHeight = otherChartEquidistantChanne.ChannelHeight;

                    break;

                case ChartObjectType.FibonacciExpansion:
                    var otherChartFibonacciExpansion = otherChartObject as ChartFibonacciExpansion;
                    var currentChartFibonacciExpansion = currentChartObject as ChartFibonacciExpansion;

                    currentChartFibonacciExpansion.Time1 = otherChartFibonacciExpansion.Time1;
                    currentChartFibonacciExpansion.Time2 = otherChartFibonacciExpansion.Time2;
                    currentChartFibonacciExpansion.Time3 = otherChartFibonacciExpansion.Time3;

                    currentChartFibonacciExpansion.Y1 = GetY(otherChartFibonacciExpansion.Y1, sourceChartInfo);
                    currentChartFibonacciExpansion.Y2 = GetY(otherChartFibonacciExpansion.Y2, sourceChartInfo);
                    currentChartFibonacciExpansion.Y3 = GetY(otherChartFibonacciExpansion.Y3, sourceChartInfo);

                    currentChartFibonacciExpansion.Color = otherChartFibonacciExpansion.Color;
                    currentChartFibonacciExpansion.LineStyle = otherChartFibonacciExpansion.LineStyle;
                    currentChartFibonacciExpansion.Thickness = otherChartFibonacciExpansion.Thickness;

                    currentChartFibonacciExpansion.DisplayPrices = otherChartFibonacciExpansion.DisplayPrices;

                    break;

                case ChartObjectType.FibonacciFan:
                    var otherChartFibonacciFan = otherChartObject as ChartFibonacciFan;
                    var currentChartFibonacciFan = currentChartObject as ChartFibonacciFan;

                    currentChartFibonacciFan.Time1 = otherChartFibonacciFan.Time1;
                    currentChartFibonacciFan.Time2 = otherChartFibonacciFan.Time2;

                    currentChartFibonacciFan.Y1 = GetY(otherChartFibonacciFan.Y1, sourceChartInfo);
                    currentChartFibonacciFan.Y2 = GetY(otherChartFibonacciFan.Y2, sourceChartInfo);

                    currentChartFibonacciFan.Color = otherChartFibonacciFan.Color;
                    currentChartFibonacciFan.LineStyle = otherChartFibonacciFan.LineStyle;
                    currentChartFibonacciFan.Thickness = otherChartFibonacciFan.Thickness;

                    currentChartFibonacciFan.DisplayPrices = otherChartFibonacciFan.DisplayPrices;

                    break;

                case ChartObjectType.FibonacciRetracement:
                    var otherChartFibonacciRetracement = otherChartObject as ChartFibonacciRetracement;
                    var currentChartFibonacciRetracement = currentChartObject as ChartFibonacciRetracement;

                    currentChartFibonacciRetracement.Time1 = otherChartFibonacciRetracement.Time1;
                    currentChartFibonacciRetracement.Time2 = otherChartFibonacciRetracement.Time2;

                    currentChartFibonacciRetracement.Y1 = GetY(otherChartFibonacciRetracement.Y1, sourceChartInfo);
                    currentChartFibonacciRetracement.Y2 = GetY(otherChartFibonacciRetracement.Y2, sourceChartInfo);

                    currentChartFibonacciRetracement.Color = otherChartFibonacciRetracement.Color;
                    currentChartFibonacciRetracement.LineStyle = otherChartFibonacciRetracement.LineStyle;
                    currentChartFibonacciRetracement.Thickness = otherChartFibonacciRetracement.Thickness;

                    currentChartFibonacciRetracement.DisplayPrices = otherChartFibonacciRetracement.DisplayPrices;

                    break;

                case ChartObjectType.HorizontalLine:
                    var otherChartHorizontalLine = otherChartObject as ChartHorizontalLine;
                    var currentChartHorizontalLine = currentChartObject as ChartHorizontalLine;

                    currentChartHorizontalLine.Y = GetY(otherChartHorizontalLine.Y, sourceChartInfo);
                    currentChartHorizontalLine.Color = otherChartHorizontalLine.Color;
                    currentChartHorizontalLine.LineStyle = otherChartHorizontalLine.LineStyle;
                    currentChartHorizontalLine.Thickness = otherChartHorizontalLine.Thickness;

                    break;

                case ChartObjectType.Icon:
                    var otherChartIcon = otherChartObject as ChartIcon;
                    var currentChartIcon = currentChartObject as ChartIcon;

                    currentChartIcon.IconType = otherChartIcon.IconType;
                    currentChartIcon.Time = otherChartIcon.Time;
                    currentChartIcon.Y = GetY(otherChartIcon.Y, sourceChartInfo);
                    currentChartIcon.Color = otherChartIcon.Color;

                    break;

                case ChartObjectType.Rectangle:
                    var otherChartRectangle = otherChartObject as ChartRectangle;
                    var currentChartRectangle = currentChartObject as ChartRectangle;

                    currentChartRectangle.Time1 = otherChartRectangle.Time1;
                    currentChartRectangle.Time2 = otherChartRectangle.Time2;

                    currentChartRectangle.Y1 = GetY(otherChartRectangle.Y1, sourceChartInfo);
                    currentChartRectangle.Y2 = GetY(otherChartRectangle.Y2, sourceChartInfo);

                    currentChartRectangle.Color = otherChartRectangle.Color;
                    currentChartRectangle.LineStyle = otherChartRectangle.LineStyle;
                    currentChartRectangle.Thickness = otherChartRectangle.Thickness;

                    break;

                case ChartObjectType.StaticText:
                    var otherChartStaticText = otherChartObject as ChartStaticText;
                    var currentChartStaticText = currentChartObject as ChartStaticText;

                    currentChartStaticText.Text = otherChartStaticText.Text;
                    currentChartStaticText.VerticalAlignment = otherChartStaticText.VerticalAlignment;
                    currentChartStaticText.HorizontalAlignment = otherChartStaticText.HorizontalAlignment;
                    currentChartStaticText.Color = otherChartStaticText.Color;

                    break;

                case ChartObjectType.Text:
                    var otherChartText = otherChartObject as ChartText;
                    var currentChartText = currentChartObject as ChartText;

                    currentChartText.Text = otherChartText.Text;
                    currentChartText.Time = otherChartText.Time;
                    currentChartText.Y = GetY(otherChartText.Y, sourceChartInfo);
                    currentChartText.Color = otherChartText.Color;
                    currentChartText.FontSize = otherChartText.FontSize;

                    break;

                case ChartObjectType.TrendLine:
                    var otherChartTrendLine = otherChartObject as ChartTrendLine;
                    var currentChartTrendLine = currentChartObject as ChartTrendLine;

                    currentChartTrendLine.Time1 = otherChartTrendLine.Time1;
                    currentChartTrendLine.Time2 = otherChartTrendLine.Time2;

                    currentChartTrendLine.Y1 = GetY(otherChartTrendLine.Y1, sourceChartInfo);
                    currentChartTrendLine.Y2 = GetY(otherChartTrendLine.Y2, sourceChartInfo);

                    currentChartTrendLine.Color = otherChartTrendLine.Color;
                    currentChartTrendLine.ExtendToInfinity = otherChartTrendLine.ExtendToInfinity;

                    currentChartTrendLine.Thickness = otherChartTrendLine.Thickness;
                    currentChartTrendLine.LineStyle = otherChartTrendLine.LineStyle;
                    currentChartTrendLine.ShowAngle = otherChartTrendLine.ShowAngle;

                    break;

                case ChartObjectType.Triangle:
                    var otherChartTriangle = otherChartObject as ChartTriangle;
                    var currentChartTriangle = currentChartObject as ChartTriangle;

                    currentChartTriangle.Time1 = otherChartTriangle.Time1;
                    currentChartTriangle.Time2 = otherChartTriangle.Time2;
                    currentChartTriangle.Time3 = otherChartTriangle.Time3;

                    currentChartTriangle.Y1 = GetY(otherChartTriangle.Y1, sourceChartInfo);
                    currentChartTriangle.Y2 = GetY(otherChartTriangle.Y2, sourceChartInfo);
                    currentChartTriangle.Y3 = GetY(otherChartTriangle.Y3, sourceChartInfo);

                    currentChartTriangle.Color = otherChartTriangle.Color;
                    currentChartTriangle.LineStyle = otherChartTriangle.LineStyle;
                    currentChartTriangle.Thickness = otherChartTriangle.Thickness;

                    break;

                case ChartObjectType.VerticalLine:
                    var otherChartVerticalLine = otherChartObject as ChartVerticalLine;
                    var currentChartVerticalLine = currentChartObject as ChartVerticalLine;

                    currentChartVerticalLine.Time = otherChartVerticalLine.Time;
                    currentChartVerticalLine.Color = otherChartVerticalLine.Color;
                    currentChartVerticalLine.LineStyle = otherChartVerticalLine.LineStyle;
                    currentChartVerticalLine.Thickness = otherChartVerticalLine.Thickness;

                    break;
            }

            currentChartObject.Comment = otherChartObject.Comment;
            currentChartObject.IsLocked = otherChartObject.IsLocked;
            currentChartObject.IsHidden = otherChartObject.IsHidden;

            if (currentChartObject.ObjectType != ChartObjectType.StaticText)
            {
                currentChartObject.IsInteractive = otherChartObject.IsInteractive;
            }

            if (currentChartObject is ChartShape)
            {
                var otherShape = otherChartObject as ChartShape;
                var currentShape = currentChartObject as ChartShape;

                currentShape.LineStyle = otherShape.LineStyle;
                currentShape.Thickness = otherShape.Thickness;
                currentShape.IsFilled = otherShape.IsFilled;
            }
        }

        private double GetY(double absoluteY, ChartInfo sourceChartInfo)
        {
            if (YAxisType == YAxisType.Absolute || sourceChartInfo.SymbolName.Equals(SymbolName, StringComparison.Ordinal))
            {
                return absoluteY;
            }

            var topToBottomDiff = sourceChartInfo.TopY - sourceChartInfo.BottomY;
            var diff = absoluteY - sourceChartInfo.BottomY;
            var percent = diff / topToBottomDiff;

            var chartTopToBottomDiff = Chart.TopY - Chart.BottomY;

            return Chart.BottomY + (chartTopToBottomDiff * percent);
        }

        public override void Calculate(int index)
        {
        }

        private List<KeyValuePair<string, SynchronizedDrawings>> GetIndicators()
        {
            Func<SynchronizedDrawings, bool> predicate;

            switch (Mode)
            {
                case Mode.Symbol:
                    predicate = indicator => indicator.SymbolName.Equals(SymbolName, StringComparison.Ordinal);
                    break;

                case Mode.TimeFrame:
                    predicate = indicator => indicator.TimeFrame == TimeFrame;
                    break;

                default:

                    predicate = null;
                    break;
            }

            var result = new List<KeyValuePair<string, SynchronizedDrawings>>(_indicatorInstances.Values.Count);

            foreach (var indicatorContianer in _indicatorInstances)
            {
                SynchronizedDrawings indicator;

                if (indicatorContianer.Value.GetIndicator(out indicator) == false || indicator == this || (predicate != null && predicate(indicator) == false))
                    continue;

                result.Add(new KeyValuePair<string, SynchronizedDrawings>(indicatorContianer.Key, indicator));
            }

            return result;
        }
    }

    public enum Mode
    {
        All,
        TimeFrame,
        Symbol
    }

    public enum YAxisType
    {
        Absolute,
        Relative,
    }

    public class IndicatorInstanceContainer<T> where T : Indicator
    {
        private readonly WeakReference _indicatorWeakReference;

        public IndicatorInstanceContainer(T indicator)
        {
            _indicatorWeakReference = new WeakReference(indicator);
        }

        public bool GetIndicator(out T indicator)
        {
            if (_indicatorWeakReference.IsAlive)
            {
                indicator = (T)_indicatorWeakReference.Target;

                return true;
            }

            indicator = null;

            return false;
        }
    }

    public enum ChartObjectOperationType
    {
        Updated,
        Added,
        Removed
    }

    public enum ObjectType
    {
        Interactive,
        NonInteractive,
        All
    }

    public class ChartInfo
    {
        public double TopY { get; set; }

        public double BottomY { get; set; }

        public string SymbolName { get; set; }
    }
}