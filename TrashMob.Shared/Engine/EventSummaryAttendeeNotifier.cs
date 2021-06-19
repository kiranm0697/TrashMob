﻿
namespace TrashMob.Shared.Engine
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TrashMob.Shared.Persistence;

    public class EventSummaryAttendeeNotifier : NotificationEngineBase, INotificationEngine
    {
        public EventSummaryAttendeeNotifier(IEventRepository eventRepository, IUserRepository userRepository, IEventAttendeeRepository eventAttendeeRepository, IEmailSender emailSender) : 
            base(eventRepository, userRepository, eventAttendeeRepository, emailSender)
        {
        }

        public Task GenerateNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
