using AutoMapper;
using BSharp.Controllers.DTO;
using M = BSharp.Data.Model;

namespace BSharp.Services.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // For every save-able entity we have 3 mappings:
            // 1 - Mapping from DtoForSave to Model
            // 2 - Mapping from Model to Dto
            // 3 - Mapping from Model to DtoForSaveBase
            // The 3rd mapping is to support a scenario where we want to map a model object of an unkown type
            // to its corresponding DTO, with the following syntax _mapper.Map<DtoForSaveBase>(model)

            // Measurement Units
            CreateMap<MeasurementUnitForSave, M.MeasurementUnit>();
            CreateDtoMap<M.MeasurementUnit, MeasurementUnit>();
        }

        /// <summary>
        /// Syntactic sugar, maps the model type to both the DTO and the DTO base
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        private IMappingExpression<TModel, TDto> CreateDtoMap<TModel, TDto>() 
            where TDto : DtoForSaveBase where TModel : M.ModelForSaveBase
        {
            CreateMap<TModel, DtoForSaveBase>()
                .ConstructUsing((model, ctx) => ctx.Mapper.Map<TDto>(model));

            return CreateMap<TModel, TDto>();
        }
    }
}
