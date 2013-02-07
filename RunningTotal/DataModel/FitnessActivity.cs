// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunningTotal.Model.SubTypes;
using System.Text;

namespace RunningTotal.Model
{

    public class FitnessActivity
    {

        [JsonProperty("climb")]
        public double Climb { get; set; }

        [JsonProperty("nearest_teammate_weight")]
        public IList<object> NearestTeammateWeight { get; set; }

        [JsonProperty("nearest_nutrition")]
        public string NearestNutrition { get; set; }

        [JsonProperty("equipment")]
        public string Equipment { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("distance")]
        public IList<Distance> Distance { get; set; }

        [JsonProperty("nearest_teammate_strength_training_activities")]
        public IList<object> NearestTeammateStrengthTrainingActivities { get; set; }

        [JsonProperty("nearest_teammate_diabetes")]
        public IList<object> NearestTeammateDiabetes { get; set; }

        [JsonProperty("path")]
        public IList<Path> Path { get; set; }

        [JsonProperty("calories")]
        public IList<object> Calories { get; set; }

        [JsonProperty("total_calories")]
        public int TotalCalories { get; set; }

        [JsonProperty("nearest_teammate_nutrition")]
        public IList<object> NearestTeammateNutrition { get; set; }

        [JsonProperty("activity")]
        public string Activity { get; set; }

        [JsonProperty("nearest_weight")]
        public string NearestWeight { get; set; }

        [JsonProperty("total_distance")]
        public double TotalDistanceInMeters { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("nearest_teammate_sleep")]
        public IList<object> NearestTeammateSleep { get; set; }

        [JsonProperty("nearest_teammate_fitness_activities")]
        public IList<string> NearestTeammateFitnessActivities { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("nearest_teammate_general_measurements")]
        public IList<object> NearestTeammateGeneralMeasurements { get; set; }

        [JsonProperty("userID")]
        public int UserID { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("heart_rate")]
        public IList<object> HeartRate { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("images")]
        public IList<object> Images { get; set; }

        [JsonProperty("is_live")]
        public bool IsLive { get; set; }

        [JsonProperty("nearest_teammate_background_activities")]
        public IList<object> NearestTeammateBackgroundActivities { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }


        #region Custom Code

        public DateTime StartTimeAsDateTime
        {
            get { return DateTime.Parse(this.StartTime); }
        }

        public string FormattedDuration
        {
            get
            {
                var t = TimeSpan.FromSeconds(this.Duration);

                var sb = new StringBuilder();

                if (t.Hours > 0)
                    sb.AppendFormat("{0}h ", t.Hours);

                var minutes = t.Seconds < 30 ? t.Minutes : t.Minutes + 1;
                sb.AppendFormat("{0}m", minutes);

                return sb.ToString();
            }
        }

        public double TotalDistanceInMiles
        {
            get
            {
                return (TotalDistanceInMeters / 1609.344);
            }
        }

        public string DistanceSummaryMiles
        {
            get
            {
                return string.Format("{0:0.00}", this.TotalDistanceInMiles);
            }
        }

        public double AverageSpeed
        {
            get
            {
                return this.TotalDistanceInMeters / this.Duration;
            }
        }

        public double AveragePace
        {
            get
            {
                return this.Duration / this.TotalDistanceInMeters;
            }
        }

        public string DistanceMilesRounded
        {
            get
            {
                return string.Format("{0:0.0}", this.TotalDistanceInMiles);
            }
        }

        public string DistanceShortSummaryMiles
        {
            get
            {
                return string.Format("{0:0.0} mi", this.TotalDistanceInMiles);
            }
        }

        public string FormattedRunDate
        {
            get
            {
                return DateTime.Parse(this.StartTime).ToString("dddd MMM d");
            }
        }

        public string SummaryFormattedDuration
        {
            get
            {
                var t = TimeSpan.FromSeconds(this.Duration);

                var s = new StringBuilder();

                if (t.Hours > 0)
                    s.AppendFormat("{0} hours, ", t.Hours);

                s.AppendFormat("{0} minutes, {1} seconds", t.Minutes, t.Seconds);

                return s.ToString();
            }
        }

        public string ShortFormattedDuration
        {
            get
            {
                var t = TimeSpan.FromSeconds(this.Duration);

                var s = new StringBuilder();

                if (t.Hours > 0)
                    s.AppendFormat("{0:0}:", t.Hours);

                s.AppendFormat("{0:0}:{1:00}", t.Minutes, t.Seconds);

                return s.ToString();
            }
        }

        #endregion
    }
}
