using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController(allowUnobtrusive: true)]
    public class IfrsConceptsController : FactGetByIdControllerBase<IfrsConcept, int>
    {
        public const string BASE_ADDRESS = "ifrs-concepts";
        private string View => BASE_ADDRESS;

        private readonly ApplicationRepository _repo;

        public IfrsConceptsController(
            ApplicationRepository repo,
            ILogger<IfrsConceptsController> logger,
            IStringLocalizer<Strings> localizer) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<IfrsConcept> Search(Query<IfrsConcept> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var labelProp = nameof(IfrsConcept.Label);
                var label2Prop = nameof(IfrsConcept.Label2);
                var label3Prop = nameof(IfrsConcept.Label3);

                // Prepare the filter string
                var filterString = $"{labelProp} {Ops.contains} '{search}' or {label2Prop} {Ops.contains} '{search}' or {label3Prop} {Ops.contains} '{search}'";

                // Apply the filter
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _repo.UserPermissions(action, View);
        }
    }
}
