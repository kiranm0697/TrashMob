﻿namespace TrashMob.Shared.Managers.Events
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using TrashMob.Models;
    using TrashMob.Shared.Engine;
    using TrashMob.Shared.Managers.Interfaces;
    using TrashMob.Shared.Persistence.Interfaces;
    using System;
    using Microsoft.EntityFrameworkCore;
    using TrashMob.Shared.Poco;
    using System.Linq;
    using EmailAddress = Poco.EmailAddress;

    public class EventPartnerManager : BaseManager<EventPartner>, IEventPartnerManager
    {
        private readonly IKeyedRepository<Event> eventRepository;
        private readonly IKeyedRepository<Partner> partnerRepository;
        private readonly IKeyedRepository<PartnerLocation> partnerLocationRepository;
        private readonly IKeyedRepository<User> userRepository;
        private readonly IEmailManager emailManager;

        public EventPartnerManager(IBaseRepository<EventPartner> repository, 
                                   IKeyedRepository<Event> eventRepository, 
                                   IKeyedRepository<Partner> partnerRepository, 
                                   IKeyedRepository<PartnerLocation> partnerLocationRepository, 
                                   IKeyedRepository<User> userRepository,
                                   IEmailManager emailManager) 
            : base(repository)
        {
            this.eventRepository = eventRepository;
            this.partnerRepository = partnerRepository;
            this.partnerLocationRepository = partnerLocationRepository;
            this.userRepository = userRepository;
            this.emailManager = emailManager;
        }

        public override async Task<EventPartner> AddAsync(EventPartner instance, CancellationToken cancellationToken = default)
        {
            var eventPartner = await base.AddAsync(instance, cancellationToken);

            // Notify Admins that a new partner request has been made
            var subject = "A New Partner Request for an Event has been made!";
            var message = $"A new partner request for an event has been made for event {instance.EventId}!";

            var recipients = new List<EmailAddress>
            {
                new EmailAddress { Name = Constants.TrashMobEmailName, Email = Constants.TrashMobEmailAddress }
            };

            var adminDynamicTemplateData = new
            {
                username = Constants.TrashMobEmailName,
                emailCopy = message,
                subject = subject,
            };

            await emailManager.SendTemplatedEmailAsync(subject, SendGridEmailTemplateId.GenericEmail, SendGridEmailGroupId.EventRelated, adminDynamicTemplateData, recipients, CancellationToken.None).ConfigureAwait(false);

            var partnerMessage = emailManager.GetHtmlEmailCopy(NotificationTypeEnum.EventPartnerRequest.ToString());
            var partnerSubject = "A TrashMob.eco Event would like to Partner with you!";

            partnerMessage = partnerMessage.Replace("{PartnerLocationName}", eventPartner.PartnerLocation.Name);

            var dynamicTemplateData = new
            {
                username = eventPartner.Partner.Name,
                emailCopy = partnerMessage,
                subject = subject,
            };

            var partnerRecipients = new List<EmailAddress>
            {
                new EmailAddress { Name = eventPartner.PartnerLocation.Name, Email = eventPartner.PartnerLocation.PrimaryEmail },
                new EmailAddress { Name = eventPartner.PartnerLocation.Name, Email = eventPartner.PartnerLocation.SecondaryEmail }
            };

            await emailManager.SendTemplatedEmailAsync(partnerSubject, SendGridEmailTemplateId.GenericEmail, SendGridEmailGroupId.EventRelated, dynamicTemplateData, partnerRecipients, CancellationToken.None).ConfigureAwait(false);

            return eventPartner;
        }

        public override async Task<EventPartner> UpdateAsync(EventPartner instance, Guid userId, CancellationToken cancellationToken = default)
        {
            var updatedEventPartner = await base.UpdateAsync(instance, userId, cancellationToken);
            
            var user = await userRepository.GetAsync(instance.CreatedByUserId, cancellationToken).ConfigureAwait(false);

            // Notify Admins that a partner request has been responded to
            var subject = "A partner request for an event has been responded to!";
            var message = $"A partner request for an event has been responded to for event {instance.EventId}!";

            var recipients = new List<EmailAddress>
            {
                new EmailAddress { Name = Constants.TrashMobEmailName, Email = Constants.TrashMobEmailAddress }
            };

            var adminDynamicTemplateData = new
            {
                username = Constants.TrashMobEmailName,
                emailCopy = message,
                subject = subject,
            };

            await emailManager.SendTemplatedEmailAsync(subject, SendGridEmailTemplateId.GenericEmail, SendGridEmailGroupId.EventRelated, adminDynamicTemplateData, recipients, CancellationToken.None).ConfigureAwait(false);

            var partnerMessage = emailManager.GetHtmlEmailCopy(NotificationTypeEnum.EventPartnerResponse.ToString());
            var partnerSubject = "A TrashMob.eco Partner has responded to your request!";

            partnerMessage = partnerMessage.Replace("{UserName}", user.UserName);

            var dashboardLink = string.Format("https://www.trashmob.eco/manageeventdashboard/{0}", instance.EventId);
            partnerMessage = partnerMessage.Replace("{PartnerResponseUrl}", dashboardLink);

            var partnerRecipients = new List<EmailAddress>
            {
                new EmailAddress { Name = user.UserName, Email = user.Email },
            };

            var dynamicTemplateData = new
            {
                username = user.UserName,
                emailCopy = partnerMessage,
                subject = subject,
            };

            await emailManager.SendTemplatedEmailAsync(partnerSubject, SendGridEmailTemplateId.GenericEmail, SendGridEmailGroupId.EventRelated, dynamicTemplateData, partnerRecipients, CancellationToken.None).ConfigureAwait(false);

            return updatedEventPartner;
        }

        public async Task<IEnumerable<DisplayEventPartner>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var displayEventPartners = new List<DisplayEventPartner>();
            var currentPartners = await GetCurrentPartnersAsync(eventId, cancellationToken).ConfigureAwait(false);
            var possiblePartners = await GetPotentialPartnerLocationsAsync(eventId, cancellationToken).ConfigureAwait(false);

            // Convert the current list of partners for the event to a display partner (reduces round trips)
            foreach (var cp in currentPartners.ToList())
            {
                var displayEventPartner = new DisplayEventPartner
                {
                    EventId = eventId,
                    PartnerId = cp.PartnerId,
                    PartnerLocationId = cp.PartnerLocationId,
                    EventPartnerStatusId = cp.EventPartnerStatusId,
                };

                var partner = await partnerRepository.GetAsync(cp.PartnerId, cancellationToken).ConfigureAwait(false);
                displayEventPartner.PartnerName = partner.Name;

                var partnerLocation = partnerLocationRepository.Get(pl => pl.PartnerId == cp.PartnerId && pl.Id == cp.PartnerLocationId).FirstOrDefault();

                displayEventPartner.PartnerLocationName = partnerLocation.Name;
                displayEventPartner.PartnerLocationNotes = partnerLocation.PublicNotes;

                displayEventPartners.Add(displayEventPartner);
            }

            // Convert the current list of possible partners for the event to a display partner unless the partner location is already included (reduces round trips)
            foreach (var pp in possiblePartners.ToList())
            {
                if (!displayEventPartners.Any(ep => ep.PartnerLocationId == pp.Id))
                {
                    var displayEventPartner = new DisplayEventPartner
                    {
                        EventId = eventId,
                        PartnerId = pp.PartnerId,
                        PartnerLocationId = pp.Id,
                        EventPartnerStatusId = (int)EventPartnerStatusEnum.None,
                        PartnerLocationName = pp.Name,
                        PartnerLocationNotes = pp.PublicNotes,
                    };

                    var partner = await partnerRepository.GetAsync(pp.PartnerId, cancellationToken).ConfigureAwait(false);
                    displayEventPartner.PartnerName = partner.Name;

                    displayEventPartners.Add(displayEventPartner);
                }
            }

            return displayEventPartners;
        }

        public async Task<IEnumerable<EventPartner>> GetCurrentPartnersAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var eventPartners = await Repository.Get(ea => ea.EventId == eventId).ToListAsync(cancellationToken).ConfigureAwait(false);

            return eventPartners;
        }

        public async Task<IEnumerable<PartnerLocation>> GetPotentialPartnerLocationsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var mobEvent = await eventRepository.GetAsync(eventId, cancellationToken);

            // Simple match on postal code or city first. Radius later
            var partnerLocations = partnerLocationRepository.Get(pl => pl.IsActive && pl.Partner.PartnerStatusId == (int)PartnerStatusEnum.Active &&
                        (pl.PostalCode == mobEvent.PostalCode || pl.City == mobEvent.City));

            return partnerLocations;
        }
    }
}
