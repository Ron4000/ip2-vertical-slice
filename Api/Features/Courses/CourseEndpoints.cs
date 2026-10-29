namespace Api.Features.Courses;

using Api.Common.Extensions;
using Application.Abstractions.Messaging;
using Application.Features.Courses.CreateCourse;
using Application.Features.Courses.DeleteCourse;//inserted  lab05
using Application.Features.Courses.GetAllCourses;
using Application.Features.Courses.GetCourseById;
using Application.Features.Courses.UpdateCourse;
using Domain.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



public static class CourseEndpoints
{
    // 1. Declare the route name constant
    private const string GetByIdRouteName = "GetCourseById";

    //inserted lab04
    private static async Task<IResult> Update(
    Guid id,
    UpdateCourseRequest request,
    ICommandHandler<UpdateCourseCommand, Result<UpdateCourseResponse>> handler,
    CancellationToken cancellationToken)
    {
        var command = new UpdateCourseCommand(id, request.Title, request.Code, request.Credits);
        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(
            onSuccess: value => TypedResults.Ok(value),
            onFailure: _ => result.ToProblemDetails());
    }

    //inserted lab05
    private static async Task<IResult> Delete(
    Guid id,
    ICommandHandler<DeleteCourseCommand, Result> handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteCourseCommand(id), cancellationToken);

        return result.Match(
            onSuccess: () => TypedResults.NoContent(),
            onFailure: _ => result.ToProblemDetails());
    }

    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/courses")
            .WithTags("Courses");

        //inserted lab04
         group.MapPut("/{id:guid}", Update)
        .WithName("UpdateCourse")
        .WithSummary("Update an existing course");

        group.MapGet("/", GetAll)
            .WithName("GetAllCourses")
            .WithSummary("Get all courses");

        group.MapPost("/", Create)
            .WithName("CreateCourse")
            .WithSummary("Create a new course");

        group.MapGet("/{id:guid}", GetById)
             .WithName(GetByIdRouteName)
             .WithSummary("Get course by ID");

        //inserted lab05
        group.MapDelete("/{id:guid}", Delete)
       .WithName("DeleteCourse")
       .WithSummary("Delete a course");
    }

    private static async Task<IResult> GetAll(
        IQueryHandler<GetAllCoursesQuery, Result<GetAllCoursesResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllCoursesQuery();
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateCourseCommand command,
        ICommandHandler<CreateCourseCommand, Result<CreateCourseResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, GetByIdRouteName, new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetCourseByIdQuery, Result<GetCourseByIdResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(id);
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }
}

//inserted lab04
public sealed record UpdateCourseRequest(string Title, string Code, int Credits);
