using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public static class AuthService
    {
        private const string ClientId = "ВАШ_APP_ID";
        private const string RedirectUri = "https://oauth.vk.com/blank.html";
        private const string Scope = "email";
        private const string ApiVersion = "5.131";

        public static string GetAuthorizationUrl()
        {
            return $"https://oauth.vk.com/authorize?" +
                   $"client_id={ClientId}&" +
                   $"display=page&" +
                   $"redirect_uri={Uri.EscapeDataString(RedirectUri)}&" +
                   $"scope={Scope}&" +
                   $"response_type=token&" +
                   $"v={ApiVersion}";
        }

        public static async Task<VkUserInfo> GetUserInfo(string accessToken, int userId)
        {
            var url = $"https://api.vk.com/method/users.get?" +
                     $"user_ids={userId}&" +
                     $"fields=first_name,last_name,photo_200&" +
                     $"access_token={accessToken}&" +
                     $"v={ApiVersion}";

            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                var json = await client.DownloadStringTaskAsync(url);
                var response = JsonConvert.DeserializeObject<VkResponse>(json);
                return response.Response[0];
            }
        }
    }

    public class VkResponse
    {
        [JsonProperty("response")]
        public List<VkUserInfo> Response { get; set; }
    }

    public class VkUserInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("photo_200")]
        public string PhotoUrl { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class VkAuthResult
    {
        public string AccessToken { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
