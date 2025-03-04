using GrpcLib;
using AiUo.Hosting;

namespace GrpcClient;

internal class TestDemo
{
    private static int _idx = 0;
    public async Task Execute()
    {
        var client = await HostingUtil.CreateGrpcClient<IUserService>("grpc_demo1");
        var dto = await client.Get(new UserIpo
        {
            Id = ++_idx
        });
        Console.WriteLine(dto.Message);
    }
}