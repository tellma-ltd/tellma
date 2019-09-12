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
    // Here I add all the readonly controllers we need for the JV

    public static class TempUtil
    {
        public static IEnumerable<AbstractPermission> UserPermissions(string view)
        {
            yield return new AbstractPermission { Action = "All", ViewId = view, };
        }
    }


    [Route("api/resource-picks")]
    [ApplicationApi]
    public class ResourcePicksController : ReadEntitiesControllerBase<ResourcePick, int>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "resource-picks";

        public ResourcePicksController(
            ILogger<ResourcePicksController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return Task.FromResult(TempUtil.UserPermissions(VIEW));
        }

        protected override Query<ResourcePick> Search(Query<ResourcePick> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var code = nameof(ResourcePick.Code);

                query = query.Filter($"{code} {Ops.contains} '{search}'");
            }

            return query;
        }
    }


    [Route("api/resources")]
    [ApplicationApi]
    public class ResourcesController : ReadEntitiesControllerBase<Resource, int>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "resources";

        public ResourcesController(
            ILogger<ResourcesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return Task.FromResult(TempUtil.UserPermissions(VIEW));
        }

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Resource.Name);
                var name2 = nameof(Resource.Name2);
                var name3 = nameof(Resource.Name3);
                var code = nameof(Resource.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }
    }

    [Route("api/responsibility-centers")]
    [ApplicationApi]
    public class ResponsibilityCentersController : ReadEntitiesControllerBase<ResponsibilityCenter, int>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "responsibility-centers";

        public ResponsibilityCentersController(
            ILogger<ResponsibilityCentersController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return Task.FromResult(TempUtil.UserPermissions(VIEW));
        }

        protected override Query<ResponsibilityCenter> Search(Query<ResponsibilityCenter> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(ResponsibilityCenter.Name);
                var name2 = nameof(ResponsibilityCenter.Name2);
                var name3 = nameof(ResponsibilityCenter.Name3);
                var code = nameof(ResponsibilityCenter.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }
    }
}
