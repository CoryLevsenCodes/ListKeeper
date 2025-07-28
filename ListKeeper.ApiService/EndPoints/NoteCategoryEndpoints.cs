using System.Net;
using ListKeeper.ApiService.Models.ViewModels;
using ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListKeeperWebApi.WebApi.Endpoints
{
    public static class NoteCategoryEndpoints
    {
        public static RouteGroupBuilder MapNoteCategoryApiEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllCategories)
                 .WithName("GetAllNoteCategories")
                 .WithDescription("Gets all note categories")
                 .RequireAuthorization("Admin");

            group.MapGet("/{id:int}", GetCategoryById)
                 .WithName("GetNoteCategoryById")
                 .WithDescription("Gets a note category by ID")
                 .RequireAuthorization("Admin");

            group.MapPost("/", CreateCategory)
                 .WithName("CreateNoteCategory")
                 .WithDescription("Creates a new note category")
                 .RequireAuthorization("Admin");

            group.MapPut("/{id:int}", UpdateCategory)
                 .WithName("UpdateNoteCategory")
                 .WithDescription("Updates a note category")
                 .RequireAuthorization("Admin");

            group.MapDelete("/{id:int}", DeleteCategory)
                 .WithName("DeleteNoteCategory")
                 .WithDescription("Deletes a note category")
                 .RequireAuthorization("Admin");

            return group;
        }

        private static async Task<IResult> GetAllCategories(
            [FromServices] INoteCategoryService categoryService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("NoteCategories");
            try
            {
                logger.LogInformation("Getting all note categories");
                var categories = await categoryService.GetAllCategoriesAsync();
                return Results.Ok(new { categories });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving note categories");
                return Results.Problem("An error occurred while retrieving note categories", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> GetCategoryById(
            int id,
            [FromServices] INoteCategoryService categoryService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("NoteCategories");
            try
            {
                logger.LogInformation("Getting note category with ID: {Id}", id);
                var category = await categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    return Results.NotFound($"Note category with ID {id} not found");
                }

                return Results.Ok(category);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving note category with ID: {Id}", id);
                return Results.Problem("An error occurred while retrieving the note category", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> CreateCategory(
            [FromBody] NoteCategoryViewModel categoryViewModel,
            [FromServices] INoteCategoryService categoryService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("NoteCategories");
            try
            {
                logger.LogInformation("Creating new note category");

                if (categoryViewModel == null)
                {
                    return Results.BadRequest("Category data is required");
                }

                var createdCategory = await categoryService.CreateCategoryAsync(categoryViewModel);

                if (createdCategory == null)
                {
                    return Results.Problem("Failed to create note category", statusCode: (int)HttpStatusCode.InternalServerError);
                }

                return Results.Created($"/api/notecategories/{createdCategory.NoteCategoryId}", createdCategory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating note category");
                return Results.Problem("An error occurred while creating the note category", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> UpdateCategory(
            int id,
            [FromBody] NoteCategoryViewModel categoryViewModel,
            [FromServices] INoteCategoryService categoryService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("NoteCategories");
            try
            {
                logger.LogInformation("Updating note category with ID: {Id}", id);

                if (categoryViewModel == null)
                {
                    return Results.BadRequest("Category data is required");
                }

                if (id != categoryViewModel.NoteCategoryId)
                {
                    return Results.BadRequest("ID in URL does not match ID in category data");
                }

                var updatedCategory = await categoryService.UpdateCategoryAsync(categoryViewModel);

                if (updatedCategory == null)
                {
                    return Results.NotFound($"Note category with ID {id} not found");
                }

                return Results.Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating note category with ID: {Id}", id);
                return Results.Problem("An error occurred while updating the note category", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }

        private static async Task<IResult> DeleteCategory(
            int id,
            [FromServices] INoteCategoryService categoryService,
            [FromServices] ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("NoteCategories");
            try
            {
                logger.LogInformation("Deleting note category with ID: {Id}", id);

                var result = await categoryService.DeleteCategoryAsync(id);

                if (!result)
                {
                    return Results.NotFound($"Note category with ID {id} not found");
                }

                return Results.Ok(new { success = true, message = "Note category deleted successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting note category with ID: {Id}", id);
                return Results.Problem("An error occurred while deleting the note category", statusCode: (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
