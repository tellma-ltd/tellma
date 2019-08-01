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
     //   [Fact]
        public async Task Test()
        {
            Func<Type, string> sources = (t) =>
            {
                switch (t.Name)
                {
                    case nameof(MeasurementUnit):
                        return "[dbo].[MeasurementUnits]";

                    case nameof(Role):
                        return "[dbo].[Roles]";

                    case nameof(Agent):
                        return "[dbo].[Custodies]";

                    case nameof(RoleMembership):
                        return "[dbo].[RoleMemberships]";

                    case nameof(LocalUser):
                        return "[dbo].[LocalUsers]";

                    case nameof(RequiredSignature):
                        return "(SELECT * FROM [dbo].[Permissions] WHERE Level = 'Sign')";

                    case nameof(Permission):
                        return "(SELECT * FROM [dbo].[Permissions] WHERE Level <> 'Sign')";
                }
                return null;
            };

            var query = new ODataQuery<LocalUser>(null, sources, new MockLocalizer(), 0, TimeZoneInfo.Local);

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
