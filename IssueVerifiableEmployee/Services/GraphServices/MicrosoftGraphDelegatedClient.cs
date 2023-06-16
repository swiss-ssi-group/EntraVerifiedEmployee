using IssuerVerifiableEmployee.Persistence;
using Microsoft.Graph;
using System.Net;

namespace IssuerVerifiableEmployee.Services.GraphServices;

public class MicrosoftGraphDelegatedClient
{
    private readonly GraphServiceClient _graphServiceClient;

    public MicrosoftGraphDelegatedClient(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<(Employee? Employee, string? Error)> GetEmployee(string? oid)
    {
        if (oid == null) return (null, "OID not defined");

        var photo = string.Empty;
        try
        {
            photo = await GetGraphApiProfilePhoto(oid);
        }
        catch (Exception)
        {
            return (null, "User MUST have a photo, upload in the Azure portal user basic profile, or using office");
        }

        var user =  await _graphServiceClient.Users[oid]
            .GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { 
                    "id", "givenName", "surname", "jobTitle", "displayName",
                    "mail",  "employeeId", "employeeType", "otherMails",
                    "mobilePhone", "accountEnabled", "photo", "preferredLanguage",
                    "userPrincipalName", "identities"};
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
            });

        if(user!.PreferredLanguage == null)
        {
            return (null, "No Preferred Language defined for the user, add this please");
        }

        if (user!.JobTitle == null)
        {
            return (null, "No JobTitle defined for the user, add this please");
        }

        if (user!.Surname == null)
        {
            return (null, "No Surname defined for the user, add this please");
        }

        if (user!.GivenName == null)
        {
            return (null, "No GivenName defined for the user, add this please");
        }

        if (user!.DisplayName == null)
        {
            return (null, "No DisplayName defined for the user, add this please");
        }

        if (user!.UserPrincipalName == null)
        {
            return (null, "No UserPrincipalName defined for the user, add this please");
        }

        var employee = new Employee
        {
            DisplayName = user.DisplayName,
            GivenName = user.GivenName,
            JobTitle = user.JobTitle,
            Surname = user.Surname,
            PreferredLanguage = user.PreferredLanguage,
            RevocationId = user.UserPrincipalName,
            Photo = photo,
            AccountEnabled = user.AccountEnabled.GetValueOrDefault()
        };

        if (user.Mail != null)
        {
            employee.Mail = user.Mail;
        }
        else
        {
            var otherMail = user.OtherMails!.FirstOrDefault();
            if (otherMail != null)
            {
                employee.Mail = otherMail;
            }
            else
            {
                return (null, "No Mail defined for the user, add this please");
            }
        }

        return (employee, null);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/azure/active-directory/verifiable-credentials/how-to-use-quickstart-verifiedemployee
    /// UrlEncode(Base64Encode(photo)) format. To use the photo, 
    /// the verifier application has to 
    /// Base64Decode(UrlDecode(photo)).
    public async Task<string> GetGraphApiProfilePhoto(string oid)
    {
        var photo = string.Empty;
        using (var photoStream = await _graphServiceClient.Users[oid].Photo
            .Content.GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream!).ToArray();
            photo = WebUtility.UrlEncode(Convert.ToBase64String(photoByte));
            //photo = Base64UrlEncoder.Encode(photoByte);
        }

        return photo;
    }
}

