﻿using Plugin.Media;
using PM2E2201810070111.Modelos;
using PM2E2201810070111.Vistas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PM2E2201810070111
{
    public partial class MainPage : ContentPage
    {
        CancellationTokenSource cts;
        string lati = "", longi = "", base64Val = "";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void btnfotocap_Clicked(object sender, EventArgs e)
        {
            var tomarfoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "miApp",
                Name = "Image.jpg"

            });




            if (tomarfoto != null)
            {
                imagen.Source = ImageSource.FromStream(() => { return tomarfoto.GetStream(); });
            }

            Byte[] imagenByte = null;

            using (var stream = new MemoryStream())
            {
                tomarfoto.GetStream().CopyTo(stream);
                tomarfoto.Dispose();
                imagenByte = stream.ToArray();

                base64Val = Convert.ToBase64String(imagenByte);
                //await EmpleController.SubirImagen(imagenByte);
            }


        }

        private async void btnsalnvar_Clicked(object sender, EventArgs e)
        {

            //condicion para ver si los campos estan vacios
            if (String.IsNullOrWhiteSpace(base64Val) == true)
            {
                await DisplayAlert("Mensaje", "Foto vacia", "Ok");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(txtdescripLarga.Text) == true)
                {
                    await DisplayAlert("Mensaje", "La Descripcion esta vacia", "Ok");
                }
                else
                {
                    _ = TenerLocacion(true);
                }
            }

        }

        private async void btnlis_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Listado_Sitios());
        }


        public async void EvaluarInternet()
        {
            var current = Connectivity.NetworkAccess;

            // en caso de tener internet obtener la ubicacion
            if (current == NetworkAccess.Internet)
            {
                // Connection to internet is available
                //es falso para no guardar informacion solo mostrar, si el parametro es true guarda
                _ = TenerLocacion(false);
            }
            else
            {
                await DisplayAlert("error", "Sin Internet", "Ok");

            }
        }

        async Task TenerLocacion(bool guardar)
        {

            // le mando parametros para al momento de dar click en guardar 
            //le mondo un valor que es true solo al darle click
            // le mando falso para que no guarde informacion

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location == null)
                {
                    await DisplayAlert("error", "GPS Inactivo", "Ok");
                }

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                    lati = location.Latitude.ToString();
                    longi = location.Longitude.ToString();
                    lbllatitud.Text = lati;
                    lbllongitud.Text = longi;


                    //si no es true solo va a mostrar los datos y NO va guardar
                    if (guardar == true)
                    {
                        GuardarSitios();
                    }

                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
                await DisplayAlert("error", "no es compatible con la excepción del dispositivo GPS", "Ok");

            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
                await DisplayAlert("error", "la ubicacion no habilitado en la excepción del dispositivo", "Ok");
            }
            catch (PermissionException)
            {
                // Handle permission exception
                await DisplayAlert("error", "No tiene Permisos de ubicacion", "Ok");
            }
            catch (Exception)
            {
                // Unable to get location
                await DisplayAlert("error", "No se puede tener la ubicacion", "Ok");
            }
        }


        protected override void OnDisappearing()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
            base.OnDisappearing();
        }

        private async void btnCapturarFoto_Clicked(object sender, EventArgs e)
        {

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            try
            {
                Stream streama = null;
                var file = await CrossMedia.Current.PickPhotoAsync().ConfigureAwait(true);


                if (file == null)
                    return;

                streama = file.GetStream();


                imagen.Source = ImageSource.FromStream(() => streama);

                Byte[] imagenByte = null;

                using (var stream = new MemoryStream())
                {
                    //file.GetStream().CopyTo(stream);
                    file.GetStream().CopyTo(stream);
                    file.Dispose();
                    imagenByte = stream.ToArray();

                    base64Val = Convert.ToBase64String(imagenByte);
                    //await EmpleController.SubirImagen(imagenByte);
                }




            }
            catch //(Exception ex)
            {
             }


           
        }

        protected override void OnAppearing()
        {

            base.OnAppearing();
            EvaluarInternet();
        }

        public async void GuardarSitios()
        {


            Sitios siti = new Sitios
            {
                descripcion = txtdescripLarga.Text,
                latitud = lati,
                longitud = longi,
                fotografia = base64Val

            };

            var resultado = await App.BaseDatos.GrabarSitios(siti);

            if (resultado == 1)
            {
                await DisplayAlert("Mensaje", "Registro exitoso!!!", "ok");
                txtdescripLarga.Text = base64Val = String.Empty;
                imagen.Source = "perfil.jpg";
            }
            else
            {
                await DisplayAlert("Error", "No se pudo Guardar", "ok");
            }

            
             

        }


    }
}
