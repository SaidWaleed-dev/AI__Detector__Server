using Application.Interfaces;
using Application.Models;
using System.Diagnostics;
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
    /// تحليل صورة
    /// </summary>
    public async Task<AiDetectionResult> DetectImageAsync(string imagePath)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(150);
        
        var indicators = new List<string> { "لا يوجد موديل صور حقيقي بعد - يعتمد على Mock Logic" };
        var random = new Random(imagePath.GetHashCode());
        var finalProbability = random.NextDouble();
        stopwatch.Stop();
        
        return new AiDetectionResult
        {
            AiProbability = finalProbability,
            IsAiGenerated = finalProbability > 0.5,
            Confidence = 0.8,
            Details = $"الوصول لموديل الصور غير مدعوم حالياً (محاكاة). الاحتمالية: {finalProbability:P0}",
            Indicators = indicators,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل فيديو
    /// </summary>
    public async Task<AiDetectionResult> DetectVideoAsync(string videoPath)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(200);
        stopwatch.Stop();
        return new AiDetectionResult
        {
            AiProbability = 0.5,
            IsAiGenerated = false,
            Confidence = 0.5,
            Details = "موديل الفيديو غير متوفر حالياً.",
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل صوتي
    /// </summary>
    public async Task<AiDetectionResult> DetectAudioAsync(string audioPath)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(120);
        stopwatch.Stop();
        return new AiDetectionResult
        {
            AiProbability = 0.5,
            IsAiGenerated = false,
            Confidence = 0.5,
            Details = "موديل الصوت غير متوفر حالياً.",
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

}
