using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JunoSDK.Connection;
using JunoSDK.Models;
using JunoSDK.Protocol;
using JunoSDK.utils;

namespace JunoSDK
{
    public class JunoModule
    {
        private readonly BaseConnection connection;
        private readonly BaseProtocol protocol;
        private bool registered = false;

        private readonly Dictionary<string, TaskCompletionSource<MessageItem>> requests =
            new Dictionary<string, TaskCompletionSource<MessageItem>>();

        private readonly Dictionary<string, FunctionListener> functions = new Dictionary<string, FunctionListener>();

        private readonly Dictionary<string, List<HookListener>> hookListeners =
            new Dictionary<string, List<HookListener>>();

        private readonly List<byte> messageBuffer = new List<byte>();
        private string? moduleId;

        public JunoModule(BaseProtocol protocol, BaseConnection connection)
        {
            this.protocol = protocol;
            this.connection = connection;
            registered = false;
        }

        public async Task Initialize(string moduleId, string version, Dictionary<string, string> dependencies)
        {
            this.moduleId = moduleId;
            await connection.SetupConnection();
            connection.SetOnDataListener(OnDataHandler);
            await SendRequest(
                protocol.Initialize(moduleId, version, dependencies)
            );
        }

        public Task DeclareFunction(string fnName, FunctionListener fn)
        {
            functions[fnName] = fn;
            return SendRequest(
                protocol.DeclareFunction(fnName)
            );
        }

        public Task CallFunction(string fnName, Dictionary<string, MessageItem> args)
        {
            return SendRequest(
                protocol.CallFunction(fnName, args)
            );
        }

        public Task RegisterHook(string hook, HookListener cb)
        {
            if (hookListeners.ContainsKey(hook))
            {
                hookListeners[hook].Add(cb);
            }
            else
            {
                hookListeners.Add(hook, new List<HookListener>()
                {
                    cb
                });
            }

            return SendRequest(
                protocol.RegisterHook(hook)
            );
        }

        public Task TriggerHook(string hook)
        {
            return SendRequest(
                protocol.TriggerHook(hook)
            );
        }

        public Task Close()
        {
            return connection.CloseConnection();
        }

        private Task SendRequest(BaseMessage request)
        {
            if (request.GetMessageType() == Constants.RequestTypes.RegisterModuleRequest && registered)
            {
                throw new Exception("Module already registered");
            }

            var encoded = protocol.Encode(request);
            if (registered || request.GetMessageType() == Constants.RequestTypes.RegisterModuleRequest)
            {
                connection.Send(
                    encoded
                );
            }
            else
            {
                messageBuffer.AddRange(encoded);
            }

            var completionSource = new TaskCompletionSource<MessageItem>();
            if (requests.ContainsKey(request.RequestId))
            {
                requests[request.RequestId] = completionSource;
            }
            else
            {
                requests.Add(request.RequestId, completionSource);
            }

            return completionSource.Task;
        }

        private async void OnDataHandler(byte[] data)
        {
            var response = protocol.Decode(data);
            MessageItem value;
            switch (response.GetMessageType())
            {
                case Constants.RequestTypes.RegisterModuleRequest:
                    {
                        value = MessageItem.True;
                        break;
                    }
                case Constants.RequestTypes.FunctionCallResponse:
                    {
                        value = (response as FunctionCallResponseMessage)?.Data ?? MessageItem.Empty;
                        break;
                    }
                case Constants.RequestTypes.DeclareFunctionResponse:
                    {
                        value = MessageItem.True;
                        break;
                    }
                case Constants.RequestTypes.RegisterHookResponse:
                    {
                        value = MessageItem.True;
                        break;
                    }
                case Constants.RequestTypes.TriggerHookResponse:
                    {
                        if (response is TriggerHookRequestMessage message)
                        {
                            await ExecuteHookTriggered(message);
                        }
                        else
                        {
                            throw new ApplicationException("Executing a non-TriggerHookRequestMessage as TriggerHookRequestMessage. How did you even get here?");
                        }
                        value = MessageItem.True;
                        break;
                    }
                case Constants.RequestTypes.FunctionCallRequest:
                    {
                        if (response is FunctionCallRequestMessage message)
                        {
                            value = await ExecuteFunctionCall(message);
                        }
                        else
                        {
                            throw new ApplicationException("Executing a non-FunctionCallRequestMessage as FunctionCallRequestMessage. How did you even get here?");
                        }
                        break;
                    }
                default:
                    {
                        value = MessageItem.Empty;
                        break;
                    }
            }

            if (requests.ContainsKey(response.RequestId))
            {
                requests[response.RequestId]?.SetResult(value);
            }
        }

        private async Task<MessageItem> ExecuteFunctionCall(FunctionCallRequestMessage request)
        {
            if (functions.ContainsKey(request.Function))
            {
                var response = await functions[request.Function](request.Arguments);
                await SendRequest(new FunctionCallResponseMessage()
                {
                    RequestId = request.RequestId,
                    Data = response
                });
                return MessageItem.True;
            }
            else
            {
                // Function wasn't found in the module.
                return MessageItem.Empty;
            }
        }

        private async Task ExecuteHookTriggered(TriggerHookRequestMessage request)
        {
            if (request.Hook == string.Empty)
            {
                return;
            }

            switch (request.Hook)
            {
                // Hook triggered by another module.
                case "juno.activated":
                    {
                        registered = true;
                        if (messageBuffer.Count == 0)
                            return;
                        await connection.Send(messageBuffer.ToArray());
                        messageBuffer.Clear();
                        break;
                    }
                case "juno.deactivated":
                    {
                        registered = false;
                        break;
                    }
                default:
                    {
                        if (hookListeners.ContainsKey(request.Hook))
                        {
                            foreach (var listener in hookListeners[request.Hook])
                            {
                                await listener(request.Data);
                            }
                        }

                        break;
                    }
            }
        }
    }

    public delegate Task<MessageItem> FunctionListener(Dictionary<string, MessageItem> arguments);
    public delegate Task<MessageItem> HookListener(MessageItem data);
}
