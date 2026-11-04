# escape=`
# SellCard ASP.NET MVC 5, .NET Framework 4.7.2
# Windows containers only; compatible ASP.NET Framework 4.8.1 IIS image.
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8.1-windowsservercore-ltsc2022

# Remove the IIS default splash screen; leave framework/IIS intact.
RUN powershell.exe -NoProfile -Command "Remove-Item 'C:\inetpub\wwwroot\iisstart.*' -Force -ErrorAction SilentlyContinue"

# Build context contains ONLY published website files, without Web.config.
COPY site/ C:/inetpub/wwwroot/
COPY start.ps1 C:/start.ps1

EXPOSE 80
ENTRYPOINT ["powershell.exe", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", "C:\\start.ps1"]
