using System.Runtime.Serialization;
using System.ServiceModel;

namespace GrpcLib;

[ServiceContract]
public interface IUserService
{
    ValueTask<UserDto> Get(UserIpo ipo);
}
[DataContract]
public class UserIpo
{
    [DataMember(Order = 1)]
    public int Id { get; set; }
}
[DataContract]
public class  UserDto
{
    [DataMember(Order =1)]
    public string? Message { get; set; }
}