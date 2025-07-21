using ListKeeper.ApiService.Models.ViewModels;
using ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services;
using ListKeeperWebApi.WebApi.Models.ViewModels;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

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

            // DO THIS OVER THE WEEKEND - CREATE, UPDATE, DELETE

            group.MapGet("/{noteId}", GetNote)
                 .WithName("GetNote")
                 .WithDescription("Gets a note by their ID")
                 .RequireAuthorization("Admin");

            group.MapPost("/", CreateNote)
                 .WithName("CreateNote")
                 .WithDescription("Creates a new note")
                 .RequireAuthorization("Admin");

            group.MapPut("/{noteId}", UpdateNote)
                 .WithName("UpdateNote")
                 .WithDescription("Updates an existing note")
                 .RequireAuthorization("Admin");

            group.MapDelete("/{noteId}", DeleteNote)
                 .WithName("DeleteNote")
                 .WithDescription("Deletes a note")
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

        private static async Task<IResult> GetNote(
            int noteId,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetNote");
            try
            {
                var note = await noteService.GetNoteByIdAsync(noteId);
                if (note == null)
                {
                    return Results.NotFound($"Note with ID {noteId} not found");
                }
                return Results.Ok(note);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving note with ID {UserId}", noteId);
                return Results.Problem("An error occurred while retrieving the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> CreateNote(
            NoteViewModel noteVM, // 'noteVM' correctly comes from the request body, so it does NOT get [FromServices]
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CreateNote");
            try
            {
                var newNote = await noteService.CreateNoteAsync(noteVM);
                if (newNote == null)
                {
                    return Results.Conflict($"Could not create note. Title {noteVM.Title} may already be in use.");
                }
                return Results.Created($"/api/notes/{newNote.Id}", newNote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating note with title {Title}", noteVM?.Title);
                return Results.Problem("An error occurred while creating the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        // --- NOTE: Apply the [FromServices] fix to ALL your handlers ---

        private static async Task<IResult> UpdateNote(
            int noteId,
            NoteViewModel noteVM,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UpdateNote");
            try
            {
                noteVM.Id = noteId;
                var updatedNote = await noteService.UpdateNoteAsync(noteVM);
                if (updatedNote == null)
                {
                    return Results.NotFound($"Note with ID {noteId} not found");
                }
                return Results.Ok(updatedNote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating note with ID {NoteId}", noteId);
                return Results.Problem("An error occurred while updating the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> DeleteNote(
            int noteId,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("DeleteNote");
            try
            {
                var success = await noteService.DeleteNoteAsync(noteId);
                if (!success)
                {
                    return Results.NotFound($"Note with ID {noteId} not found");
                }
                return Results.Ok(new { status = "200", result = $"note: {noteId} deleted" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting note with ID {NoteId}", noteId);
                return Results.Problem("An error occurred while deleting the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
