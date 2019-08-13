using AutoMapper;
using BSharp.Controllers.Dto;
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

            // Settings
            CreateMap<SettingsForSave, M.Settings>();
            CreateDtoMap<M.Settings, Settings>();
            CreateMap<M.Settings, SettingsForClient>();

            // Global Settings
            CreateMap<GlobalSettingsForSave, M.GlobalSettings>();
            CreateDtoMap<M.GlobalSettings, Controllers.Dto.GlobalSettings>();
            CreateMap<M.GlobalSettings, GlobalSettingsForClient>();

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
