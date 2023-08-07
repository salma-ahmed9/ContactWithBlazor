using ContactWithBlazor.Client;
using EdgeDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ContactWithBlazor.Client.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("contactdb"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});
builder.Services.AddApiAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpClient();
var app = builder.Build();

app.MapGet("/existingcontact", async (HttpContext context, EdgeDBClient client) =>
{
    var result = await client.QueryAsync<Contact>("SELECT Contact {*} Order by .first_name;");
    return Results.Ok(result.ToList());
});

app.MapGet("getcontactwithid", async (HttpContext context, EdgeDBClient client) =>
{
    var id = context.Request.Query["id"];
    Contact result = await client.QuerySingleAsync<Contact>("SELECT Contact{*} FILTER .id = <uuid>$id", new Dictionary<string, object?> { { "id", id } });
    
});

app.MapGet("/logoutuser", async (HttpContext context, EdgeDBClient client) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapGet("/getuser", async (HttpContext context, EdgeDBClient client) =>
{
    LoginInput loginObject = new LoginInput();
    var user = context.User;
    if (user.Identity.IsAuthenticated)
    {
        loginObject.UserName = user.FindFirstValue(ClaimTypes.Name);
        loginObject.Role= user.FindFirstValue(ClaimTypes.Role);
    }
    return await Task.FromResult(loginObject);
});

app.MapPost("/addcontact", async (HttpContext context, EdgeDBClient client, Contact contact) =>
{
    if (string.IsNullOrEmpty(contact.FirstName) || string.IsNullOrEmpty(contact.LastName) || string.IsNullOrEmpty(contact.Email) || string.IsNullOrEmpty(contact.Title) || string.IsNullOrEmpty(contact.BirthDate.ToString()) || string.IsNullOrEmpty(contact.MarriageStatus.ToString()))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("All fields are required.");
        return;
    }
    var passwordHasher = new PasswordHasher<string>();
    contact.Password = passwordHasher.HashPassword(null, contact.Password);
    var query = "INSERT Contact {username := <str>$username, password := <str>$password, role := <str>$role,first_name := <str>$first_name, last_name := <str>$last_name, email := <str>$email, title := <str>$title, birth_date := <datetime>$birth_date, description := <str>$description, marriage_status := <bool>$marriage_status}";
    await client.ExecuteAsync(query, new Dictionary<string, object?>
    {
       {"username", contact.Username},
       {"password", contact.Password},
       {"role", contact.Role},
       {"first_name", contact.FirstName},
       {"last_name", contact.LastName},
       {"email", contact.Email},
       {"title", contact.Title},
       {"birth_date", contact.BirthDate},
       {"description", contact.Description},
       {"marriage_status", contact.MarriageStatus}
    });
});

app.MapPost("/login", async (HttpContext context, EdgeDBClient client,LoginInput loginInput) =>
{
    string username = loginInput.UserName;
    string password = loginInput.Password;
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        context.Response.StatusCode = 400; 
        await context.Response.WriteAsync("Username and password are required.");
        return;
    }
    var query = @"SELECT Contact {username, password, role } FILTER Contact.username = <str>$username";
    var result = await client.QueryAsync<Contact>(query, new Dictionary<string, object?>
    {
       { "username",loginInput.UserName }
    });
    if (result.Count > 0)
    {
        var passwordHasher = new PasswordHasher<string>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, result.First().Password, password);
        if(passwordVerificationResult == PasswordVerificationResult.Success)
        {
            var claims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, result.First().Username),
               new Claim(ClaimTypes.Role, result.First().Role),
            };
            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true

            };
            var user = new ClaimsPrincipal(claimsIdentity);
            await context.SignInAsync(scheme, user, authProperties);
            context.Response.StatusCode = 200;
        }
        else
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid password.");
            return;
        }
    }
    else
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Unsuccessful login attempt.");
        return;
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
