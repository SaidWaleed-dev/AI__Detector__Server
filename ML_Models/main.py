import os
import torch
from fastapi import FastAPI, HTTPException
from transformers import AutoModelForSequenceClassification, AutoTokenizer


base_dir = os.path.dirname(os.path.abspath(__file__))

print("Loading English model...")
en_path = os.path.join(base_dir, "english_model")
en_tokenizer = AutoTokenizer.from_pretrained(en_path)
en_model = AutoModelForSequenceClassification.from_pretrained(en_path)
en_model.eval()

print("Loading Arabic model...")
ar_path = os.path.join(base_dir, "arabic_model")
ar_tokenizer = AutoTokenizer.from_pretrained(ar_path)
ar_model = AutoModelForSequenceClassification.from_pretrained(ar_path)
ar_model.eval()

app = FastAPI(title="AI Detection Text Models API")

from models.TextRequest import TextRequest
from models.DetectionResponse import DetectionResponse

def predict(text: str, model, tokenizer, ai_index: int):
    inputs = tokenizer(text, return_tensors="pt", truncation=True, max_length=512)
    with torch.no_grad():
        outputs = model(**inputs)
        
    probs = torch.nn.functional.softmax(outputs.logits, dim=-1)
    
    predicted_class_id = probs.argmax().item()
    confidence = probs[0][predicted_class_id].item()
    
    
    ai_prob = probs[0][ai_index].item()
    is_ai = bool(predicted_class_id == ai_index) 

    return {
        "is_ai": is_ai,
        "ai_probability": round(ai_prob, 4),
        "confidence": round(confidence, 4)
    }

@app.post("/predict/en", response_model=DetectionResponse)
async def predict_english(req: TextRequest):
    
    result = predict(req.text, en_model, en_tokenizer, ai_index=0)
    result["language"] = "en"
    return result

@app.post("/predict/ar", response_model=DetectionResponse)
async def predict_arabic(req: TextRequest):
    
    result = predict(req.text, ar_model, ar_tokenizer, ai_index=1)
    result["language"] = "ar"
    return result

@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "models_loaded": ["en", "ar"]
    }

