using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class LegacyTypesController : FactWithIdControllerBase<LegacyType, int>
    {
        public const string BASE_ADDRESS = "legacy-types";

        private readonly ApplicationRepository _repo;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        private string View => BASE_ADDRESS;

        public LegacyTypesController(
            ILogger<LegacyTypesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<LegacyType> Search(Query<LegacyType> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(LegacyType.Name);
                var name2 = nameof(LegacyType.Name2);
                var name3 = nameof(LegacyType.Name3);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}'");
            }

            return query;
        }
    }
}