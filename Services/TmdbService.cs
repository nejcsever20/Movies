using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Movies.Services
{
    public class TmdbService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public TmdbService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["TMDB:ApiKey"];
        }

        public async Task<TmdbSearchResult?> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";

            return await _http.GetFromJsonAsync<TmdbSearchResult>(url);
        }

        public string GetPosterUrl(string? posterPath)
        {
            return string.IsNullOrEmpty(posterPath)
                ? "/images/movies/default.jpg"
                : $"https://image.tmdb.org/t/p/w500{posterPath}";
        }

        public string? GetTrailerUrl(TmdbMovieDetails movie)
        {
            var youtubeKey = movie?.videos?.results?.FirstOrDefault(v => v.type == "Trailer" && v.site == "YouTube")?.key;
            return youtubeKey != null ? $"https://www.youtube.com/embed/{youtubeKey}" : null;
        }

        public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId)
        {
            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&append_to_response=videos";
            return await _http.GetFromJsonAsync<TmdbMovieDetails>(url);
        }
    }

    public class TmdbSearchResult
    {
        public int page { get; set; }
        public List<TmdbMovie> results { get; set; } = new();
    }

    public class TmdbMovie
    {
        public int id { get; set; }
        public string? title { get; set; }
        public string? release_date { get; set; }
        public string? poster_path { get; set; }
    }

    public class TmdbMovieDetails
    {
        public int id { get; set; }
        public string? title { get; set; }
        public int? runtime { get; set; }
        public Videos videos { get; set; } = new();
    }

    public class Videos
    {
        public List<Video> results { get; set; } = new();
    }

    public class Video
    {
        public string key { get; set; }
        public string site { get; set; }
        public string type { get; set; }
    }
}
