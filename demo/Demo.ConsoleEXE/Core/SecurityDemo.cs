using AiUo.Security;

namespace AiUo.Demos.Core;

internal class SecurityDemo : DemoBase
{
    public override async Task Execute()
    {
        /*
        var pwd = "admin";
        var salt = SecurityUtil.GetPasswordSalt();
        var epwd = SecurityUtil.EncryptPassword(pwd, salt);
        Console.WriteLine(SecurityUtil.ValidatePassword(pwd, epwd, salt));

        // rsa 签名
        var src = "{\"user\":\"3nYTOSjdlF6UTz9Ir\",\"country\":\"XX\",\"currency\":\"EUR\",\"operator_id\":1,\"token\":\"cd6bd8560f3bb8f84325152101adeb45\",\"platform\":\"GPL_DESKTOP\",\"game_id\":39,\"lang\":\"en\",\"lobby_url\":\"https://examplecasino.io\",\"ip\":\"::ffff:10.0.0.39\"}";
        var pem = "-----BEGIN RSA PRIVATE KEY-----\r\nMIIEowIBAAKCAQEAx3IRpSri/9SjA7f9me35v6LtJzn8drb1vg/UeGaPPFR16KsU\r\nOPqbGJ2r1pRPJMedqqbO7Agt/HavWDcQhNZlc9VrVQcWK/w2HD9PflQYv0oQMiPK\r\n5Mut/eIdFOpwwwaRAU4s6WOkJdSmP9F4cfr/amTZZoY59/t3SZWYjgZ9/LDI2X8S\r\n3uLW/JPiH+6dm2bU8ykhxoWwLE/piJxynS73EzM0tgjHyTUMkarhK9qRVZk581/k\r\nzmtJLLBZQl9XrQQcIfQ+zFZj7ijddOptxpxqCmq8gQNohB56p34yjVH3uAJAaFvI\r\nVg5mEkprrvNVDJwGonHSaaq7AICmzrF6h/r5dwIDAQABAoIBAFNIGIIlpGA7hE57\r\nN9RdANq6x9iHaBqST48rwQb9nHYOtqWPOoSIcNcYj7ase1faWsX1nZYF3F39mT52\r\nz9kIRZjW11jL+sAnMtkcvq77otHNtXGabJCZVHAdSROAydFGHqqy4CIcz2BUqY8g\r\ngvDlZF4i+nzLM82PHcKGSwuTPmyTED37RqtscSxd7cGQkuL6OnohSFpW+5tvZcGZ\r\nUi4oVRrX4oVXz//3TDESRfondwQVKPoqQr7aiyYSKOJJMngIXmCvJ6p3XNiEW+dP\r\nuXxm5N9QRkkX00v3vPTdsuwUjt3wepDJhR9BecRERRYNJIrqsgNxkJTJEDlsGqi5\r\nkIKPb/ECgYEA8nKLPCfIaZ4G/7pUXQ64MMemS7qH17aU7Eb+mqDpFRt2bNr/yw12\r\nqIhUkb6XyI/ZwOF2gdhPzte25CuZzNsMu3GqlEQ77AR1XCyu1AY8oiJS4TSmRovJ\r\nx86BK8C8OY75myYcmRIvsusxviZfUCunDaexVGOqIKMRNrgJ2i8194UCgYEA0pgp\r\nnzxI4H/Ej+5KOmEw9P2xstIXpW+CcEDbrh5pqW+PwNP/TUxC445Rn4R8AVXFVnbH\r\n6DLrW35A9KBcA7Ve37vkVUILxPC+S967+gdRAd3BQTuUmi31+7cBjmNLKJMMC8Fl\r\nDZ5S8zwRuVrU6o3bbPUbC5gDb/cN/7GVtb/D18sCgYEA2ma+8LCxxBr8GRAkATRK\r\nTn77WgqtZm/uVa5amrbLYR09IDBj7umw837kF+qGVsDnGu6/z5Ypxp3h/kccpELL\r\nhGuPi0KwbBtUEXWbBBqeMjwTRxYjlzdDzP9Es0JLDNq0FcROTMHqQBXI2I8+mzzH\r\nnvBqOSgS0JW04wMEtQyEY/UCgYBNMDKJR9paVtpf+vJABaGhGl+IcJL0MzP3Gv6q\r\nCkGmNdrVzZ5U4a/eoipusmuVPa/P6keJZyh254a9Yw122oKEtOSTD1sq+yZ0vpXd\r\npdLeQT51P3ZPMKtpcIFkhCZnH8aZhHAalr5GouzIKG/D7OzROeGI1VXlMwNxhdCe\r\nxkPtEwKBgG8IJGHMuzBGDtykqPwOdz7llWwzl3GgWiEDqt6kbIEr8F7CbbnlvrhJ\r\nHRmd9tFEtYz2/3x9VTv9NCjd8FiWyPcivkd4dvMInfgBoCBJxw0I+nyjqQpiLuhx\r\nx4xyX1tFKW+sAN+Mb9dO+ZHGpcz2FpPz5uXv6oCzTG9BLcgg0mwx\r\n-----END RSA PRIVATE KEY-----";
        var dest = "FFdiceKlsnmrBMhWopZYQXrJSMqxJWkynbaiOnuUE1p4mrg2UydlSAdKbOWQZ7o5USbA3SHPum3XuRCa9INWbcp+fdcOhjz0S0JLKF6uXJR2T8zlF+L+8v2lkBzPOvLg6yRuUyspxtcHB6Vlyd0Sj4v8vk5HxoVv1ZV7EpJglzHs7xKchuifjjhUwkX7AAZwvJCNKo/VZrhtfxHd/k8aq7+9h7AztiiiLO7CXbEbo8snDqMqoB/tBAImv45NcAuReBZGb3QY37MbRoOT/uQGj+9ae5VRNpJ74qs3COraDqZ5kqzsa5SjN1cML2foP13Deqp0FTY0Ek521d8CPWQY6A==";
        var sign = SecurityUtil.RSASignData(src, pem, RSAKeyMode.RSAPrivateKey, HashAlgorithmName.SHA256, CipherEncode.Base64);
        Console.WriteLine($"RSA-SHA256 BASE64签名验证结果：{sign == dest}");
        */

        var src1 = "64620230203140901S820230202014240000000f79d3484-b4fb-486c-a0b6-db978050e227http://123.127.93.180:29010/api/bank/mongopay/callback/pay307077ee07674acb9e314424c5005982";
        var privateKey1 = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCojGumVGNkLWI5HLV8autqWi1IzdVK/WtDBQqVG7A4P40i5YwOVIR0rpifhMr+2KdLrEXhMpKuooIUmSWn6SArlYTqqSVcHkgxWVOCKzMIE4eoUomiCoKd/veoNL0gOmav8e4xLOvSY/8sL58f3GNiv0FYL3Un5Mu45ufzlYS16V/W+s75WNAUNnaQPxzyg1ynRrsJRJ90Gjo/0gY3hhmxYOAJymq0ZrcU1XnsvIQWknVWHVWKb7Q6DYd+15v9upaG2AyYePLTFRnPxAcm8fZIE1tZuOCk+Er9fZ1OQYEOCGBsG9sFgzSNW/8FBypP9uWkNgI5peCepzlYCfBmdrd5AgMBAAECggEATjgMZT5Yec8N/E80ci70XYDH7zeGuqxssMeQJ66X0yK7DuzUWHokljnIno7z3d0Rlm62Z1Ie/GhyUbjY3tmzkkZrTrN8oQl0m2JG/OurXR4jIJxdPxP9lfETYb5H3sL7kL+HuM6OCi9Z5lmbUR/oB5WWb6RmjEMDwCV4k3RaCEkqmjG9ncYuuNk7lreB7Hu88461J8k4EtJAORfxC7IgCp7sPHyA1IoKewiVWqyGD9WUv5xBqKA6AICKrFR9dE0YZ/PXZ5XxKqm9Z4JSJmPnf5kXqTlTDcqVodeew0/7yrgC/pCCRgP+1rEEjS9XnpL7At6o0nhc2pIE3LalVZESWQKBgQDbQy47nOTxTcfv2U0XnZuhfzcPxbh0VYQ52SSY7UiyMRmdydCxRBQ8JDMzzDeysGG3LPHAvKBapHeYOUpT+V+4NfZ3WkJwC37hWAkEqAffj1E2g9zwXAs34zebvjnka5+oiApDgN0RECwgtr+64tl5cdg9f3MEBgI/5sSlZQownwKBgQDEyfgcYUkoQ+YHlP+EPvS/sNwqrc2FRGBXeU/LBteTVNq8wiqIbNjTnULe0iOcN2kmWv3Ql5Zk38DV5R1rOlI+R57F2T744mVzbYxMGzzdPJD+aq7wJAuIZptRSEIz89amKNMLkdyXL4xUOozR1y7iI7TSTZts8WeVky3gQdQo5wKBgQCC8BaO8P3jgGCSwkEAhlVnVxIpsBicGGmTJjJtxN3x09KJYPjos1AmLjQJjb4BTsW73vIr+DkGiEsHM8dIaJaZ6lfaJKOiR6sopSsVhcbV4b9M/+gT/dORN3aA4lOuLfs1aLBAaZca1n6Ttq6+yzO2C59n8lFcKL51LLloT/OK3wKBgAdLKQv8Oxbn9VWgmxvOrYmRR4e5a12eHCgB0ghHV6QZSAYo+CSYTC0Drh/OEA3RK++E/dVGWQeiF2aWGFIKFlRCmUrh82iME5CEpeeINuhIP2N+lr9Fxrumeek4UWPVjMYZy0CphlCALaZNiTyZyh4Njr0grTsLc3hivReLuQ5PAoGARZNZdcLeSoRmm8JarNhU21oxhcrgocEhHadaOlFT5ZVME2HTIXXBbla70hl4xHiSGsbZVBnByKdH12Fp/CVTqhfvL2SJSU5Fv96tohH+5QGPUHiEOGhqIFJHcNAZOSAxtOPQMqbKOO9g+ssNVpgYpBUVC4zaEROZKL4kV4EO+yA=";
        var dest1 = SecurityUtil.RSAEncryptUsePrivateKey(src1, privateKey1);
        var to_dest1 = "IztPt8+79trHvkhqSKvsnghkBgSCiQexVZXbVSdRpaPotTXnt0+hYRxfpa04VkEsHQbcfkvsden9FXVsukQjIfRsf7lMso3UsU7QEuk9SDCUCorXNt9O1fBUVDw6lTDh+BPdld74eWKBx41bX8fSCXiGXzuuTIptAOelvGbDhUXWEOFEHyDi2sk4yRJkOmAAnZfl91EVtnEw+vCpYkdc7Bh0P54WgCXVOaWWwp/npYiEQdJaMVrXZMghKYoeDTcg4mtszaKIv5h2b6xCgu6LT9Ryvtzzqio2j+N1jInYgBAZm8xfVhiNpXWmiC0ZwlWs0RbatuEt9k27YVet6TUJRg==";
        Console.WriteLine($"RSA私钥加密结果比较:{dest1 == to_dest1}");

        /*
        var dest2 = "B0WfT75XfN/KIJg26tYY+xPnyUqIv5rl/aAz6sAz7vq9lvIx9ptC6ILjmcBajE0lTCmuhfLdhZOmgnPm2AclH8AVuA3J2nXSVBbFsV3nB9SkSnKPDLTYcGkFCdCtbfXiHTyrVMxMBH6zB1xJnO09gI0AR6lcy+VigHtw2En14fUP0M3fNLxSkIpRuo3KrI0KMuWa06bn9d8JLeJj9ZQY7W/axIztKofG/xbWg2mgoC3yoTjGbnGjRMGvp4W7kUb+N7sU+NLaNvkee/qRBGHH1HBjCSZ+z+uLwlfO4+TnwuIvsGuOI8NWch59cd8Ud3xiKpbLYWODLafCtVsl1klkcg==";
        var publicKey2 = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCBFLlQnTzX11NJdlTVaMwWSSdPrwyiT1X2QtK40qtGSAkP6bHVgaKqwWwZEx8nujAI86bi1iY8RI+xpBwzNxJNa5GUHLbi6Dc4TO6HXFxFJyG0FubUaPOF1fSJVE4JvuaKnfe9S07tqJSU9xdd1pZD3S4iWoeegY5kYpPnQZXKVwIDAQAB";
        var src2 = SecurityUtil.RSADecryptUsePublicKey(dest2, publicKey2);
        var src2_2 = "Payment Successf79d3484-b4fb-486c-a0b6-db978050e227C1EBD63A-A038-4A21-BBBC-F0A4D4CDDF767.00100.00646_1623521152158797824SUCCESS124716402598";
        Console.WriteLine($"RSA公钥解密结果比较:{src2 == src2_2}");
        */
    }
}