using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace IPTVProxy.Common.Models.Xtream
{
    public class PlayerApiRequest
    {
        [FromQuery(Name = "username")]
        public required string Username { get; set; } = string.Empty;

        [FromQuery(Name = "password")]
        public required string Password { get; set; }

        [FromQuery(Name = "action")]
        public string? Action { get; set; }

        [FromQuery(Name = "category_id")]
        public string? CategoryId { get; set; }

        [FromQuery(Name = "stream_id")]
        public int? StreamId { get; set; }

        [FromQuery(Name = "vod_id")]
        public int? VodId { get; set; }

        [FromQuery(Name = "series_id")]
        public int? SeriesId { get; set; }

        [FromQuery(Name = "limit")]
        public int? Limit { get; set; }
    }
}
