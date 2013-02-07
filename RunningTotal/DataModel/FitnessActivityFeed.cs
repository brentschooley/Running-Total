// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using RunningTotal.Model.SubTypes;

namespace RunningTotal.Model
{

    public class FitnessActivityFeed
    {

        [JsonProperty("items")]
        public IList<Item> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        public List<FitnessActivity> Activities { get; set; }
    }

    public class ActivityYear
    {
        public string Year { get; set; }
        public List<FitnessActivity> Activities { get; set; }
        public int TotalActivitiesCount { get; set; }
        public string YearPlusTotal
        {
            get
            {
                return string.Format("{0} - {1} runs", Year, TotalActivitiesCount);
            }
        }
    }

    public class ActivityMonth
    {
        public string Month { get; set; }
        public List<FitnessActivity> Activities { get; set; }
        public List<FitnessActivity> TotalActivities { get; set; }
        public int TotalActivitiesCount { get; set; }

        public string MinutesRun
        {
            get
            {
                return string.Format("{0:0}", this.TotalActivities.Sum(a => a.Duration) / 60);
            }
        }

        public string DistanceRun
        {
            get
            {
                return string.Format("{0:0}", this.TotalActivities.Sum(a => a.TotalDistanceInMiles)); 
            }
        }
        public string MonthPlusTotal
        {
            get
            {
                return string.Format("{0} - {1} runs", Month, TotalActivitiesCount);
            }
        }
    }
}
