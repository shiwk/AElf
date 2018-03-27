using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Xunit;

namespace AElf.RPC.Test
{
    public class RPCTest
    {

        private Task<Server> StartUp(int port)
        {
           
            Console.WriteLine("RPC server listening on port " + port);
            // create a server
            Server server = new Server
            {
                Services = { AElfRPC.BindService(new SmartContractExecution()) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };
            server.Start();
            return Task.FromResult(server);

        }
        
        [Fact]
        public async Task SimpleRPC()
        {
            var server = await StartUp(50052);
            RPCClient client = new RPCClient(50052);
            client.SimpleRPC();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ServerSideStream()
        {
            var server = await StartUp(50053);
            RPCClient client = new RPCClient(50053);
            await client.ServerSideStream();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ClientSideStream()
        {
            var server = await StartUp(50054);
            var client = new RPCClient(50054);
            await client.ClientSideStream();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task BiDirectional()
        {
            var server = await StartUp(50055);
            var client = new RPCClient(50055);
            await client.BiDirectional();
            await server.ShutdownAsync();
        }
    }
}