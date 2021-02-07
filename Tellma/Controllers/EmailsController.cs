using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class EmailsController : FactGetByIdControllerBase<EmailForQuery, int>
    {
        public const string BASE_ADDRESS = "emails";

        private readonly EmailsService _service;

        public EmailsController(EmailsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<EmailForQuery, int> GetFactGetByIdService()
        {
            return _service;
        }
    }

    public class EmailsService : FactGetByIdServiceBase<EmailForQuery, int>
    {
        private string View => EmailsController.BASE_ADDRESS;

        private readonly ApplicationRepository _repo;

        public EmailsService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<EmailForQuery> Search(Query<EmailForQuery> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var toEmail = nameof(EmailForQuery.ToEmail);
                var subject = nameof(EmailForQuery.Subject);

                // Prepare the filter string
                var filterString = $"{toEmail} contains '{search}' or {subject} contains '{search}'";

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }
    }
}
