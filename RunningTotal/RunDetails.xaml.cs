using Infragistics;
using Infragistics.Controls.Charts;
using Infragistics.Controls.Maps;
using RunningTotal.Model;
using RunningTotal.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace RunningTotal
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class RunDetailsPage : RunningTotal.Common.LayoutAwarePage
    {
        public RunDetailsPage()
        {
            this.InitializeComponent();
        }

        FastReflectionHelper _frh = new FastReflectionHelper();

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var item = (FitnessActivity)navigationParameter;
            var start = DateTime.Parse(item.StartTime);

            // Summary details
            this.DefaultViewModel["RunDate"] = start.ToString("M");
            this.DefaultViewModel["CaloriesBurned"] = item.TotalCalories;
            this.DefaultViewModel["TotalDistanceRun"] = item.DistanceSummaryMiles;
            this.DefaultViewModel["TotalTime"] = item.FormattedDuration;

            // Fetch the Path related GPSPoints for the run
            var points = GPSPoint.GeneratePoints(item);
            this.DefaultViewModel["GPSPoints"] = points;
            this.DefaultViewModel["Path"] = item.Path;

            // Perform calculations on the path data
            var avg = TrendCalculators.SMA((from p in points select p.CurrentPaceInMinPerMile).ToList<double>(), 10).ToList();
            this.DefaultViewModel["AveragePace"] = avg;
            this.DefaultViewModel["MaxAltitude"] = item.Path.OrderBy(p => p.Altitude).Last();
            this.DefaultViewModel["MinAltitude"] = item.Path.OrderBy(p => p.Altitude).First();
            this.DefaultViewModel["MinPaceRange"] = 0;
            this.DefaultViewModel["MaxPaceRange"] = points.Max(p => p.AvgPaceInMinPerMile) + 4.0;
            
            // Project points onto a list for consumption in the PolylineSeries
            this.DefaultViewModel["PolylinePoints"] = points.Select(p => new Point(p.Longitude, p.Latitude)).ToList<Point>();

            // Set up the initial map viewing rectangle
            var minLatitude = item.Path.Min(p => p.Latitude);
            var maxLatitude = item.Path.Max(p => p.Latitude);
            var minLongitude = item.Path.Min(p => p.Longitude);
            var maxLongitude = item.Path.Max(p => p.Longitude);
            var geoRegion = GetRectForLatitudeLongitude(minLongitude - 3.0, maxLatitude + 3.0, maxLongitude + 3.0, minLatitude - 3.0);
            this.map.WindowRect = this.map.GetZoomFromGeographic(geoRegion);
            this.snappedMap.WindowRect = this.snappedMap.GetZoomFromGeographic(geoRegion);

            // Wait and then zoom to the polyline.  This will allow for some caching of map tiles and avoid exceptions in map display.
            await Task.Delay(500);
            geoRegion = GetRectForLatitudeLongitude(minLongitude, maxLatitude, maxLongitude, minLatitude);
            this.map.WindowRect = this.map.GetZoomFromGeographic(geoRegion);
            this.snappedMap.WindowRect = this.snappedMap.GetZoomFromGeographic(geoRegion);

            // Set up polyline and marker point tracking.
            var series = new GeographicPolylineSeries();
            this.DefaultViewModel["PolylineWrapper"] = new List<PointWrapper>() { new PointWrapper() { Points = points.Select(p => new Point(p.Longitude, p.Latitude)).ToList<Point>() }};
            var poi = new ObservableCollection<GPSPoint>();
            poi.Add(points[0]);
            this.DefaultViewModel["PointsOfInterest"] = poi;
            series.ShapeMemberPath = "Points";
            
        }

        private Rect GetRectForLatitudeLongitude(double minLongitude, double maxLatitude, double maxLongitude, double minLatitude)
        {
            return new Rect(Math.Min(minLongitude, maxLongitude), Math.Min(maxLatitude, minLatitude), Math.Abs(minLongitude - maxLongitude), Math.Abs(maxLatitude - minLatitude));
        }

        private void chart_SeriesCursorMouseMove_1(object sender, ChartCursorEventArgs e)
        {
            var sv = e.SeriesViewer;
            var c = sv.CrosshairPoint;

            if (double.IsNaN(c.X) || double.IsNaN(c.Y))
            {
                return;
            }

            overlay.Children.Clear();


            var items = (IList<Model.SubTypes.Path>)DefaultViewModel["Path"];
            var count = items.Count;

            var index = (int)Math.Round(c.X * (items.Count - 1));

            if (index < 0 || index > items.Count - 1)
            {
                return;
            }

            var xVal = sv.CrosshairPoint.X;

            var item = items[index];

            var lat = item.Latitude;
            var lon = item.Longitude;

            var pois = (ObservableCollection<GPSPoint>)DefaultViewModel["PointsOfInterest"];

            pois[0] = new GPSPoint(item);

            foreach (var series in sv.Series)
            {
                if (series is HorizontalAnchoredCategorySeries)
                {
                    var acs = (HorizontalAnchoredCategorySeries)series;
                    var il = series.ItemsSource as IList;
                    var ie = series.ItemsSource.OfType<object>();
                    int itemCount;

                    if (il != null)
                    {
                        itemCount = il.Count;
                    }
                    else
                    {
                        itemCount = ie.Count();
                    }

                    var itemIndex = xVal * (itemCount - 1);
                    var previousIndex = (int)Math.Floor(itemIndex);
                    var nextIndex = (int)Math.Ceiling(itemIndex);
                    if (previousIndex < 0)
                    {
                        previousIndex = 0;
                    }
                    if (nextIndex < 0)
                    {
                        nextIndex = 0;
                    }

                    if (nextIndex > itemCount - 1)
                    {
                        nextIndex = itemCount - 1;
                    }
                    if (previousIndex > itemCount - 1)
                    {
                        previousIndex = itemCount - 1;
                    }

                    object previousItem;
                    object nextItem;

                    if (il != null)
                    {
                        previousItem = il[previousIndex];
                        nextItem = il[nextIndex];
                    }
                    else
                    {
                        previousItem = ie.Skip(previousIndex).Take(1).First();
                        nextItem = ie.Skip(nextIndex).Take(1).First();
                    }

                    _frh.PropertyName = acs.ValueMemberPath;
                    var previousValue = Convert.ToDouble(_frh.GetPropertyValue(previousItem));
                    var nextValue = Convert.ToDouble(_frh.GetPropertyValue(nextItem));

                    var interpolatedValue = previousValue + (nextValue - previousValue) * (itemIndex - previousIndex);

                    if (series is LineSeries && interpolatedValue > (double)this.DefaultViewModel["MaxPaceRange"])
                        return;

                    var displayY = acs.YAxis.ScaleValue(interpolatedValue);
                    var displayX = acs.XAxis.ScaleValue(itemIndex);

                    var pos = series.TransformToVisual(overlay).TransformPoint(new Point(displayX, displayY));

                    var el = new Ellipse()
                    {
                        Fill = new SolidColorBrush(Colors.Blue),
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = 1,
                        Width = 14,
                        Height = 14
                    };

                    Canvas.SetTop(el, pos.Y - 7.0);
                    Canvas.SetLeft(el, pos.X - 7.0);

                    overlay.Children.Add(el);

                    var tb = new TextBlock();
                    tb.FontSize = 14;
                    if (series is AreaSeries)
                        tb.Text = Math.Round(interpolatedValue, 2).ToString() + " ft.";
                    else
                        tb.Text = Math.Round(interpolatedValue, 2).ToString() + " min/mi";

                    Canvas.SetTop(tb, pos.Y - 10.0);
                    Canvas.SetLeft(tb, pos.X + 10.0);

                   overlay.Children.Add(tb);
                }
            }

        }


        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            //var selectedItem = (SampleDataItem)this.flipView.SelectedItem;
            //pageState["SelectedItem"] = selectedItem.UniqueId;
        }
    }

    public class PointWrapper
    {
        public IEnumerable<Point> Points { get; set; }

    }
}
