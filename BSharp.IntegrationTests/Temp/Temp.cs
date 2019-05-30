using BSharp.Controllers.DTO;
using BSharp.Services.OData;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BSharp.IntegrationTests.Temp
{
    public class Temp
    {
        [Fact]
        public async Task Test()
        {
            Func<Type, string> sources = (t) =>
            {
                switch (t.Name)
                {
                    case nameof(MeasurementUnitForQuery):
                        return "[dbo].[MeasurementUnits]";

                    case nameof(RoleForQuery):
                        return "[dbo].[Roles]";

                    case nameof(AgentForQuery):
                        return "[dbo].[Custodies]";

                    case nameof(RoleMembershipForQuery):
                        return "[dbo].[RoleMemberships]";

                    case nameof(LocalUserForQuery):
                        return "[dbo].[LocalUsers]";

                    case nameof(RequiredSignatureForQuery):
                        return "(SELECT * FROM [dbo].[Permissions] WHERE Level = 'Sign')";

                    case nameof(PermissionForQuery):
                        return "(SELECT * FROM [dbo].[Permissions] WHERE Level <> 'Sign')";
                }
                return null;
            };

            var query = new ODataQuery<LocalUserForQuery, int?>(null, sources, new MockLocalizer(), 0, TimeZoneInfo.Local);

            query.Select("Roles/Memo");
            query.Filter("IsActive eq true");
            query.Expand("Roles");
            query.OrderBy("BaseAmount desc, Name");
            query.FilterByIds(3, 4, 7);
            query.Skip(0);
            query.Top(5);

            await query.CountAsync();

        }
    }


    public class MockLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new LocalizedString(name, name);

        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
