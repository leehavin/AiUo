using Amazon.EC2;
using Amazon.EC2.Model;

namespace AiUo.Extensions.AWS.EC2;

internal class EC2Service
{
    public async Task<Vpc> GetVpc(string vpcName)
    {
        var rsp = await GetClient().DescribeVpcsAsync();
        if (rsp.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            foreach (var vpc in rsp.Vpcs)
            {
                foreach (var tag in vpc.Tags)
                {
                    if (tag.Key == "Name" && tag.Value == vpcName)
                        return vpc;
                }
            }
            return rsp.Vpcs?.Find(x => x.IsDefault);
        }
        return null;
    }
    private IAmazonEC2 GetClient()
        => DIUtil.GetService<IAmazonEC2>();
}