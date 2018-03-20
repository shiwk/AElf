using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace AElf.RPC
{
    public class SmartContract
    {
        private readonly AElfRPC.AElfRPCClient _client;
        private readonly SmartContractReg _registration;

        public SmartContract(AElfRPC.AElfRPCClient client, SmartContractReg registration)
        {
            _client = client;
            _registration = registration;
        }

        /// <summary>
        /// end-to-end simple rpc
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public Task<Result> Invoke(string methodName, params object[] objs)
        {
            
            var paramList = new ParamList();
            paramList.SetParam(objs);

            // create request options
            var options = new InvokeOption
            {
                ClassName = _registration.Name,
                MethodName = methodName,
                Reg = _registration,
                Params = paramList
            };

            return Task.FromResult(_client.Invoke(options));
        }


        /// <summary>
        /// server side stream rpc
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public async Task ListResults(string methodName, params object[] objs)
        {
            var paramList = new ParamList();
            paramList.SetParam(objs);

            // create request options
            var options = new InvokeOption
            {
                ClassName = _registration.Name,
                Reg = _registration,
                Params = paramList,
                MethodName = methodName
            };

            try
            {
                using (var call = _client.ListResults(options))
                {
                    var responseStream = call.ResponseStream;

                    while (await responseStream.MoveNext())
                    {
                        var result = responseStream.Current;
                        Console.WriteLine(result.Res);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("RPC Failed");
                throw;
            }
            
        }


        /// <summary>
        /// client side stream rpc
        /// </summary>
        /// <returns></returns>
        public async Task ListInvoke(string methodName, params object[] objs)
        {
            
            using (var  call = _client.ListInvoke())
            {
                var paramList = new ParamList();
                paramList.SetParam(objs);
                
                var options = new InvokeOption
                {
                    ClassName = _registration.Name,
                    Reg = _registration,
                    Params = paramList,
                    MethodName = methodName
                };
                
                for (var i = 1; i <= 3; i++)
                {
                    await call.RequestStream.WriteAsync(options);
                    await Task.Delay(500);    
                }

                await call.RequestStream.CompleteAsync();
                
                var summary = await call.ResponseAsync;
            }   
        }



        public async Task BiDirectional(string methodName, params object[] objs)
        {
            using (var  call = _client.BiDirectional())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (call != null && await call.ResponseStream.MoveNext())
                    {
                        var res = call.ResponseStream.Current;
                        Console.WriteLine("Elapsed time : {0} ms", res.Res);
                    }
                });
                
                
                var paramList = new ParamList();
                paramList.SetParam(objs);
                
                var options = new InvokeOption
                {
                    ClassName = _registration.Name,
                    Reg = _registration,
                    Params = paramList,
                    MethodName = methodName
                };
                
                for (var i = 1; i <= 3; i++)
                {
                    await call.RequestStream.WriteAsync(options);
                    await Task.Delay(500);    
                }

                await call.RequestStream.CompleteAsync();
                await responseReaderTask;
                
            }
        }
    }
    
    
    public class RPCClient
    {
        private int _port;

        public RPCClient(int port)
        {
            _port = port;
        }
        
        private Task<SmartContract> Client(string className)
        {
            // load data 
            var data = File.ReadAllBytes("../../../contracts/Contract.dll");
            var smartContractRegistration = new SmartContractReg {Byte = ByteString.CopyFrom(data), Name = className};
            var channel = new Channel("127.0.0.1:" + _port, ChannelCredentials.Insecure);
            
            // create a real client
            var smartContract = new SmartContract(new AElfRPC.AElfRPCClient(channel), smartContractRegistration);
            return Task.FromResult(smartContract);
        }
        
        public void SimpleRPC()
        {
            var smartContract = Client("Contract.Contract").Result;
            var res = smartContract.Invoke("HelloWorld", 1);
            Console.WriteLine(res.Result.Res);
        }

        public  async Task ServerSideStream()
        {
            var smartContract = Client("Contract.ListContract").Result;
            await smartContract.ListResults("WaitSecondsTwice", 2);
        }

        public async Task ClientSideStream()
        {
            var smartContract = Client("Contract.ListContract").Result;
            await smartContract.ListInvoke("WaitSecondsTwice", 2);
        }

        public async Task BiDirectional()
        {
            var smartContract = Client("Contract.ListContract").Result;
            await smartContract.BiDirectional("WaitSeconds", 2);
        }
    }
}