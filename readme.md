# What is in this repo?

## code
### functionapp
This azure function app exposes a single function ```function1``` which accepts HTTP GET requests and converts them into messages and publishes them to a ServiceBus topic. Requests should provide a ```message``` and ```storeid``` value via query string parameters.

### storeclient
This client app listens to a store specific subscription and logs all messages as they are delivered. The application is a .net core console applicaiton and can be run from any workstation or vm. The application requires three environment variables in order to run:
  * ```STOREID``` - Unique store identifier, acceptable values are 0, 1 or 2 
  * ```SBCONNECTION``` - The service bus connection string
  * ```SBTOPICNAME``` - The service bus topic name associated with the target subscription

```bash
export STOREID="0"
export SBCONNECTION="Endpoint=sb://mynamespace.servicebus.windows.net/;SharedAccessKeyName=functionpolicy;SharedAccessKey=aOc1ONr0NwxNwnwPfJl1oK6YlCUDZIbjnH8kNj9v0vX="
export SBTOPICNAME="functiontop"
dotnet run
```

## terraform
This folder contains the definition for the infrastructure needed to run this example. The definition is written in [terraform](https://www.terraform.io/docs/providers/azurerm/index.html).

The infrastructure is composed of:
  * A function app for running the app described above
  * A servicebus namespace with a single topic
  * Three servicebus subscriptions each with a defined [correlation filter](https://docs.microsoft.com/en-us/azure/service-bus-messaging/topic-filters) that matches on ```CorrelationId == storeid```
  * An application insights instance registered to the function app
  * A storage account used by the function app

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