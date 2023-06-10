using Microsoft.Graph;
using Microsoft.Graph.SecurityNamespace;

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

        var user1 =  await _graphServiceClient.Users[oid]
            .Request()
            .GetAsync();

        return user1;
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

