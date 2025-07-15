// These 'using' statements import necessary namespaces.
using ListKeeperWebApi.WebApi.Data;                // Access to the INoteRepository interface.
using ListKeeperWebApi.WebApi.Models;              // Access to the main 'Note' domain model.
using ListKeeperWebApi.WebApi.Models.ViewModels;   // Access to the NoteViewModel and LoginViewModel.
using ListKeeperWebApi.WebApi.Models.Extensions;   // Access to the ToViewModel() and ToDomain() extension methods.
using Microsoft.Extensions.Configuration;          // Provides access to the application's configuration (appsettings.json).
using Microsoft.Extensions.Logging;                // Provides logging capabilities.
using System.Security.Cryptography;                // Provides classes for cryptography, like HMACSHA256.
using System.Text;
using ListKeeper.ApiService.Models.ViewModels;
using ListKeeper.ApiService.Models;
using ListKeeper.ApiService.Models.Extensions;                                 // Provides text encoding functionalities (e.g., UTF8).

namespace ListKeeperWebApi.WebApi.Services
{
    /// <summary>
    /// This is the "Service Layer" for note-related operations.
    /// Its job is to contain the core business logic. It acts as a middle-man between the API endpoints (the "presentation" layer)
    /// and the repository (the "data access" layer). This separation makes the code cleaner and easier to manage.
    /// </summary>
    public class NoteService : INoteService
    {
        // These are private fields to hold the "dependencies" that this service needs to do its job.
        private readonly INoteRepository _repo;
        private readonly ILogger<NoteService> _logger;
        private readonly IConfiguration _config; // Added to access appsettings.json

        /// <summary>
        /// This is the constructor. When an instance of `NoteService` is created,
        /// the dependency injection system (configured in Program.cs) automatically provides
        /// an instance of `INoteRepository`, `ILogger<NoteService>`, and `IConfiguration`.
        /// </summary>
        public NoteService(INoteRepository repo, ILogger<NoteService> logger, IConfiguration config)
        {
            _repo = repo;
            _logger = logger;
            _config = config; // Store the injected configuration service.
        }


        public async Task<NoteViewModel?> CreateNoteAsync(NoteViewModel createNoteVm)
        {
            if (createNoteVm == null) return null;

            // Map the data from the view model to a 'Note' domain entity, which is what the database stores.
            var note = new Note
            {
                Title = createNoteVm.Title,
                Content = createNoteVm.Content,
                Color = createNoteVm.Color,
                IsCompleted = createNoteVm.IsCompleted,
                DueDate = createNoteVm.DueDate,
                Id = createNoteVm.Id
            };

            // Delegate the actual database "add" operation to the repository.
            var createdNote = await _repo.AddAsync(note);
            return createdNote?.ToViewModel();
        }


        public async Task<NoteViewModel?> UpdateNoteAsync(NoteViewModel noteVm)
        {
            if (noteVm == null) return null;

            // First, retrieve the existing note from the database.
            var note = await _repo.GetByIdAsync(noteVm.Id);
            if (note == null) return null; // Can't update a note that doesn't exist.

            // --- LOGIC CORRECTION ---
            // Map the updated properties from the view model to the database entity.
            note.Title = noteVm.Title;
            note.Content = noteVm.Content;
            note.Color = noteVm.Color;
            note.IsCompleted = noteVm.IsCompleted;
            note.DueDate = noteVm.DueDate;
            note.Id = noteVm.Id;

            var updatedNote = await _repo.Update(note);
            return updatedNote?.ToViewModel();
        }


        public async Task<IEnumerable<NoteViewModel>> GetAllNotesAsync()
        {
            var notes = await _repo.GetAllAsync();
            // The `?.` is a null-conditional operator. If 'notes' is null, it won't throw an error.
            // The `??` is a null-coalescing operator.
            // If the result of the Select is null, it returns an empty list instead.
            return notes?.Select(u => u.ToViewModel()) ?? Enumerable.Empty<NoteViewModel>();
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            return await _repo.Delete(id);
        }

        /// <summary>
        /// Deletes a note based on a view model object. This is another overload.
        /// </summary>
        public async Task<bool> DeleteNoteAsync(NoteViewModel noteVm)
        {
            if (noteVm == null) return false;
            // The `ToDomain()` extension method converts the view model back to a database entity.
            var note = noteVm.ToDomain();
            return await _repo.Delete(note);
        }

        /// <summary>
        /// Retrieves a single note by their ID.
        /// </summary>
        public async Task<NoteViewModel?> GetNoteByIdAsync(int id)
        {
            var note = await _repo.GetByIdAsync(id);
            return note?.ToViewModel();
        }
    }
}
