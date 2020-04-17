using MailboxIntegration.TokenStorage;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MailboxIntegration.Helpers
{
    public class GraphHelper
    {
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];
        public static async Task<User> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));
            return await graphClient.Me.Request().GetAsync();
        }

        public static async Task<IEnumerable<Message>> GetEventsAsync()
        {
            try
            {
                var graphClient = GetAuthenticatedClient();
                var events = await graphClient.Me.MailFolders.Inbox.Messages.Request().Expand("attachments").GetAsync(); //Select("webLink,subject,hasAttachments,BodyPreview")
                return events.CurrentPage;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public static async Task<IEnumerable<Message>> OpenSharedMailbox(string sharedMailId)
        {
            try
            {
                var graphClient = GetAuthenticatedClient();
                var user = await graphClient.Users[sharedMailId].MailFolders.Inbox.Messages.Request().Expand("attachments").GetAsync();
                return user.CurrentPage;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<IList<Attachment>> DownloadAttachments(string messageId)
        {
            try
            {
                var graphClient = GraphHelper.GetAuthenticatedClient();
                var attachmentRequest = await graphClient.Me.MailFolders.Inbox.Messages[messageId].Request().Select("webLink,subject,hasAttachments,BodyPreview").Expand("attachments").GetAsync();
                return attachmentRequest.Attachments.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static AuthorizationCodeProvider getAuthProvider()
        {
            var idClient = ConfidentialClientApplicationBuilder.Create(appId)
                        .WithRedirectUri(redirectUri)
                        .WithClientSecret(appSecret)
                        .Build();

            var tokenStore = new SessionStore(idClient.UserTokenCache,
                    HttpContext.Current, ClaimsPrincipal.Current);

            // By calling this here, the token can be refreshed
            // if it's expired right before the Graph call is made
            var scopes = graphScopes.Split(' ');
            AuthorizationCodeProvider authenticationProvider = new AuthorizationCodeProvider(idClient, scopes);
            return authenticationProvider;
        }
        public static GraphServiceClient GetAuthenticatedClient()
        {
            try
            {
                return new GraphServiceClient(
             new DelegateAuthenticationProvider(
                 async (requestMessage) =>
                 {
                     var idClient = ConfidentialClientApplicationBuilder.Create(appId)
                         .WithRedirectUri(redirectUri)
                         .WithClientSecret(appSecret)
                         .Build();

                     var tokenStore = new SessionStore(idClient.UserTokenCache,
                             HttpContext.Current, ClaimsPrincipal.Current);

                     var accounts = await idClient.GetAccountsAsync();

                     // By calling this here, the token can be refreshed
                     // if it's expired right before the Graph call is made
                     var scopes = graphScopes.Split(' ');
                     var result = await idClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                         .ExecuteAsync();

                     requestMessage.Headers.Authorization =
                         new AuthenticationHeaderValue("Bearer", result.AccessToken);
                 }));
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}