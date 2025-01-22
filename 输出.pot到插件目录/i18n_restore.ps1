param(
    [Parameter(Mandatory)]
    [string]$t
)

# 运行 "dotnet tool restore" 以使“GetText.Extractor”命令可用
Write-Host "Running 'dotnet tool restore'..."
dotnet tool restore

# 获取文件扩展名
$ext = [System.IO.Path]::GetExtension($t)

# 处理通配符 *
if ($t -eq "*") {
    # 获取所有 .csproj 文件
    $projects = Get-ChildItem src/**/*.csproj -Recurse
    foreach ($p in $projects) {
        Write-Host "Processing project: $($p.FullName)"
        $pot = [System.IO.Path]::Combine($p.DirectoryName, "i18n", "template.pot")
        New-Item -Path $p.DirectoryName -Name i18n -ItemType Directory -Force
        dotnet tool run GetText.Extractor -u -o -s $p.FullName -t $pot
        Write-Host "请按Enter继续..."
        Read-Host
    }
}
# 处理单个 .csproj 文件
elseif ((Test-Path $t) -and $ext -eq ".csproj") {
    Write-Host "Processing project: $t"
    $FullName = [System.IO.Path]::GetFullPath($t)
    $dirName = [System.IO.Path]::GetDirectoryName($t)
    $pot = [System.IO.Path]::Combine($dirName, "i18n", "template.pot")
    New-Item -Path $dirName -Name i18n -ItemType Directory -Force
    dotnet tool run GetText.Extractor -u -o -s $FullName -t $pot
    Write-Host "请按Enter继续..."
    Read-Host
}
else {
    Write-Host "csproj file path does not exist or is not a valid .csproj file!"
}