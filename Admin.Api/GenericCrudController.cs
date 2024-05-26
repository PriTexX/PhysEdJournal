using System.Text.Json;
using Admin.Api.GetMany;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhysEdJournal.Infrastructure.Database;

namespace Admin.Api;

public sealed class ResourceOptions<T>
{
    public required string Name { get; init; }

    public bool IsDeletable { get; init; } = false;
    public bool IsCreatable { get; init; } = false;

    public string[]? SortFields { get; init; }
    public string[]? FilterFields { get; init; }

    public required IValidator<T> Validator { get; init; }
}

public sealed class GenericCrudController<TModel, TPrimaryKey>
    where TModel : class
{
    private static IValidator<TModel> _validator;

    public static void MapEndpoints(IEndpointRouteBuilder router, ResourceOptions<TModel> options)
    {
        var resourceRouter = router.MapGroup($"/{options.Name}").WithTags(options.Name);

        resourceRouter.MapGet("/{id}", GetOne).RequireAuthorization();

        resourceRouter
            .MapPost("/many", GetMany)
            .AddValidation(new GetManyRequestValidator(options.SortFields, options.FilterFields));

        if (options.IsDeletable)
        {
            resourceRouter.MapDelete("/{id}", Delete);
        }

        if (options.IsCreatable)
        {
            resourceRouter.MapPost("/", Create).AddValidation(options.Validator);
        }

        // Validation filter won't work for Update endpoint.
        // Filter requires validated object to be 1st argument,
        // but Update endpoint cannot allow this as it uses JsonPatchDocument
        // and has to deserialize it by itself through the use of HttpContext.
        //
        // So we set validator to static property that will be used later
        // by Update to validate object.
        _validator = options.Validator;
        resourceRouter.MapPatch("/{id}", Update);
    }

    private static async Task<IResult> GetOne(
        TPrimaryKey id,
        [FromServices] ApplicationContext dbCtx
    )
    {
        var entity = await dbCtx.Set<TModel>().FindAsync(id);

        if (entity is null)
        {
            return Results.NotFound();
        }

        return Results.Json(entity);
    }

    private static async Task<IResult> GetMany(
        [FromBody] GetManyRequest req,
        HttpContext httpCtx,
        [FromServices] ApplicationContext dbCtx
    )
    {
        IQueryable<TModel> query = dbCtx.Set<TModel>();
        IQueryable<TModel> totalNumberQuery = dbCtx.Set<TModel>();

        if (req.Filters is not null && req.Filters.Count > 0)
        {
            query = Filter.Apply(query, req.Filters);
            totalNumberQuery = Filter.Apply(totalNumberQuery, req.Filters);
        }

        if (req.Sorters is not null && req.Sorters.Count > 0)
        {
            query = Sort.Apply(query, req.Sorters);
        }

        var data = await query.Skip(req.Offset).Take(req.Limit).ToListAsync();

        var total = await totalNumberQuery.CountAsync();

        return Results.Json(new { data, total });
    }

    private static async Task<IResult> Delete(
        TPrimaryKey id,
        [FromServices] ApplicationContext dbCtx
    )
    {
        var entity = await dbCtx.Set<TModel>().FindAsync(id);

        if (entity is null)
        {
            return Results.NotFound();
        }

        dbCtx.Set<TModel>().Remove(entity);

        await dbCtx.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> Create(
        TModel entity,
        [FromServices] ApplicationContext dbCtx
    )
    {
        dbCtx.Set<TModel>().Add(entity);

        await dbCtx.SaveChangesAsync();

        return Results.Created();
    }

    private static async Task<IResult> Update(
        TPrimaryKey id,
        [FromBody] Dictionary<string, JsonElement> data,
        [FromServices] ApplicationContext dbCtx
    )
    {
        var updates = BuildJsonPatchDocument(data);

        var dbEntity = await dbCtx.Set<TModel>().FindAsync(id);

        if (dbEntity is null)
        {
            return Results.NotFound();
        }

        updates.ApplyTo(dbEntity);

        var validation = _validator.Validate(dbEntity);

        if (!validation.IsValid)
        {
            return Results.ValidationProblem(
                validation.ToDictionary(),
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        await dbCtx.SaveChangesAsync();

        return Results.Ok(dbEntity);
    }

    private static JsonPatchDocument<TModel> BuildJsonPatchDocument(
        Dictionary<string, JsonElement> data
    )
    {
        var jsonPatchDocument = new JsonPatchDocument<TModel>();

        foreach (var kv in data)
        {
            jsonPatchDocument.Operations.Add(
                new Operation<TModel>("add", $"/{kv.Key}", null, UnpackJsonElement.Unpack(kv.Value))
            );
        }

        return jsonPatchDocument;
    }
}
