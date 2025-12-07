# AI Content Detector - Backend API

🔍 Backend API لنظام كشف المحتوى المُنشأ بالذكاء الاصطناعي - مشروع تخرج

## 📋 نظرة عامة

هذا API يوفر خدمات كشف ما إذا كان المحتوى (نص، صورة، أو فيديو) مُنشأ بواسطة الذكاء الاصطناعي أم لا.

**Features:**
- ✅ تحليل نصوص باستخدام pattern detection ذكي
- ✅ رفع وتحليل صور
- ✅ رفع وتحليل فيديوهات
- ✅ نظام authentication كامل
- ✅ تاريخ التحليلات لكل مستخدم
- ✅ Mock AI service جاهز (يمكن استبداله بموديل حقيقي)
- ✅ Clean Architecture
- ✅ Swagger UI للتجربة

## 🏗️ Architecture

المشروع يستخدم Clean Architecture مع الطبقات التالية:

```
├── API             # Controllers & Configuration
├── Application     # DTOs, Interfaces, Models
├── Domain          # Entities & Business Logic
└── Infrastructure  # Database, Repositories, Services
```

## 🚀 Quick Start

### المتطلبات

- .NET 8.0 SDK
- SQL Server (أو يمكن تعديله لـ SQLite)

### التشغيل

1. **Clone the repository:**
   ```bash
   cd AI__Detector__Server
   ```

2. **Update connection string** في `API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=AI_Detector_DB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
   }
   ```

3. **Build & Run:**
   ```bash
   dotnet build
   dotnet run --project API/API.csproj
   ```

4. **Open Swagger UI:**
   ```
   http://localhost:5050/swagger
   ```

## 📡 API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | تسجيل مستخدم جديد |
| POST | `/api/auth/login` | تسجيل دخول |
| GET | `/api/auth/user/{id}` | معلومات المستخدم |

### Analysis

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/analysis/analyze` | تحليل محتوى (نص/صورة/فيديو) |
| GET | `/api/analysis/history/{userId}` | تاريخ التحليلات |
| GET | `/api/analysis/{id}` | تفاصيل تحليل محدد |
| GET | `/api/analysis/content/{id}` | المحتوى الأصلي |

## 💡 Usage Examples

### تسجيل مستخدم جديد

```bash
curl -X POST "http://localhost:5050/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Ahmed Mohamed",
    "email": "ahmed@example.com",
    "password": "SecurePass123"
  }'
```

### تحليل نص

```bash
curl -X POST "http://localhost:5050/api/analysis/analyze" \
  -F "userId=your-user-guid" \
  -F "contentType=1" \
  -F "textContent=As an AI language model, I can help you..."
```

### تحليل صورة

```bash
curl -X POST "http://localhost:5050/api/analysis/analyze" \
  -F "userId=your-user-guid" \
  -F "contentType=2" \
  -F "file=@/path/to/image.jpg"
```

### الحصول على التاريخ

```bash
curl -X GET "http://localhost:5050/api/analysis/history/your-user-guid"
```

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  },
  "FileUpload": {
    "BaseUrl": "http://localhost:5050",
    "MaxImageSizeMB": 10,
    "MaxVideoSizeMB": 100,
    "AllowedImageExtensions": [ ".jpg", ".jpeg", ".png", ".gif", ".webp" ],
    "AllowedVideoExtensions": [ ".mp4", ".mpeg", ".mov", ".avi" ]
  }
}
```

## 🧪 Mock AI Detection

حالياً يستخدم النظام `MockAiDetectionService` الذي يحاكي نموذج ML حقيقي:

### للنصوص:
- كشف عبارات مميزة للـ AI
- تحليل نسبة الكلمات الرسمية
- فحص طول الجمل وتناسقها
- كشف التكرار المفرط

### للصور والفيديوهات:
- تحليل حجم وامتداد الملف
- نتائج واقعية مع عشوائية محكومة

## 🔄 Integrating Real ML Model

عند جاهزية نموذج الـ ML الحقيقي:

1. **Create new service:**
   ```csharp
   public class RealAiDetectionService : IAiDetectionService
   {
       // Implement methods with real ML calls
   }
   ```

2. **Update Program.cs:**
   ```csharp
   // Replace:
   builder.Services.AddScoped<IAiDetectionService, MockAiDetectionService>();
   
   // With:
   builder.Services.AddScoped<IAiDetectionService, RealAiDetectionService>();
   ```

**لا تحتاج تغيير أي شيء آخر!** ✨

## 📊 Database Schema

### Users
- Id (Guid, PK)
- FullName, Email, PasswordHash
- CreatedAt, UpdatedAt, IsActive

### Analyses
- Id (Guid, PK)
- UserId (FK)
- ContentType (Text=1, Image=2, Video=3)
- Content (text or file URL)
- AiProbability (0-1)
- IsAiGenerated (bool)
- Details (JSON)
- AnalyzedAt, Status

## 🌐 CORS Configuration

الـ API مُعد للعمل مع:
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)
- `https://authenticity-checker-mhc4.vercel.app` (Production)

## 📁 Project Structure

```
AI__Detector__Server/
├── API/
│   ├── Controllers/
│   │   ├── AnalysisController.cs
│   │   └── AuthController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── wwwroot/uploads/
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   │   └── IAiDetectionService.cs
│   └── Models/
│       └── AiDetectionResult.cs
├── Domain/
│   ├── Entities/
│   └── Enums/
└── Infrastructure/
    ├── Data/
    ├── Repositories/
    └── Services/
        ├── FileService.cs
        └── MockAiDetectionService.cs
```

## 🔐 Security

- Passwords are hashed using BCrypt
- CORS configured for specific origins
- File type validation
- File size limits

## 📝 Development Notes

- **Port:** 5050
- **Environment:** Development
- **Database:** SQL Server (EF Core)
- **Logging:** Console + File

## 🐛 Troubleshooting

### Database Connection Error
```bash
# تأكد من أن SQL Server شغّال
# تأكد من صحة connection string في appsettings.json
```

### CORS Error
```bash
# تأكد من إضافة origin الخاص بك في Program.cs
```

### File Upload Error
```bash
# تأكد من وجود folder wwwroot/uploads
# تأكد من صحة file size و type
```

## 🚀 Deployment

### Update for Production:

1. Update `appsettings.json`:
   ```json
   {
     "FileUpload": {
       "BaseUrl": "https://your-domain.com"
     }
   }
   ```

2. Set environment variables:
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   ```

3. Update CORS in `Program.cs`

## 📖 Documentation

- **Swagger UI:** `http://localhost:5050/swagger`
- **Walkthrough:** Check `walkthrough.md` in artifacts
- **Implementation Plan:** Check `implementation_plan.md` in artifacts

## 🤝 Contributing

هذا مشروع تخرج. للمساهمة:
1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## 📧 Contact

للأسئلة والدعم، تواصل مع فريق المشروع.

## 📄 License

This project is for educational purposes (Graduation Project).

---

**Built with ❤️ using .NET 8.0 & Clean Architecture**
