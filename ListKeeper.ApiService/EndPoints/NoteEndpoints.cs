using System.Net;
using ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeperWebApi.WebApi.Endpoints
{
    public static class NoteEndpoints
    {
        public static RouteGroupBuilder MapNoteApiEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllNotes)
                 .WithName("GetAllNotes")
                 .WithDescription("Gets all notes")
                 .RequireAuthorization("Admin");

            group.MapPost("/search", GetAllNotesBySearchCriteria)
                 .WithName("GetAllNotesBySearchCriteria")
                 .WithDescription("Gets notes by search criteria")
                 .RequireAuthorization("Admin");

            group.MapGet("/{id:int}", GetNoteById)
                 .WithName("GetNoteById")
                 .WithDescription("Gets a note by ID")
                 .RequireAuthorization("Admin");

            group.MapPost("/", CreateNote)
                 .WithName("CreateNote")
                 .WithDescription("Creates a new note")
                 .RequireAuthorization("Admin");

            group.MapPut("/{id:int}", UpdateNote)
                 .WithName("UpdateNote")
                 .WithDescription("Updates an existing note")
                 .RequireAuthorization("Admin");

            group.MapDelete("/{id:int}", DeleteNote)
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

        private static async Task<IResult> GetNoteById(
            int id,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Getting note with ID: {Id}", id);
                var note = await noteService.GetNoteByIdAsync(id);
                
                if (note == null)
                {
                    return Results.NotFound($"Note with ID {id} not found");
                }
                
                return Results.Ok(note);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving note with ID: {Id}", id);
                return Results.Problem("An error occurred while retrieving the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> CreateNote(
            [FromBody] NoteViewModel noteViewModel,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Creating new note");
                
                if (noteViewModel == null)
                {
                    return Results.BadRequest("Note data is required");
                }

                var createdNote = await noteService.CreateNoteAsync(noteViewModel);
                
                if (createdNote == null)
                {
                    return Results.Problem("Failed to create note", statusCode: (int)HttpStatusCode.InternalServerError);
                }
                
                return Results.Created($"/api/notes/{createdNote.Id}", createdNote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating note");
                return Results.Problem("An error occurred while creating the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> UpdateNote(
            int id,
            [FromBody] NoteViewModel noteViewModel,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Updating note with ID: {Id}", id);
                
                if (noteViewModel == null)
                {
                    return Results.BadRequest("Note data is required");
                }

                if (id != noteViewModel.Id)
                {
                    return Results.BadRequest("ID in URL does not match ID in note data");
                }

                var updatedNote = await noteService.UpdateNoteAsync(noteViewModel);
                
                if (updatedNote == null)
                {
                    return Results.NotFound($"Note with ID {id} not found");
                }
                
                return Results.Ok(updatedNote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating note with ID: {Id}", id);
                return Results.Problem("An error occurred while updating the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> DeleteNote(
            int id,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Deleting note with ID: {Id}", id);
                
                var result = await noteService.DeleteNoteAsync(id);
                
                if (!result)
                {
                    return Results.NotFound($"Note with ID {id} not found");
                }
                
                return Results.Ok(new { success = true, message = "Note deleted successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting note with ID: {Id}", id);
                return Results.Problem("An error occurred while deleting the note", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> GetAllNotesBySearchCriteria(
            [FromBody] SearchCriteriaViewModel searchCriteria,
            [FromServices] INoteService noteService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Getting notes by search criteria");
                
                if (searchCriteria == null)
                {
                    return Results.BadRequest("Search criteria is required");
                }

                var notes = await noteService.GetAllNotesBySearchCriteriaAsync(searchCriteria);
                return Results.Ok(new { notes });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving notes by search criteria");
                return Results.Problem("An error occurred while retrieving notes", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
