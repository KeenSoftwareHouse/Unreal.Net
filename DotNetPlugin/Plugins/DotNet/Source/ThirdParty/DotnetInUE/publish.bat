cd %~dp0

(
dotnet publish Tests/Tests.csproj
) && (
  xcopy /y /i Artefacts ..\..\..\Binaries\ThirdParty\DotNetLibrary\Win64
) || (
  echo Build Failed!
  pause
)