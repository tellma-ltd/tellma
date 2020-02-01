using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class DetailsEntriesController : FactWithIdControllerBase<DetailsEntry, int>
    {
        public const string BASE_ADDRESS = "details-entries";

        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public DetailsEntriesController(
            ILogger<DetailsEntriesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return new ParameteredRepository<DetailsEntry>(_repo,
                ("@CountUnitId", GetInt("CountUnitId", false)),
                ("@MassUnitId", GetInt("MassUnitId", false)),
                ("@VolumeUnitId", GetInt("VolumeUnitId", false))
            );
        }

        private int? GetInt(string key, bool isRequired)
        {
            int? value = null;
            if (Request.Query.ContainsKey(key))
            {
                string dateString = Request.Query[key].FirstOrDefault();
                try
                {
                    value = int.Parse(dateString);
                }
                catch
                {
                    throw new BadRequestException($"Failed to convert {key}: '{dateString}' to a valid DateTime value");
                }
            }
            else if (isRequired)
            {
                throw new BadRequestException($"The parameter {key} is required");
            }

            return value;
        }

        protected override Query<DetailsEntry> Search(Query<DetailsEntry> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(DetailsEntry.AccountId));
        }
    }
}
