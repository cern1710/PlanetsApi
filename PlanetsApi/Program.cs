using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlanetDb>(opt =>
    opt.UseInMemoryDatabase("PlanetList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient();
var app = builder.Build();

/******* GET a list of Planets from the SWAPI API *******/
async Task<IResult> GetPlanetsAsync(IHttpClientFactory clientFactory)
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
        return Results.NotFound("Error occurred with GET request to SWAPI!");

    return Results.Ok(planets);
}

app.MapGet("/planets", GetPlanetsAsync);


/******* GET a list of favourite Planets *******/
async Task<IResult> GetFavoritesAsync(PlanetDb dbContext)
{
    var favoritePlanets =
        await dbContext.Planets.Where(p => p.isFavorite).ToListAsync();

    if (!favoritePlanets.Any())
        return Results.NotFound("No favorite planets found!");

    return Results.Ok(favoritePlanets);
}

app.MapGet("/favorites", GetFavoritesAsync);


/******* POST a favourite Planet and save to an in memory entity   *******/
/******* framework database - Planets can only be favourited once. *******/
async Task<Planet?> FetchPlanetFromSwapi(string planetName, HttpClient client)
{
    string? nextUrl = "https://swapi.dev/api/planets/";

    // iterate through the urls one by one until the next one is null or the planet is found
    while (!string.IsNullOrEmpty(nextUrl))
    {
        var response = await client.GetStringAsync(nextUrl);
        var jsonResponse = JsonConvert.DeserializeObject<SwapiResponse>(response);

        var foundPlanet = jsonResponse?.Results?.FirstOrDefault(
            p => p.Name.Equals(planetName, StringComparison.OrdinalIgnoreCase));

        if (foundPlanet != null)
            return foundPlanet;

        nextUrl = jsonResponse?.Next;
    }

    return null; // Planet not found
}

async Task<IResult> PostFavoriteAsync(IHttpClientFactory clientFactory,
    PlanetDb dbContext, string planetName)
{
    var client = clientFactory.CreateClient();
    var planet = await FetchPlanetFromSwapi(planetName, client);

    if (planet == null)
        return Results.NotFound($"Planet named {planetName} not found in SWAPI.");

    var fetchedPlanet = planet; // Get the Planet instance from the Task<Planet>

    var existingPlanet = await dbContext.Planets
    .FirstOrDefaultAsync(p => p.Name.Equals(fetchedPlanet.Name,
                            StringComparison.OrdinalIgnoreCase));



    if (existingPlanet != null)
    {
        if (existingPlanet.isFavorite)
            return Results.BadRequest("Planet is already favorited.");

        existingPlanet.isFavorite = true;
    }
    else
    {
        fetchedPlanet.isFavorite = true;
        dbContext.Planets.Add(fetchedPlanet);
    }

    await dbContext.SaveChangesAsync();
    return Results.Ok($"Added {planetName} to favorites.");
}

app.MapPost("/favorites/{planetName}", PostFavoriteAsync);


/******* DELETE a favourite planet. *******/
async Task<IResult> DeleteFavoritePlanetAsync(string planetName, PlanetDb dbContext)
{
    var planet = await dbContext.Planets
    .FirstOrDefaultAsync(p =>
        p.Name.Equals(planetName, StringComparison.OrdinalIgnoreCase));


    if (planet == null || !planet.isFavorite)
        return Results.NotFound("Planet not found or not favorited.");

    planet.isFavorite = false;
    await dbContext.SaveChangesAsync();

    return Results.Ok($"Removed {planetName} from favorites.");
}

app.MapDelete("/favorites/{planetName}", DeleteFavoritePlanetAsync);


/******* GET a random planet that has not yet *******/
/******* been favourited from the SWAPI API.  *******/
async Task<IResult> GetRandomAsync(IHttpClientFactory clientFactory, PlanetDb dbContext)
{
    var client = clientFactory.CreateClient();
    var allPlanets = new List<Planet>();
    string? nextUrl = "https://swapi.dev/api/planets/";

    // Fetch all planets from the SWAPI API
    while (!string.IsNullOrEmpty(nextUrl))
    {
        var response = await client.GetStringAsync(nextUrl);
        var jsonResponse = JsonConvert.DeserializeObject<SwapiResponse>(response);

        if (jsonResponse?.Results != null)
            allPlanets.AddRange(jsonResponse.Results);

        nextUrl = jsonResponse?.Next;
    }

    // Fetch all favorited planets from your database
    var favoritedPlanetsNames = await dbContext.Planets
        .Where(p => p.isFavorite)
        .Select(p => p.Name)
        .ToListAsync();

    // Filter out the favorited planets from the allPlanets list
    var notFavoritedPlanets = allPlanets.Where(p =>
            !favoritedPlanetsNames.Contains(p.Name)).ToList();

    if (!notFavoritedPlanets.Any())
        return Results.NotFound("All planets have been favorited!");

    // Select a random planet from the notFavoritedPlanets list
    var random = new Random();
    var randomPlanet = notFavoritedPlanets[random.Next(notFavoritedPlanets.Count)];

    return Results.Ok(randomPlanet);
}

app.MapGet("/random", GetRandomAsync);


/******* Runnng the program *******/
app.Run();