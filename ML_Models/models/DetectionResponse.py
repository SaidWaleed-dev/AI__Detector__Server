from pydantic import BaseModel

class DetectionResponse(BaseModel):
    is_ai: bool
    ai_probability: float
    confidence: float
    language: str
