升级当前 dotnet tool 工具
-s默认使用环境变量 NUGET_SOURCE
update -s http://10.0.0.101:5555/v3/index.json

在当前目录创建配置文件，默认tcmd.json
config-init

显示配置文件（默认tcmd.json）中定义的简易命令列表
ecmd-list

执行配置文件（默认tcmd.json）中定义的简易命令批处理
ecmd-bach --verbs 1,2,3