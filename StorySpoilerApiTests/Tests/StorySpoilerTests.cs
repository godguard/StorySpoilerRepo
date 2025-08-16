using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using StorySpoilerApiTests.DTOs;

namespace StorySpoilerApiTests
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private static RestClient Client;
        private static string Token;
        private static string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";
        private static string CreatedStoryId;

        [OneTimeSetUp]
        public void Setup()
        {
            Client = new RestClient(BaseUrl);

            var loginRequest = new RestRequest("/User/Authentication", Method.Post);
            loginRequest.AddJsonBody(new
            {
                userName = "smokedout",
                password = "TestuserQA2025"
            });

            var response = Client.Execute(loginRequest);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = System.Text.Json.JsonDocument.Parse(response.Content);
            Token = json.RootElement.GetProperty("accessToken").GetString();

            Assert.That(Token, Is.Not.Null.And.Not.Empty);

            Client = new RestClient(new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(Token)
            });
        }

        [Test, Order(1)]
        public void Test_CreateStory()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var story = new StoryDTO
            {
                Title = "Exam Story",
                Description = "This is created during QA exam",
                Url = ""
            };
            request.AddJsonBody(story);

            var response = Client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data.StoryId, Is.Not.Null);
            Assert.That(response.Data.Msg, Is.EqualTo("Successfully created!"));

            CreatedStoryId = response.Data.StoryId;
        }

        [Test, Order(2)]
        public void Test_EditStory()
        {
            var request = new RestRequest($"/Story/Edit/{CreatedStoryId}", Method.Put);
            var updated = new StoryDTO
            {
                Title = "Updated Exam Story",
                Description = "Edited successfully",
                Url = ""
            };
            request.AddJsonBody(updated);

            var response = Client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void Test_GetAllStories()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = Client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("title"));
        }

        [Test, Order(4)]
        public void Test_DeleteStory()
        {
            var request = new RestRequest($"/Story/Delete/{CreatedStoryId}", Method.Delete);
            var response = Client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void Test_CreateStoryWithoutRequiredFields()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var invalidStory = new StoryDTO
            {
                Title = "",
                Description = ""
            };
            request.AddJsonBody(invalidStory);

            var response = Client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Test_EditNonExistingStory()
        {
            var request = new RestRequest("/Story/Edit/invalid-id-123", Method.Put);
            var story = new StoryDTO
            {
                Title = "Does Not Exist",
                Description = "Should fail"
            };
            request.AddJsonBody(story);

            var response = Client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Data.Msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void Test_DeleteNonExistingStory()
        {
            var request = new RestRequest("/Story/Delete/invalid-id-123", Method.Delete);
            var response = Client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            Client?.Dispose();
        }
    }
}
