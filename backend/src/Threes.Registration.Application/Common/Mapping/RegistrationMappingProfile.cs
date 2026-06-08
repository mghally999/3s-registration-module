using AutoMapper;
using Threes.Registration.Application.Registrations.Queries.GetRegistrationById;
using Threes.Registration.Domain.Registrations;

namespace Threes.Registration.Application.Common.Mapping;

// automapper profile for turning the domain aggregate into the read dto. every
// value object is unwrapped to its primitive here. the governorate/city names
// are left for the query handler to fill from the lookup cache, so they are
// explicitly ignored to keep the "AssertConfigurationIsValid" test happy.
public sealed class RegistrationMappingProfile : Profile
{
    public RegistrationMappingProfile()
    {
        CreateMap<RegistrationAggregate, RegistrationDetailsDto>()
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Value))
            .ForMember(d => d.MiddleName, o => o.MapFrom(s => s.MiddleName != null ? s.MiddleName.Value : null))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Value))
            .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate.Value))
            .ForMember(d => d.MobileNumber, o => o.MapFrom(s => s.Mobile.Value))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Value))
            .ForMember(d => d.Addresses, o => o.MapFrom(s => s.Addresses));

        CreateMap<Address, AddressDetailsDto>()
            .ForMember(d => d.Street, o => o.MapFrom(s => s.Street.Value))
            .ForMember(d => d.BuildingNumber, o => o.MapFrom(s => s.BuildingNumber.Value))
            .ForMember(d => d.FlatNumber, o => o.MapFrom(s => s.FlatNumber.Value))
            .ForMember(d => d.GovernorateName, o => o.Ignore())
            .ForMember(d => d.CityName, o => o.Ignore());
    }
}
