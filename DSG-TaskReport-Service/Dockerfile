#添加基础镜像
#FROM microsoft/dotnet:2.2-aspnetcore-runtime
FROM stulzq/dotnet:2.2.0-aspnetcore-runtime-with-image

RUN /bin/cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
RUN echo "Asia/Shanghai" > /etc/timezone

#容器中系统的工作空间
WORKDIR /app
#拷贝当前文件夹下的文件到容器中系统的工作空间
COPY . /app
#开启的外部端口(对外暴露的端口) 可多个
EXPOSE 5000
ENTRYPOINT ["dotnet", "DSG-TaskReport-Service.dll"]