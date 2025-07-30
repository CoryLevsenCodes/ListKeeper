using System.Net;
using ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services;
using ListKeeper.ApiService.Helpers;
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
                 .RequireAuthorization();

            group.MapPost("/search", GetAllNotesBySearchCriteria)
                 .WithName("GetAllNotesBySearchCriteria")
                 .WithDescription("Gets notes by search criteria")
                 .RequireAuthorization();

            group.MapGet("/{id:int}", GetNoteById)
                 .WithName("GetNoteById")
                 .WithDescription("Gets a note by ID")
                 .RequireAuthorization();

            group.MapPost("/", CreateNote)
                 .WithName("CreateNote")
                 .WithDescription("Creates a new note")
                 .RequireAuthorization();

            group.MapPut("/{id:int}", UpdateNote)
                 .WithName("UpdateNote")
                 .WithDescription("Updates an existing note")
                 .RequireAuthorization();

            group.MapDelete("/{id:int}", DeleteNote)
                 .WithName("DeleteNote")
                 .WithDescription("Deletes a note")
                 .RequireAuthorization();

            return group;
        }


        private static async Task<IResult> GetAllNotes(
            // The [FromServices] attribute tells the API to get these objects
            // from the services container, not the request body. This is the fix.
            [FromServices] INoteService noteService,
            [FromServices] ICurrentUserHelper currentUserHelper,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Getting all notes");
                
                // Check if current user is admin
                if (currentUserHelper.IsCurrentUserAdmin())
                {
                    // Admin can see all notes
                    var allNotes = await noteService.GetAllNotesAsync();
                    return Results.Ok(new { notes = allNotes });
                }
                else
                {
                    // Regular users only see their own notes
                    var userId = currentUserHelper.GetCurrentUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }
                    
                    var userNotes = await noteService.GetAllNotesAsync(userId.Value);
                    return Results.Ok(new { notes = userNotes });
                }
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
            [FromServices] ICurrentUserHelper currentUserHelper,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Getting note with ID: {Id}", id);
                
                NoteViewModel? note;
                
                if (currentUserHelper.IsCurrentUserAdmin())
                {
                    // Admin can see any note
                    note = await noteService.GetNoteByIdAsync(id);
                }
                else
                {
                    // Regular users only see their own notes
                    var userId = currentUserHelper.GetCurrentUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }
                    
                    note = await noteService.GetNoteByIdAsync(id, userId.Value);
                }
                
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
            [FromServices] ICurrentUserHelper currentUserHelper,
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

                // Get current user ID and assign it to the note
                var userId = currentUserHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Results.Unauthorized();
                }
                
                // Always assign the note to the current user (security measure)
                noteViewModel.UserId = userId.Value;

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
            [FromServices] ICurrentUserHelper currentUserHelper,
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

                var userId = currentUserHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Results.Unauthorized();
                }

                // Security check: ensure user can only update their own notes (unless admin)
                if (!currentUserHelper.IsCurrentUserAdmin())
                {
                    var existingNote = await noteService.GetNoteByIdAsync(id, userId.Value);
                    if (existingNote == null)
                    {
                        return Results.NotFound($"Note with ID {id} not found");
                    }
                    
                    // Ensure the note stays assigned to the current user
                    noteViewModel.UserId = userId.Value;
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
            [FromServices] ICurrentUserHelper currentUserHelper,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Notes");
            try
            {
                logger.LogInformation("Deleting note with ID: {Id}", id);
                
                var userId = currentUserHelper.GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Results.Unauthorized();
                }

                // Security check: ensure user can only delete their own notes (unless admin)
                if (!currentUserHelper.IsCurrentUserAdmin())
                {
                    var existingNote = await noteService.GetNoteByIdAsync(id, userId.Value);
                    if (existingNote == null)
                    {
                        return Results.NotFound($"Note with ID {id} not found");
                    }
                }
                
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
            [FromServices] ICurrentUserHelper currentUserHelper,
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

                IEnumerable<NoteViewModel> notes;
                
                if (currentUserHelper.IsCurrentUserAdmin())
                {
                    // Admin can search all notes
                    notes = await noteService.GetAllNotesBySearchCriteriaAsync(searchCriteria);
                }
                else
                {
                    // Regular users only search their own notes
                    var userId = currentUserHelper.GetCurrentUserId();
                    if (!userId.HasValue)
                    {
                        return Results.Unauthorized();
                    }
                    
                    notes = await noteService.GetAllNotesBySearchCriteriaAsync(searchCriteria, userId.Value);
                }
                
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
