param (
    [Parameter(Mandatory=$true)]
    [String]
    $CertificateName,

    [Parameter(Mandatory=$true)]
    [SecureString]
    $Password
)

$path = "$env:TMP\$CertificateName.pfx"

# Create a new self-signed certificate
$certificate = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My" -Subject "CN=$CertificateName"

# Export the certificate as a PFX file
Export-PfxCertificate -Cert $certificate -FilePath $path -Password $Password

# Display the thumbprint and other details of the generated certificate
Write-Host "Certificate Thumbprint: $($certificate.Thumbprint)"
Write-Host "Certificate Subject: $($certificate.Subject)"
Write-Host "Certificate has been exported to: $path"

Remove-Item -Path Cert:\CurrentUser\My\$($certificate.Thumbprint) -DeleteKey
