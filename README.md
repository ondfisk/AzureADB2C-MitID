# Azure AD B2C - MitID integration

Sample app demonstrating how to use Azure AD B2C with MitID to obtain a validated CPR number and subsequently write back to Azure AD.

This is done to be [NSIS](https://digst.dk/it-loesninger/standarder/nsis/) compliant.

## Prerequisites

Sample requires you to create an account on <https://www.criipto.com/> and create an App Registration on their side. Capture client id and client secret.

## Setup

1. Clone code
1. Search and replace all instances of `ondfiskb2c` with `<your-azure-ad-b2c-tenant-name>`
1. Search and replace all instances of `ondfisk` with `<your-azure-ad-tenant-name>`

## Azure AD

### Create test user

```http
POST https://graph.microsoft.com/v1.0/users

{
    "accountEnabled": true,
    "displayName": "Åge Petersen",
    "givenName": "Åge",
    "surname": "Petersen",
    "mailNickname": "aagep",
    "passwordProfile": {
        "forceChangePasswordNextSignIn": true,
        "forceChangePasswordNextSignInWithMfa": false,
        "password": "..."
    },
    "userPrincipalName": "aagep@ondfisk.dk",
    "jobTitle": "Senior Tester"
}
```

Capture `id`:

- `id`: `e110ccf5-f660-4777-ace0-ed9c56f04981`

### Create app registration for storing user data

```http
POST https://graph.microsoft.com/v1.0/applications/
Content-type: application/json

{
    "displayName": "ondfisk extensions app - DO NOT DELETE",
    "description": "ondfisk extensions app. Reserved for directory extensions. DO NOT DELETE. Used by ondfisk for storing user data.",
    "signInAudience": "AzureADMyOrg",
    "tags": [
        "Directory extensions",
        "Extensions",
        "Extension attributes"
    ]
}
```

Capture `id` and `appId`

- `id`: `c45cf23e-e189-4b24-95d7-8814c4d09736`
- `appId`: `1be97e58-6e49-44ee-a17a-42fd1fc944cf`

### Create service principal for app

```http
POST https://graph.microsoft.com/v1.0/servicePrincipals

{
    "appId": "1be97e58-6e49-44ee-a17a-42fd1fc944cf"
}
```

### Create directory extension definitions

```http
POST https://graph.microsoft.com/v1.0/applications/c45cf23e-e189-4b24-95d7-8814c4d09736/extensionProperties
Content-type: application/json

{
    "name": "civilRegistrationNumber",
    "dataType": "String",
    "targetObjects": [
        "User"
    ]
}
```

```http
POST https://graph.microsoft.com/v1.0/applications/c45cf23e-e189-4b24-95d7-8814c4d09736/extensionProperties
Content-type: application/json

{
    "name": "civilRegistrationNumberValidated",
    "dataType": "DateTime",
    "targetObjects": [
        "User"
    ]
}
```

### Compute civil registration number hash for test user

```powershell
$civilRegistrationNumber = "0905540335"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($civilRegistrationNumber)
$hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
$base64 = [System.Convert]::ToBase64String($hash)
$base64
```

### Update civil registration number hash on test user

```http
PATCH https://graph.microsoft.com/v1.0/users/e110ccf5-f660-4777-ace0-ed9c56f04981

{
    "extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber": "yqaVIggNuNCpgvScLH9GdX0gB3LMo+LUuGc8Jv+bxSc="
}
```

## Azure

Deploy resources:

```bash
./infrastructure/deploy.sh
```

### Azure Functions

Under `/src/Ondfisk.B2C.Functions`, create a `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

Deploy `/src/Ondfisk.B2C.Functions`.

Capture the *default function key* from the `UpdateUser` function.

## Azure AD B2C

1. [Add signing and encryption keys for Identity Experience Framework applications](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications)
1. [Register Identity Experience Framework applications](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#register-identity-experience-framework-applications)
1. Configure App Registration:

    - Name: `jwt.ms`
    - Supported account types: `Accounts in any identity provider or organizational directory (for authenticating users with user flows)`
    - Redirect URI (recommended): `Single-page application (SPA)` `https://jwt.ms`
    - Permissions: `[X]` Grant admin consent to openid and offline_access permissions
    - Authentication/Implicit grant and hybrid flows: `[X]` Access tokens (used for implicit flows)

### Configure Application Insights for Azure AD B2C

Using your newly created Application Insights resource follow this guide:

[Collect Azure Active Directory B2C logs with Application Insights](https://learn.microsoft.com/en-us/azure/active-directory-b2c/troubleshoot-with-application-insights?pivots%253Db2c-custom-policy#see-the-logs-in-application-insights)

### Update policies

Using the data from your Criipto App Registration:

- Update `policies/TrustFrameworkExtensions.xml` with client id
- Create policy key `MitIDClientSecret` with client secret
- Create policy key `FunctionsKey` with function key

### Upload policies

Upload `/policies`.

## Test

You should now be able to test your app registration using:

- Username: Åge29164
- Password: ZXzx11^x
- CPR number: 0905540335
