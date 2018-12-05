using AutoMapper;
using BSharp.Controllers.Application.DTO;
using BSharp.Controllers.Shared;
using M = BSharp.Data.Model.Application;

namespace BSharp.Controllers.Application.Mapper
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            // For every type we have 3 mappings:
            // 1 - Mapping from DtoForSave to Model
            // 2 - Mapping from Model to Dto
            // 3 - Mapping from Model to Dto

            // Measurement Units
            CreateMap<MeasurementUnitForSave, M.MeasurementUnit>();
            CreateDtoMap<M.MeasurementUnit, MeasurementUnit>();
        }

        /// <summary>
        /// Syntaxtic sugar, maps the model type to both the DTO and the DTO base
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
