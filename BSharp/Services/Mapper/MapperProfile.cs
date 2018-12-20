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

            // Measurement Units
            CreateMap<MeasurementUnitForSave, M.MeasurementUnit>();
            CreateDtoMap<M.MeasurementUnit, MeasurementUnit>();

            // Translations
            CreateMap<TranslationForSave, M.Translation>()
                .ForMember(e => e.Culture, opt => opt.MapFrom(e => e.Id == null ? null : e.Id.Split(SEPARATOR)[0]))
                .ForMember(e => e.Name, opt => opt.MapFrom(e => e.Id == null ? null : string.Join("|", e.Id.Split(SEPARATOR).Skip(1)) ));

            CreateDtoMap<M.Translation, Translation>()
                .ForMember(e => e.Id, opt => opt.MapFrom(e => $"{e.Culture}|{e.Name}"));
        }

        /// <summary>
        /// Syntactic sugar, maps the model type to both the DTO and <see cref="DtoForSaveBase"/>
        /// </summary>
        private IMappingExpression<TModel, TDto> CreateDtoMap<TModel, TDto>()
            where TDto : DtoForSaveBase where TModel : M.ModelForSaveBase
        {
            CreateMap<TModel, DtoForSaveBase>()
                .ConstructUsing((model, ctx) => ctx.Mapper.Map<TDto>(model));

            return CreateMap<TModel, TDto>();
        }
    }
}
