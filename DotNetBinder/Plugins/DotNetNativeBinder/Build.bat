mkdir C:\Epic\UE_4.26\Engine\Plugins\DotNetNativeBinder
call C:\Epic\UE_4.26\Engine\Build\BatchFiles\RunUAT.bat BuildPlugin -Plugin=%~dp0\DotNetNativeBinder.uplugin -Package=%~dp0\Build && (
  xcopy /s /y %~dp0\Build\* C:\Epic\UE_4.26\Engine\Plugins\DotNetNativeBinder
  (call )
)
