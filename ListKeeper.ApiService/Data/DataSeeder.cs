using ListKeeper.ApiService.Models;
using ListKeeperWebApi.WebApi.Models;
using ListKeeperWebApi.WebApi.Models.ViewModels;
using ListKeeperWebApi.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ListKeeperWebApi.WebApi.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAdminUserAsync(IHost app)
        {
            // A "service scope" is created to get instances of the services we need.
            // This is the correct way to access services in a method that runs at startup.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DataSeeder");
                var dbContext = services.GetRequiredService<DatabaseContext>();
                var config = services.GetRequiredService<IConfiguration>(); // Get config to read the hashing secret.

                try
                {
                    logger.LogInformation("Starting database seeding process.");

                    // Use the DbContext to check if there are any users in the database already.
                    if (!await dbContext.Users.AnyAsync())
                    {
                        logger.LogInformation("No users found. Seeding admin user directly.");

                        // Get the password hashing secret from configuration.
                        var secret = config["ApiSettings:UserPasswordHash"];
                        if (string.IsNullOrEmpty(secret))
                        {
                            logger.LogError("UserPasswordHash secret is not configured. Cannot seed user.");
                            return; // Stop seeding if the secret is missing.
                        }

                        // Hash the password directly within the seeder for maximum control.
                        string hashedPassword;
                        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                        {
                            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("AppleRocks!"));
                            hashedPassword = Convert.ToBase64String(hash);
                        }

                        // Create the User database entity directly. This bypasses the UserService
                        // and ensures the data is exactly as we define it here.
                        var adminUser = new User
                        {
                            Username = "Admin",
                            Email = "admin@example.com",
                            Password = hashedPassword,
                            Role = "Admin",
                            Firstname = "Admin",
                            Lastname = "User"
                        };

                        // Add the new user to the DbContext and save it.
                        // This will trigger the auditing logic in the DbContext's SaveChangesAsync method.
                        await dbContext.Users.AddAsync(adminUser);
                        await dbContext.SaveChangesAsync();

                        logger.LogInformation("Admin user seeded successfully.");
                    }
                    else
                    {
                        logger.LogInformation("Database already contains users. Seeding process skipped.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during the database seeding process.");
                }
            }
        }

        public static async Task SeedNotesAsync(IHost app)
        {
            // A "service scope" is created to get instances of the services we need.
            // This is the correct way to access services in a method that runs at startup.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DataSeeder");
                var dbContext = services.GetRequiredService<DatabaseContext>();

                try
                {
                    logger.LogInformation("Starting notes seeding process.");

                    // Use the DbContext to check if there are any notes in the database already.
                    if (!await dbContext.Notes.AnyAsync())
                    {
                        logger.LogInformation("No notes found. Seeding sample notes.");

                        // Create the note entities directly.
                        var sampleNotes = new List<Note>
                        {
                            new Note
                            {
                                Title = "Finalize quarterly report",
                                Content = "Compile sales data and performance metrics for the Q2 report. Draft slides for the presentation on Friday.",
                                DueDate = new DateTime(2025, 7, 15, 17, 0, 0, DateTimeKind.Utc),
                                IsCompleted = true,
                                Color = "#D1E7DD"
                            },
                            new Note
                            {
                                Title = "Grocery Shopping",
                                Content = "Milk, bread, eggs, chicken breast, spinach, and coffee beans.",
                                DueDate = new DateTime(2025, 6, 23, 18, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#F8D7DA"
                            },
                            new Note
                            {
                                Title = "Schedule dentist appointment",
                                Content = "Call Dr. Smith's office to schedule a routine check-up and cleaning.",
                                DueDate = new DateTime(2025, 6, 20, 12, 0, 0, DateTimeKind.Utc),
                                IsCompleted = true,
                                Color = "#FFF3CD"
                            },
                            new Note
                            {
                                Title = "Pay monthly credit card bill",
                                Content = "Due by the 25th. Check statement for any unusual charges.",
                                DueDate = new DateTime(2025, 6, 25, 23, 59, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#D1E7DD"
                            },
                            new Note
                            {
                                Title = "Plan weekend trip to Canmore",
                                Content = "Book hotel/Airbnb, check hiking trail conditions, and make dinner reservations.",
                                DueDate = new DateTime(2025, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#CFF4FC"
                            },
                            new Note
                            {
                                Title = "Renew driver's license",
                                Content = "License expires in August. Gather necessary documents and visit the registry.",
                                DueDate = new DateTime(2025, 7, 30, 9, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#F8D7DA"
                            },
                            new Note
                            {
                                Title = "Project Phoenix Kick-off Meeting",
                                Content = "Prepare agenda and introductory slides. Room 3B at 10 AM.",
                                DueDate = new DateTime(2025, 6, 24, 10, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#D1E7DD"
                            },
                            new Note
                            {
                                Title = "Pick up dry cleaning",
                                Content = "Ticket number is 452. Ready for pickup after 3 PM on Wednesday.",
                                DueDate = new DateTime(2025, 6, 18, 15, 0, 0, DateTimeKind.Utc),
                                IsCompleted = true,
                                Color = "#FFF3CD"
                            },
                            new Note
                            {
                                Title = "Research new CRMs",
                                Content = "Evaluate options for a new customer relationship management system. Focus on Salesforce, HubSpot, and Zoho.",
                                DueDate = new DateTime(2025, 7, 18, 17, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#CFF4FC"
                            },
                            new Note
                            {
                                Title = "Call mom for her birthday",
                                Content = "Her birthday is on the 28th. Don't forget!",
                                DueDate = new DateTime(2025, 6, 28, 14, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#F8D7DA"
                            },
                            new Note
                            {
                                Title = "Fix leaky faucet in kitchen",
                                Content = "Buy a new washer kit from Canadian Tire. Watch YouTube tutorial.",
                                DueDate = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#FFF3CD"
                            },
                            new Note
                            {
                                Title = "Submit expense report",
                                Content = "Include receipts from the Calgary business trip. Deadline is EOD Friday.",
                                DueDate = new DateTime(2025, 6, 27, 17, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#D1E7DD"
                            },
                            new Note
                            {
                                Title = "Gym Session - Legs",
                                Content = "Squats, deadlifts, leg press, and calf raises.",
                                DueDate = new DateTime(2025, 6, 22, 19, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#CFF4FC"
                            },
                            new Note
                            {
                                Title = "Read \"Atomic Habits\"",
                                Content = "Finish chapter 5. Take notes on the concept of habit stacking.",
                                DueDate = new DateTime(2025, 6, 29, 21, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#E2D9F3"
                            },
                            new Note
                            {
                                Title = "Water the plants",
                                Content = "Ferns in the living room and the succulents on the balcony.",
                                DueDate = new DateTime(2025, 6, 22, 8, 0, 0, DateTimeKind.Utc),
                                IsCompleted = true,
                                Color = "#D1E7DD"
                            },
                            new Note
                            {
                                Title = "Update LinkedIn Profile",
                                Content = "Add new skills and recent project accomplishments.",
                                DueDate = new DateTime(2025, 7, 5, 11, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#CFF4FC"
                            },
                            new Note
                            {
                                Title = "Organize garage",
                                Content = "Sort tools, donate old items, and sweep the floor. It's a mess!",
                                DueDate = new DateTime(2025, 7, 12, 10, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#FFF3CD"
                            },
                            new Note
                            {
                                Title = "Buy concert tickets for July Talk",
                                Content = "Tickets go on sale Friday at 10 AM. Set a reminder.",
                                DueDate = new DateTime(2025, 6, 27, 10, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#F8D7DA"
                            },
                            new Note
                            {
                                Title = "Review pull request from Sarah",
                                Content = "Check the new API endpoint logic in the `feature/user-auth` branch.",
                                DueDate = new DateTime(2025, 6, 23, 16, 0, 0, DateTimeKind.Utc),
                                IsCompleted = false,
                                Color = "#E2D9F3"
                            },
                            new Note
                            {
                                Title = "Book oil change for car",
                                Content = "Due for a service. Call the dealership to schedule for next week.",
                                DueDate = new DateTime(2025, 6, 20, 12, 0, 0, DateTimeKind.Utc),
                                IsCompleted = true,
                                Color = "#FFF3CD"
                            }
                        };

                        // Add all the notes to the DbContext and save them.
                        // This will trigger the auditing logic in the DbContext's SaveChangesAsync method.
                        await dbContext.Notes.AddRangeAsync(sampleNotes);
                        await dbContext.SaveChangesAsync();

                        logger.LogInformation($"Successfully seeded {sampleNotes.Count} sample notes.");
                    }
                    else
                    {
                        logger.LogInformation("Database already contains notes. Notes seeding process skipped.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during the notes seeding process.");
                }
            }
        }

    }
}

