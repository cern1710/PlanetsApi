using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlanetDb>(opt => opt.UseInMemoryDatabase("PlanetList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/swapi/planets", async () => 
{
    var client = new HttpClient();
    var response = await client.GetStringAsync("https://swapi.dev/api/planets/");
    var jsonResponse = JsonConvert.DeserializeObject<SwapiResponse>(response);
    
    if (jsonResponse?.Results == null)
    {
        return Results.NotFound("No planets found or an error occurred while fetching planets from SWAPI.");
    }

    return Results.Ok(jsonResponse.Results);
});

app.MapGet("/planetitems", async (PlanetDb db) =>
    await db.Planets.ToListAsync());

app.MapGet("/planetitems/complete", async (PlanetDb db) =>
    await db.Planets.Where(p => p.IsComplete).ToListAsync());

app.MapGet("/planetitems/{id}", async (int id, PlanetDb db) =>
    await db.Planets.FindAsync(id)
        is Planet planet
            ? Results.Ok(planet)
            : Results.NotFound());

app.MapPost("/planetitems", async (Planet planet, PlanetDb db) =>
{
    db.Planets.Add(planet);
    await db.SaveChangesAsync();

    return Results.Created($"/planetitems/{planet.Id}", planet);
});

app.MapPut("/planetitems/{id}", async (int id, Planet inputPlanet, PlanetDb db) =>
{
    var planet = await db.Planets.FindAsync(id);

    if (planet is null) return Results.NotFound();

    planet.Name = inputPlanet.Name;
    planet.Rotation_period = inputPlanet.Rotation_period;
    planet.Orbital_period = inputPlanet.Orbital_period;
    planet.Diameter = inputPlanet.Diameter;
    planet.Climate = inputPlanet.Climate;
    planet.Gravity = inputPlanet.Gravity;
    planet.Terrain = inputPlanet.Terrain;
    planet.Surface_water = inputPlanet.Surface_water;
    planet.Population = inputPlanet.Population;
    planet.ResidentsData = inputPlanet.ResidentsData;
    planet.FilmsData = inputPlanet.FilmsData;
    planet.Created = inputPlanet.Created;
    planet.Edited = inputPlanet.Edited;
    planet.Url = inputPlanet.Url;

    planet.IsComplete = inputPlanet.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/planetitems/{id}", async (int id, PlanetDb db) =>
{
    if (await db.Planets.FindAsync(id) is Planet planet)
    {
        db.Planets.Remove(planet);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();