using Microsoft.EntityFrameworkCore;         // The main library for Entity Framework Core, our Object-Relational Mapper (ORM).
using Microsoft.Extensions.Configuration;      // Provides access to the application's configuration (appsettings.json).
using Microsoft.Extensions.Logging;            // Provides logging capabilities.
using Microsoft.IdentityModel.Tokens;        // Contains classes for creating and validating security tokens (JWTs).
using System.IdentityModel.Tokens.Jwt;         // The main library for handling JSON Web Tokens.
using System.Security.Claims;                  // Allows us to create claims (pieces of information) about a note.
using System.Text;                             // Provides text encoding functionalities (e.g., UTF8).
using ListKeeperWebApi.WebApi.Models;          // Access to our main 'Note' domain model.
using ListKeeperWebApi.WebApi.Models.Interfaces;
using ListKeeper.ApiService.Models; // Access to the INoteRepository interface.

namespace ListKeeperWebApi.WebApi.Data
{
    public class NoteRepository : INoteRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<NoteRepository> _logger;
        private readonly IConfiguration _configuration;

        public NoteRepository(DatabaseContext context, ILogger<NoteRepository> logger, IConfiguration configuration)
        {
            _context = context; // Our gateway to the database.
            _logger = logger;   // Our tool for logging information and errors.
            _configuration = configuration; // Our tool for reading settings from appsettings.json.
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to find note by ID: {NoteId}", id);
            try
            {
                // `FindAsync` is a highly efficient way to look up an entity by its primary key.
                return await _context.Notes.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting note by ID: {NoteId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            _logger.LogInformation("Attempting to get all notes");
            try
            {
                // `ToListAsync` executes the query and returns all matching records as a List.
                return await _context.Notes.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all notes");
                throw;
            }
        }


        public async Task<Note> AddAsync(Note note)
        {
            _logger.LogInformation("Attempting to add a new note with Title: {NoteTitle}", note.Title);
            try
            {
                // `AddAsync` stages the new note to be inserted. It doesn't hit the database yet.
                await _context.Notes.AddAsync(note);
                // `SaveChangesAsync` is the command that actually executes the insert operation against the database.
                await _context.SaveChangesAsync();
                return note; // Return the note, which now has its database-generated ID.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding note with Title: {NoteTitle}", note.Title);
                throw;
            }
        }

        public async Task<Note> Update(Note note)
        {
            _logger.LogInformation("Attempting to update note with ID: {NoteId}", note.Id);
            try
            {
                // `Update` tells Entity Framework to track this entity as "modified".
                _context.Notes.Update(note);
                // `SaveChangesAsync` executes the update operation.
                await _context.SaveChangesAsync();
                return note;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating note with ID: {NoteId}", note.Id);
                throw;
            }
        }

        public async Task<Boolean> Delete(Note note)
        {
            _logger.LogInformation("Attempting to delete note with ID: {NoteId}", note.Id);
            try
            {
                // `Remove` stages the note for deletion.
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                return true; // Return true on success.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting note with ID: {NoteId}", note.Id);
                throw;
            }
        }

        public async Task<Boolean> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete note with ID: {id}", id);
            try
            {
                var note = await _context.Notes.FindAsync(id);
                if (note == null)
                {
                    _logger.LogWarning("Note with ID: {NoteId} not found to delete", id);
                    return false; // Can't delete a note that doesn't exist.
                }

                // Call the other `Delete` method to perform the actual removal.
                return await Delete(note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting note with ID: {id}", id);
                throw;
            }
        }
    }
}
