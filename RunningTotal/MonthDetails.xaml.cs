using RunningTotal.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RunningTotal
{
    /// <summary>
    /// A page that displays an overview of a month's worth of running as a chart.  Each column/bar represents a single day.
    /// Tapping the bar will navigate to the details for that day.
    /// </summary>
    public sealed partial class MonthDetails : RunningTotal.Common.LayoutAwarePage
    {
        public MonthDetails()
        {
            this.InitializeComponent();

            this.DistanceChart.SeriesMouseLeftButtonUp += DistanceChart_SeriesMouseLeftButtonUp;
        }

        void DistanceChart_SeriesMouseLeftButtonUp(object sender, Infragistics.Controls.Charts.DataChartMouseButtonEventArgs e)
        {
            try
            {
                var activity = ((ActivityDayWrapper)e.Item).Activity;
                this.Frame.Navigate(typeof(RunDetailsPage), activity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
                var group = (ActivityMonth)navigationParameter;
                this.DefaultViewModel["Group"] = group;
                this.DefaultViewModel["Month"] = group.Month;
                this.DefaultViewModel["Year"] = group.Activities[0].StartTimeAsDateTime.Year;
                this.DefaultViewModel["Header"] = "Month in Review";
                this.DefaultViewModel["TotalActivities"] = group.Activities.Count.ToString();
                this.DefaultViewModel["TotalDistanceRun"] = string.Format("{0:0}", group.Activities.Sum(a => a.TotalDistanceInMiles));
                this.DefaultViewModel["TotalActivityTime"] = string.Format("{0:0}", this.TotalTimeInMinutes(group.Activities));
                this.DefaultViewModel["Summary"] = string.Format("{0} {1}", this.DefaultViewModel["Month"], this.DefaultViewModel["Year"]);

                var activitiesByDay = new List<ActivityDayWrapper>(31);
                for(int i = 1; i < 32; i++)
                {
                    var activity = group.Activities.FirstOrDefault(a => a.StartTimeAsDateTime.Day == i);
                    if (activity != null)
                        activitiesByDay.Add(new ActivityDayWrapper { Activity = activity, Day = i });
                    else
                        activitiesByDay.Add(new ActivityDayWrapper { Activity = new FitnessActivity { TotalDistanceInMeters = 0 }, Day = i });
                }
                this.DefaultViewModel["ActivitiesByDay"] = activitiesByDay;

                
        }

        private string GetActivitySum(List<FitnessActivity> activities)
        {
            try
            {
                var totalTimeInSeconds = activities.Sum(a => a.Duration);
                var t = TimeSpan.FromSeconds(totalTimeInSeconds);

                var sb = new StringBuilder();

                if (t.Hours > 0)
                    sb.AppendFormat("{0}h", t.Hours);

                sb.AppendFormat(" {0}m", t.Minutes);

                return sb.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private double TotalTimeInMinutes(List<FitnessActivity> activities)
        {
            return activities.Sum(a => a.Duration) / 60;
        }
        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                var activity = (FitnessActivity)e.ClickedItem;
                this.Frame.Navigate(typeof(RunDetailsPage), activity);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void DistanceChart_Copy_Loaded_1(object sender, RoutedEventArgs e)
        {
            foreach (UIElement i in FindElements(this.DistanceChart_Copy))
            {
                i.ManipulationMode = ManipulationModes.None;
            }
        }

        private IEnumerable<DependencyObject> FindElements(DependencyObject parent)
        {
            if (parent == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject o = VisualTreeHelper.GetChild(parent, i);

                foreach (DependencyObject obj in FindElements(o))
                    yield return (UIElement)obj;
            }

            yield return parent;
        }

        
    }

    public class ActivityDayWrapper
    {
        public int Day { get; set; }
        public FitnessActivity Activity { get; set; }
    }
}
