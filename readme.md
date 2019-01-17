# What is in this repo?

## code
### functionapp
This azure function app exposes a single function ```function1``` which accepts HTTP GET requests and converts them into messages and publishes them to a ServiceBus topic. Requests should provide a ```message``` and ```storeid``` value via query string parameters.

### storeclient
This client app listens to a store specific subscription and logs all messages as they are delivered. The application is a .net core console application and can be run from any workstation or vm. The application requires several environment variables in order to run:
  * ```STOREID``` - Unique store identifier, acceptable values are 0, 1 or 2 
  * ```SBURI``` - The service bus endpoint URI
  * ```SBTOPICNAME``` - The service bus topic name associated with the target subscription
  * ```TOKENPROVIDERURI``` - The URI to the Service Bus token provider
  * ```AADTENANTID``` - The Azure AD tenantID to use when authenticating the user
  * ```CLIENTID``` - The ClientID for the client application to use when authenticating the user

```bash
export STOREID="0"
export SBURI="sb://mynamespace.servicebus.windows.net/"
export SBTOPICNAME="functiontop"
export TOKENPROVIDERURI="https://customtokenprovider.azurewebsites.net/api/getServiceBusToken"
export AADTENANTID="00000000000-0000-0000-0000-0000000000000"
export CLIENTID= "99999999-9999-9999-9999-999999999999"
dotnet run
```

## terraform
This folder contains the definition for the infrastructure needed to run this example. The definition is written in [terraform](https://www.terraform.io/docs/providers/azurerm/index.html).

The infrastructure is composed of:
  * A function app for running the app described above\*
  * A servicebus namespace with a single topic
  * Three servicebus subscriptions each with a defined [correlation filter](https://docs.microsoft.com/en-us/azure/service-bus-messaging/topic-filters) that matches on ```CorrelationId == storeid```
  * An application insights instance registered to the function app
  * A storage account used by the function app

  
\* *Note that the infrastructure definition uses ARM to call an undocumented API for configuring Authentication on the Function App.*

You can deploy the infrastructure by following these steps:
```bash
# authenticate to azure and select the target subscription
az login
az account set -s 0000-00000-00000

# change into the terraform directory
cd ./terraform

# init the terraform provider
terraform init

# generate a plan detailing the necessary infrastructure changes
# NOTE: you will be prompted for inputs
terraform plan -out=plan.tfplan 

# build the infrastructure
terraform apply plan.tfplan
```