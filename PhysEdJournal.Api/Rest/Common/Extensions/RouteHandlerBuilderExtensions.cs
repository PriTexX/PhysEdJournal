using FluentValidation;

namespace PhysEdJournal.Api.Rest.Common.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder AddValidation<TReq, TVal>(this RouteHandlerBuilder builder) where TVal : IValidator<TReq> 
    {
        return builder.AddEndpointFilter<ValidationFilter<TReq, TVal>>();
    }
}