using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
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
    public class AccountTypesController : FactTreeControllerBase<AccountType, string>
    {
        public const string BASE_ADDRESS = "account-types";

        private readonly ILogger _logger;
        private readonly ApplicationRepository _repo;

        private string ViewId => BASE_ADDRESS;

        public AccountTypesController(
            ILogger<AccountTypesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, ViewId);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<AccountType> Search(Query<AccountType> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(AccountType.Name);
                var name2 = nameof(AccountType.Name2);
                var name3 = nameof(AccountType.Name3);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}
