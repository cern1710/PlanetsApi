using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

public class Planet
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Rotation_period { get; set; }
    public string? Orbital_period { get; set; }
    public string? Diameter { get; set; }
    public string? Climate { get; set; }
    public string? Gravity { get; set; }
    public string? Terrain { get; set; }
    public string? Surface_water { get; set; }
    public string? Population { get; set; }

    public string? ResidentsData { get; set; }
    public string? FilmsData { get; set; }

    [NotMapped] // This ensures EF Core ignores this property
    public List<string> Residents
    {
        get
        {
            var data = ResidentsData ?? "[]";
            return JsonConvert.DeserializeObject<List<string>>(data) ?? new List<string>();
        }
        set => ResidentsData = JsonConvert.SerializeObject(value);
    }

    [NotMapped]
    public List<string> Films
    {
        get
        {
            var data = FilmsData ?? "[]";
            return JsonConvert.DeserializeObject<List<string>>(data) ?? new List<string>();
        }
        set => FilmsData = JsonConvert.SerializeObject(value);
    }

    //public List<string>? Residents { get; set; }
    //public List<string>? Films { get; set; }
    public string? Created { get; set; }
    public string? Edited { get; set; }
    public string? Url { get; set; }
    public bool IsComplete { get; set; }
}