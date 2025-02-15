﻿namespace TrashMob.Shared.Managers.Interfaces
{
    using System.Threading.Tasks;
    using TrashMob.Models;

    public interface IPartnerManager
    {
        Task CreatePartnerAsync(PartnerRequest partnerRequest);
    }
}
