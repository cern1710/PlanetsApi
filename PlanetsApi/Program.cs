using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlanetDb>(opt => opt.UseInMemoryDatabase("PlanetList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpClient();
var app = builder.Build();

async Task<IResult> FetchPlanetsAsync(IHttpClientFactory clientFactory)
{
    var client = clientFactory.CreateClient();
    var planets = new List<Planet>();

    string? nextUrl = "https://swapi.dev/api/planets/";

    // iterate through the urls one by one until the next one is null
    while (!string.IsNullOrEmpty(nextUrl))
    {
        var response = await client.GetStringAsync(nextUrl);
        var jsonResponse = JsonConvert.DeserializeObject<SwapiResponse>(response);

        if (jsonResponse?.Results != null)
            planets.AddRange(jsonResponse.Results);

        nextUrl = jsonResponse?.Next;
    }

    if (planets.Count == 0)
        return Results.NotFound("No planets found! Error occurred while fetching planets from the Star Wars API.");

    return Results.Ok(planets);
}

app.MapGet("/planets", FetchPlanetsAsync);


app.MapPut("/favorites/{id}", async (int id, Planet inputPlanet, PlanetDb db) =>
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
    planet.Residents = inputPlanet.Residents;
    planet.Films = inputPlanet.Films;
    planet.Created = inputPlanet.Created;
    planet.Edited = inputPlanet.Edited;
    planet.Url = inputPlanet.Url;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/favorites/{id}", async (int id, PlanetDb db) =>
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