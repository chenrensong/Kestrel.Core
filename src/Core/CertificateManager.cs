// Microsoft.AspNetCore.Certificates.Generation.CertificateManager
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

internal enum CertificatePurpose
{
    All,
    HTTPS
}


internal class CertificateManager
{
    private class UserCancelledTrustException : Exception
    {
    }

    private enum RemoveLocations
    {
        Undefined,
        Local,
        Trusted,
        All
    }

    internal class DetailedEnsureCertificateResult
    {
        public EnsureCertificateResult ResultCode { get; set; }

        public DiagnosticInformation Diagnostics { get; set; } = new DiagnosticInformation();

    }

    internal enum EnsureCertificateResult
    {
        Succeeded = 1,
        ValidCertificatePresent,
        ErrorCreatingTheCertificate,
        ErrorSavingTheCertificateIntoTheCurrentUserPersonalStore,
        ErrorExportingTheCertificate,
        FailedToTrustTheCertificate,
        UserCancelledTrustStep
    }


    internal class DiagnosticInformation
    {
        public IList<string> Messages { get; } = new List<string>();


        public IList<Exception> Exceptions { get; } = new List<Exception>();


        internal void Debug(params string[] messages)
        {
            foreach (string item in messages)
            {
                Messages.Add(item);
            }
        }

        internal string[] DescribeCertificates(params X509Certificate2[] certificates)
        {
            return DescribeCertificates(certificates.AsEnumerable());
        }

        internal string[] DescribeCertificates(IEnumerable<X509Certificate2> certificates)
        {
            List<string> list = new List<string>();
            list.Add($"'{certificates.Count()}' found matching the criteria.");
            list.Add("SUBJECT - THUMBPRINT - NOT BEFORE - EXPIRES - HAS PRIVATE KEY");
            foreach (X509Certificate2 certificate in certificates)
            {
                list.Add(DescribeCertificate(certificate));
            }
            return list.ToArray();
        }

        private static string DescribeCertificate(X509Certificate2 certificate)
        {
            return $"{certificate.Subject} - {certificate.Thumbprint} - {certificate.NotBefore} - {certificate.NotAfter} - {certificate.HasPrivateKey}";
        }

        internal void Error(string preamble, Exception e)
        {
            Messages.Add(preamble);
            if (Exceptions.Count <= 0 || Exceptions[Exceptions.Count - 1] != e)
            {
                for (Exception ex = e; ex != null; ex = ex.InnerException)
                {
                    Messages.Add("Exception message: " + ex.Message);
                }
            }
        }
    }

    public const string AspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";

    public const string AspNetHttpsOidFriendlyName = "ASP.NET Core HTTPS development certificate";

    private const string ServerAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";

    private const string ServerAuthenticationEnhancedKeyUsageOidFriendlyName = "Server Authentication";

    private const string LocalhostHttpsDnsName = "localhost";

    private const string LocalhostHttpsDistinguishedName = "CN=localhost";

    public const int RSAMinimumKeySizeInBits = 2048;

    private static readonly TimeSpan MaxRegexTimeout = TimeSpan.FromMinutes(1.0);

    private const string CertificateSubjectRegex = "CN=(.*[^,]+).*";

    private const string MacOSSystemKeyChain = "/Library/Keychains/System.keychain";

    private static readonly string MacOSUserKeyChain = Environment.GetEnvironmentVariable("HOME") + "/Library/Keychains/login.keychain-db";

    private const string MacOSFindCertificateCommandLine = "security";

    private static readonly string MacOSFindCertificateCommandLineArgumentsFormat = "find-certificate -c {0} -a -Z -p /Library/Keychains/System.keychain";

    private const string MacOSFindCertificateOutputRegex = "SHA-1 hash: ([0-9A-Z]+)";

    private const string MacOSRemoveCertificateTrustCommandLine = "sudo";

    private const string MacOSRemoveCertificateTrustCommandLineArgumentsFormat = "security remove-trusted-cert -d {0}";

    private const string MacOSDeleteCertificateCommandLine = "sudo";

    private const string MacOSDeleteCertificateCommandLineArgumentsFormat = "security delete-certificate -Z {0} {1}";

    private const string MacOSTrustCertificateCommandLine = "sudo";

    private static readonly string MacOSTrustCertificateCommandLineArguments = "security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain ";

    private const int UserCancelledErrorCode = 1223;

    public IList<X509Certificate2> ListCertificates(CertificatePurpose purpose, StoreName storeName, StoreLocation location, bool isValid, bool requireExportable = true, DiagnosticInformation diagnostics = null)
    {
        diagnostics?.Debug($"Listing '{purpose.ToString()}' certificates on '{location}\\{storeName}'.");
        List<X509Certificate2> list = new List<X509Certificate2>();
        try
        {
            using X509Store x509Store = new X509Store(storeName, location);
            x509Store.Open(OpenFlags.ReadOnly);
            list.AddRange(x509Store.Certificates.OfType<X509Certificate2>());
            IEnumerable<X509Certificate2> enumerable = list;
            switch (purpose)
            {
                case CertificatePurpose.All:
                    enumerable = enumerable.Where((X509Certificate2 c) => HasOid(c, "1.3.6.1.4.1.311.84.1.1"));
                    break;
                case CertificatePurpose.HTTPS:
                    enumerable = enumerable.Where((X509Certificate2 c) => HasOid(c, "1.3.6.1.4.1.311.84.1.1"));
                    break;
            }
            diagnostics?.Debug(diagnostics.DescribeCertificates(enumerable));
            if (isValid)
            {
                diagnostics?.Debug("Checking certificates for validity.");
                DateTimeOffset now = DateTimeOffset.Now;
                X509Certificate2[] array = enumerable.Where((X509Certificate2 c) => c.NotBefore <= now && now <= c.NotAfter && (!requireExportable || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IsExportable(c))).ToArray();
                IEnumerable<X509Certificate2> certificates = enumerable.Except(array);
                diagnostics?.Debug("Listing valid certificates");
                diagnostics?.Debug(diagnostics.DescribeCertificates(array));
                diagnostics?.Debug("Listing invalid certificates");
                diagnostics?.Debug(diagnostics.DescribeCertificates(certificates));
                enumerable = array;
            }
            enumerable = enumerable.ToList();
            DisposeCertificates(list.Except(enumerable));
            x509Store.Close();
            return (IList<X509Certificate2>)enumerable;
        }
        catch
        {
            DisposeCertificates(list);
            list.Clear();
            return list;
        }
        static bool HasOid(X509Certificate2 certificate, string oid)
        {
            return certificate.Extensions.OfType<X509Extension>().Any((X509Extension e) => string.Equals(oid, e.Oid.Value, StringComparison.Ordinal));
        }
        static bool IsExportable(X509Certificate2 c)
        {
            if (!(c.GetRSAPrivateKey() is RSACryptoServiceProvider rSACryptoServiceProvider) || !rSACryptoServiceProvider.CspKeyContainerInfo.Exportable)
            {
                if (c.GetRSAPrivateKey() is RSACng rSACng)
                {
                    return rSACng.Key.ExportPolicy == CngExportPolicies.AllowExport;
                }
                return false;
            }
            return true;
        }
    }

    private static void DisposeCertificates(IEnumerable<X509Certificate2> disposables)
    {
        foreach (X509Certificate2 disposable in disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
            }
        }
    }

    public X509Certificate2 CreateAspNetCoreHttpsDevelopmentCertificate(DateTimeOffset notBefore, DateTimeOffset notAfter, string subjectOverride, DiagnosticInformation diagnostics = null)
    {
        X500DistinguishedName subject = new X500DistinguishedName(subjectOverride ?? "CN=localhost");
        List<X509Extension> list = new List<X509Extension>();
        SubjectAlternativeNameBuilder subjectAlternativeNameBuilder = new SubjectAlternativeNameBuilder();
        subjectAlternativeNameBuilder.AddDnsName("localhost");
        X509KeyUsageExtension item = new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true);
        X509EnhancedKeyUsageExtension item2 = new X509EnhancedKeyUsageExtension(new OidCollection
        {
            new Oid("1.3.6.1.5.5.7.3.1", "Server Authentication")
        }, critical: true);
        X509BasicConstraintsExtension item3 = new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, 0, critical: true);
        X509Extension item4 = new X509Extension(new AsnEncodedData(new Oid("1.3.6.1.4.1.311.84.1.1", "ASP.NET Core HTTPS development certificate"), Encoding.ASCII.GetBytes("ASP.NET Core HTTPS development certificate")), critical: false);
        list.Add(item3);
        list.Add(item);
        list.Add(item2);
        list.Add(subjectAlternativeNameBuilder.Build(critical: true));
        list.Add(item4);
        X509Certificate2 x509Certificate = CreateSelfSignedCertificate(subject, list, notBefore, notAfter);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            x509Certificate.FriendlyName = "ASP.NET Core HTTPS development certificate";
        }
        return x509Certificate;
    }

    public X509Certificate2 CreateSelfSignedCertificate(X500DistinguishedName subject, IEnumerable<X509Extension> extensions, DateTimeOffset notBefore, DateTimeOffset notAfter)
    {
        RSA key = CreateKeyMaterial(2048);
        CertificateRequest certificateRequest = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        foreach (X509Extension extension in extensions)
        {
            certificateRequest.CertificateExtensions.Add(extension);
        }
        return certificateRequest.CreateSelfSigned(notBefore, notAfter);
        static RSA CreateKeyMaterial(int minimumKeySize)
        {
            RSA rSA = RSA.Create(minimumKeySize);
            if (rSA.KeySize < minimumKeySize)
            {
                throw new InvalidOperationException($"Failed to create a key with a size of {minimumKeySize} bits");
            }
            return rSA;
        }
    }

    public X509Certificate2 SaveCertificateInStore(X509Certificate2 certificate, StoreName name, StoreLocation location, DiagnosticInformation diagnostics = null)
    {
        diagnostics?.Debug("Saving the certificate into the certificate store.");
        X509Certificate2 x509Certificate = certificate;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            byte[] array = certificate.Export(X509ContentType.Pfx, "");
            x509Certificate = new X509Certificate2(array, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            Array.Clear(array, 0, array.Length);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            x509Certificate.FriendlyName = certificate.FriendlyName;
        }
        using X509Store x509Store = new X509Store(name, location);
        x509Store.Open(OpenFlags.ReadWrite);
        x509Store.Add(x509Certificate);
        x509Store.Close();
        return x509Certificate;
    }

    public void ExportCertificate(X509Certificate2 certificate, string path, bool includePrivateKey, string password, DiagnosticInformation diagnostics = null)
    {
        diagnostics?.Debug("Exporting certificate to '" + path + "'", includePrivateKey ? "The certificate will contain the private key" : "The certificate will not contain the private key");
        if (includePrivateKey && password == null)
        {
            diagnostics?.Debug("No password was provided for the certificate.");
        }
        string directoryName = Path.GetDirectoryName(path);
        if (directoryName != "")
        {
            diagnostics?.Debug("Ensuring that the directory for the target exported certificate path exists '" + directoryName + "'");
            Directory.CreateDirectory(directoryName);
        }
        byte[] array;
        if (includePrivateKey)
        {
            try
            {
                diagnostics?.Debug("Exporting the certificate including the private key.");
                array = certificate.Export(X509ContentType.Pfx, password);
            }
            catch (Exception e)
            {
                diagnostics?.Error("Failed to export the certificate with the private key", e);
                throw;
            }
        }
        else
        {
            try
            {
                diagnostics?.Debug("Exporting the certificate without the private key.");
                array = certificate.Export(X509ContentType.Cert);
            }
            catch (Exception e2)
            {
                diagnostics?.Error("Failed to export the certificate without the private key", e2);
                throw;
            }
        }
        try
        {
            diagnostics?.Debug("Writing exported certificate to path '" + path + "'.");
            File.WriteAllBytes(path, array);
        }
        catch (Exception e3)
        {
            diagnostics?.Error("Failed writing the certificate to the target path", e3);
            throw;
        }
        finally
        {
            Array.Clear(array, 0, array.Length);
        }
    }

    public void TrustCertificate(X509Certificate2 certificate, DiagnosticInformation diagnostics = null)
    {
        X509Certificate2 x509Certificate = new X509Certificate2(certificate.Export(X509ContentType.Cert));
        if (!IsTrusted(x509Certificate))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                diagnostics?.Debug("Trusting the certificate on Windows.");
                TrustCertificateOnWindows(certificate, x509Certificate, diagnostics);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                diagnostics?.Debug("Trusting the certificate on MAC.");
                TrustCertificateOnMac(x509Certificate, diagnostics);
            }
        }
    }

    private void TrustCertificateOnMac(X509Certificate2 publicCertificate, DiagnosticInformation diagnostics)
    {
        string tempFileName = Path.GetTempFileName();
        try
        {
            ExportCertificate(publicCertificate, tempFileName, includePrivateKey: false, null);
            diagnostics?.Debug("Running the trust command on Mac OS");
            using Process process = Process.Start("sudo", MacOSTrustCertificateCommandLineArguments + tempFileName);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("There was an error trusting the certificate.");
            }
        }
        finally
        {
            try
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
            catch
            {
            }
        }
    }

    private static void TrustCertificateOnWindows(X509Certificate2 certificate, X509Certificate2 publicCertificate, DiagnosticInformation diagnostics = null)
    {
        publicCertificate.FriendlyName = certificate.FriendlyName;
        using X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        x509Store.Open(OpenFlags.ReadWrite);
        X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, publicCertificate.Thumbprint, validOnly: false);
        if (x509Certificate2Collection.Count > 0)
        {
            diagnostics?.Debug("Certificate already trusted. Skipping trust step.");
            DisposeCertificates(x509Certificate2Collection.OfType<X509Certificate2>());
            return;
        }
        try
        {
            diagnostics?.Debug("Adding certificate to the store.");
            x509Store.Add(publicCertificate);
        }
        catch (CryptographicException ex) when (ex.HResult == 1223)
        {
            diagnostics?.Debug("User cancelled the trust prompt.");
            throw new UserCancelledTrustException();
        }
        x509Store.Close();
    }

    public bool IsTrusted(X509Certificate2 certificate)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ListCertificates(CertificatePurpose.HTTPS, StoreName.Root, StoreLocation.CurrentUser, isValid: true, requireExportable: false).Any((X509Certificate2 c) => c.Thumbprint == certificate.Thumbprint);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Match match = Regex.Match(certificate.Subject, "CN=(.*[^,]+).*", RegexOptions.Singleline, MaxRegexTimeout);
            if (!match.Success)
            {
                throw new InvalidOperationException("Can't determine the subject for the certificate with subject '" + certificate.Subject + "'.");
            }
            string value = match.Groups[1].Value;
            using Process process = Process.Start(new ProcessStartInfo("security", string.Format(MacOSFindCertificateCommandLineArgumentsFormat, value))
            {
                RedirectStandardOutput = true
            });
            string input = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return (from m in Regex.Matches(input, "SHA-1 hash: ([0-9A-Z]+)", RegexOptions.Multiline, MaxRegexTimeout).OfType<Match>()
                    select m.Groups[1].Value).ToList().Any((string h) => string.Equals(h, certificate.Thumbprint, StringComparison.Ordinal));
        }
        return false;
    }

    public void CleanupHttpsCertificates(string subject = "CN=localhost")
    {
        CleanupCertificates(CertificatePurpose.HTTPS, subject);
    }

    public void CleanupCertificates(CertificatePurpose purpose, string subject)
    {
        foreach (X509Certificate2 item in ListCertificates(purpose, StoreName.My, StoreLocation.CurrentUser, isValid: false))
        {
            RemoveCertificate(item, RemoveLocations.All);
        }
    }

    public DiagnosticInformation CleanupHttpsCertificates2(string subject = "CN=localhost")
    {
        return CleanupCertificates2(CertificatePurpose.HTTPS, subject);
    }

    public DiagnosticInformation CleanupCertificates2(CertificatePurpose purpose, string subject)
    {
        DiagnosticInformation diagnosticInformation = new DiagnosticInformation();
        foreach (X509Certificate2 item in ListCertificates(purpose, StoreName.My, StoreLocation.CurrentUser, isValid: false, requireExportable: true, diagnosticInformation))
        {
            RemoveCertificate(item, RemoveLocations.All, diagnosticInformation);
        }
        return diagnosticInformation;
    }

    public void RemoveAllCertificates(CertificatePurpose purpose, StoreName storeName, StoreLocation storeLocation, string subject = null)
    {
        IList<X509Certificate2> list = (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ListCertificates(purpose, StoreName.My, StoreLocation.CurrentUser, isValid: false) : ListCertificates(purpose, storeName, storeLocation, isValid: false));
        if (subject != null)
        {
            list.Where((X509Certificate2 c) => c.Subject == subject);
        }
        RemoveLocations locations = ((storeName == StoreName.My) ? RemoveLocations.Local : RemoveLocations.Trusted);
        foreach (X509Certificate2 item in list)
        {
            RemoveCertificate(item, locations);
        }
        DisposeCertificates(list);
    }

    private void RemoveCertificate(X509Certificate2 certificate, RemoveLocations locations, DiagnosticInformation diagnostics = null)
    {
        switch (locations)
        {
            case RemoveLocations.Undefined:
                throw new InvalidOperationException("'Undefined' is not a valid location.");
            case RemoveLocations.Local:
                RemoveCertificateFromUserStore(certificate, diagnostics);
                return;
            case RemoveLocations.Trusted:
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    RemoveCertificateFromTrustedRoots(certificate, diagnostics);
                    return;
                }
                break;
            case RemoveLocations.All:
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    RemoveCertificateFromTrustedRoots(certificate, diagnostics);
                }
                RemoveCertificateFromUserStore(certificate, diagnostics);
                return;
        }
        throw new InvalidOperationException("Invalid location.");
    }

    private static void RemoveCertificateFromUserStore(X509Certificate2 certificate, DiagnosticInformation diagnostics)
    {
        diagnostics?.Debug($"Trying to remove certificate with thumbprint '{certificate.Thumbprint}' from certificate store '{StoreLocation.CurrentUser}\\{StoreName.My}'.");
        using X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        x509Store.Open(OpenFlags.ReadWrite);
        X509Certificate2 certificate2 = x509Store.Certificates.OfType<X509Certificate2>().Single((X509Certificate2 c) => c.SerialNumber == certificate.SerialNumber);
        x509Store.Remove(certificate2);
        x509Store.Close();
    }

    private void RemoveCertificateFromTrustedRoots(X509Certificate2 certificate, DiagnosticInformation diagnostics)
    {
        diagnostics?.Debug($"Trying to remove certificate with thumbprint '{certificate.Thumbprint}' from certificate store '{StoreLocation.CurrentUser}\\{StoreName.Root}'.");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                x509Store.Open(OpenFlags.ReadWrite);
                X509Certificate2 x509Certificate = x509Store.Certificates.OfType<X509Certificate2>().SingleOrDefault((X509Certificate2 c) => c.SerialNumber == certificate.SerialNumber);
                if (x509Certificate != null)
                {
                    x509Store.Remove(x509Certificate);
                }
                x509Store.Close();
                return;
            }
        }
        if (IsTrusted(certificate))
        {
            try
            {
                diagnostics?.Debug("Trying to remove the certificate trust rule.");
                RemoveCertificateTrustRule(certificate);
            }
            catch
            {
                diagnostics?.Debug("Failed to remove the certificate trust rule.");
            }
            RemoveCertificateFromKeyChain("/Library/Keychains/System.keychain", certificate);
        }
        else
        {
            diagnostics?.Debug("The certificate was not trusted.");
        }
    }

    private static void RemoveCertificateTrustRule(X509Certificate2 certificate)
    {
        string tempFileName = Path.GetTempFileName();
        try
        {
            byte[] bytes = certificate.Export(X509ContentType.Cert);
            File.WriteAllBytes(tempFileName, bytes);
            using Process process = Process.Start(new ProcessStartInfo("sudo", $"security remove-trusted-cert -d {tempFileName}"));
            process.WaitForExit();
        }
        finally
        {
            try
            {
                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
            catch
            {
            }
        }
    }

    private static void RemoveCertificateFromKeyChain(string keyChain, X509Certificate2 certificate)
    {
        using Process process = Process.Start(new ProcessStartInfo("sudo", $"security delete-certificate -Z {certificate.Thumbprint.ToUpperInvariant()} {keyChain}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        string text = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("There was an error removing the certificate with thumbprint '" + certificate.Thumbprint + "'.\r\n\r\n" + text);
        }
    }

    public EnsureCertificateResult EnsureAspNetCoreHttpsDevelopmentCertificate(DateTimeOffset notBefore, DateTimeOffset notAfter, string path = null, bool trust = false, bool includePrivateKey = false, string password = null, string subject = "CN=localhost")
    {
        return EnsureValidCertificateExists(notBefore, notAfter, CertificatePurpose.HTTPS, path, trust, includePrivateKey, password, subject);
    }

    public EnsureCertificateResult EnsureValidCertificateExists(DateTimeOffset notBefore, DateTimeOffset notAfter, CertificatePurpose purpose, string path = null, bool trust = false, bool includePrivateKey = false, string password = null, string subjectOverride = null)
    {
        if (purpose == CertificatePurpose.All)
        {
            throw new ArgumentException("The certificate must have a specific purpose.");
        }
        IEnumerable<X509Certificate2> enumerable = ListCertificates(purpose, StoreName.My, StoreLocation.CurrentUser, isValid: true).Concat(ListCertificates(purpose, StoreName.My, StoreLocation.LocalMachine, isValid: true));
        enumerable = ((subjectOverride == null) ? enumerable : enumerable.Where((X509Certificate2 c) => c.Subject == subjectOverride));
        EnsureCertificateResult result = EnsureCertificateResult.Succeeded;
        X509Certificate2 x509Certificate = null;
        if (enumerable.Count() > 0)
        {
            x509Certificate = enumerable.FirstOrDefault();
            result = EnsureCertificateResult.ValidCertificatePresent;
        }
        else
        {
            try
            {
                x509Certificate = purpose switch
                {
                    CertificatePurpose.All => throw new InvalidOperationException("The certificate must have a specific purpose."),
                    CertificatePurpose.HTTPS => CreateAspNetCoreHttpsDevelopmentCertificate(notBefore, notAfter, subjectOverride),
                    _ => throw new InvalidOperationException("The certificate must have a purpose."),
                };
            }
            catch
            {
                return EnsureCertificateResult.ErrorCreatingTheCertificate;
            }
            try
            {
                x509Certificate = SaveCertificateInStore(x509Certificate, StoreName.My, StoreLocation.CurrentUser);
            }
            catch
            {
                return EnsureCertificateResult.ErrorSavingTheCertificateIntoTheCurrentUserPersonalStore;
            }
        }
        if (path != null)
        {
            try
            {
                ExportCertificate(x509Certificate, path, includePrivateKey, password);
            }
            catch
            {
                return EnsureCertificateResult.ErrorExportingTheCertificate;
            }
        }
        if ((RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) && trust)
        {
            try
            {
                TrustCertificate(x509Certificate);
                return result;
            }
            catch (UserCancelledTrustException)
            {
                return EnsureCertificateResult.UserCancelledTrustStep;
            }
            catch
            {
                return EnsureCertificateResult.FailedToTrustTheCertificate;
            }
        }
        return result;
    }

    public DetailedEnsureCertificateResult EnsureAspNetCoreHttpsDevelopmentCertificate2(DateTimeOffset notBefore, DateTimeOffset notAfter, string path = null, bool trust = false, bool includePrivateKey = false, string password = null, string subject = "CN=localhost")
    {
        return EnsureValidCertificateExists2(notBefore, notAfter, CertificatePurpose.HTTPS, path, trust, includePrivateKey, password, subject);
    }

    public DetailedEnsureCertificateResult EnsureValidCertificateExists2(DateTimeOffset notBefore, DateTimeOffset notAfter, CertificatePurpose purpose, string path, bool trust, bool includePrivateKey, string password, string subject)
    {
        if (purpose == CertificatePurpose.All)
        {
            throw new ArgumentException("The certificate must have a specific purpose.");
        }
        DetailedEnsureCertificateResult detailedEnsureCertificateResult = new DetailedEnsureCertificateResult();
        IEnumerable<X509Certificate2> enumerable = ListCertificates(purpose, StoreName.My, StoreLocation.CurrentUser, isValid: true, requireExportable: true, detailedEnsureCertificateResult.Diagnostics).Concat(ListCertificates(purpose, StoreName.My, StoreLocation.LocalMachine, isValid: true, requireExportable: true, detailedEnsureCertificateResult.Diagnostics));
        IEnumerable<X509Certificate2> enumerable2 = ((subject == null) ? enumerable : enumerable.Where((X509Certificate2 c) => c.Subject == subject));
        if (subject != null)
        {
            IEnumerable<X509Certificate2> certificates = enumerable.Except(enumerable2);
            detailedEnsureCertificateResult.Diagnostics.Debug("Filtering found certificates to those with a subject equal to '" + subject + "'");
            detailedEnsureCertificateResult.Diagnostics.Debug(detailedEnsureCertificateResult.Diagnostics.DescribeCertificates(enumerable2));
            detailedEnsureCertificateResult.Diagnostics.Debug("Listing certificates excluded from consideration.");
            detailedEnsureCertificateResult.Diagnostics.Debug(detailedEnsureCertificateResult.Diagnostics.DescribeCertificates(certificates));
        }
        else
        {
            detailedEnsureCertificateResult.Diagnostics.Debug("Skipped filtering certificates by subject.");
        }
        enumerable = enumerable2;
        detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.Succeeded;
        X509Certificate2 x509Certificate = null;
        if (enumerable.Count() > 0)
        {
            detailedEnsureCertificateResult.Diagnostics.Debug("Found valid certificates present on the machine.");
            detailedEnsureCertificateResult.Diagnostics.Debug(detailedEnsureCertificateResult.Diagnostics.DescribeCertificates(enumerable));
            x509Certificate = enumerable.First();
            detailedEnsureCertificateResult.Diagnostics.Debug("Selected certificate");
            detailedEnsureCertificateResult.Diagnostics.Debug(detailedEnsureCertificateResult.Diagnostics.DescribeCertificates(x509Certificate));
            detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.ValidCertificatePresent;
        }
        else
        {
            detailedEnsureCertificateResult.Diagnostics.Debug("No valid certificates present on this machine. Trying to create one.");
            try
            {
                x509Certificate = purpose switch
                {
                    CertificatePurpose.All => throw new InvalidOperationException("The certificate must have a specific purpose."),
                    CertificatePurpose.HTTPS => CreateAspNetCoreHttpsDevelopmentCertificate(notBefore, notAfter, subject, detailedEnsureCertificateResult.Diagnostics),
                    _ => throw new InvalidOperationException("The certificate must have a purpose."),
                };
            }
            catch (Exception e)
            {
                detailedEnsureCertificateResult.Diagnostics.Error("Error creating the certificate.", e);
                detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.ErrorCreatingTheCertificate;
                return detailedEnsureCertificateResult;
            }
            try
            {
                x509Certificate = SaveCertificateInStore(x509Certificate, StoreName.My, StoreLocation.CurrentUser, detailedEnsureCertificateResult.Diagnostics);
            }
            catch (Exception e2)
            {
                detailedEnsureCertificateResult.Diagnostics.Error($"Error saving the certificate in the certificate store '{StoreLocation.CurrentUser}\\{StoreName.My}'.", e2);
                detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.ErrorSavingTheCertificateIntoTheCurrentUserPersonalStore;
                return detailedEnsureCertificateResult;
            }
        }
        if (path != null)
        {
            detailedEnsureCertificateResult.Diagnostics.Debug("Trying to export the certificate.");
            detailedEnsureCertificateResult.Diagnostics.Debug(detailedEnsureCertificateResult.Diagnostics.DescribeCertificates(x509Certificate));
            try
            {
                ExportCertificate(x509Certificate, path, includePrivateKey, password, detailedEnsureCertificateResult.Diagnostics);
            }
            catch (Exception e3)
            {
                detailedEnsureCertificateResult.Diagnostics.Error("An error ocurred exporting the certificate.", e3);
                detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.ErrorExportingTheCertificate;
                return detailedEnsureCertificateResult;
            }
        }
        if ((RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) && trust)
        {
            try
            {
                detailedEnsureCertificateResult.Diagnostics.Debug("Trying to export the certificate.");
                TrustCertificate(x509Certificate, detailedEnsureCertificateResult.Diagnostics);
                return detailedEnsureCertificateResult;
            }
            catch (UserCancelledTrustException)
            {
                detailedEnsureCertificateResult.Diagnostics.Error("The user cancelled trusting the certificate.", null);
                detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.UserCancelledTrustStep;
                return detailedEnsureCertificateResult;
            }
            catch (Exception e4)
            {
                detailedEnsureCertificateResult.Diagnostics.Error("There was an error trusting the certificate.", e4);
                detailedEnsureCertificateResult.ResultCode = EnsureCertificateResult.FailedToTrustTheCertificate;
                return detailedEnsureCertificateResult;
            }
        }
        return detailedEnsureCertificateResult;
    }
}
