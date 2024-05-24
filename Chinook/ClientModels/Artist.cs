namespace Chinook.ClientModels
{
    public partial class Artist
    {
        public long ArtistId { get; set; }
        public string? Name { get; set; }
        public List<Album> Albums { get; set; }
    }
}
