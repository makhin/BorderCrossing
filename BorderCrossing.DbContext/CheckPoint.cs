﻿using NetTopologySuite.Geometries;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BorderCrossing.DbContext
{
    public class CheckPoint
    {
        public int CheckPointId { get; set; }

        public Request Request { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "geometry")]
        public Geometry Point { get; set; }

        public string CountryName { get; set; }
    }
}