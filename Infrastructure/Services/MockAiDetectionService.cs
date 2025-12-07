using Application.Interfaces;
using Application.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// تطبيق وهمي (Mock) لخدمة كشف الذكاء الاصطناعي
/// يستخدم خوارزميات بسيطة لمحاكاة نتائج واقعية
/// سيتم استبداله بنموذج ML حقيقي لاحقاً
/// </summary>
public class MockAiDetectionService : IAiDetectionService
{
    // كلمات وعبارات شائعة في المحتوى المُنشأ بالذكاء الاصطناعي
    private readonly string[] _aiIndicatorPhrases = new[]
    {
        "as an ai", "as a language model", "i don't have personal",
        "i cannot", "i apologize", "however", "furthermore",
        "moreover", "in conclusion", "to summarize", "كنموذج لغوي",
        "كذكاء اصطناعي", "لا أستطيع", "من المهم أن نلاحظ",
        "في الختام", "باختصار", "علاوة على ذلك"
    };

    private readonly string[] _formalWords = new[]
    {
        "furthermore", "moreover", "consequently", "nevertheless",
        "notwithstanding", "henceforth", "بالتالي", "علاوة على ذلك",
        "بناءً على ذلك", "من ناحية أخرى"
    };

    /// <summary>
    /// تحليل نص للكشف عن الذكاء الاصطناعي
    /// </summary>
    public async Task<AiDetectionResult> DetectTextAsync(string text)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await Task.Delay(100); // محاكاة وقت المعالجة

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

        var indicators = new List<string>();
        double aiScore = 0.0;

        // 1. فحص العبارات المميزة للـ AI
        var lowerText = text.ToLower();
        var aiPhraseCount = _aiIndicatorPhrases.Count(phrase => lowerText.Contains(phrase.ToLower()));
        if (aiPhraseCount > 0)
        {
            aiScore += aiPhraseCount * 0.15;
            indicators.Add($"وُجدت {aiPhraseCount} عبارة مميزة للـ AI");
        }

        // 2. فحص الكلمات الرسمية المفرطة
        var words = Regex.Split(text, @"\W+").Where(w => w.Length > 0).ToArray();
        var formalWordCount = words.Count(word => 
            _formalWords.Any(fw => fw.Equals(word, StringComparison.OrdinalIgnoreCase)));
        
        if (words.Length > 0)
        {
            var formalRatio = (double)formalWordCount / words.Length;
            if (formalRatio > 0.05) // أكثر من 5% كلمات رسمية
            {
                aiScore += formalRatio * 0.3;
                indicators.Add($"نسبة عالية من الكلمات الرسمية: {formalRatio:P0}");
            }
        }

        // 3. فحص طول الجمل
        var sentences = Regex.Split(text, @"[.!?]+").Where(s => s.Trim().Length > 0).ToArray();
        if (sentences.Length > 0)
        {
            var avgSentenceLength = sentences.Average(s => s.Split(' ').Length);
            
            // AI غالباً بتكتب جمل متوسطة الطول (15-25 كلمة)
            if (avgSentenceLength >= 15 && avgSentenceLength <= 25)
            {
                aiScore += 0.1;
                indicators.Add($"متوسط طول الجملة متسق ({avgSentenceLength:F1} كلمة)");
            }

            // فحص التناسق في طول الجمل
            var sentenceLengths = sentences.Select(s => s.Split(' ').Length).ToArray();
            var stdDev = CalculateStandardDeviation(sentenceLengths);
            if (stdDev < 5) // تناسق عالي
            {
                aiScore += 0.1;
                indicators.Add("تناسق عالي في طول الجمل");
            }
        }

        // 4. فحص التكرار
        var wordFrequency = words
            .Where(w => w.Length > 4)
            .GroupBy(w => w.ToLower())
            .Select(g => new { Word = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var maxRepetition = wordFrequency.FirstOrDefault()?.Count ?? 0;
        if (maxRepetition > 5 && words.Length > 50)
        {
            aiScore += 0.08;
            indicators.Add($"تكرار مرتفع لبعض الكلمات");
        }

        // 5. إضافة عشوائية للواقعية
        var random = new Random(text.GetHashCode()); // نفس النص = نفس النتيجة
        var randomFactor = (random.NextDouble() - 0.5) * 0.2; // ±10%
        
        // حساب النتيجة النهائية
        var finalProbability = Math.Clamp(aiScore + randomFactor, 0.0, 1.0);
        var confidence = CalculateConfidence(indicators.Count, text.Length);

        stopwatch.Stop();

        return new AiDetectionResult
        {
            AiProbability = finalProbability,
            IsAiGenerated = finalProbability > 0.5,
            Confidence = confidence,
            Details = GenerateTextAnalysisDetails(text, finalProbability, indicators),
            Indicators = indicators,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// تحليل صورة للكشف عن الذكاء الاصطناعي
    /// </summary>
    public async Task<AiDetectionResult> DetectImageAsync(string imagePath)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await Task.Delay(150); // محاكاة وقت المعالجة

        var indicators = new List<string>();
        double aiScore = 0.4; // قيمة أساسية

        try
        {
            var fileInfo = new FileInfo(imagePath);
            
            // 1. فحص حجم الملف
            var fileSizeKb = fileInfo.Length / 1024.0;
            if (fileSizeKb > 1000 && fileSizeKb < 3000)
            {
                aiScore += 0.1;
                indicators.Add("حجم الملف مناسب لصور AI (1-3 MB)");
            }

            // 2. فحص امتداد الملف
            var extension = fileInfo.Extension.ToLower();
            if (extension == ".png" || extension == ".webp")
            {
                aiScore += 0.05;
                indicators.Add($"امتداد الملف ({extension}) شائع في صور AI");
            }

            // 3. إضافة عشوائية بناءً على اسم الملف
            var random = new Random(imagePath.GetHashCode());
            var randomFactor = (random.NextDouble() - 0.5) * 0.3;
            
            var finalProbability = Math.Clamp(aiScore + randomFactor, 0.0, 1.0);
            var confidence = CalculateConfidence(indicators.Count, 3);

            stopwatch.Stop();

            return new AiDetectionResult
            {
                AiProbability = finalProbability,
                IsAiGenerated = finalProbability > 0.5,
                Confidence = confidence,
                Details = $"تم تحليل الصورة ({fileSizeKb:F1} KB). " +
                         $"احتمالية AI: {finalProbability:P0}. " +
                         (finalProbability > 0.7 ? "مؤشرات قوية على محتوى AI." :
                          finalProbability > 0.5 ? "مؤشرات متوسطة على محتوى AI." :
                          "مؤشرات ضعيفة على محتوى AI."),
                Indicators = indicators,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AiDetectionResult
            {
                AiProbability = 0,
                IsAiGenerated = false,
                Confidence = 0,
                Details = $"خطأ في تحليل الصورة: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// تحليل فيديو للكشف عن الذكاء الاصطناعي
    /// </summary>
    public async Task<AiDetectionResult> DetectVideoAsync(string videoPath)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await Task.Delay(200); // محاكاة وقت المعالجة الأطول

        var indicators = new List<string>();
        double aiScore = 0.35; // قيمة أساسية

        try
        {
            var fileInfo = new FileInfo(videoPath);
            
            // 1. فحص حجم الملف
            var fileSizeMb = fileInfo.Length / (1024.0 * 1024.0);
            if (fileSizeMb > 5 && fileSizeMb < 50)
            {
                aiScore += 0.1;
                indicators.Add($"حجم الفيديو ({fileSizeMb:F1} MB) مناسب لفيديوهات AI");
            }

            // 2. فحص الامتداد
            var extension = fileInfo.Extension.ToLower();
            if (extension == ".mp4")
            {
                aiScore += 0.05;
                indicators.Add("صيغة MP4 شائعة في فيديوهات AI");
            }

            // 3. إضافة عشوائية بناءً على اسم الملف
            var random = new Random(videoPath.GetHashCode());
            var randomFactor = (random.NextDouble() - 0.5) * 0.3;
            
            var finalProbability = Math.Clamp(aiScore + randomFactor, 0.0, 1.0);
            var confidence = CalculateConfidence(indicators.Count, 2);

            stopwatch.Stop();

            return new AiDetectionResult
            {
                AiProbability = finalProbability,
                IsAiGenerated = finalProbability > 0.5,
                Confidence = confidence,
                Details = $"تم تحليل الفيديو ({fileSizeMb:F1} MB). " +
                         $"احتمالية AI: {finalProbability:P0}. " +
                         (finalProbability > 0.7 ? "مؤشرات قوية على محتوى AI." :
                          finalProbability > 0.5 ? "مؤشرات متوسطة على محتوى AI." :
                          "مؤشرات ضعيفة على محتوى AI."),
                Indicators = indicators,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AiDetectionResult
            {
                AiProbability = 0,
                IsAiGenerated = false,
                Confidence = 0,
                Details = $"خطأ في تحليل الفيديو: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    // Helper Methods

    private double CalculateStandardDeviation(int[] values)
    {
        if (values.Length == 0) return 0;
        
        var avg = values.Average();
        var sumOfSquares = values.Sum(val => Math.Pow(val - avg, 2));
        return Math.Sqrt(sumOfSquares / values.Length);
    }

    private double CalculateConfidence(int indicatorCount, int contentLength)
    {
        // كلما زادت المؤشرات وطول المحتوى، زادت الثقة
        var baseConfidence = 0.6;
        var indicatorBonus = Math.Min(indicatorCount * 0.08, 0.3);
        var lengthBonus = contentLength > 100 ? 0.1 : 0.0;
        
        return Math.Clamp(baseConfidence + indicatorBonus + lengthBonus, 0.0, 0.95);
    }

    private string GenerateTextAnalysisDetails(string text, double probability, List<string> indicators)
    {
        var words = Regex.Split(text, @"\W+").Where(w => w.Length > 0).ToArray();
        var sentences = Regex.Split(text, @"[.!?]+").Where(s => s.Trim().Length > 0).ToArray();
        
        var details = $"تحليل النص: {words.Length} كلمة، {sentences.Length} جملة. ";
        details += $"احتمالية AI: {probability:P0}. ";
        
        if (probability > 0.7)
            details += "المحتوى يحتوي على مؤشرات قوية على أنه مُنشأ بالذكاء الاصطناعي.";
        else if (probability > 0.5)
            details += "المحتوى يحتوي على بعض المؤشرات على أنه مُنشأ بالذكاء الاصطناعي.";
        else if (probability > 0.3)
            details += "المحتوى يحتوي على مؤشرات ضعيفة على أنه مُنشأ بالذكاء الاصطناعي.";
        else
            details += "المحتوى على الأرجح مكتوب بواسطة إنسان.";

        if (indicators.Count > 0)
        {
            details += $" المؤشرات المكتشفة: {string.Join(", ", indicators)}.";
        }

        return details;
    }
}
