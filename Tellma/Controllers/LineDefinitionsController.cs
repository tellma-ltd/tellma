using Microsoft.AspNetCore.Mvc;
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
    public class LineDefinitionsController : FactGetByIdControllerBase<LineDefinition, int>
    {
        public const string BASE_ADDRESS = "line-definitions";

        private readonly LineDefinitionsService _service;

        public LineDefinitionsController(LineDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactGetByIdServiceBase<LineDefinition, int> GetFactGetByIdService()
        {
            return _service;
        }
    }


    public class LineDefinitionsService : FactGetByIdServiceBase<LineDefinition, int>
    {
        private string View => LineDefinitionsController.BASE_ADDRESS;

        private readonly ApplicationRepository _repo;

        public LineDefinitionsService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<LineDefinition> Search(Query<LineDefinition> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(LineDefinition.TitlePlural);
                var titleP2 = nameof(LineDefinition.TitlePlural2);
                var titleP3 = nameof(LineDefinition.TitlePlural3);

                var titleS = nameof(LineDefinition.TitleSingular);
                var titleS2 = nameof(LineDefinition.TitleSingular2);
                var titleS3 = nameof(LineDefinition.TitleSingular3);
                var code = nameof(LineDefinition.Code);

                query = query.Filter($"{titleS} {Ops.contains} '{search}' or {titleS2} {Ops.contains} '{search}' or {titleS3} {Ops.contains} '{search}' or {titleP} {Ops.contains} '{search}' or {titleP2} {Ops.contains} '{search}' or {titleP3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }
    }
}
