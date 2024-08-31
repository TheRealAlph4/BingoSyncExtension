using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace BingoSyncExtension
{
    class NewCardClient
    {
        public enum State
        {
            None, Disconnected, Connected, Loading
        };

        public string nickname = "Board Generator";

        private CookieContainer cookieContainer = null;
        private HttpClientHandler handler = null;
        private HttpClient client = null;
        private ClientWebSocket webSocketClient = null;

        private State forcedState = State.None;

        private int maxRetries = 5;

        public NewCardClient()
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
        }

        public State GetState()
        {
            if (forcedState != State.None)
                return forcedState;
            if (webSocketClient.State == WebSocketState.Open)
                return State.Connected;
            else if (webSocketClient.State == WebSocketState.Connecting)
                return State.Loading;
            return State.Disconnected;
        }

        private void LoadCookie()
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

        public void JoinRoom(string room, string password)
        {
            if (GetState() == State.Loading)
            {
                return;
            }
            forcedState = State.Loading;
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
                try
                {
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    var readTask = response.Content.ReadAsStringAsync();
                    readTask.ContinueWith(joinRoomResponse =>
                    {
                        var socketJoin = JsonConvert.DeserializeObject<SocketJoin>(joinRoomResponse.Result);
                        ConnectToBroadcastSocket(socketJoin);
                    });
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"could not join room: {_ex.Message}");
                }
                finally
                {
                    forcedState = State.None;
                }
            });
        }

        public void ExitRoom()
        {
            forcedState = State.Loading;
            RetryHelper.RetryWithExponentialBackoff(() =>
            {
                return webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "exiting room", CancellationToken.None).ContinueWith(result =>
                {
                    if (result.Exception != null)
                    {
                        throw result.Exception;
                    }
                    webSocketClient = new ClientWebSocket();
                    forcedState = State.None;
                });
            }, maxRetries, nameof(ExitRoom), () =>
            {
                webSocketClient = new ClientWebSocket();
                forcedState = State.None;
            });
        }

        public void NewCard(string room, string customJSON, bool lockout = true, bool hideCard = true)
        {
            var newCard = new NewCard
            {
                Room = room,
                Game = 18, // this is supposed to be custom alread
                Variant = 18, // but this is also required for custom ???
                CustomJSON = customJSON,
                Lockout = !lockout, // false is lockout here for some godforsaken reason
                Seed = "",
                HideCard = hideCard,
            };
            RetryHelper.RetryWithExponentialBackoff(() =>
            {
                var payload = JsonConvert.SerializeObject(newCard);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var task = client.PostAsync("api/new-card", content);
                return task.ContinueWith(responseTask => {});
            }, maxRetries, nameof(ChatMessage));
        }

        public void ChatMessage(string room, string text)
        {
            var chatMessageObject = new ChatMessage
            {
                Room = room,
                Text = text,
            };
            RetryHelper.RetryWithExponentialBackoff(() =>
            {
                var payload = JsonConvert.SerializeObject(chatMessageObject);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var task = client.PutAsync("api/chat", content);
                return task.ContinueWith(responseTask =>
                {
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                });
            }, maxRetries, nameof(ChatMessage));
        }

        public void ConnectToBroadcastSocket(SocketJoin socketJoin)
        {
            var socketUri = new Uri("wss://sockets.bingosync.com/broadcast");
            RetryHelper.RetryWithExponentialBackoff(() =>
            {
                webSocketClient = new ClientWebSocket();
                var connectTask = webSocketClient.ConnectAsync(socketUri, CancellationToken.None);
                return connectTask.ContinueWith(connectResponse =>
                {
                    if (connectResponse.Exception != null)
                    {
                        Console.WriteLine($"error connecting to websocket: {connectResponse.Exception}");
                        throw connectResponse.Exception;
                    }

                    Console.WriteLine($"connected to the socket, sending socketJoin object");
                    var serializedSocketJoin = JsonConvert.SerializeObject(socketJoin);
                    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serializedSocketJoin));
                    var sendTask = webSocketClient.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    sendTask.ContinueWith(_ => {});
                });
            }, maxRetries, nameof(ConnectToBroadcastSocket));
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
 
    [DataContract]
    public class ChatMessage
    {
        [JsonProperty("room")]
        public string Room;
        [JsonProperty("text")]
        public string Text;
    }

}