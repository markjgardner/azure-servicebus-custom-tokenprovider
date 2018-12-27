variable "name" {
  description = "The general application name to be used when constructing the infrastructure"
}

variable "location" {
  description = "Specifies which region the resources should be created in"
  default     = "eastus2"
}
