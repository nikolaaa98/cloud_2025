#!/bin/bash
# 🚀 Pokretanje kompletne Cloud mikroservis aplikacije
# Autor: Nikola Miljković
# Datum: 2025-10-11

# Fail immediately on error
set -e

echo "==============================================="
echo "   🚀 Pokrećem Cloud mikroservisnu aplikaciju..."
echo "==============================================="

# 1️⃣ Pokreni Python LLM servis
echo "[1/5] Pokrećem LLM servis (FastAPI)..."
cd llm
if ! command -v uvicorn &> /dev/null; then
  echo "⚠️  Uvicorn nije instaliran — instaliram FastAPI i Uvicorn..."
  pip install fastapi uvicorn
fi
nohup uvicorn main:app --host 0.0.0.0 --port 8000 > ../logs/llm.log 2>&1 &
LLM_PID=$!
echo "✅ LLM servis pokrenut na portu 8000 (PID: $LLM_PID)"
cd ..

# 2️⃣ Pokreni DocumentService (.NET)
echo "[2/5] Pokrećem DocumentService..."
cd DocumentService
dotnet run --launch-profile "http" > ../logs/document.log 2>&1 &
DOC_PID=$!
echo "✅ DocumentService pokrenut (PID: $DOC_PID)"
cd ..

# 3️⃣ Pokreni ChatService (.NET)
echo "[3/5] Pokrećem ChatService..."
cd ChatService
dotnet run --launch-profile "http" > ../logs/chat.log 2>&1 &
CHAT_PID=$!
echo "✅ ChatService pokrenut (PID: $CHAT_PID)"
cd ..

# 4️⃣ Pokreni GatewayService (.NET)
echo "[4/5] Pokrećem GatewayService..."
cd GatewayService
dotnet run --launch-profile "http" > ../logs/gateway.log 2>&1 &
GATE_PID=$!
echo "✅ GatewayService pokrenut (PID: $GATE_PID)"
cd ..

# 5️⃣ Pokreni frontend (Vite)
echo "[5/5] Pokrećem Frontend (Vite React)..."
cd frontend
if [ ! -d "node_modules" ]; then
  echo "📦 Instaliram npm zavisnosti..."
  npm install
fi
nohup npm run dev > ../logs/frontend.log 2>&1 &
FRONT_PID=$!
echo "✅ Frontend pokrenut (PID: $FRONT_PID)"
cd ..

# 🧩 Prikaz informacija o sistemu
echo "==============================================="
echo "✅ Svi servisi su uspešno pokrenuti!"
echo "-----------------------------------------------"
echo "LLM service:        http://localhost:8000"
echo "DocumentService:    http://localhost:5100"
echo "ChatService:        http://localhost:5072"
echo "GatewayService:     http://localhost:36444"
echo "Frontend:           http://localhost:5173"
echo "-----------------------------------------------"
echo "Log fajlovi su u folderu: ./logs"
echo "==============================================="

# Čuvanje PID-ova za kasnije gašenje
echo "$LLM_PID" > .pids
echo "$DOC_PID" >> .pids
echo "$CHAT_PID" >> .pids
echo "$GATE_PID" >> .pids
echo "$FRONT_PID" >> .pids

echo "💾 PID fajl kreiran (.pids)"
