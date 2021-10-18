﻿namespace TrashMobMobile
{
    using TrashMobMobile.Services;
    using TrashMobMobile.Views;
    using TrashMobMobile.Features.LogOn;
    using Xamarin.Forms;

    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<B2CAuthenticationService>();

            MainPage = new WelomePage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
