# Azure AD B2C - MitID integration

Sample app demonstrating how to use Azure AD B2C with MitID to obtain a validated CPR number and subsequently write back to Azure AD.

This is done to be [NSIS](https://digst.dk/it-loesninger/standarder/nsis/) compliant.

## Setup

1. Clone code
1. Search and replace all instances of `ondfiskb2c` with `<your-tenant-name>`
1. Create Azure AD B2C Tenant
1. Configure App Registration

    - Name: `jwt.ms`
    - Supported account types: `Accounts in any identity provider or organizational directory (for authenticating users with user flows)`
    - Redirect URI (recommended): `Single-page application (SPA)` `https://jwt.ms`
    - Permissions: `[X]` Grant admin consent to openid and offline_access permissions
    - Authentication/Implicit grant and hybrid flows: `[X]` Access tokens (used for implicit flows)

1. [Add signing and encryption keys for Identity Experience Framework applications](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#add-signing-and-encryption-keys-for-identity-experience-framework-applications)
1. [Register Identity Experience Framework applications](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy#register-identity-experience-framework-applications)
1. Configure application <https://www.criipto.com/>:

    - Update `policies/TrustFrameworkExtensions.xml` with client id
    - Create policy key `MitIDClientSecret` with cliet secret

1. Upload custom policies:

    1. `policies/TrustFrameworkBase.xml`
    1. `policies/TrustFrameworkExtensions.xml`
    1. `policies/SignUpOrSignin.xml`

- Username: Åge29164
- Password: ZXzx11^x
- CPR number: 0905540335
