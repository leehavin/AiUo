using MQTTnet;
using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;

// 这是一个测试程序，用于验证MQTTnet 5.0版本的API用法
var factory = new MqttFactory();
var client = factory.CreateMqttClient();

// 创建MQTT客户端选项
var clientOptionsBuilder = new MqttClientOptionsBuilder()
    .WithClientId("test-client")
    .WithTcpServer("localhost", 1883)
    .WithCleanStart(true);

// 设置保活时间 - 在MQTTnet 5.0中需要使用TimeSpan
int keepAlivePeriodSeconds = 60;
clientOptionsBuilder.WithKeepAlivePeriod(TimeSpan.FromSeconds(keepAlivePeriodSeconds));

// 添加TLS/SSL支持
var tlsOptions = new MqttClientTlsOptions
{
    AllowUntrustedCertificates = true,
    IgnoreCertificateChainErrors = true,
    IgnoreCertificateRevocationErrors = true,
    CertificateValidationHandler = context => true
};

clientOptionsBuilder.WithTls(tlsOptions);

// 添加客户端证书 - 在MQTTnet 5.0中需要使用WithTls方法
try
{
    var certificatePath = "path/to/certificate.pfx";
    var certificatePassword = "password";
    
    // 注意：在MQTTnet 5.0中，客户端证书需要通过TlsOptions设置
    var certificate = new X509Certificate2(certificatePath, certificatePassword);
    tlsOptions.Certificates = new List<X509Certificate2> { certificate };
}
catch (Exception ex)
{
    Console.WriteLine($"加载客户端证书失败: {ex.Message}");
}

Console.WriteLine("MQTTnet 5.0 API测试完成");
