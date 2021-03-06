﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
    public class IfrsConceptsController : FactGetByIdControllerBase<IfrsConcept, int>
    {
        public const string BASE_ADDRESS = "ifrs-concepts";

        private readonly IfrsConceptsService _service;

        public IfrsConceptsController(IfrsConceptsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<IfrsConcept, int> GetFactGetByIdService()
        {
            return _service;
        }
    }

    public class IfrsConceptsService : FactGetByIdServiceBase<IfrsConcept, int>
    {
        private string View => IfrsConceptsController.BASE_ADDRESS;

        private readonly ApplicationRepository _repo;

        public IfrsConceptsService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<IfrsConcept> Search(Query<IfrsConcept> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var labelProp = nameof(IfrsConcept.Label);
                var label2Prop = nameof(IfrsConcept.Label2);
                var label3Prop = nameof(IfrsConcept.Label3);

                // Prepare the filter string
                var filterString = $"{labelProp} contains '{search}' or {label2Prop} contains '{search}' or {label3Prop} contains '{search}'";

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
