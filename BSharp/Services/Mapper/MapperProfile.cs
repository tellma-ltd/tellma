using AutoMapper;
using BSharp.Controllers.DTO;
using System.Linq;
using System.Linq.Expressions;
using M = BSharp.Data.Model;

namespace BSharp.Services.Mapper
{
    public class MapperProfile : Profile
    {
        private static readonly char[] SEPARATOR = new char[] { '|' };
        public MapperProfile()
        {
            // NOTES...
            // For every save-able entity we have 3 mappings:
            // 1 - Mapping from DtoForSave to Model
            // 2 - Mapping from Model to Dto
            // 3 - Mapping from Model to DtoForSaveBase
            // The 3rd mapping is to support a scenario where we want to map a model object of an unkown type
            // to its corresponding DTO, with the following syntax _mapper.Map<DtoForSaveBase>(model)

            // By default automapper maps null collection properties to empty collections (https://bit.ly/2WgruTD)
            // here we disabled this behavior, i.e a null maps to a null
            AllowNullCollections = true;

            // Measurement Units
            CreateDtoMap2<MeasurementUnitForQuery, MeasurementUnit>();

            // Agents
            CreateDtoMap2<AgentForQuery, Agent>();

            // Local Users
            CreateDtoMap2<LocalUserForQuery, LocalUser>();

            // Role Membership
            CreateDtoMap2<RoleMembershipForQuery, RoleMembership>();

            // Roles
            CreateDtoMap2<RoleForQuery, Role>();

            // Permissions
            CreateDtoMap2<PermissionForQuery, Permission>();

            // Views
            CreateDtoMap2<ViewForQuery, View>();

            // Translations
            CreateDtoMap2<TranslationForQuery, Translation>();

            // IFRS Notes
            CreateDtoMap2<IfrsNoteForQuery, IfrsNote>();

            // IFRS Notes
            CreateDtoMap2<ProductCategoryForQuery, ProductCategory>();

            //CreateMap<TranslationForSave, M.Translation>()
            //    .ForMember(e => e.Culture, opt => opt.MapFrom(e => e.Id == null ? null : e.Id.Split(SEPARATOR)[0]))
            //    .ForMember(e => e.Name, opt => opt.MapFrom(e => e.Id == null ? null : string.Join("|", e.Id.Split(SEPARATOR).Skip(1)) ));

            //CreateDtoMap<M.Translation, Translation>()
            //    .ForMember(e => e.Id, opt => opt.MapFrom(e => $"{e.Culture}|{e.Name}"));

            // Settings
            CreateMap<SettingsForSave, M.Settings>();
            CreateDtoMap<M.Settings, Settings>();
            CreateMap<M.Settings, SettingsForClient>();

            // Global Settings
            CreateMap<GlobalSettingsForSave, M.GlobalSettings>();
            CreateDtoMap<M.GlobalSettings, Controllers.DTO.GlobalSettings>();
            CreateMap<M.GlobalSettings, GlobalSettingsForClient>();

            // Cultures
            CreateDtoMap2<CultureForQuery, Culture>();

            // Tenants
            CreateMap<M.Tenant, TenantForClient>();
        }

        /// <summary>
        /// Syntactic sugar, maps the model type to both the DTO and <see cref="DtoBase"/>
        /// </summary>
        private IMappingExpression<TModel, TDto> CreateDtoMap<TModel, TDto>()
            where TDto : DtoBase where TModel : M.ModelBase
        {
            CreateMap<TModel, DtoBase>()
                .ConstructUsing((model, ctx) => ctx.Mapper.Map<TDto>(model));

            return CreateMap<TModel, TDto>();
        }

        private IMappingExpression<TDtoForQuery, TDto> CreateDtoMap2<TDtoForQuery, TDto>()
            where TDto : DtoBase where TDtoForQuery : DtoBase
        {
            CreateMap<TDtoForQuery, DtoBase>()
                .ConstructUsing((model, ctx) => ctx.Mapper.Map<TDto>(model));

            return CreateMap<TDtoForQuery, TDto>();
        }
    }
}
