namespace ScrobblerApi.Models.DTOs;

public class TrackDto
{
    public string Name { get; set; } = "Faixa Desconhecida";
    public int DurationInSeconds { get; set; } = 180;
}

public class AlbumInfoDto
{
    public string Album { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<TrackDto> Tracks { get; set; } = new();
}