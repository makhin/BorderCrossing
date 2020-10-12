﻿using System;
using System.Collections.Generic;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BorderCrossing.DbContext;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BorderCrossing.UWP2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultPage : Page
    {
        public BorderCrossingResponse Response { get; set; }

        public ResultPage()
        {
            this.InitializeComponent();
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var checkPoints = (List<CheckPoint>)e.Parameter;
            if (checkPoints == null || !checkPoints.Any())
            {
                return;
            }

            Response = new BorderCrossingResponse();
            var arrivalPoint = checkPoints.First();
            Response.Periods.Add(new Period
            {
                ArrivalPoint = arrivalPoint,
                Country = arrivalPoint.CountryName
            });
            var last = Response.Periods.Last();

            foreach (var checkPoint in checkPoints.Skip(1).Take(checkPoints.Count - 2))
            {
                last.DeparturePoint = checkPoint;
                Response.Periods.Add(new Period
                {
                    ArrivalPoint = checkPoint,
                    Country = checkPoint.CountryName,
                });
                last = Response.Periods.Last();
            }

            last.DeparturePoint = checkPoints.Last();
        }
    }
}