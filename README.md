Get-Command *ServiceFabric*
cd "C:\Program Files\Microsoft SDKs\Service Fabric\ClusterSetup"
Connect-ServiceFabricCluster -ConnectionEndpoint "localhost:19000"
Get-ServiceFabricClusterHealth
Get-ServiceFabricNode