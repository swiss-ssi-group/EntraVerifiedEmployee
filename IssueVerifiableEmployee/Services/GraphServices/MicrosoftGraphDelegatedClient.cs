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

    public async Task<(User? User ,string? Photo, string? Error)> GetGraphApiUser(string? oid)
    {
        if (oid == null) return (null,null, "OID not defined");

        var photo = string.Empty;
        try
        {
            photo = await GetGraphApiProfilePhoto(oid);
        }
        catch (Exception)
        {
            return (null, null, "User MUST have a photo, upload in the Azure portal user basic profile, or using office");
        }

        var user =  await _graphServiceClient.Users[oid]
            .Request()
            // .Filter($"userType eq 'Member'")
            .Select(u => new
            {
                u.Id,
                u.GivenName,
                u.Surname,
                u.JobTitle,
                u.DisplayName,
                u.Mail,
                u.EmployeeId,
                u.EmployeeType,
                u.BusinessPhones,
                u.MobilePhone,
                u.AccountEnabled,
                u.Photo,
                u.PreferredLanguage,
                u.UserPrincipalName
            })
            .GetAsync();

        if(user.PreferredLanguage == null)
        {
            return (null, null, "No Preferred Language defined for the user, add this please");
        }

        return (user, photo, null);
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
            .Content.Request().GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream).ToArray();
            photo = WebUtility.UrlEncode(Convert.ToBase64String(photoByte));
            //photo = Base64UrlEncoder.Encode(photoByte);
        }

        return photo;
    }
}

