using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingoSyncExtension
{
    public static class SendBoardAPI
    {
        public enum State
        {
            None, Disconnected, Connected, Loading
        };

        public static string room = "";
        public static string password = "";
        public static string nickname = "";
        public static string color = "";

        public static List<BoardSquare> board = null;
        public static bool isHidden = true;

        private static CookieContainer cookieContainer = null;
        private static HttpClientHandler handler = null;
        private static HttpClient client = null;
        private static ClientWebSocket webSocketClient = null;

        private static State forcedState = State.None;
        private static WebSocketState lastSocketState = WebSocketState.None;

        private static bool shouldConnect = false;

        public static List<Action> BoardUpdated;

        private static int maxRetries = 5;

        public static void Setup()
        {
            cookieContainer = new CookieContainer();
            handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://bingosync.com"),
            };
            LoadCookie();

            webSocketClient = new ClientWebSocket();

            BoardUpdated = new List<Action>();
        }

        public static void Update()
        {
            if (webSocketClient.State == lastSocketState)
                return;
            BoardUpdated.ForEach(f => f());
            forcedState = State.None;
            lastSocketState = webSocketClient.State;
        }

        public static State GetState()
        {
            if (forcedState != State.None)
                return forcedState;
            if (webSocketClient.State == WebSocketState.Open)
                return State.Connected;
            else if (webSocketClient.State == WebSocketState.Connecting)
                return State.Loading;
            return State.Disconnected;
        }

        private static void LoadCookie()
        {
            RetryHelper.RetryWithExponentialBackoff(() =>
            {
                var task = client.GetAsync("");
                return task.ContinueWith(responseTask =>
                {
                    HttpResponseMessage response = null;
                    response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
                    {
                        foreach (string cookieHeader in values)
                        {
                            string[] cookieParts = cookieHeader.Split(';');
                            string cookieName = cookieParts[0].Split('=')[0];
                            string cookieValue = cookieParts[0].Split('=')[1];

                            Cookie cookie = new Cookie(cookieName.Trim(), cookieValue.Trim(), "/", response.RequestMessage.RequestUri.Host);
                            cookieContainer.Add(response.RequestMessage.RequestUri, cookie);
                        }
                    }
                });
            }, maxRetries, nameof(LoadCookie));
        }

        public static void JoinRoom(Action<Exception> callback)
        {
            if (GetState() == State.Loading)
            {
                return;
            }
            forcedState = State.Loading;
            shouldConnect = true;

            var joinRoomInput = new JoinRoomInput
            {
                Room = room,
                Nickname = nickname,
                Password = password,
            };
            var payload = JsonConvert.SerializeObject(joinRoomInput);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var task = client.PostAsync("api/join-room", content);
            _ = task.ContinueWith(responseTask =>
            {
                Exception ex = null;
                try
                {
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    var readTask = response.Content.ReadAsStringAsync();
                    readTask.ContinueWith(joinRoomResponse =>
                    {
                        var socketJoin = JsonConvert.DeserializeObject<SocketJoin>(joinRoomResponse.Result);
                    });
                }
                catch (Exception _ex)
                {
                    ex = _ex;
                    Console.WriteLine($"could not join room: {ex.Message}");
                }
                finally
                {
                    callback(ex);
                    forcedState = State.None;
                }
            });
        }

        public static void NewCard(string customJSON, bool lockout = true, bool hideCard = true)
        {
            var newCard = new NewCard
            {
                Room = room,
                Game = 18,
                Variant = 18,
                CustomJSON = customJSON,
                Lockout = lockout,
                Seed = "1",
                HideCard = hideCard,
            };
            var payload = JsonConvert.SerializeObject(newCard);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var task = client.PostAsync("api/new-card", content);
            _ = task.ContinueWith(responseTask => {});
        }
    }

    [DataContract]
    public class JoinRoomInput
    {
        [JsonProperty("room")]
        public string Room;
        [JsonProperty("nickname")]
        public string Nickname;
        [JsonProperty("password")]
        public string Password;
    }

    [DataContract]
    public class BoardSquare
    {
        [JsonProperty("name")]
        public string Name = string.Empty;
        [JsonProperty("colors")]
        public string Colors = string.Empty;
        [JsonProperty("slot")]
        public string Slot = string.Empty;
    }

    [DataContract]
    public class SocketJoin
    {
        [JsonProperty("socket_key")]
        public string SocketKey = string.Empty;
    }

    [DataContract]
    public class NewCard
    {
        [JsonProperty("room")]
        public string Room;
        [JsonProperty("game_type")]
        public int Game;
        [JsonProperty("variant_type")]
        public int Variant;
        [JsonProperty("custom_json")]
        public string CustomJSON;
        [JsonProperty("lockout_mode")]
        public bool Lockout;
        [JsonProperty("seed")]
        public string Seed;
        [JsonProperty("hide_card")]
        public bool HideCard;
    }
}