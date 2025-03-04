﻿using System.Collections.Generic;

namespace AiUo.Net;

internal static class FileExtContentTypeMapper
{
    #region Dicts
    private static readonly Dictionary<string, string> _extToType = new()
    {
        {".001","application/x-001"},
        {".323","text/h323"},
        {".907","drawing/907"},
        {".acp","audio/x-mei-aac"},
        {".aif","audio/aiff"},
        {".aiff","audio/aiff"},
        {".asa","text/asa"},
        {".asp","text/asp"},
        {".au","audio/basic"},
        {".awf","application/vnd.adobe.workflow"},
        {".bmp","application/x-bmp"},
        {".c4t","application/x-c4t"},
        {".cal","application/x-cals"},
        {".cdf","application/x-netcdf"},
        {".cel","application/x-cel"},
        {".cg4","application/x-g4"},
        {".cit","application/x-cit"},
        {".cml","text/xml"},
        {".cmx","application/x-cmx"},
        {".crl","application/pkix-crl"},
        {".csi","application/x-csi"},
        {".cut","application/x-cut"},
        {".dbm","application/x-dbm"},
        {".dcd","text/xml"},
        {".der","application/x-x509-ca-cert"},
        {".dib","application/x-dib"},
        {".doc","application/msword"},
        {".drw","application/x-drw"},
        {".dwf","Model/vnd.dwf"},
        {".dwg","application/x-dwg"},
        {".dxf","application/x-dxf"},
        {".emf","application/x-emf"},
        {".ent","text/xml"},
        {".eps","application/x-ps"},
        {".etd","application/x-ebx"},
        {".fax","image/fax"},
        {".fif","application/fractals"},
        {".frm","application/x-frm"},
        {".gbr","application/x-gbr"},
        {".gif","image/gif"},
        {".gp4","application/x-gp4"},
        {".hmr","application/x-hmr"},
        {".hpl","application/x-hpl"},
        {".hrf","application/x-hrf"},
        {".htc","text/x-component"},
        {".html","text/html"},
        {".htx","text/html"},
        {".ico","image/x-icon"},
        {".iff","application/x-iff"},
        {".igs","application/x-igs"},
        {".img","application/x-img"},
        {".isp","application/x-internet-signup"},
        {".java","java/*"},
        {".jpe","image/jpeg"},
        {".jpeg","image/jpeg"},
        {".jpg","application/x-jpg"},
        {".jsp","text/html"},
        {".lar","application/x-laplayer-reg"},
        {".lavs","audio/x-liquid-secure"},
        {".lmsff","audio/x-la-lms"},
        {".ltr","application/x-ltr"},
        {".m2v","video/x-mpeg"},
        {".m4e","video/mpeg4"},
        {".man","application/x-troff-man"},
        {".mdb","application/msaccess"},
        {".mfp","application/x-shockwave-flash"},
        {".mhtml","message/rfc822"},
        {".mid","audio/mid"},
        {".mil","application/x-mil"},
        {".mnd","audio/x-musicnet-download"},
        {".mocha","application/x-javascript"},
        {".mp1","audio/mp1"},
        {".mp2v","video/mpeg"},
        {".mp4","video/mpeg4"},
        {".mpd","application/vnd.ms-project"},
        {".mpeg","video/mpg"},
        {".mpga","audio/rn-mpeg"},
        {".mps","video/x-mpeg"},
        {".mpv","video/mpg"},
        {".mpw","application/vnd.ms-project"},
        {".mtx","text/xml"},
        {".net","image/pnetvue"},
        {".nws","message/rfc822"},
        {".out","application/x-out"},
        {".p12","application/x-pkcs12"},
        {".p7c","application/pkcs7-mime"},
        {".p7r","application/x-pkcs7-certreqresp"},
        {".pc5","application/x-pc5"},
        {".pcl","application/x-pcl"},
        {".pdf","application/pdf"},
        {".pdx","application/vnd.adobe.pdx"},
        {".pgl","application/x-pgl"},
        {".pko","application/vnd.ms-pki.pko"},
        {".plg","text/html"},
        {".plt","application/x-plt"},
        {".png","application/x-png"},
        {".ppa","application/vnd.ms-powerpoint"},
        {".pps","application/vnd.ms-powerpoint"},
        {".ppt","application/x-ppt"},
        {".prf","application/pics-rules"},
        {".prt","application/x-prt"},
        {".ps","application/postscript"},
        {".pwz","application/vnd.ms-powerpoint"},
        {".ra","audio/vnd.rn-realaudio"},
        {".ras","application/x-ras"},
        {".rdf","text/xml"},
        {".red","application/x-red"},
        {".rjs","application/vnd.rn-realsystem-rjs"},
        {".rlc","application/x-rlc"},
        {".rm","application/vnd.rn-realmedia"},
        {".rmi","audio/mid"},
        {".rmm","audio/x-pn-realaudio"},
        {".rms","application/vnd.rn-realmedia-secure"},
        {".rmx","application/vnd.rn-realsystem-rmx"},
        {".rp","image/vnd.rn-realpix"},
        {".rsml","application/vnd.rn-rsml"},
        {".rtf","application/msword"},
        {".rv","video/vnd.rn-realvideo"},
        {".sat","application/x-sat"},
        {".sdw","application/x-sdw"},
        {".slb","application/x-slb"},
        {".slk","drawing/x-slk"},
        {".smil","application/smil"},
        {".snd","audio/basic"},
        {".sor","text/plain"},
        {".spl","application/futuresplash"},
        {".ssm","application/streamingmedia"},
        {".stl","application/vnd.ms-pki.stl"},
        {".sty","application/x-sty"},
        {".swf","application/x-shockwave-flash"},
        {".tg4","application/x-tg4"},
        {".tif","image/tiff"},
        {".tiff","image/tiff"},
        {".top","drawing/x-top"},
        {".tsd","text/xml"},
        {".uin","application/x-icq"},
        {".vcf","text/x-vcard"},
        {".vdx","application/vnd.visio"},
        {".vpg","application/x-vpeg005"},
        {".vsd","application/x-vsd"},
        {".vst","application/vnd.visio"},
        {".vsw","application/vnd.visio"},
        {".vtx","application/vnd.visio"},
        {".wav","audio/wav"},
        {".wb1","application/x-wb1"},
        {".wb3","application/x-wb3"},
        {".wiz","application/msword"},
        {".wk4","application/x-wk4"},
        {".wks","application/x-wks"},
        {".wma","audio/x-ms-wma"},
        {".wmf","application/x-wmf"},
        {".wmv","video/x-ms-wmv"},
        {".wmz","application/x-ms-wmz"},
        {".wpd","application/x-wpd"},
        {".wpl","application/vnd.ms-wpl"},
        {".wr1","application/x-wr1"},
        {".wrk","application/x-wrk"},
        {".ws2","application/x-ws"},
        {".wsdl","text/xml"},
        {".xdp","application/vnd.adobe.xdp"},
        {".xfd","application/vnd.adobe.xfd"},
        {".xhtml","text/html"},
        {".xls","application/x-xls"},
        {".xml","text/xml"},
        {".xq","text/xml"},
        {".xquery","text/xml"},
        {".xsl","text/xml"},
        {".xwd","application/x-xwd"},
        {".sis","application/vnd.symbian.install"},
        {".x_t","application/x-x_t"},
        {".apk","application/vnd.android.package-archive"},
        {".tif","image/tiff"},
        {".301","application/x-301"},
        {".906","application/x-906"},
        {".a11","application/x-a11"},
        {".ai","application/postscript"},
        {".aifc","audio/aiff"},
        {".anv","application/x-anv"},
        {".asf","video/x-ms-asf"},
        {".asx","video/x-ms-asf"},
        {".avi","video/avi"},
        {".biz","text/xml"},
        {".bot","application/x-bot"},
        {".c90","application/x-c90"},
        {".cat","application/vnd.ms-pki.seccat"},
        {".cdr","application/x-cdr"},
        {".cer","application/x-x509-ca-cert"},
        {".cgm","application/x-cgm"},
        {".class","java/*"},
        {".cmp","application/x-cmp"},
        {".cot","application/x-cot"},
        {".crt","application/x-x509-ca-cert"},
        {".css","text/css"},
        {".dbf","application/x-dbf"},
        {".dbx","application/x-dbx"},
        {".dcx","application/x-dcx"},
        {".dgn","application/x-dgn"},
        {".dll","application/x-msdownload"},
        {".dot","application/msword"},
        {".dtd","text/xml"},
        {".dwf","application/x-dwf"},
        {".dxb","application/x-dxb"},
        {".edn","application/vnd.adobe.edn"},
        {".eml","message/rfc822"},
        {".epi","application/x-epi"},
        {".eps","application/postscript"},
        {".exe","application/x-msdownload"},
        {".fdf","application/vnd.fdf"},
        {".fo","text/xml"},
        {".g4","application/x-g4"},
        {".","application/x-"},
        {".gl2","application/x-gl2"},
        {".hgl","application/x-hgl"},
        {".hpg","application/x-hpgl"},
        {".hqx","application/mac-binhex40"},
        {".hta","application/hta"},
        {".htm","text/html"},
        {".htt","text/webviewhtml"},
        {".icb","application/x-icb"},
        {".ico","application/x-ico"},
        {".ig4","application/x-g4"},
        {".iii","application/x-iphone"},
        {".ins","application/x-internet-signup"},
        {".IVF","video/x-ivf"},
        {".jfif","image/jpeg"},
        {".jpe","application/x-jpe"},
        {".jpg","image/jpeg"},
        {".js","application/x-javascript"},
        {".la1","audio/x-liquid-file"},
        {".latex","application/x-latex"},
        {".lbm","application/x-lbm"},
        {".ls","application/x-javascript"},
        {".m1v","video/x-mpeg"},
        {".m3u","audio/mpegurl"},
        {".mac","application/x-mac"},
        {".math","text/xml"},
        {".mdb","application/x-mdb"},
        {".mht","message/rfc822"},
        {".mi","application/x-mi"},
        {".midi","audio/mid"},
        {".mml","text/xml"},
        {".mns","audio/x-musicnet-stream"},
        {".movie","video/x-sgi-movie"},
        {".mp2","audio/mp2"},
        {".mp3","audio/mp3"},
        {".mpa","video/x-mpg"},
        {".mpe","video/x-mpeg"},
        {".mpg","video/mpg"},
        {".mpp","application/vnd.ms-project"},
        {".mpt","application/vnd.ms-project"},
        {".mpv2","video/mpeg"},
        {".mpx","application/vnd.ms-project"},
        {".mxp","application/x-mmxp"},
        {".nrf","application/x-nrf"},
        {".odc","text/x-ms-odc"},
        {".p10","application/pkcs10"},
        {".p7b","application/x-pkcs7-certificates"},
        {".p7m","application/pkcs7-mime"},
        {".p7s","application/pkcs7-signature"},
        {".pci","application/x-pci"},
        {".pcx","application/x-pcx"},
        {".pdf","application/pdf"},
        {".pfx","application/x-pkcs12"},
        {".pic","application/x-pic"},
        {".pl","application/x-perl"},
        {".pls","audio/scpls"},
        {".png","image/png"},
        {".pot","application/vnd.ms-powerpoint"},
        {".ppm","application/x-ppm"},
        {".ppt","application/vnd.ms-powerpoint"},
        {".pr","application/x-pr"},
        {".prn","application/x-prn"},
        {".ps","application/x-ps"},
        {".ptn","application/x-ptn"},
        {".r3t","text/vnd.rn-realtext3d"},
        {".ram","audio/x-pn-realaudio"},
        {".rat","application/rat-file"},
        {".rec","application/vnd.rn-recording"},
        {".rgb","application/x-rgb"},
        {".rjt","application/vnd.rn-realsystem-rjt"},
        {".rle","application/x-rle"},
        {".rmf","application/vnd.adobe.rmf"},
        {".rmj","application/vnd.rn-realsystem-rmj"},
        {".rmp","application/vnd.rn-rn_music_package"},
        {".rmvb","application/vnd.rn-realmedia-vbr"},
        {".rnx","application/vnd.rn-realplayer"},
        {".rpm","audio/x-pn-realaudio-plugin"},
        {".rt","text/vnd.rn-realtext"},
        {".rtf","application/x-rtf"},
        {".sam","application/x-sam"},
        {".sdp","application/sdp"},
        {".sit","application/x-stuffit"},
        {".sld","application/x-sld"},
        {".smi","application/smil"},
        {".smk","application/x-smk"},
        {".sol","text/plain"},
        {".spc","application/x-pkcs7-certificates"},
        {".spp","text/xml"},
        {".sst","application/vnd.ms-pki.certstore"},
        {".stm","text/html"},
        {".svg","text/xml"},
        {".tdf","application/x-tdf"},
        {".tga","application/x-tga"},
        {".tif","application/x-tif"},
        {".tld","text/xml"},
        {".torrent","application/x-bittorrent"},
        {".txt","text/plain"},
        {".uls","text/iuls"},
        {".vda","application/x-vda"},
        {".vml","text/xml"},
        {".vsd","application/vnd.visio"},
        {".vss","application/vnd.visio"},
        {".vst","application/x-vst"},
        {".vsx","application/vnd.visio"},
        {".vxml","text/xml"},
        {".wax","audio/x-ms-wax"},
        {".wb2","application/x-wb2"},
        {".wbmp","image/vnd.wap.wbmp"},
        {".wk3","application/x-wk3"},
        {".wkq","application/x-wkq"},
        {".wm","video/x-ms-wm"},
        {".wmd","application/x-ms-wmd"},
        {".wml","text/vnd.wap.wml"},
        {".wmx","video/x-ms-wmx"},
        {".wp6","application/x-wp6"},
        {".wpg","application/x-wpg"},
        {".wq1","application/x-wq1"},
        {".wri","application/x-wri"},
        {".ws","application/x-ws"},
        {".wsc","text/scriptlet"},
        {".wvx","video/x-ms-wvx"},
        {".xdr","text/xml"},
        {".xfdf","application/vnd.adobe.xfdf"},
        {".xls","application/vnd.ms-excel"},
        {".xlw","application/x-xlw"},
        {".xpl","audio/scpls"},
        {".xql","text/xml"},
        {".xsd","text/xml"},
        {".xslt","text/xml"},
        {".x_b","application/x-x_b"},
        {".sisx","application/vnd.symbian.install"},
        {".ipa","application/vnd.iphone"},
        {".xap","application/x-silverlight-app"},
    };

    private static readonly Dictionary<string, string> _typeToExt = new()
    {
        {"application/octet-stream",".*"},
        {"application/x-001",".001"},
        {"text/h323",".323"},
        {"drawing/907",".907"},
        {"audio/x-mei-aac",".acp"},
        {"audio/aiff",".aif"},
        {"audio/aiff",".aiff"},
        {"text/asa",".asa"},
        {"text/asp",".asp"},
        {"audio/basic",".au"},
        {"application/vnd.adobe.workflow",".awf"},
        {"application/x-bmp",".bmp"},
        {"application/x-c4t",".c4t"},
        {"application/x-cals",".cal"},
        {"application/x-netcdf",".cdf"},
        {"application/x-cel",".cel"},
        {"application/x-g4",".cg4"},
        {"application/x-cit",".cit"},
        {"text/xml",".cml"},
        {"application/x-cmx",".cmx"},
        {"application/pkix-crl",".crl"},
        {"application/x-csi",".csi"},
        {"application/x-cut",".cut"},
        {"application/x-dbm",".dbm"},
        {"text/xml",".dcd"},
        {"application/x-x509-ca-cert",".der"},
        {"application/x-dib",".dib"},
        {"application/msword",".doc"},
        {"application/x-drw",".drw"},
        {"Model/vnd.dwf",".dwf"},
        {"application/x-dwg",".dwg"},
        {"application/x-dxf",".dxf"},
        {"application/x-emf",".emf"},
        {"text/xml",".ent"},
        {"application/x-ps",".eps"},
        {"application/x-ebx",".etd"},
        {"image/fax",".fax"},
        {"application/fractals",".fif"},
        {"application/x-frm",".frm"},
        {"application/x-gbr",".gbr"},
        {"image/gif",".gif"},
        {"application/x-gp4",".gp4"},
        {"application/x-hmr",".hmr"},
        {"application/x-hpl",".hpl"},
        {"application/x-hrf",".hrf"},
        {"text/x-component",".htc"},
        {"text/html",".html"},
        {"text/html",".htx"},
        {"image/x-icon",".ico"},
        {"application/x-iff",".iff"},
        {"application/x-igs",".igs"},
        {"application/x-img",".img"},
        {"application/x-internet-signup",".isp"},
        {"java/*",".java"},
        {"image/jpeg",".jpe"},
        {"image/jpeg",".jpeg"},
        {"application/x-jpg",".jpg"},
        {"text/html",".jsp"},
        {"application/x-laplayer-reg",".lar"},
        {"audio/x-liquid-secure",".lavs"},
        {"audio/x-la-lms",".lmsff"},
        {"application/x-ltr",".ltr"},
        {"video/x-mpeg",".m2v"},
        {"video/mpeg4",".m4e"},
        {"application/x-troff-man",".man"},
        {"application/msaccess",".mdb"},
        {"application/x-shockwave-flash",".mfp"},
        {"message/rfc822",".mhtml"},
        {"audio/mid",".mid"},
        {"application/x-mil",".mil"},
        {"audio/x-musicnet-download",".mnd"},
        {"application/x-javascript",".mocha"},
        {"audio/mp1",".mp1"},
        {"video/mpeg",".mp2v"},
        {"video/mpeg4",".mp4"},
        {"application/vnd.ms-project",".mpd"},
        {"video/mpg",".mpeg"},
        {"audio/rn-mpeg",".mpga"},
        {"video/x-mpeg",".mps"},
        {"video/mpg",".mpv"},
        {"application/vnd.ms-project",".mpw"},
        {"text/xml",".mtx"},
        {"image/pnetvue",".net"},
        {"message/rfc822",".nws"},
        {"application/x-out",".out"},
        {"application/x-pkcs12",".p12"},
        {"application/pkcs7-mime",".p7c"},
        {"application/x-pkcs7-certreqresp",".p7r"},
        {"application/x-pc5",".pc5"},
        {"application/x-pcl",".pcl"},
        {"application/pdf",".pdf"},
        {"application/vnd.adobe.pdx",".pdx"},
        {"application/x-pgl",".pgl"},
        {"application/vnd.ms-pki.pko",".pko"},
        {"text/html",".plg"},
        {"application/x-plt",".plt"},
        {"application/x-png",".png"},
        {"application/vnd.ms-powerpoint",".ppa"},
        {"application/vnd.ms-powerpoint",".pps"},
        {"application/x-ppt",".ppt"},
        {"application/pics-rules",".prf"},
        {"application/x-prt",".prt"},
        {"application/postscript",".ps"},
        {"application/vnd.ms-powerpoint",".pwz"},
        {"audio/vnd.rn-realaudio",".ra"},
        {"application/x-ras",".ras"},
        {"text/xml",".rdf"},
        {"application/x-red",".red"},
        {"application/vnd.rn-realsystem-rjs",".rjs"},
        {"application/x-rlc",".rlc"},
        {"application/vnd.rn-realmedia",".rm"},
        {"audio/mid",".rmi"},
        {"audio/x-pn-realaudio",".rmm"},
        {"application/vnd.rn-realmedia-secure",".rms"},
        {"application/vnd.rn-realsystem-rmx",".rmx"},
        {"image/vnd.rn-realpix",".rp"},
        {"application/vnd.rn-rsml",".rsml"},
        {"application/msword",".rtf"},
        {"video/vnd.rn-realvideo",".rv"},
        {"application/x-sat",".sat"},
        {"application/x-sdw",".sdw"},
        {"application/x-slb",".slb"},
        {"drawing/x-slk",".slk"},
        {"application/smil",".smil"},
        {"audio/basic",".snd"},
        {"text/plain",".sor"},
        {"application/futuresplash",".spl"},
        {"application/streamingmedia",".ssm"},
        {"application/vnd.ms-pki.stl",".stl"},
        {"application/x-sty",".sty"},
        {"application/x-shockwave-flash",".swf"},
        {"application/x-tg4",".tg4"},
        {"image/tiff",".tif"},
        {"image/tiff",".tiff"},
        {"drawing/x-top",".top"},
        {"text/xml",".tsd"},
        {"application/x-icq",".uin"},
        {"text/x-vcard",".vcf"},
        {"application/vnd.visio",".vdx"},
        {"application/x-vpeg005",".vpg"},
        {"application/x-vsd",".vsd"},
        {"application/vnd.visio",".vst"},
        {"application/vnd.visio",".vsw"},
        {"application/vnd.visio",".vtx"},
        {"audio/wav",".wav"},
        {"application/x-wb1",".wb1"},
        {"application/x-wb3",".wb3"},
        {"application/msword",".wiz"},
        {"application/x-wk4",".wk4"},
        {"application/x-wks",".wks"},
        {"audio/x-ms-wma",".wma"},
        {"application/x-wmf",".wmf"},
        {"video/x-ms-wmv",".wmv"},
        {"application/x-ms-wmz",".wmz"},
        {"application/x-wpd",".wpd"},
        {"application/vnd.ms-wpl",".wpl"},
        {"application/x-wr1",".wr1"},
        {"application/x-wrk",".wrk"},
        {"application/x-ws",".ws2"},
        {"text/xml",".wsdl"},
        {"application/vnd.adobe.xdp",".xdp"},
        {"application/vnd.adobe.xfd",".xfd"},
        {"text/html",".xhtml"},
        {"application/x-xls",".xls"},
        {"text/xml",".xml"},
        {"text/xml",".xq"},
        {"text/xml",".xquery"},
        {"text/xml",".xsl"},
        {"application/x-xwd",".xwd"},
        {"application/vnd.symbian.install",".sis"},
        {"application/x-x_t",".x_t"},
        {"application/vnd.android.package-archive",".apk"},
        {"image/tiff",".tif"},
        {"application/x-301",".301"},
        {"application/x-906",".906"},
        {"application/x-a11",".a11"},
        {"application/postscript",".ai"},
        {"audio/aiff",".aifc"},
        {"application/x-anv",".anv"},
        {"video/x-ms-asf",".asf"},
        {"video/x-ms-asf",".asx"},
        {"video/avi",".avi"},
        {"text/xml",".biz"},
        {"application/x-bot",".bot"},
        {"application/x-c90",".c90"},
        {"application/vnd.ms-pki.seccat",".cat"},
        {"application/x-cdr",".cdr"},
        {"application/x-x509-ca-cert",".cer"},
        {"application/x-cgm",".cgm"},
        {"java/*",".class"},
        {"application/x-cmp",".cmp"},
        {"application/x-cot",".cot"},
        {"application/x-x509-ca-cert",".crt"},
        {"text/css",".css"},
        {"application/x-dbf",".dbf"},
        {"application/x-dbx",".dbx"},
        {"application/x-dcx",".dcx"},
        {"application/x-dgn",".dgn"},
        {"application/x-msdownload",".dll"},
        {"application/msword",".dot"},
        {"text/xml",".dtd"},
        {"application/x-dwf",".dwf"},
        {"application/x-dxb",".dxb"},
        {"application/vnd.adobe.edn",".edn"},
        {"message/rfc822",".eml"},
        {"application/x-epi",".epi"},
        {"application/postscript",".eps"},
        {"application/x-msdownload",".exe"},
        {"application/vnd.fdf",".fdf"},
        {"text/xml",".fo"},
        {"application/x-g4",".g4"},
        {"application/x-","."},
        {"application/x-gl2",".gl2"},
        {"application/x-hgl",".hgl"},
        {"application/x-hpgl",".hpg"},
        {"application/mac-binhex40",".hqx"},
        {"application/hta",".hta"},
        {"text/html",".htm"},
        {"text/webviewhtml",".htt"},
        {"application/x-icb",".icb"},
        {"application/x-ico",".ico"},
        {"application/x-g4",".ig4"},
        {"application/x-iphone",".iii"},
        {"application/x-internet-signup",".ins"},
        {"video/x-ivf",".IVF"},
        {"image/jpeg",".jfif"},
        {"application/x-jpe",".jpe"},
        {"image/jpeg",".jpg"},
        {"application/x-javascript",".js"},
        {"audio/x-liquid-file",".la1"},
        {"application/x-latex",".latex"},
        {"application/x-lbm",".lbm"},
        {"application/x-javascript",".ls"},
        {"video/x-mpeg",".m1v"},
        {"audio/mpegurl",".m3u"},
        {"application/x-mac",".mac"},
        {"text/xml",".math"},
        {"application/x-mdb",".mdb"},
        {"message/rfc822",".mht"},
        {"application/x-mi",".mi"},
        {"audio/mid",".midi"},
        {"text/xml",".mml"},
        {"audio/x-musicnet-stream",".mns"},
        {"video/x-sgi-movie",".movie"},
        {"audio/mp2",".mp2"},
        {"audio/mp3",".mp3"},
        {"video/x-mpg",".mpa"},
        {"video/x-mpeg",".mpe"},
        {"video/mpg",".mpg"},
        {"application/vnd.ms-project",".mpp"},
        {"application/vnd.ms-project",".mpt"},
        {"video/mpeg",".mpv2"},
        {"application/vnd.ms-project",".mpx"},
        {"application/x-mmxp",".mxp"},
        {"application/x-nrf",".nrf"},
        {"text/x-ms-odc",".odc"},
        {"application/pkcs10",".p10"},
        {"application/x-pkcs7-certificates",".p7b"},
        {"application/pkcs7-mime",".p7m"},
        {"application/pkcs7-signature",".p7s"},
        {"application/x-pci",".pci"},
        {"application/x-pcx",".pcx"},
        {"application/pdf",".pdf"},
        {"application/x-pkcs12",".pfx"},
        {"application/x-pic",".pic"},
        {"application/x-perl",".pl"},
        {"audio/scpls",".pls"},
        {"image/png",".png"},
        {"application/vnd.ms-powerpoint",".pot"},
        {"application/x-ppm",".ppm"},
        {"application/vnd.ms-powerpoint",".ppt"},
        {"application/x-pr",".pr"},
        {"application/x-prn",".prn"},
        {"application/x-ps",".ps"},
        {"application/x-ptn",".ptn"},
        {"text/vnd.rn-realtext3d",".r3t"},
        {"audio/x-pn-realaudio",".ram"},
        {"application/rat-file",".rat"},
        {"application/vnd.rn-recording",".rec"},
        {"application/x-rgb",".rgb"},
        {"application/vnd.rn-realsystem-rjt",".rjt"},
        {"application/x-rle",".rle"},
        {"application/vnd.adobe.rmf",".rmf"},
        {"application/vnd.rn-realsystem-rmj",".rmj"},
        {"application/vnd.rn-rn_music_package",".rmp"},
        {"application/vnd.rn-realmedia-vbr",".rmvb"},
        {"application/vnd.rn-realplayer",".rnx"},
        {"audio/x-pn-realaudio-plugin",".rpm"},
        {"text/vnd.rn-realtext",".rt"},
        {"application/x-rtf",".rtf"},
        {"application/x-sam",".sam"},
        {"application/sdp",".sdp"},
        {"application/x-stuffit",".sit"},
        {"application/x-sld",".sld"},
        {"application/smil",".smi"},
        {"application/x-smk",".smk"},
        {"text/plain",".sol"},
        {"application/x-pkcs7-certificates",".spc"},
        {"text/xml",".spp"},
        {"application/vnd.ms-pki.certstore",".sst"},
        {"text/html",".stm"},
        {"text/xml",".svg"},
        {"application/x-tdf",".tdf"},
        {"application/x-tga",".tga"},
        {"application/x-tif",".tif"},
        {"text/xml",".tld"},
        {"application/x-bittorrent",".torrent"},
        {"text/plain",".txt"},
        {"text/iuls",".uls"},
        {"application/x-vda",".vda"},
        {"text/xml",".vml"},
        {"application/vnd.visio",".vsd"},
        {"application/vnd.visio",".vss"},
        {"application/x-vst",".vst"},
        {"application/vnd.visio",".vsx"},
        {"text/xml",".vxml"},
        {"audio/x-ms-wax",".wax"},
        {"application/x-wb2",".wb2"},
        {"image/vnd.wap.wbmp",".wbmp"},
        {"application/x-wk3",".wk3"},
        {"application/x-wkq",".wkq"},
        {"video/x-ms-wm",".wm"},
        {"application/x-ms-wmd",".wmd"},
        {"text/vnd.wap.wml",".wml"},
        {"video/x-ms-wmx",".wmx"},
        {"application/x-wp6",".wp6"},
        {"application/x-wpg",".wpg"},
        {"application/x-wq1",".wq1"},
        {"application/x-wri",".wri"},
        {"application/x-ws",".ws"},
        {"text/scriptlet",".wsc"},
        {"video/x-ms-wvx",".wvx"},
        {"text/xml",".xdr"},
        {"application/vnd.adobe.xfdf",".xfdf"},
        {"application/vnd.ms-excel",".xls"},
        {"application/x-xlw",".xlw"},
        {"audio/scpls",".xpl"},
        {"text/xml",".xql"},
        {"text/xml",".xsd"},
        {"text/xml",".xslt"},
        {"application/x-x_b",".x_b"},
        {"application/vnd.symbian.install",".sisx"},
        {"application/vnd.iphone",".ipa"},
        {"application/x-silverlight-app",".xap"},
    };
    #endregion

    public static string GetContentType(string fileExt)
        => _extToType.ContainsKey(fileExt) ? _extToType[fileExt] : "application/octet-stream";
    public static string GetFileExtension(string contentType)
        => _typeToExt[contentType];
}