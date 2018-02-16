using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Android.Content;
using Android.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace GPSApp
{
    [Activity(Label = "GPSApp", MainLauncher = true)]
    public class MainActivity : Activity, ILocationListener
    {
        LocationManager locMgr;
        TextView LatitudeTextView;
        TextView LongitudeTextView;
        TextView EstadoTextView;
        TextView DisponibilidadTextView;
        Database db;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            LatitudeTextView = FindViewById(Resource.Id.latitude) as TextView;
            LongitudeTextView = FindViewById(Resource.Id.longitude) as TextView;
            EstadoTextView = FindViewById(Resource.Id.estado) as TextView;
            DisponibilidadTextView = FindViewById(Resource.Id.disponibilidad) as TextView;
            locMgr = GetSystemService(Context.LocationService) as LocationManager;

            LatitudeTextView.Text = "000";
            LongitudeTextView.Text = "000";

            db = new Database();
            var coods = db.SelectCoordenate();
        }

        protected override void OnResume()
        {
            base.OnResume();
            string Provider = LocationManager.GpsProvider;
            if (locMgr.IsProviderEnabled(Provider))
            //if (locMgr.IsProviderEnabled(Provider) && locMgr.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                locMgr.RequestLocationUpdates(Provider, 100, 0.3f, this);
                locMgr.RequestLocationUpdates(LocationManager.NetworkProvider, 0, 0, this);
            }
            else
            {
                Toast.MakeText(this as Context, $"{Provider} is not available", ToastLength.Short).Show();
                //RequestPermissions(new string[] {
                //    Manifest.Permission.AccessFineLocation
                //}, 0);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            locMgr.RemoveUpdates(this);
        }

        public void OnLocationChanged(Location location)
        {
            LatitudeTextView.Text = $"{location.Latitude.ToString()}\nSexigesimal {ConvertToDMS(location.Latitude, "\nDegree {0}\nMins {1}\nSecs {2}\n")}";
            LongitudeTextView.Text = $"{location.Longitude.ToString()}\nSexigesimal {ConvertToDMS(location.Longitude, "\nDegree {0}\nMins {1}\nSecs {2}\n")}";
            Task.Run(async () =>
            {
                await SaveCoordenates(location.Latitude, location.Longitude);
                string time = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                db.InsertCoordenate(location.Latitude, location.Longitude, time);
                var gpsSender = new GPSSender();

                gpsSender.Send(gpsSender.Serialize(new
                {
                    location.Latitude,
                    location.Longitude,
                    Date = time
                }));
            });
        }

        public void OnProviderDisabled(string provider)
        {
            EstadoTextView.Text = $"Status {provider} disabled";
        }

        public void OnProviderEnabled(string provider)
        {
            EstadoTextView.Text = $"Status {provider} enabled";
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            DisponibilidadTextView.Text = $"{provider} Availability {Enum.GetName(typeof(Availability), Availability.Available)}";
        }

        async Task SaveCoordenates(double latitude, double longitude)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.Path;
            string filepath = System.IO.Path.Combine(path, "Coordenates.txt");

            using (var streamWriter = new StreamWriter(filepath, true))
            {
                string time = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                streamWriter.WriteLine(string.Format("{0},{1} {2}\n", latitude, longitude, time));
            }
        }

        string ConvertToDMS(double value, string format = "{0} Degrs, {1} Mins, {2} Secs")
        {
            value = Math.Abs(value);
            double degree = Math.Truncate(value);
            double minutes = Math.Truncate((value - degree) * 60);
            double seconds = (((value - degree) * 60) - minutes) * 60;
            return string.Format(format, degree, minutes, seconds);
        }
    }
}

