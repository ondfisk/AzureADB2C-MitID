#!/bin/bash

# Get folder of script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

SUBSCRIPTION_ID="2b554ca5-5009-4849-9b8b-730a9820de6a"
RESOURCE_GROUP_NAME="ondfiskb2c"
LOCATION="swedencentral"
FUNCTION_APP_NAME="ondfiskb2c"

# Set subscription
az account set --subscription $SUBSCRIPTION_ID

# Create resource group
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION

# Deploy resources
az deployment group create \
  --name "ondfiskb2c" \
  --resource-group $RESOURCE_GROUP_NAME \
  --template-file "$DIR/azuredeploy.bicep" \
  --parameters @"$DIR/azuredeploy.parameters.json"

# Get function app managed identity
APP_ROLE="User.ReadWrite.All"
MANAGED_IDENTITY=$(az functionapp identity show --resource-group $RESOURCE_GROUP_NAME --name $FUNCTION_APP_NAME --query principalId --output tsv)
MICROSOFT_GRAPH=$(az rest --method GET --uri "https://graph.microsoft.com/v1.0/servicePrincipals?\$filter=displayName+eq+'Microsoft+Graph'" --query "value[].id" --output tsv)
APP_ROLE_ID=$(az rest --method GET --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$MICROSOFT_GRAPH" --query "appRoles[?value=='$APP_ROLE'].id" --output tsv)
CURRENT=$(az rest --method GET --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$MANAGED_IDENTITY/appRoleAssignments" --query "value[?appRoleId=='$APP_ROLE_ID'].id" --output tsv)

if [ -z $CURRENT ]; then
    az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$MANAGED_IDENTITY/appRoleAssignments" --body "{\"principalId\":\"$MANAGED_IDENTITY\",\"resourceId\":\"$MICROSOFT_GRAPH\",\"appRoleId\":\"$APP_ROLE_ID\"}"
fi
