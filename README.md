# RUN ServiceFabric
```bash
Get-Command *ServiceFabric*
cd "C:\Program Files\Microsoft SDKs\Service Fabric\ClusterSetup"
Connect-ServiceFabricCluster -ConnectionEndpoint "localhost:19000"
Get-ServiceFabricClusterHealth
Get-ServiceFabricNode
```

# RUN SERVERS : 

```bash
cd ChatService
dotnet run --launch-profile http
cd DocumentService
dotnet run --launch-profile http
cd GatewayService
dotnet run --launch-profile http
```

# RUN LLM 
```bash
cd llm
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 5070 --reload
```

# RUN FRONTEND
```bash
cd frontend
npm install
npm run dev
```