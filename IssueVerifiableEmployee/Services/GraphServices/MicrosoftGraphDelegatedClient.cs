using Microsoft.Graph;
using Microsoft.Graph.SecurityNamespace;
using System.Security.Cryptography;

namespace IssuerVerifiableEmployee.Services.GraphServices;

public class MicrosoftGraphDelegatedClient
{
    private readonly GraphServiceClient _graphServiceClient;

    public MicrosoftGraphDelegatedClient(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<User?> GetGraphApiUser(string? oid)
    {
        if (oid == null) return null;

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

        var pre = user.PreferredLanguage;
        var photo = user.Photo;
        var ac = user.AccountEnabled;
        var job = user.JobTitle;
        return user;
    }

    public async Task<string> GetGraphApiProfilePhoto(string oid)
    {
        var photo = string.Empty;
        // Get user photo
        using (var photoStream = await _graphServiceClient.Users[oid].Photo
            .Content.Request().GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream).ToArray();
            photo = Convert.ToBase64String(photoByte);
        }

        return photo;
    }

    public async Task SendEmailAsync(Message message)
    {
        var saveToSentItems = true;

        await _graphServiceClient.Me
            .SendMail(message, saveToSentItems)
            .Request()
            .PostAsync();
    }

    public async Task<OnlineMeeting> CreateOnlineMeeting(OnlineMeeting onlineMeeting)
    {
        return await _graphServiceClient.Me
            .OnlineMeetings
            .Request()
            .AddAsync(onlineMeeting);
    }

    public async Task<OnlineMeeting> UpdateOnlineMeeting(OnlineMeeting onlineMeeting)
    {
        return await _graphServiceClient.Me
            .OnlineMeetings[onlineMeeting.Id]
            .Request()
            .UpdateAsync(onlineMeeting);
    }

    public async Task<OnlineMeeting> GetOnlineMeeting(string onlineMeetingId)
    {
        return await _graphServiceClient.Me
            .OnlineMeetings[onlineMeetingId]
            .Request()
            .GetAsync();
    }
}

