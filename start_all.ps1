# ======================================
# Startuje ceo projekat: Chat, Document, Gateway, LLM i Frontend
# ======================================

# Funkcija za otvaranje novog terminala i pokretanje komande
function Start-NewWindow($name, $command, $workingDir) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command `"cd '$workingDir'; $command`"" -WindowStyle Normal -WorkingDirectory $workingDir
    Write-Host "$name started in new terminal window."
}

# Putanje do servisa (meni primer)
$chatServicePath      = "C:\Users\User\Desktop\klaud\ChatService"
$documentServicePath  = "C:\Users\User\Desktop\klaud\DocumentService"
$gatewayServicePath   = "C:\Users\User\Desktop\klaud\GatewayService"
$llmPath              = "C:\Users\User\Desktop\klaud\llm"
$frontendPath         = "C:\Users\User\Desktop\klaud\frontend"

# 1️⃣ Pokreni ChatService
Start-NewWindow "ChatService" "dotnet run --launch-profile http" $chatServicePath

# 2️⃣ Pokreni DocumentService
Start-NewWindow "DocumentService" "dotnet run --launch-profile http" $documentServicePath

# 3️⃣ Pokreni GatewayService
Start-NewWindow "GatewayService" "dotnet run --launch-profile http" $gatewayServicePath

# 4️⃣ Pokreni LLM FastAPI
Start-NewWindow "LLM" ".\.venv\Scripts\activate; pip install -r requirements.txt; uvicorn main:app --host 0.0.0.0 --port 5070 --reload" $llmPath

# 5️⃣ Pokreni Frontend
Start-NewWindow "Frontend" "npm install; npm run dev" $frontendPath

Write-Host "All services are starting..."
