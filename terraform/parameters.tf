variable "name" {
  description = "The general application name to be used when constructing the infrastructure"
}

variable "location" {
  description = "Specifies which region the resources should be created in"
  default     = "eastus2"
}

variable "aadClientId" {
  description = "The client id to use when registering AAD as the authentication provider for the functionApp"
}

variable "aadClientSecret" {
  description = "The client secret to use when registering AAD as the authentication provider for the functionApp"
}

variable "nativeAppClientId" {
  description = "The client id for the service principal associated with the client application"
}
