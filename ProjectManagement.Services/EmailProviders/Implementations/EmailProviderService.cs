﻿using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using ProjectManagement.Models.Configuration;
using ProjectManagement.Services.Domain.Notification.Dtos;
using ProjectManagement.Services.EmailProviders.Interfaces;
using ProjectManagement.Services.Utility;
using System.Net;

namespace ProjectManagement.Services.EmailProviders.Implementations;
public class EmailProviderService : IEmailProviderService
{

    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly AppSetting _appSetting;

    public EmailProviderService(IAmazonSimpleEmailService sesClient, AppSetting appSetting)
    {
        _sesClient = sesClient;
        _appSetting = appSetting;
    }
    public async Task<EmailTemplateModel> ProcessEmailNotificationRequestAsync(SendEmailNotification emailContent)
    {
        List<string> CCs = new List<string>();
        List<string> BCCs = new List<string>();

        if (emailContent.BCCs != null)
        {
            foreach (var item in emailContent?.BCCs!)
            {
                BCCs = new List<string>() { item?.Email! };
            }
        }

        if (emailContent.CCs != null)

        {
            foreach (var item in emailContent?.CCs!)
            {
                CCs = new List<string>() { item?.Email! };
            }
        }

        EmailTemplateModel emailTemplate = new EmailTemplateModel
        {
            DeliveryDate = emailContent.CommandSentAt.ToLongDateString(),
            Email = emailContent.To.Email,
            Subject = emailContent.Subject,
            EmailBodyHtml = emailContent.Content,
            TemplateKey = emailContent.MessageId,
            BCCs = BCCs,
            CCs = BCCs
        };
        return await Task.FromResult(emailTemplate);
    }

    public Task<EmailTemplateModel> ProcessEmailNotificationRequestAsync(SendBroadcastEmailNotification emailContent)
    {
        throw new NotImplementedException();
    }

    public async Task<HttpStatusCode> SendMailAsync(EmailTemplateModel emailTemplate)
    {
        var sendRequest = new SendEmailRequest
        {
            Source = _appSetting.Aws.Ses.EmailFrom,
            Destination = new Destination
            {
                ToAddresses = new List<string> { emailTemplate.Email },
                BccAddresses = emailTemplate.BCCs,
                CcAddresses = emailTemplate.CCs,
            },
            Message = new Message
            {
                Subject = new Content(emailTemplate.Subject),
                Body = new Body
                {
                    Text = new Content(emailTemplate.EmailBodyHtml)
                }
            }
        };

        SendEmailResponse response = await _sesClient.SendEmailAsync(sendRequest);
        return response.HttpStatusCode;
    }

    public async Task<HttpStatusCode> SendMailAsync(EmailTemplateModel model, string cpName)
    {
        if (model == null) throw new InvalidOperationException("Object values cannot be empty.");
        MimeMessage email = new();
        email.From.Add(new MailboxAddress(name: cpName, address: _appSetting.Aws.Ses.EmailFrom));
        email.To.Add(MailboxAddress.Parse(model.Email));
        email.Subject = model.Subject;
        email.Body = new TextPart(TextFormat.Html) { Text = model.EmailBodyHtml };
        if (int.TryParse(_appSetting.Aws.Ses.SmtpPort, out var smtpPort))
        {
            using SmtpClient smtpClient = new();
            await smtpClient.ConnectAsync(_appSetting.Aws.Ses.SmtpHost, smtpPort, useSsl: true);
            await smtpClient.AuthenticateAsync(userName: _appSetting.Aws.Ses.EmailFrom, password: _appSetting.Aws.Ses.SmtpPass);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
            smtpClient.Dispose();
            return HttpStatusCode.OK;
        }
        return HttpStatusCode.BadRequest;
    }
}
