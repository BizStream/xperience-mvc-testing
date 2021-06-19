dotnet clean
dotnet build

Remove-Item .\packages\* -Recurse -Force

dotnet pack .\src\Testing\src -o .\packages

Get-ChildItem \NugetServer\BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing | Remove-Item -Recurse -Force -Confirm:$false

nuget init .\packages \NugetServer
