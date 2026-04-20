namespace Avenue804.Web.Domain;

public enum AreaType { City, Community, District, Island, Development, Building, Industrial, FreeZone }

public class UaeArea
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emirate { get; set; } = string.Empty;
    public string? City { get; set; }
    public AreaType Type { get; set; }
    public int SortOrder { get; set; }      // higher = shown first in autocomplete
    public bool IsActive { get; set; } = true;
}
