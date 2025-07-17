using System.Net;
using ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListKeeperWebApi.WebApi.Endpoints
{
    public static class NoteEndpoints
    {
        public static RouteGroupBuilder MapNoteApiEndpoints(this RouteGroupBuilder group)
        {
            // The endpoint mapping remains the same.
            group.MapGet("/", GetAllNotes)
                 .WithName("GetAllNotes")
                 .WithDescription("Gets all notes")
                 .RequireAuthorization("Admin");

            return group;
        }


        private static async Task<IResult> GetAllNotes(
            // The [FromServices] attribute tells the API to get these objects
            // from the services container, not the request body. This is the fix.
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Getting all notes");
                var notes = await noteService.GetAllNotesAsync();
                return Results.Ok(new { notes });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all notes");
                return Results.Problem("An error occurred while retrieving notes", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
