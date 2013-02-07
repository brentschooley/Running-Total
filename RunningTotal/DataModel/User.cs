using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RunningTotal.DataModel
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "userID")]
        public int Id { get; set; }

        [DataMember(Name = "profile")]
        public string ProfileUriString { get; set; }

        [DataMember(Name = "settings")]
        public string SettingsUriString { get; set; }

        [DataMember(Name = "fitness_activities")]
        public string FitnessActivitiesUriString { get; set; }

        [DataMember(Name = "strength_training_activities")]
        public string StrengthTrainingActivitiesUriString { get; set; }

        [DataMember(Name = "background_activities")]
        public string BackgroundActivitiesUriString { get; set; }

        [DataMember(Name = "sleep")]
        public string SleepFeedUriString { get; set; }

        [DataMember(Name = "nutrition")]
        public string NutritionFeedUriString { get; set; }

        [DataMember(Name = "weight")]
        public string WeightMeasurementsFeedUriString { get; set; }

        [DataMember(Name = "general_measurements")]
        public string GeneralMeasurementsFeedUriString { get; set; }

        [DataMember(Name = "diabetes")]
        public string DiabetesMeasurementsFeedUriString { get; set; }

        [DataMember(Name = "records")]
        public string PersonalRecordsUriString { get; set; }

        [DataMember(Name = "team")]
        public string StreetTeamUriString { get; set; }
    }
}
