provider "azurerm" {}

locals {
  functionAppName = "${var.name}-fn"
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "fnrg" {
  name     = "${var.name}-rg"
  location = "${var.location}"
}

resource "azurerm_storage_account" "fnsa" {
  name                      = "${substr(var.name, 0, min(length(var.name),20))}fnsa"
  resource_group_name       = "${azurerm_resource_group.fnrg.name}"
  location                  = "${var.location}"
  account_tier              = "standard"
  account_replication_type  = "lrs"
  enable_https_traffic_only = "true"
}

resource "azurerm_app_service_plan" "fnasp" {
  name                = "${local.functionAppName}-asp"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  location            = "${var.location}"

  sku {
    tier = "Standard"
    size = "S1"
  }
}

resource "azurerm_function_app" "fnapp" {
  name                      = "${local.functionAppName}"
  resource_group_name       = "${azurerm_resource_group.fnrg.name}"
  location                  = "${var.location}"
  app_service_plan_id       = "${azurerm_app_service_plan.fnasp.id}"
  storage_connection_string = "${azurerm_storage_account.fnsa.primary_connection_string}"
  version                   = "~2"

  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.fnai.instrumentation_key}"
    AzureWebJobsServiceBus         = "${azurerm_servicebus_namespace_authorization_rule.fnsbnpolicy.primary_connection_string}"
    functionTopicName              = "${azurerm_servicebus_topic.fntopic.name}"
  }
}

resource "azurerm_template_deployment" "authsettings" {
  name                = "functionApp_AAD_auth_settings"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  deployment_mode     = "incremental"
  template_body       = "${file("functionapp_authentication_settings.json")}"

  parameters = {
    "functionAppName"              = "${azurerm_function_app.fnapp.name}"
    "servicePrincipalClientId"     = "${var.aadClientId}"
    "servicePrincipalClientSecret" = "${var.aadClientSecret}"
    "aadDirectoryId"               = "${data.azurerm_client_config.current.tenant_id}"
  }
}

resource "azurerm_application_insights" "fnai" {
  name                = "${local.functionAppName}-ai"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  location            = "eastus"
  application_type    = "web"
}

resource "azurerm_servicebus_namespace" "fnsbn" {
  name                = "${var.name}-sn"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  location            = "${var.location}"
  sku                 = "standard"
}

resource "azurerm_servicebus_topic" "fntopic" {
  name                = "functiontop"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  namespace_name      = "${azurerm_servicebus_namespace.fnsbn.name}"
  enable_partitioning = true
}

resource "azurerm_servicebus_namespace_authorization_rule" "fnsbnpolicy" {
  name                = "functionpolicy"
  namespace_name      = "${azurerm_servicebus_namespace.fnsbn.name}"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  listen              = true
  send                = true
  manage              = false
}

resource "azurerm_servicebus_subscription" "fnsub" {
  name                = "storesub${count.index}"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  namespace_name      = "${azurerm_servicebus_namespace.fnsbn.name}"
  topic_name          = "${azurerm_servicebus_topic.fntopic.name}"
  max_delivery_count  = 1
  count               = 3
}

resource "azurerm_servicebus_subscription" "fnsub2" {
  name                = "everything"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  namespace_name      = "${azurerm_servicebus_namespace.fnsbn.name}"
  topic_name          = "${azurerm_servicebus_topic.fntopic.name}"
  max_delivery_count  = 1
}

resource "azurerm_servicebus_subscription_rule" "fnsubrule" {
  name                = "storefilter${count.index}"
  resource_group_name = "${azurerm_resource_group.fnrg.name}"
  namespace_name      = "${azurerm_servicebus_namespace.fnsbn.name}"
  topic_name          = "${azurerm_servicebus_topic.fntopic.name}"
  subscription_name   = "${element(azurerm_servicebus_subscription.fnsub.*.name, count.index)}"
  filter_type         = "CorrelationFilter"

  correlation_filter = {
    correlation_id = "${count.index}"
  }

  count = 3
}
