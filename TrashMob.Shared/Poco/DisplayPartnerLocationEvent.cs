﻿
namespace TrashMob.Poco
{
    using System;

    public class DisplayPartnerLocationServiceEvent
    {
        public Guid EventId { get; set; }

        public Guid PartnerLocationId { get; set; }

        public int ServiceTypeId { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public string EventName { get; set; }

        public string EventStreetAddress { get; set; }

        public string EventCity { get; set; }

        public string EventRegion { get; set; }

        public string EventCountry { get; set; }

        public string EventPostalCode { get; set; }

        public string EventDescription { get; set; }

        public string PartnerName { get; set; }

        public string PartnerLocationName { get; set; }

        public int EventPartnerLocationStatusId { get; set; }
    }
}
