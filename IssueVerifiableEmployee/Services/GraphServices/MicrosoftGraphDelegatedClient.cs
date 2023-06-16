using Microsoft.Graph;
using Microsoft.Graph.Models;
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


        var result = await _graphServiceClient.Users.GetAsync();


        var user =  await _graphServiceClient.Users[oid]
            .GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = new string[] { 
                    "id", "givenName", "surname", "jobTitle", "displayName",
                    "mail",  "employeeId", "employeeType",
                    "mobilePhone", "accountEnabled", "photo", "preferredLanguage",
                    "userPrincipalName", "identities"};
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
            });

        if(user!.PreferredLanguage == null)
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
            .Content.GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream!).ToArray();
            photo = WebUtility.UrlEncode(Convert.ToBase64String(photoByte));
            //photo = Base64UrlEncoder.Encode(photoByte);
        }

        return photo;
    }
}

