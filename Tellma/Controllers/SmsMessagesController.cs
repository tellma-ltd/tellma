using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class SmsMessagesController : FactGetByIdControllerBase<SmsMessageForQuery, int>
    {
        public const string BASE_ADDRESS = "sms-messages";

        private readonly SmsMessagesService _service;

        public SmsMessagesController(SmsMessagesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<SmsMessageForQuery, int> GetFactGetByIdService()
        {
            return _service;
        }
    }

    public class SmsMessagesService : FactGetByIdServiceBase<SmsMessageForQuery, int>
    {
        private static readonly PhoneAttribute phoneAtt = new PhoneAttribute();
        private string View => SmsMessagesController.BASE_ADDRESS;

        private readonly ApplicationRepository _repo;

        public SmsMessagesService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<SmsMessageForQuery> Search(Query<SmsMessageForQuery> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                // Prepare the filter string
                var message = nameof(SmsMessageForQuery.Message);
                var filterString = $"{message} contains '{search}'";

                // If the search term looks like a phone number, include the contact mobile in the search
                if (phoneAtt.IsValid(search))
                {
                    var e164 = ControllerUtilities.ToE164(search);
                    var toPhone = nameof(SmsMessageForQuery.ToPhoneNumber);

                    filterString += $" or {toPhone} startsw '{e164}'";
                }

                // Apply the filter
                query = query.Filter(filterString);
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }
    }
}
