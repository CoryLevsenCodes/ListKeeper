# Fixing 403 Forbidden Error: Authorization Guide

This guide explains how to resolve the 403 Forbidden error when trying to add/edit/delete notes after user signup.

## The Problem

When users sign up through the application, they encounter a **403 Forbidden** error when trying to perform note operations:

```
https://localhost:7534/api/notes
Request Method: POST
Status Code: 403 Forbidden
```

## Root Cause Analysis

The issue occurs due to a mismatch between user roles and endpoint authorization requirements:

### 🔍 **What's Happening:**

1. **User Signup Process:**
   ```csharp
   // In UserService.SignupAsync()
   var userViewModel = new UserViewModel
   {
       // ... other properties
       Role = "User", // ← Default role for new signups
   };
   ```

2. **Note Endpoints Authorization:**
   ```csharp
   // In NoteEndpoints.cs (BEFORE fix)
   group.MapPost("/", CreateNote)
        .RequireAuthorization("Admin"); // ← Requires "Admin" role
   ```

3. **The Conflict:**
   - New users get `"User"` role
   - Note operations require `"Admin"` role
   - Result: Access denied (403 Forbidden)

## The Solution

Change the note endpoints to require basic authentication instead of specific "Admin" role:

### **File:** `ListKeeper.ApiService/EndPoints/NoteEndpoints.cs`

**BEFORE (Restrictive):**
```csharp
group.MapPost("/", CreateNote)
     .WithName("CreateNote")
     .WithDescription("Creates a new note")
     .RequireAuthorization("Admin"); // ← Only Admins can create notes
```

**AFTER (Fixed):**
```csharp
group.MapPost("/", CreateNote)
     .WithName("CreateNote")
     .WithDescription("Creates a new note")
     .RequireAuthorization(); // ← Any authenticated user can create notes
```

### **Complete Fix:**

Apply this change to ALL note endpoints:

```csharp
public static RouteGroupBuilder MapNoteApiEndpoints(this RouteGroupBuilder group)
{
    group.MapGet("/", GetAllNotes)
         .WithName("GetAllNotes")
         .WithDescription("Gets all notes")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    group.MapPost("/search", GetAllNotesBySearchCriteria)
         .WithName("GetAllNotesBySearchCriteria")
         .WithDescription("Gets notes by search criteria")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    group.MapGet("/{id:int}", GetNoteById)
         .WithName("GetNoteById")
         .WithDescription("Gets a note by ID")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    group.MapPost("/", CreateNote)
         .WithName("CreateNote")
         .WithDescription("Creates a new note")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    group.MapPut("/{id:int}", UpdateNote)
         .WithName("UpdateNote")
         .WithDescription("Updates an existing note")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    group.MapDelete("/{id:int}", DeleteNote)
         .WithName("DeleteNote")
         .WithDescription("Deletes a note")
         .RequireAuthorization(); // ✅ Changed from "Admin"

    return group;
}
```

## Understanding Authorization Levels

### **`.RequireAuthorization()`** (Basic)
- ✅ Requires valid JWT token
- ✅ User must be authenticated
- ✅ Works with any role ("User", "Admin", etc.)
- ✅ Perfect for regular user operations

### **`.RequireAuthorization("Admin")`** (Role-Specific)
- ✅ Requires valid JWT token
- ✅ User must be authenticated
- ❌ User must have "Admin" role specifically
- ✅ Perfect for administrative operations

### **`.AllowAnonymous()`** (No Auth)
- ✅ No authentication required
- ✅ Perfect for public endpoints (signup, login)

## Testing the Fix

### **Step 1: Rebuild Backend**
```bash
cd ListKeeper.ApiService
dotnet build
```

### **Step 2: Restart Backend Server**
```bash
dotnet run
```

### **Step 3: Test Note Operations**
1. **Sign up** a new user (gets "User" role)
2. **Try adding** a note
3. **Verify:** Should now work without 403 error

### **Step 4: Verify in Browser Network Tab**
- Open Developer Tools → Network
- Try adding a note
- Look for: `POST /api/notes` → Status: `201 Created` ✅

## Security Considerations

### **Why This Fix Is Secure:**

1. **Still Requires Authentication:**
   ```csharp
   .RequireAuthorization() // ← Must have valid JWT token
   ```

2. **JWT Token Validation:**
   - Token must be valid and not expired
   - Token must contain user information
   - Token signature must be verified

3. **User-Specific Data:**
   - Each user should only see/edit their own notes
   - Consider adding user filtering in repository layer

### **Future Enhancement - User Isolation:**

For production apps, consider adding user-specific filtering:

```csharp
// In NoteRepository.cs
public async Task<List<Note>> GetNotesByUserIdAsync(int userId)
{
    return await _context.Notes
        .Where(n => n.UserId == userId && n.DeletedAt == null)
        .ToListAsync();
}
```

## Alternative Solutions

### **Option 1: Create User/Admin Policy (Advanced)**

**File:** `Program.cs`
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});
```

**File:** `NoteEndpoints.cs`
```csharp
.RequireAuthorization("UserOrAdmin")
```

### **Option 2: Upgrade Default Role**

**File:** `UserService.cs`
```csharp
var userViewModel = new UserViewModel
{
    // ... other properties
    Role = "Admin", // ← Make all new users Admins
};
```

⚠️ **Not recommended for production** - gives all users admin privileges

### **Option 3: Role-Based Endpoint Split**

```csharp
// User endpoints
group.MapPost("/user/notes", CreateUserNote)
     .RequireAuthorization();

// Admin endpoints  
group.MapGet("/admin/notes", GetAllNotesAdmin)
     .RequireAuthorization("Admin");
```

## Quick Debugging Checklist

When you encounter 403 Forbidden errors:

### ✅ **Check These Items:**

1. **User Role vs Endpoint Requirements:**
   - What role does the user have?
   - What role does the endpoint require?

2. **JWT Token Validity:**
   - Is the token included in the request header?
   - Is the token format correct: `Authorization: Bearer <token>`?
   - Has the token expired?

3. **Authorization Configuration:**
   - Is JWT authentication configured in `Program.cs`?
   - Are the JWT settings correct in `appsettings.json`?

4. **Browser Storage:**
   - Check localStorage for user data
   - Verify token is being sent with requests

### **Common Error Patterns:**

```bash
# 401 Unauthorized = No token or invalid token
# 403 Forbidden = Valid token but insufficient role/permissions
# 404 Not Found = Endpoint doesn't exist
# 500 Internal Server Error = Server-side exception
```

## Conclusion

The 403 Forbidden error was caused by a role mismatch:
- **New users** get `"User"` role
- **Note endpoints** required `"Admin"` role
- **Solution:** Change to basic authentication (`RequireAuthorization()`)

This fix maintains security while allowing regular users to manage their notes. For production applications, consider implementing user-specific data isolation to ensure users only access their own notes.

### **Key Takeaways:**

1. **Always match** user roles with endpoint requirements
2. **Use basic auth** (`.RequireAuthorization()`) for user operations
3. **Use role-specific auth** (`.RequireAuthorization("Admin")`) for admin operations
4. **Test authorization** thoroughly after role changes
5. **Consider user isolation** for production applications

The application should now work correctly for newly signed-up users! 🎉
