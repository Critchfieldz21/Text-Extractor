using System.Net.Http.Json;
using System.Text.Json;

namespace TextExtractorUI.Services
{
    public class BackendApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BackendApiService> _logger;
        private readonly string _backendUrl;

        public BackendApiService(HttpClient httpClient, IConfiguration configuration, ILogger<BackendApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            // Get backend URL from configuration, default to localhost
            _backendUrl = configuration["BackendUrl"] ?? "http://localhost:5000";
        }

        /// <summary>
        /// Upload a PDF file to the backend for processing
        /// </summary>
        public async Task<ProcessingResult?> UploadAndProcessPdfAsync(string fileName, byte[] fileContent)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = new MemoryStream(fileContent);
                content.Add(new StreamContent(fileStream), "file", fileName);

                var response = await _httpClient.PostAsync($"{_backendUrl}/api/upload", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ProcessingResult>(jsonContent);
                }
                else
                {
                    _logger.LogError($"Failed to upload PDF: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading PDF: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get processing history from the backend
        /// </summary>
        public async Task<List<ProcessedTicket>?> GetHistoryAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ProcessedTicket>>($"{_backendUrl}/api/history");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting history: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get a specific ticket by ID
        /// </summary>
        public async Task<ProcessedTicket?> GetTicketAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProcessedTicket>($"{_backendUrl}/api/ticket/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting ticket: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Export ticket data as JSON
        /// </summary>
        public async Task<string?> ExportAsJsonAsync(int ticketId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_backendUrl}/api/export/true/{ticketId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Export ticket data as CSV
        /// </summary>
        public async Task<string?> ExportAscsvAsync(int ticketId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_backendUrl}/api/export/false/{ticketId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting CSV: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if backend is healthy
        /// </summary>
        public async Task<bool> IsBackendHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_backendUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ProcessingResult
    {
        public int TicketId { get; set; }
        public string? FileName { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    public class ProcessedTicket
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string? Status { get; set; }
        public object? ExtractedData { get; set; }
    }
}
