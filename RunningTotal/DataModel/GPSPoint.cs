using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunningTotal.Model.SubTypes;

namespace RunningTotal.Model
{
    public class GPSPoint
    {
        public double Timestamp { get; set; }

        public double TimestampFromPreviousPoint { get; set; }

        public double TimestampInMinutes
        {
            get
            {
                return Timestamp / 60;
            }
        }

        public double TimestampFromPreviousPointInMinutes
        {
            get
            {
                return TimestampFromPreviousPoint / 60;
            }
        }

        public double TimestampInHours
        {
            get
            {
                return TimestampInMinutes / 60;
            }
        }

        public double TimestampFromPreviousPointInHours
        {
            get
            {
                return TimestampFromPreviousPointInMinutes / 60;
            }
        }

        public double Altitude { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        /// <summary>
        /// Distance from start to this point in meters
        /// </summary>
        public double Distance { get; set; }

        public double DistanceFromPreviousPoint { get; set; }

        /// <summary>
        /// Distance from start to this point in miles
        /// </summary>
        public double DistanceInMiles
        {
            get
            {
                return Distance * 0.000621371;
            }
        }

        /// <summary>
        /// Distance from previous point to this point in miles
        /// </summary>
        public double DistanceFromPreviousPointInMiles
        {
            get
            {
                return DistanceFromPreviousPoint * 0.000621371;
            }
        }

        /// <summary>
        /// Returns current pace in seconds per meter
        /// </summary>
        public double CurrentPace {
            get
            {
                if (DistanceFromPreviousPoint == 0)
                {
                    return 0;
                }
                return TimestampFromPreviousPoint / DistanceFromPreviousPoint;
            }
        }

        public double CurrentPaceInMinPerMile
        {
            get
            {
                if (DistanceFromPreviousPoint == 0)
                {
                    return 0;
                }
                return TimestampFromPreviousPointInMinutes / DistanceFromPreviousPointInMiles;
            }
        }

        /// <summary>
        /// Returns current speed in meters per second
        /// </summary>
        public double CurrentSpeed {
            get
            {
                if (TimestampFromPreviousPoint == 0)
                {
                    return 0;
                }
                return DistanceFromPreviousPoint / TimestampFromPreviousPoint;
            }
        }

        public double CurrentSpeedInMilePerHour
        {
            get
            {
                if (Timestamp == 0)
                { 
                    return 0; 
                }
                return DistanceFromPreviousPointInMiles / TimestampFromPreviousPointInHours;
            }
        }

        /// <summary>
        /// Returns average pace up to this point in seconds per meter
        /// </summary>
        public double AvgPace {
            get
            {
                if (Distance == 0)
                {
                    return 0;
                }
                return Timestamp / Distance;
            }
        }

        public double AvgPaceInMinPerMile
        {
            get
            {
                if(Distance == 0)
                {
                    return 0;
                }
                return TimestampInMinutes / DistanceInMiles;
            }
        }

        /// <summary>
        /// Returns average speed up to this point in meters per second
        /// </summary>
        public double AvgSpeed {
            get
            {
                if (Timestamp == 0)
                {
                    return 0;
                }
                return Distance / Timestamp;
            }
        }

        public double AvgSpeedInMilePerHour
        {
            get
            {
                if (Timestamp == 0)
                {
                    return 0;
                }
                return DistanceInMiles / TimestampInHours;
            }
        }

        /// <summary>
        /// Average radius of the earth in meters
        /// </summary>
        private double EarthRadius
        {
            get
            {
                return 6371010;
            }
        }

        /// <summary>
        /// Radius of the polar coordinate after altitude has been accounted for
        /// </summary>
        private double Radius
        {
            get
            {
                return Altitude + EarthRadius;
            }
        }

        private double X
        {
            get
            {
                return Radius * Math.Sin(Latitude.ToRadian()) * Math.Cos(Longitude.ToRadian());
            }
        }

        private double Y
        {
            get
            {
                return Radius * Math.Sin(Latitude.ToRadian()) * Math.Sin(Longitude.ToRadian());
            }
        }

        public double Test
        {
            get
            {
                return 0;
            }
        }

        private double Z
        {
            get
            {
                return Radius * Math.Cos(Latitude.ToRadian());
            }
        }

        public double CartesianDistance(GPSPoint p)
        {
            double deltaX = this.X - p.X;
            double deltaY = this.Y - p.Y;
            double deltaZ = this.Z - p.Z;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public double HaversineDistance(GPSPoint p)
        {
            double dLat = (p.Latitude - this.Latitude).ToRadian();
            double dLon = (p.Longitude - this.Longitude).ToRadian();
            double lat1 = this.Latitude.ToRadian();
            double lat2 = p.Latitude.ToRadian();
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = Radius * c;
            return d;
        }

        public GPSPoint() { }

        public GPSPoint(Path p, Distance d, Path prevP, Distance prevD)
        {
            Altitude = p.Altitude;
            Distance = d.Meters;
            DistanceFromPreviousPoint = d.Meters - prevD.Meters;
            TimestampFromPreviousPoint = d.Timestamp - prevD.Timestamp;
            Latitude = p.Latitude;
            Longitude = p.Longitude;
            Timestamp = p.Timestamp;
        }

        public GPSPoint(Path p)
        {
            Altitude = p.Altitude;
            Distance = 0;
            Latitude = p.Latitude;
            Longitude = p.Longitude;
            Timestamp = p.Timestamp;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Total distance traveled: " + this.DistanceInMiles + " miles\n");
            sb.Append("Average pace: " + this.AvgPaceInMinPerMile.ToMinuteString() + " min/mile\n");
            sb.Append("Average speed: " + this.AvgSpeedInMilePerHour + " mph\n");
            sb.Append("Current pace: " + this.CurrentPaceInMinPerMile.ToMinuteString() + " min/mile\n");
            sb.Append("Current speed: " + this.CurrentSpeedInMilePerHour + " mph\n");
            return sb.ToString();
        }

        public static List<GPSPoint> GeneratePoints(FitnessActivity fitnessActivity)
        {
            List<GPSPoint> points = new List<GPSPoint>();
            int index = 0;
            foreach (Path path in fitnessActivity.Path)
            {
                if (index > 0)
                {
                    points.Add(new GPSPoint(path, fitnessActivity.Distance[index], fitnessActivity.Path[index - 1], fitnessActivity.Distance[index - 1]));
                }
                else
                {
                    points.Add(new GPSPoint(path));
                }
                index++;
            }
           
            return points;
        }
    }
}
