using RunningTotal.Model;
using RunningTotal.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Tools;

namespace RunningTotal
{
    /// <summary>
    /// A page that displays a header with summary details for the year and a grid of individual runs grouped by month.
    /// </summary>
    public sealed partial class HubPage : RunningTotal.Common.LayoutAwarePage
    {
        public HubPage()
        {
            this.InitializeComponent();
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            try
            {
                // Start progress indicators
                VisualStateManager.GoToState(this, "SpinnersActive", false);

                // TODO: Fetch the feed in extended splash screen to eliminate need for on screen spinners
                // Fetch feed
                FitnessActivityFeed feed = await HealthGraphClient.GetFitnessActivityFeedFromFile();

                // Turn off progress indicators
                VisualStateManager.GoToState(this, "SpinnersInactive", true);

                this.DefaultViewModel["Activities"] = feed.Activities;

                // These properties are used for the "This Year's Totals" section in the header
                this.DefaultViewModel["YearlyMiles"] = string.Format("{0:0}", feed.Activities.Sum(a => a.TotalDistanceInMiles));
                this.DefaultViewModel["YearlyMinutes"] = string.Format("{0:0}", feed.Activities.Sum(a => a.Duration)/60);
                this.DefaultViewModel["YearlyCalories"] = feed.Activities.Sum(a => a.TotalCalories);
               
                // Set up the "distance by month" (DistanceByMonth) data that will be used in the "monthly totals" chart in the header
                var dbm = ( from activity in feed.Items
                            orderby activity.StartTimeAsDateTime.Ticks ascending
                            group activity by activity.StartTimeAsDateTime.Month into activityMonth
                            select new { Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(activityMonth.Key).Substring(0,3), MonthNumber=activityMonth.Key, TotalDistance = activityMonth.Sum(a => a.TotalDistanceInMiles) }).ToList();

                for (int i = 1; i < 13; i++)
                {
                    if (dbm.Exists(a => a.MonthNumber == i))
                        continue;
                    dbm.Add(new { Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i).Substring(0,3), MonthNumber=i, TotalDistance = 0.0 });
                }

                this.DefaultViewModel["DistanceByMonth"] = dbm.OrderBy(a => a.MonthNumber);

                // Set up the groups for use in the main hub (both snapped and full) as well as semantic zoom.
                // Some properties (initialYear-related) are for future features.
                var activitiesByYear = feed.Activities.GroupBy(i => i.StartTimeAsDateTime.Year.ToString()).Select(a => new ActivityYear { Year = a.Key, Activities = a.ToList() }).ToDictionary(i => i.Year);
                var initialYear = (ActivityYear)activitiesByYear["2010"];
                this.DefaultViewModel["MonthlyActivityGroups"] = initialYear.Activities.GroupBy(a => String.Format("{0:MMMM}", a.StartTimeAsDateTime)).Select(m => new ActivityMonth { Month = m.Key, Activities = m.ToList() }).ToDictionary(i => i.Month);

                var result = initialYear.Activities.OrderBy(a => a.StartTimeAsDateTime.Month).GroupBy(a => String.Format("{0:MMMM}", a.StartTimeAsDateTime)).Select(m => new ActivityMonth { Month = m.Key, TotalActivities = m.ToList(), TotalActivitiesCount = m.Count(), Activities = m.Take(10).ToList() });
                var result1 = feed.Activities.GroupBy(i => i.StartTimeAsDateTime.Year.ToString()).Select(a => new ActivityYear { Year = a.Key, TotalActivitiesCount = a.Count(), Activities = a.Take(10).ToList() });
                this.groupedItemsViewSource.Source = result.ToList();

                var collectionGroups = groupedItemsViewSource.View.CollectionGroups;
                ((ListViewBase)this.semanticZoom.ZoomedOutView).ItemsSource = collectionGroups;
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determine what group the Button instance represents
                var clickedGroup = (ActivityMonth)(sender as FrameworkElement).DataContext;
                var group = ((Dictionary<string, ActivityMonth>)this.DefaultViewModel["MonthlyActivityGroups"])[clickedGroup.Month];
                // Navigate to the appropriate destination page, configuring the new page
                // by passing required information as a navigation parameter
                this.Frame.Navigate(typeof(MonthDetails), ((ActivityMonth)group));
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
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

    }

    public class PaceWrapper
    {
        public DateTime Date { get; set; }
        public double AveragePace { get; set; }
    }
}
