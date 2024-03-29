﻿using ImageMagick;
using IssuerVerifiableEmployee.Persistence;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;

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

        var user = await _graphServiceClient.Users[oid]
            .GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Select = [
                    "id",
                    "givenName",
                    "surname",
                    "jobTitle",
                    "displayName",
                    "mail",
                    "employeeId",
                    "employeeType",
                    "otherMails",
                    "mobilePhone",
                    "accountEnabled",
                    "photo",
                    "preferredLanguage",
                    "userPrincipalName",
                    "identities"];

                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
            });

        if (user!.PreferredLanguage == null)
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
                var validEmail = IsEmailValid(user.UserPrincipalName);
                if (validEmail)
                {
                    employee.Mail = user.UserPrincipalName;
                }
                else
                {
                    return (null, "No Mail defined for the user, add this please");
                }
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
        byte[] photoByte;

        var streamPhoto = new MemoryStream();
        using (var photoStream = await _graphServiceClient.Users[oid].Photo
            .Content.GetAsync())
        {
            photoStream!.CopyTo(streamPhoto);
            photoByte = streamPhoto!.ToArray();
        }

        using var imageFromFile = new MagickImage(photoByte);
        // Sets the output format to jpeg
        imageFromFile.Format = MagickFormat.Jpeg;
        var size = new MagickGeometry(400, 400);

        // This will resize the image to a fixed size without maintaining the aspect ratio.
        // Normally an image will be resized to fit inside the specified size.
        //size.IgnoreAspectRatio = true;

        imageFromFile.Resize(size);

        // Create byte array that contains a jpeg file
        var data = imageFromFile.ToByteArray();
        photo = Base64UrlEncoder.Encode(data);

        return photo;
    }

    public static bool IsEmailValid(string email)
    {
        if (email.Contains("#EXT#"))
            return false;

        if (!MailAddress.TryCreate(email, out var mailAddress))
            return false;

        // And if you want to be more strict:
        var hostParts = mailAddress.Host.Split('.');
        if (hostParts.Length == 1)
            return false; // No dot.
        if (hostParts.Any(p => p == string.Empty))
            return false; // Double dot.
        if (hostParts[^1].Length < 2)
            return false; // TLD only one letter.

        if (mailAddress.User.Contains(' '))
            return false;
        if (mailAddress.User.Split('.').Any(p => p == string.Empty))
            return false; // Double dot or dot at end of user part.

        return true;
    }

    public async Task SetPreferredLanguage(string oid, string preferredLanguage)
    {
        await _graphServiceClient.Users[oid].PatchAsync(
            new User
            {
                PreferredLanguage = preferredLanguage
            });
    }
}

