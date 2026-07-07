using Application.Interfaces;
using Application.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

/// <summary>
/// خدمة كشف الذكاء الاصطناعي الحقيقية المتصلة بموديلات البايثون (ML_Models)
/// </summary>
public class RealAiDetectionService : IAiDetectionService
{
    private readonly HttpClient _httpClient;
    private readonly string _pythonApiUrl;

    
    private readonly string[] _aiIndicatorPhrases = new[]
    {
        "as an ai", "as a language model", "i don't have personal",
        "i cannot", "i apologize", "however", "furthermore",
        "moreover", "in conclusion", "to summarize", "كنموذج لغوي",
        "كذكاء اصطناعي", "لا أستطيع", "من المهم أن نلاحظ",
        "في الختام", "باختصار", "علاوة على ذلك"
    };

    public RealAiDetectionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _pythonApiUrl = configuration["AiDetectorSettings:PythonApiUrl"] ?? "http://127.0.0.1:8000";
    }


    /// <summary>
    /// تحليل نص للذكاء الاصطناعي باستخدام موديل البايثون (Hugging Face / PyTorch)
    /// </summary>
    public async Task<AiDetectionResult> DetectTextAsync(string text)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(text))
        {
            return new AiDetectionResult
            {
                AiProbability = 0,
                IsAiGenerated = false,
                Confidence = 0,
                Details = "النص فارغ",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        
        bool isArabic = Regex.IsMatch(text, @"\p{IsArabic}");
        string endpoint = isArabic ? $"{_pythonApiUrl}/predict/ar" : $"{_pythonApiUrl}/predict/en";

        PythonApiResponse pythonResponse = null;

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(new { text = text }), Encoding.UTF8, "application/json");
            var res = await _httpClient.PostAsync(endpoint, content);
            
            if (res.IsSuccessStatusCode)
            {
                var responseString = await res.Content.ReadAsStringAsync();
                pythonResponse = JsonSerializer.Deserialize<PythonApiResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
        
            return new AiDetectionResult
            {
                AiProbability = 0,
                IsAiGenerated = false,
                Confidence = 0,
                Details = $"فشل الاتصال بموديل الذكاء الاصطناعي (تأكد من تشغيل سيرفر البايثون على بورت 8000). الخطأ: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        if (pythonResponse == null)
        {
            return new AiDetectionResult
            {
                AiProbability = 0,
                IsAiGenerated = false,
                Confidence = 0,
                Details = "لم يتم الحصول على نتيجة صالحة من موديل الذكاء الاصطناعي.",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        
        var indicators = new List<string>();
        var lowerText = text.ToLower();
        var aiPhraseCount = _aiIndicatorPhrases.Count(phrase => lowerText.Contains(phrase.ToLower()));
        if (aiPhraseCount > 0)
        {
            indicators.Add($"وُجدت {aiPhraseCount} عبارة مميزة للـ AI");
        }

        indicators.Add(isArabic ? "استُخدم الموديل العربي (DistilBERT)" : "استُخدم الموديل الإنجليزي (DistilBERT)");

        stopwatch.Stop();

        return new AiDetectionResult
        {
            AiProbability = pythonResponse.Ai_Probability,
            IsAiGenerated = pythonResponse.Is_Ai,
            Confidence = pythonResponse.Confidence,
            Details = $"تم التحليل بواسطة نموذج {pythonResponse.Language.ToUpper()}. " + 
                    (pythonResponse.Is_Ai ? "النص على الأرجح من إنشاء الذكاء الاصطناعي." : "النص على الأرجح مكتوب بشرياً."),
            Indicators = indicators,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل صورة باستخدام EfficientNet-B0 عبر سيرفر البايثون
    /// </summary>
    public async Task<AiDetectionResult> DetectImageAsync(string imagePath)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(imagePath))
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"الملف غير موجود: {imagePath}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        PythonApiResponse pythonResponse = null;
        try
        {
            using var form    = new MultipartFormDataContent();
            await using var fs = File.OpenRead(imagePath);
            var fileContent   = new StreamContent(fs);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(imagePath));

            var res = await _httpClient.PostAsync($"{_pythonApiUrl}/predict/image", form);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                pythonResponse = JsonSerializer.Deserialize<PythonApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"فشل الاتصال بموديل الصور: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        stopwatch.Stop();
        if (pythonResponse == null)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = "لم يتم الحصول على نتيجة صالحة من موديل الصور.",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        return new AiDetectionResult
        {
            AiProbability = pythonResponse.Ai_Probability,
            IsAiGenerated = pythonResponse.Is_Ai,
            Confidence    = pythonResponse.Confidence,
            Details       = pythonResponse.Details ?? (pythonResponse.Is_Ai
                ? "الصورة على الأرجح مولّدة بالذكاء الاصطناعي."
                : "الصورة على الأرجح حقيقية."),
            Indicators    = new List<string> { "EfficientNet-B0" },
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل فيديو باستخدام فحص الفريمات والصوت عبر سيرفر البايثون
    /// </summary>
    public async Task<AiDetectionResult> DetectVideoAsync(string videoPath)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(videoPath))
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"الملف غير موجود: {videoPath}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var ext = Path.GetExtension(videoPath).ToLowerInvariant();
        var allowedVideoTypes = new[] { ".mp4", ".avi", ".mov", ".mkv", ".webm" };
        if (!allowedVideoTypes.Contains(ext))
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"نوع الفيديو غير مدعوم: {ext}. استخدم mp4, avi, mov, mkv أو webm",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        PythonApiResponse pythonResponse = null;
        try
        {
            using var form    = new MultipartFormDataContent();
            await using var fs = File.OpenRead(videoPath);
            var fileContent   = new StreamContent(fs);
            var mimeType      = ext == ".mp4" ? "video/mp4" : "application/octet-stream";
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            form.Add(fileContent, "file", Path.GetFileName(videoPath));

            // Video inference can take longer - use a generous timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            var res = await _httpClient.PostAsync($"{_pythonApiUrl}/predict/video", form, cts.Token);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                pythonResponse = JsonSerializer.Deserialize<PythonApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"فشل الاتصال بموديل الفيديو: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        stopwatch.Stop();
        if (pythonResponse == null)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = "لم يتم الحصول على نتيجة صالحة من موديل الفيديو.",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        return new AiDetectionResult
        {
            AiProbability = pythonResponse.Ai_Probability,
            IsAiGenerated = pythonResponse.Is_Ai,
            Confidence    = pythonResponse.Confidence,
            Details       = pythonResponse.Details ?? (pythonResponse.Is_Ai
                ? "الفيديو على الأرجح مولّد بالذكاء الاصطناعي / مزيّف (Deepfake)."
                : "الفيديو على الأرجح حقيقي."),
            Indicators    = new List<string> { "Multimodal Video (EfficientNet + Wav2Vec2)" },
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل صوتي باستخدام Wav2Vec2 عبر سيرفر البايثون
    /// يدعم .wav و .mp3 | يحوّل لـ mono 16kHz | يقسّم إلى chunks بحد أقصى 3 ثوانٍ
    /// </summary>
    public async Task<AiDetectionResult> DetectAudioAsync(string audioPath)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(audioPath))
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"الملف غير موجود: {audioPath}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var ext = Path.GetExtension(audioPath).ToLowerInvariant();
        if (ext != ".wav" && ext != ".mp3")
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"نوع الملف غير مدعوم: {ext}. استخدم .wav أو .mp3",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        PythonApiResponse pythonResponse = null;
        try
        {
            using var form    = new MultipartFormDataContent();
            await using var fs = File.OpenRead(audioPath);
            var fileContent   = new StreamContent(fs);
            var mimeType      = ext == ".mp3" ? "audio/mpeg" : "audio/wav";
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            form.Add(fileContent, "file", Path.GetFileName(audioPath));

            // Audio inference can be slow — use generous timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var res = await _httpClient.PostAsync($"{_pythonApiUrl}/predict/audio", form, cts.Token);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                pythonResponse = JsonSerializer.Deserialize<PythonApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = $"فشل الاتصال بموديل الصوت: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        stopwatch.Stop();
        if (pythonResponse == null)
        {
            return new AiDetectionResult
            {
                AiProbability = 0, IsAiGenerated = false, Confidence = 0,
                Details = "لم يتم الحصول على نتيجة صالحة من موديل الصوت.",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        return new AiDetectionResult
        {
            AiProbability = pythonResponse.Ai_Probability,
            IsAiGenerated = pythonResponse.Is_Ai,
            Confidence    = pythonResponse.Confidence,
            Details       = pythonResponse.Details ?? (pythonResponse.Is_Ai
                ? "الصوت على الأرجح مولّد بالذكاء الاصطناعي / مزوّر."
                : "الصوت على الأرجح حقيقي."),
            Indicators    = new List<string> { "Wav2Vec2 (ASVspoof)", "16kHz mono", "3s chunks" },
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

}
