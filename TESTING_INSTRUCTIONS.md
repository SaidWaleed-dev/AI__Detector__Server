# 🤖 AI Detector API — Testing Guide

> **Base URL:** `http://localhost:5050`  
> **Swagger UI:** `http://localhost:5050/swagger`  
> **Postman Collection:** `AI_Detector_Full.postman_collection.json`

---

## 📋 Table of Contents
1. [Core Requirements (Important)](#core-requirements)
2. [Quick Start](#quick-start)
3. [Auth Endpoints](#auth-endpoints)
4. [Detection Endpoints](#detection-endpoints)
5. [Postman Setup](#postman-setup)
6. [Swagger Setup](#swagger-setup)
7. [ContentType Reference](#contenttype-reference)
8. [Response Schemas](#response-schemas)

---

## ⚠️ Core Requirements (Important)

حتى تعمل عملية فحص النصوص بنجاح، يجب أن يكون **سيرفر نماذج الذكاء الاصطناعي (Python)** الملحق بالمشروع يعمل بالتوازي مع سيرفر الـ C#.

**خطوات تشغيل سيرفر الـ Python (يتم لمرة واحدة):**
1. افتح Terminal وتوجه لمجلد `ML_Models`.
2. قم بإنشاء بيئة وهمية ותفعيلها: `python3 -m venv venv` ثم `source venv/bin/activate` (أو `venv\Scripts\activate` في الويندوز).
3. قم بتثبيت المكاتب: `pip install -r requirements.txt`.
4. شغل السيرفر المصغر: `uvicorn main:app --port 8000`.

*ملاحظة:* إذا لم يكن سيرفر البايثون يعمل، سيقوم سيرفر الـ C# بإرجاع رسالة خطأ تفيد بفشل الاتصال بنموذج الذكاء الاصطناعي.

---

## ⚡ Quick Start

### Option A — Register & Use
```
POST /api/auth/register   →  بتاخد token و userId
POST /api/detect  →  تبعت الـ userId بالإضافة للنص
```

### Option B — Guest Mode (بدون تسجيل)
```
POST /api/auth/guest  →  بتاخد guest token (صالح لساعتين) و userId
POST /api/detect  →  تبعت الـ userId الخاص بالضيف بالإضافة للنص
```
*(ملاحظة: حساب الضيف يتم حفظه في قاعدة البيانات لتتمكن من استخدامه في الفحص فوراً).*

---

## 🔐 Auth Endpoints

### 1️⃣ Register — `POST /api/auth/register`
**No Auth Required**
```json
// Body (JSON)
{
    "fullName": "Ahmed Mohamed",
    "email": "ahmed@example.com",
    "password": "Test@123456"
}
```

**Success 200:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "k7fP2mX9nQ1rT4wY...",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "ahmed@example.com",
    "fullName": "Ahmed Mohamed"
}
```

---

### 2️⃣ Continue as Guest — `POST /api/auth/guest`
**No Auth Required** | **No Body Required**

**Success 200:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Guest",
    "isGuest": true,
    "message": "You are browsing as a guest. Data will not be saved.",
    "expiresInMinutes": 120
}
```

---

*باقي الـ Auth Endpoints كما هي (Login, Logout, Forgot Password, Google Auth).*

---

## 🧪 Detection Endpoints

> ⚠️ **تنويه:** الـ Detection API أصبح الآن **مفتوحاً (AllowAnonymous)** لتسهيل التجربة السريعة، ولا يتطلب حاليًا `Authorization: Bearer <token>`.
> 
> Content-Type: **`multipart/form-data`** (مش JSON!)

### 📝 Detect Text — `POST /api/detect` أو `POST /api/detection/detect`

```
Form Data:
├── userId        = 3fa85f64-5717-4562-b3fc-2c963f66afa6  (GUID)
├── contentType   = 1                                        (Text)
└── textContent   = "your text here..."
```

**Mock Data للتست:**

```
AI-written English: 
"In the realm of artificial intelligence, the utilization of machine learning algorithms has revolutionized the way we process and analyze data. Neural networks, particularly deep learning architectures, have demonstrated remarkable capabilities in pattern recognition, natural language processing, and computer vision tasks."

AI-written Arabic:
"في ظل التطورات المتسارعة التي يشهدها العالم اليوم، أصبحت التكنولوجيا الذكية جزءاً لا يتجزأ من حياتنا اليومية، بل وباتت تشكل ركيزة أساسية في بناء المجتمعات الحديثة. وعلاوة على ذلك، فإن استخدام الذكاء الاصطناعي كنموذج لغوي ساهم في إثراء المحتوى المعرفي، ومن المهم أن نلاحظ تأثيراته الواضحة في شتى المجالات."

Human-written Arabic:
"يا جدع انا مش عارف ليه الكود ده مش شغال خالص! بعمل debug من الصبح ولقيت ان المشكلة في الـ null reference exception في الـ repository."
```

---

### 🖼️ Detect Image / Video / Audio — `POST /api/detect`

```
Form Data:
├── userId        = <your-user-id>
├── contentType   = 2 (Image) أو 3 (Video) أو 4 (Audio)
└── file          = [select file]
```

*(تنويه: تحليل الوسائط حالياً يعتمد على Mock Logic لحين ربط النماذج الحقيقية الخاصة بها).*

---

### 📋 Detection History — `GET /api/detection/history/{userId}` 🔒

**Auth Required:** لا تحتاج `token` حالياً لسهولة التستنج، يتم جلبها عن طريق تمرير `userId`.

**Success 200:**
```json
[
    {
        "contentId": "guid",
        "contentType": 1,
        "data": "The text that was analyzed...",
        "uploadedAt": "2024-01-15T10:35:00Z",
        "results": [
            {
                "resultId": "guid",
                "modelName": "Mock-Detector-v1",
                "aiProbability": 0.9995,
                "isAiGenerated": true,
                "details": "تم التحليل بواسطة نموذج AR. النص على الأرجح من إنشاء الذكاء الاصطناعي.",
                "analyzedAt": "2024-01-15T10:35:01Z"
            }
        ]
    }
]
```

---

## 📮 Postman Setup

### تشغيل الـ Collection
1. افتح **Variables** وقم بالتأكد من `baseUrl` (`http://localhost:5050`).
2. قم بتشغيل طلب **Continue as Guest** (رقم 6) أو **Register**.
3. *الـ Postman سيقوم آلياً بتخزين `userId` الذي سنستخدمه في الفحص.*
4. اتجه لمجلد Detection وجرب إرسال النصوص للحصول على ردود موديل الـ ML الحقيقية.

---

*📅 Updated: March 2026 | AI Detector Server v2.0 with Real ML Models Integration*
