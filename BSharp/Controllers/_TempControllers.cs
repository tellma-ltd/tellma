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


    [Route("api/accounts")]
    [ApplicationApi]
    public class AccountsController : FactGetByIdControllerBase<Account, int>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "accounts";

        public AccountsController(
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

        protected override Query<Account> Search(Query<Account> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Account.Name);
                var name2 = nameof(Account.Name2);
                var name3 = nameof(Account.Name3);
                var code = nameof(Account.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }
    }


    [Route("api/ifrs-entry-classifications")]
    [ApplicationApi]
    public class IfrsEntryClassificationsController : FactGetByIdControllerBase<IfrsEntryClassification, string>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "ifrs-entry-classifications";

        public IfrsEntryClassificationsController(
            ILogger<IfrsEntryClassification> logger,
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

        protected override Query<IfrsEntryClassification> Search(Query<IfrsEntryClassification> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var label = nameof(IfrsEntryClassification.Label);
                var label2 = nameof(IfrsEntryClassification.Label2);
                var label3 = nameof(IfrsEntryClassification.Label3);

                query = query.Filter($"{label} {Ops.contains} '{search}' or {label2} {Ops.contains} '{search}' or {label3} {Ops.contains} '{search}'");
            }

            return query;
        }
    }



    [Route("api/ifrs-account-classifications")]
    [ApplicationApi]
    public class IfrsAccountClassificationsController : FactGetByIdControllerBase<IfrsAccountClassification, string>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "ifrs-account-classifications";

        public IfrsAccountClassificationsController(
            ILogger<IfrsAccountClassificationsController> logger,
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

        protected override Query<IfrsAccountClassification> Search(Query<IfrsAccountClassification> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var label = nameof(IfrsAccountClassification.Label);
                var label2 = nameof(IfrsAccountClassification.Label2);
                var label3 = nameof(IfrsAccountClassification.Label3);

                query = query.Filter($"{label} {Ops.contains} '{search}' or {label2} {Ops.contains} '{search}' or {label3} {Ops.contains} '{search}'");
            }

            return query;
        }
    }


    [Route("api/voucher-booklets")]
    [ApplicationApi]
    public class VoucherBookletsController : FactGetByIdControllerBase<VoucherBooklet, int>
    {
        private readonly ApplicationRepository _repo;

        private string VIEW => "voucher-booklets";

        public VoucherBookletsController(
            ILogger<VoucherBookletsController> logger,
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

        protected override Query<VoucherBooklet> Search(Query<VoucherBooklet> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var stringPrefix = nameof(VoucherBooklet.StringPrefix); // TODO: Search the 

                query = query.Filter($"{stringPrefix} {Ops.contains} '{search}'");
            }

            return query;
        }
    }

    [Route("api/resource-picks")]
    [ApplicationApi]
    public class ResourcePicksController : FactGetByIdControllerBase<ResourcePick, int>
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


    [Route("api/responsibility-centers")]
    [ApplicationApi]
    public class ResponsibilityCentersController : FactGetByIdControllerBase<ResponsibilityCenter, int>
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
