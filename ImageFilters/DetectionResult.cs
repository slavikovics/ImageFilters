using System.Text.Json.Serialization;

namespace ImageFilters;

public class DetectionResult
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("detected_image_path")]
    public string DetectedImagePath { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
}