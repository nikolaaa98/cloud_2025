from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import requests
import json

app = FastAPI(title="AI Chat Service")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

class Message(BaseModel):
    prompt: str

# 🎯 BESPLATNI AI PROVIDER-I (izaberi jedan):
AI_PROVIDERS = {
    "huggingface": "https://api-inference.huggingface.co/models/microsoft/DialoGPT-large",
    "deepinfra": "https://api.deepinfra.com/v1/openai/chat/completions",
    "openrouter": "https://openrouter.ai/api/v1/chat/completions",
    "localai": "http://localhost:8080/v1/chat/completions"  # ako imaš LocalAI
}

# ✅ NAJJEDNOSTAVNIJI - Koristi Hugging Face Inference API
def get_ai_response_simple(prompt: str) -> str:
    """Najjednostavniji način - HTTP request"""
    try:
        # Možeš koristiti bilo koji public AI API
        response = requests.post(
            "https://api.deepinfra.com/v1/inference/microsoft/DialoGPT-large",
            headers={
                "Authorization": "Bearer HF_TOKEN",  # opcionalno
                "Content-Type": "application/json"
            },
            json={
                "inputs": prompt,
                "parameters": {"max_length": 100}
            },
            timeout=30
        )
        
        if response.status_code == 200:
            result = response.json()
            return result[0].get('generated_text', 'No response')
        else:
            return f"AI API error: {response.status_code}"
            
    except Exception as e:
        return f"AI service error: {str(e)}"

# ✅ BOLJI NAČIN - Simuliraj pametan chatbot
def get_smart_response(prompt: str) -> str:
    """Simuliraj inteligentan odgovor bez pravog AI"""
    prompt_lower = prompt.lower()
    
    # Prosta pravila-based AI
    if "glavni grad" in prompt_lower:
        if "srbij" in prompt_lower:
            return "Glavni grad Srbije je Beograd."
        elif "bugarsk" in prompt_lower:
            return "Glavni grad Bugarske je Sofija."
        elif "englesk" in prompt_lower:
            return "Glavni grad Engleske je London."
        elif "amerik" in prompt_lower or "sad" in prompt_lower:
            return "Glavni grad SAD je Vašington."
        else:
            return "Glavni grad kog grada te zanima?"
    
    elif "zdravo" in prompt_lower or "ćao" in prompt_lower or "hello" in prompt_lower:
        return "Zdravo! Kako mogu da ti pomognem danas?"
    
    elif "kako si" in prompt_lower:
        return "Hvala, odlično! Radim kao AI asistent. Kako si ti?"
    
    elif "ime" in prompt_lower:
        return "Ja sam AI asistent. Kako se ti zoveš?"
    
    elif "hvala" in prompt_lower:
        return "Nema na čemu! Drago mi je što sam mogao da pomognem."
    
    elif "vreme" in prompt_lower:
        return "Žao mi je, nemam pristup trenutnim vremenskim podacima."
    
    elif "matematik" in prompt_lower or "račun" in prompt_lower:
        if "2+2" in prompt:
            return "2 + 2 = 4"
        elif "5*5" in prompt:
            return "5 × 5 = 25"
        else:
            return "Mogu da pomognem sa jednostavnim matematičkim pitanjima!"
    
    else:
        # Generički odgovori za sve ostalo
        responses = [
            f"Zanimljivo pitanje: '{prompt}'. Kako bi ti odgovorio/odgovorila na to?",
            f"Razmišljam o tvom pitanju: '{prompt}'. Šta misliš o ovoj temi?",
            f"To je dobro pitanje! '{prompt}' je važna tema za razgovor.",
            f"Volim kada me pitaš o '{prompt}'. Šta još te zanima?",
            f"Hmm, '{prompt}'... Šta bi ti rekao/rekla na to?",
            f"Interesantno! Pitao/la si me o '{prompt}'. Da li imaš još pitanja?",
        ]
        import random
        return random.choice(responses)

@app.get("/")
async def root():
    return {"ok": True, "service": "AI Chat", "type": "Rule-based Smart Assistant"}

@app.post("/chat")
async def chat(message: Message):
    print(f"💬 Received: {message.prompt}")
    
    # 🎯 IZABERI JEDAN OD OVA DVA NAČINA:
    
    # 1. PAMETNI SIMULATOR (preporučujem)
    response = get_smart_response(message.prompt)
    
    # 2. ILI pravi AI API (ako želiš)
    # response = get_ai_response_simple(message.prompt)
    
    print(f"🤖 Response: {response}")
    return {"response": response}

@app.get("/test")
async def test_chat():
    test_response = get_smart_response("Zdravo, kako si?")
    return {"test_response": test_response}

@app.get("/health")
async def health():
    return {"status": "healthy", "ai_type": "rule_based_smart_assistant"}

# Test multiple questions
@app.get("/test-all")
async def test_all():
    test_questions = [
        "Zdravo",
        "Koji je glavni grad Srbije",
        "Kako si?",
        "Koliko je 2+2",
        "Šta misliš o vremenu",
        "Hvala ti"
    ]
    
    results = []
    for question in test_questions:
        response = get_smart_response(question)
        results.append({"question": question, "response": response})
    
    return {"tests": results}