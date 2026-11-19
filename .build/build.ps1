<#
.SYNOPSIS
    Toltek build script — yerel geliştirme ve CI için ortak giriş noktası.

.DESCRIPTION
    Repo'ya özel ayarlar .build/build.settings.json dosyasından okunur; bu script
    jeneriktir ve diğer Toltek repolarına aynen kopyalanabilir.

    Action'lar:
      build      Incremental derleme (varsayılan hızlı yol, clean YOK).
      pack       build + SADECE DEĞİŞEN projeleri paketle + yerel feed'e kopyala
                 + global NuGet cache'inden sadece o paketleri düşür.
                 -All ile tüm projeler paketlenir.
      test       Ayarlardaki test projelerini çalıştırır.
      clean      dotnet clean (istisnai durumlar için).
      nuke       Tüm yerel nupkg'ları ve global cache girdilerini siler + clean.
                 (Eski "delete" davranışı — nükleer seçenek.)
      restore    dotnet restore.
      sln-all    src + tests + samples projelerinden master solution üretir
                 (slnf'lerin ebeveyni; VS'te açmak için değil).
      slnf       Bir modül için bağımlılık kapanışı + test + sample içeren
                 solution filter (.slnf) üretir. Örn: build.ps1 slnf -Module Caching

.EXAMPLE
    .\build.ps1 build
    .\build.ps1 pack
    .\build.ps1 pack -All -Configuration Release
    .\build.ps1 slnf -Module Caching
#>
#Requires -Version 7
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('build', 'pack', 'test', 'clean', 'nuke', 'restore', 'sln-all', 'slnf', 'help')]
    [string]$Action = 'help',

    [string]$Solution,               # settings.json'daki solution'ı geçersiz kılar
    [string]$Configuration,          # varsayılan settings.json'dan (Debug)
    [switch]$All,                    # pack: değişiklik tespiti yapmadan tümünü paketle
    [string]$Module,                 # slnf: modül adı (proje adlarında aranır)
    [string[]]$Projects,             # slnf: tohum proje adları (Module yerine)
    [string]$Filter,                 # test: settings.testFilter'ı geçersiz kılan xUnit filtresi; 'all'/'*' → filtresiz
    [switch]$DryRun,                 # pack/nuke: ne yapılacağını göster, yapma
    [switch]$NoPause                 # bat sarmalayıcıları için; script zaten beklemez
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)

# ---------------------------------------------------------------------------
# Ortam ve ayarlar
# ---------------------------------------------------------------------------
$script:RepoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$script:IsCi       = $env:CI -eq 'true'
$script:StartedAt  = Get-Date

$settingsPath = Join-Path $PSScriptRoot 'build.settings.json'
if (-not (Test-Path $settingsPath)) {
    Write-Error "Ayar dosyası bulunamadı: $settingsPath"
    exit 1
}
$script:Settings = Get-Content $settingsPath -Raw | ConvertFrom-Json

function Resolve-RepoPath([string]$Relative) {
    return [System.IO.Path]::GetFullPath((Join-Path $script:RepoRoot $Relative))
}

if (-not $Solution)      { $Solution      = $script:Settings.solution }
if (-not $Configuration) { $Configuration = $script:Settings.configuration }
if (-not $Configuration) { $Configuration = 'Debug' }

$script:SolutionPath  = Resolve-RepoPath $Solution
$script:PackageFolder = $env:TOLTEK_PACKAGE_FOLDER
if (-not $script:PackageFolder) { $script:PackageFolder = $script:Settings.packageFolder }
$script:BinRoot = $script:Settings.binRoot

# ---------------------------------------------------------------------------
# Loglama
# ---------------------------------------------------------------------------
function Write-Log([string]$Message, [ConsoleColor]$Color = [ConsoleColor]::Gray) {
    Write-Host ("[{0:HH:mm:ss}] {1}" -f (Get-Date), $Message) -ForegroundColor $Color
}
function Write-Info([string]$m) { Write-Log $m ([ConsoleColor]::Cyan) }
function Write-Ok([string]$m)   { Write-Log $m ([ConsoleColor]::Green) }
function Write-Warn2([string]$m){ Write-Log $m ([ConsoleColor]::Yellow) }
function Write-Err([string]$m)  { Write-Log $m ([ConsoleColor]::Red) }

# Adımı çalıştırır, süresini ölçer, hata durumunda script'i durdurur.
function Invoke-Step([string]$Name, [scriptblock]$Body) {
    Write-Info "==> $Name"
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    & $Body
    $sw.Stop()
    Write-Ok ("<== {0} ({1:n1} sn)" -f $Name, $sw.Elapsed.TotalSeconds)
}

# Harici komut çalıştırır, exit code'u denetler.
function Invoke-Exec([string]$Description, [string[]]$CommandLine) {
    Write-Log ("$ " + ($CommandLine -join ' ')) ([ConsoleColor]::DarkGray)
    & $CommandLine[0] @($CommandLine[1..($CommandLine.Length - 1)])
    if ($LASTEXITCODE -ne 0) {
        Write-Err "$Description başarısız (exit $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}

# ---------------------------------------------------------------------------
# Solution / proje yardımcıları
# ---------------------------------------------------------------------------
function Get-SolutionProjects {
    # `dotnet sln list` hem .sln hem .slnx formatını destekler; regex parse yok.
    $slnDir = Split-Path $script:SolutionPath -Parent
    $lines = & dotnet sln $script:SolutionPath list 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Solution okunamadı: $script:SolutionPath"
        exit 1
    }
    $projects = @()
    foreach ($line in $lines) {
        if ($line -notmatch '\.csproj\s*$') { continue }
        $full = [System.IO.Path]::GetFullPath((Join-Path $slnDir $line.Trim()))
        if (-not (Test-Path $full)) { Write-Warn2 "Proje dosyası yok: $full"; continue }
        $projects += [pscustomobject]@{
            Name = [System.IO.Path]::GetFileNameWithoutExtension($full)
            Path = $full
        }
    }
    return $projects
}

function Get-RepoDefaultVersion {
    $targetsPath = Join-Path $script:RepoRoot 'Directory.Build.targets'
    if (Test-Path $targetsPath) {
        [xml]$xml = Get-Content $targetsPath -Raw
        $v = ($xml.Project.PropertyGroup | ForEach-Object { $_.Version } | Where-Object { $_ }) | Select-Object -First 1
        if ($v) { return [string]$v }
    }
    return $null
}

# csproj'dan PackageId/Version okur; yoksa dosya adı / repo varsayılanı kullanılır.
# IsPackable=false olan veya test altyapı paketleri referanslayan projeler
# paketlenemez sayılır (Test.Sdk ve test adapter'ları IsPackable'ı örtük false yapar).
$script:TestInfraPackages = @('Microsoft.NET.Test.Sdk', 'NUnit3TestAdapter', 'xunit.runner.visualstudio', 'MSTest', 'coverlet.collector')
function Get-ProjectMeta([object]$Project, [string]$DefaultVersion) {
    [xml]$xml = Get-Content $Project.Path -Raw
    $pgs = @($xml.Project.PropertyGroup)
    $packageId  = ($pgs | ForEach-Object { $_.PackageId }  | Where-Object { $_ }) | Select-Object -First 1
    $version    = ($pgs | ForEach-Object { $_.Version }    | Where-Object { $_ }) | Select-Object -First 1
    $isPackable = ($pgs | ForEach-Object { $_.IsPackable } | Where-Object { $_ }) | Select-Object -First 1
    $usesMaui   = ($pgs | ForEach-Object { $_.UseMaui }    | Where-Object { $_ }) | Select-Object -First 1
    $usesTestSdk = @($xml.Project.ItemGroup.PackageReference | Where-Object { $script:TestInfraPackages -contains $_.Include }).Count -gt 0
    if (-not $packageId) { $packageId = $Project.Name }
    if (-not $version)   { $version   = $DefaultVersion }
    return [pscustomobject]@{
        Name      = $Project.Name
        Path      = $Project.Path
        PackageId = [string]$packageId
        Version   = [string]$version
        Packable  = (([string]$isPackable) -ne 'false') -and (-not $usesTestSdk)
        # MAUI nupkg'ları feed'e kopyalanmaz (CopyPackage UseMaui'yi atlar);
        # feed'de izi olmadığı için değişiklik tespitine dahil edilemez.
        UsesMaui  = (([string]$usesMaui) -eq 'true')
    }
}

function Get-GlobalPackagesPath {
    $line = (& dotnet nuget locals global-packages --list) | Select-Object -First 1
    return ($line -replace '^[^:]+:\s*', '').Trim()
}

# Paketlenebilir projeler: settings.packableRoots altındakiler (örn. sadece src/).
function Select-PackableProjects([object[]]$AllProjects) {
    $roots = @($script:Settings.packableRoots)
    if (-not $roots -or $roots.Count -eq 0) { return $AllProjects }
    $rootPaths = $roots | ForEach-Object { (Resolve-RepoPath $_) + [System.IO.Path]::DirectorySeparatorChar }
    return @($AllProjects | Where-Object {
        $p = $_.Path
        ($rootPaths | Where-Object { $p.StartsWith($_, [System.StringComparison]::OrdinalIgnoreCase) }).Count -gt 0
    })
}

# Projenin KENDİ assembly'sinin en yeni yazılma zamanı (incremental build'de
# sadece gerçekten derlenen projenin kendi dll'i yenilenir → değişiklik sinyali).
# Bin'e kopyalanan bağımlılık dll'lerine bakılmaz — yoksa bir bağımlılık
# değiştiğinde tüm dependent zinciri sonsuza dek "değişmiş" görünür.
# Hem opsiyonel binRoot (çıktı yönlendirmesi) hem proje yanı bin taranır,
# en yeni zaman alınır — bayat bir cache klasörü yanlış negatif üretemez.
function Get-NewestOutputTime([object]$Project, [string]$Config) {
    $candidates = @()
    if ($script:BinRoot) { $candidates += [System.IO.Path]::Combine($script:BinRoot, $Project.Name, $Config) }
    $candidates += [System.IO.Path]::Combine((Split-Path $Project.Path -Parent), 'bin', $Config)
    $ownAssembly = "$($Project.Name).dll"
    $newest = $null
    foreach ($dir in $candidates) {
        if (-not (Test-Path $dir)) { continue }
        $top = Get-ChildItem $dir -Recurse -Filter $ownAssembly -File -ErrorAction SilentlyContinue |
               Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
        if ($top -and (-not $newest -or $top.LastWriteTimeUtc -gt $newest)) { $newest = $top.LastWriteTimeUtc }
    }
    return $newest
}

# ---------------------------------------------------------------------------
# Action'lar
# ---------------------------------------------------------------------------
function Action-Build {
    Invoke-Step "Build ($Configuration) :: $Solution" {
        Invoke-Exec 'dotnet build' @('dotnet', 'build', $script:SolutionPath, '-c', $Configuration, '--nologo', '-v', 'minimal')
    }
}

function Action-Restore {
    Invoke-Step "Restore :: $Solution" {
        Invoke-Exec 'dotnet restore' @('dotnet', 'restore', $script:SolutionPath)
    }
}

function Action-Clean {
    Invoke-Step "Clean ($Configuration) :: $Solution" {
        Invoke-Exec 'dotnet clean' @('dotnet', 'clean', $script:SolutionPath, '-c', $Configuration, '--nologo', '-v', 'minimal')
    }
}

function Action-Test {
    $targets = @($script:Settings.testTargets)
    if ($targets.Count -eq 0) { Write-Warn2 'Ayarlarda testTargets tanımlı değil.'; return }

    # Ref-flip tuzağı: test projesini çıplak derlemek bayat feed paketine bağlanır (CS0246/CS0618 sahte
    # hataları). Önce dev slnx TAM derlenir → SolutionPath doğru set → PackageReference→ProjectReference
    # flip olur; sonra testler --no-build + -p:SolutionPath ile o çıktıya karşı koşar (settings.testSolution).
    $testSolution = [string]$script:Settings.testSolution
    $solutionPathProp = $null
    if ($testSolution) {
        $testSlnFull = Resolve-RepoPath $testSolution
        Invoke-Step "Build (test solution) :: $testSolution" {
            Invoke-Exec 'dotnet build' @('dotnet', 'build', $testSlnFull, '-c', $Configuration, '--nologo', '-v', 'minimal')
        }
        $solutionPathProp = "-p:SolutionPath=$testSlnFull"
    }

    # Filtre önceliği: -Filter parametresi > settings.testFilter. '-Filter all' (veya '*') → filtresiz (tüm testler).
    $effectiveFilter = if ($Filter) { $Filter } else { [string]$script:Settings.testFilter }
    if ($effectiveFilter -in @('all', '*')) { $effectiveFilter = $null }
    if ($effectiveFilter) { Write-Info "Test filtresi: $effectiveFilter" }

    foreach ($t in $targets) {
        $path = Resolve-RepoPath $t
        $cmd = @('dotnet', 'test', $path, '-c', $Configuration, '--nologo')
        if ($testSolution)    { $cmd += @('--no-build', $solutionPathProp) }
        if ($effectiveFilter) { $cmd += @('--filter', $effectiveFilter) }
        Invoke-Step "Test :: $t" { Invoke-Exec 'dotnet test' $cmd }
    }
}

function Action-Pack {
    $runStart = (Get-Date).ToUniversalTime()
    if ($DryRun) { Write-Warn2 'DryRun — build atlandı, mevcut çıktılara göre tespit yapılıyor.' }
    else { Action-Build }

    $defaultVersion = Get-RepoDefaultVersion
    $allProjects    = Get-SolutionProjects
    $packable       = Select-PackableProjects $allProjects
    Write-Info "Solution'da $($allProjects.Count) proje, paketlenebilir $($packable.Count)."

    # 1) Değişen projeleri tespit et: çıktı dll'i, feed'deki nupkg'dan yeniyse değişmiştir.
    $toPack = @()
    foreach ($proj in $packable) {
        $meta = Get-ProjectMeta $proj $defaultVersion
        if (-not $meta.Packable) { Write-Log "   paketlenemez, atlandı: $($meta.Name)"; continue }
        if (-not $meta.Version) { Write-Warn2 "Versiyon bulunamadı, atlandı: $($meta.Name)"; continue }

        if ($All) { $toPack += $meta; continue }
        if ($meta.UsesMaui) { continue } # feed'de izi yok, tespit yapılamaz (sadece -All ile paketlenir)

        $outputTime = Get-NewestOutputTime $proj $Configuration
        if (-not $outputTime) { Write-Warn2 "Derleme çıktısı yok, atlandı: $($meta.Name)"; continue }

        $nupkg = Join-Path $script:PackageFolder "$($meta.PackageId).$($meta.Version).nupkg"
        if (-not (Test-Path $nupkg) -or $outputTime -gt (Get-Item $nupkg).LastWriteTimeUtc) {
            $toPack += $meta
        }
    }

    if ($toPack.Count -eq 0) {
        Write-Ok 'Tüm paketler güncel — paketlenecek bir şey yok.'
        return
    }

    Write-Info "Paketlenecek $($toPack.Count) proje:"
    $toPack | ForEach-Object { Write-Log "   * $($_.PackageId) $($_.Version)" }
    if ($DryRun) { Write-Warn2 'DryRun — paketleme yapılmadı.'; return }

    # 2) Değişenleri tek MSBuild çağrısında paralel paketle (--no-build eşdeğeri).
    #    ToltekPack=true → GeneratePackageOnBuild=true → CopyPackage nupkg'ı feed'e kopyalar.
    $packProj = Join-Path ([System.IO.Path]::GetTempPath()) 'toltek-pack.proj'
    $items = ($toPack | ForEach-Object { "    <ProjectToPack Include=`"$($_.Path)`" />" }) -join "`r`n"
    @"
<Project>
  <ItemGroup>
$items
  </ItemGroup>
  <Target Name="PackChanged">
    <MSBuild Projects="@(ProjectToPack)" Targets="Pack" BuildInParallel="true"
             Properties="Configuration=$Configuration;NoBuild=true;ToltekPack=true" />
  </Target>
</Project>
"@ | Set-Content -Path $packProj -Encoding UTF8

    Invoke-Step "Pack ($($toPack.Count) proje)" {
        Invoke-Exec 'dotnet msbuild pack' @('dotnet', 'msbuild', $packProj, '-t:PackChanged', '-m', '--nologo', '-v', 'minimal')
    }

    # 3) Global NuGet cache'inden SADECE yeniden paketlenenleri düşür.
    #    Tüketici repo bir sonraki build'de bunları yerel feed'den yeniden açar.
    $gpf = Get-GlobalPackagesPath
    Invoke-Step 'Global cache eviction' {
        foreach ($meta in $toPack) {
            $dir = [System.IO.Path]::Combine($gpf, $meta.PackageId.ToLowerInvariant(), $meta.Version)
            if (Test-Path $dir) {
                Remove-Item $dir -Recurse -Force
                Write-Log "   cache silindi: $($meta.PackageId)/$($meta.Version)"
            }
        }
    }

    # 3b) Emniyet ağı: bu çalıştırma sırasında feed'de yenilenen ama toPack'te OLMAYAN
    #     nupkg'lar da cache'ten düşürülür. Bir proje GeneratePackageOnBuild=true ile BUILD
    #     adımında paketlenirse nupkg tazelenir, değişiklik tespiti "dll nupkg'dan yeni değil"
    #     deyip projeyi atlar ve cache bayat kalırdı (Toltek.Berry / 'rail' enum vakası).
    Invoke-Step 'Global cache eviction (pack-on-build kaçakları)' {
        foreach ($nupkg in Get-ChildItem $script:PackageFolder -Filter '*.nupkg' -File) {
            if ($nupkg.LastWriteTimeUtc -lt $runStart) { continue }
            if ($nupkg.BaseName -notmatch '^(?<id>.+?)\.(?<ver>\d+(\.\d+)+(-[0-9A-Za-z.-]+)?)$') { continue }
            $id = $Matches.id; $ver = $Matches.ver
            if ($toPack | Where-Object { $_.PackageId -eq $id -and $_.Version -eq $ver }) { continue }
            $dir = [System.IO.Path]::Combine($gpf, $id.ToLowerInvariant(), $ver)
            if (Test-Path $dir) {
                Remove-Item $dir -Recurse -Force
                Write-Log "   cache silindi (kaçak): $id/$ver"
            }
        }
    }

    Write-Ok ("Özet: {0} paket yenilendi → {1}" -f $toPack.Count, $script:PackageFolder)
}

function Action-Nuke {
    $defaultVersion = Get-RepoDefaultVersion
    $allProjects    = Get-SolutionProjects
    $gpf            = Get-GlobalPackagesPath
    Write-Warn2 "NUKE: $($allProjects.Count) projenin nupkg + cache girdileri silinecek."

    $deletedFeed = 0; $deletedCache = 0
    foreach ($proj in $allProjects) {
        $meta = Get-ProjectMeta $proj $defaultVersion
        if (-not $meta.Version) { continue }

        $nupkg = Join-Path $script:PackageFolder "$($meta.PackageId).$($meta.Version).nupkg"
        if (Test-Path $nupkg) {
            if (-not $DryRun) { Remove-Item $nupkg -Force }
            $deletedFeed++
        }
        $cacheDir = [System.IO.Path]::Combine($gpf, $meta.PackageId.ToLowerInvariant(), $meta.Version)
        if (Test-Path $cacheDir) {
            if (-not $DryRun) { Remove-Item $cacheDir -Recurse -Force }
            $deletedCache++
        }
    }
    Write-Ok "Feed'den $deletedFeed nupkg, cache'ten $deletedCache klasör silindi.$(if ($DryRun) { ' (DryRun)' })"
    if (-not $DryRun) { Action-Clean }
}

# ---------------------------------------------------------------------------
# Master solution + solution filter üretimi
# ---------------------------------------------------------------------------
function Get-AllRepoProjects {
    $dirs = @('src', 'tests', 'samples') | ForEach-Object { Resolve-RepoPath $_ } | Where-Object { Test-Path $_ }
    return @(Get-ChildItem $dirs -Recurse -Filter *.csproj -File)
}

function Action-SlnAll {
    $master = Resolve-RepoPath $script:Settings.masterSolution
    $masterDir = Split-Path $master -Parent
    $masterName = [System.IO.Path]::GetFileNameWithoutExtension($master)

    $projects = Get-AllRepoProjects
    Write-Info "Master solution: $master ($($projects.Count) proje)"
    if ($DryRun) { Write-Warn2 'DryRun — üretilmedi.'; return }

    if (Test-Path $master) { Remove-Item $master -Force }
    # Format masterSolution uzantısından gelir: `dotnet new sln` adı -n'den, uzantıyı
    # --format'tan alır; sabit bir format öteki uzantıdaki repoda $master'ı hiç üretmez.
    # SmartPackageReference (target-actions.targets) iki formatı da işler.
    $masterFormat = if ([System.IO.Path]::GetExtension($master).ToLowerInvariant() -eq '.slnx') { 'slnx' } else { 'sln' }
    Invoke-Exec 'dotnet new sln' @('dotnet', 'new', 'sln', '-n', $masterName, '-o', $masterDir, '--format', $masterFormat)

    # Argüman uzunluğu sınırına takılmamak için parça parça ekle.
    $chunkSize = 30
    for ($i = 0; $i -lt $projects.Count; $i += $chunkSize) {
        $chunk = $projects[$i..([Math]::Min($i + $chunkSize - 1, $projects.Count - 1))] | ForEach-Object { $_.FullName }
        Invoke-Exec 'dotnet sln add' (@('dotnet', 'sln', $master, 'add') + $chunk)
    }
    Write-Ok "Master solution üretildi: $master"
}

function Action-Slnf {
    if (-not $Module -and (-not $Projects -or $Projects.Count -eq 0)) {
        Write-Err 'slnf için -Module <ad> veya -Projects <ad,ad> verin. Örn: build.ps1 slnf -Module Caching'
        exit 1
    }
    $master = Resolve-RepoPath $script:Settings.masterSolution
    if (-not (Test-Path $master)) {
        Write-Warn2 "Master solution yok, önce üretiliyor (sln-all)..."
        Action-SlnAll
    }

    # Tüm projelerin adı→yol ve bağımlılık haritası (PackageReference adları da
    # dahili projelere işaret edebilir — SmartPackageReference modeli).
    $all = Get-AllRepoProjects
    $byName = @{}
    foreach ($p in $all) { $byName[[System.IO.Path]::GetFileNameWithoutExtension($p.Name)] = $p.FullName }

    $depsOf = @{}
    foreach ($p in $all) {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($p.Name)
        [xml]$xml = Get-Content $p.FullName -Raw
        $deps = New-Object System.Collections.Generic.List[string]
        foreach ($ig in @($xml.Project.ItemGroup)) {
            foreach ($pr in @($ig.PackageReference)) {
                if ($pr.Include -and $byName.ContainsKey([string]$pr.Include)) { $deps.Add([string]$pr.Include) }
            }
            foreach ($pj in @($ig.ProjectReference)) {
                if (-not $pj.Include) { continue }
                $refPath = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $p.FullName -Parent) ([string]$pj.Include)))
                $refName = [System.IO.Path]::GetFileNameWithoutExtension($refPath)
                if ($byName.ContainsKey($refName)) { $deps.Add($refName) }
            }
        }
        $depsOf[$name] = $deps
    }

    # Tohum projeler
    $srcRoot = (Resolve-RepoPath 'src') + [System.IO.Path]::DirectorySeparatorChar
    if ($Projects -and $Projects.Count -gt 0) {
        $seeds = @($Projects | Where-Object { $byName.ContainsKey($_) })
        $missing = @($Projects | Where-Object { -not $byName.ContainsKey($_) })
        $missing | ForEach-Object { Write-Warn2 "Proje bulunamadı: $_" }
        $filterName = $Projects[0]
    }
    else {
        $seeds = @($byName.Keys | Where-Object {
            $_ -like "*$Module*" -and $byName[$_].StartsWith($srcRoot, [System.StringComparison]::OrdinalIgnoreCase)
        })
        $filterName = "Toltek.$Module"
    }
    if ($seeds.Count -eq 0) { Write-Err 'Tohum proje bulunamadı.'; exit 1 }
    Write-Info "Tohum projeler: $($seeds -join ', ')"

    # Bağımlılık kapanışı
    $include = New-Object System.Collections.Generic.HashSet[string]
    $stack = New-Object System.Collections.Generic.Stack[string]
    $seeds | ForEach-Object { [void]$stack.Push($_) }
    while ($stack.Count -gt 0) {
        $cur = $stack.Pop()
        if (-not $include.Add($cur)) { continue }
        foreach ($d in $depsOf[$cur]) { if (-not $include.Contains($d)) { [void]$stack.Push($d) } }
    }

    # Tohumlara referans veren test/sample projeleri + onların kapanışı
    $seedSet = New-Object System.Collections.Generic.HashSet[string]
    $seeds | ForEach-Object { [void]$seedSet.Add($_) }
    foreach ($name in $byName.Keys) {
        if ($byName[$name].StartsWith($srcRoot, [System.StringComparison]::OrdinalIgnoreCase)) { continue }
        $refsSeed = @($depsOf[$name] | Where-Object { $seedSet.Contains($_) }).Count -gt 0
        if (-not $refsSeed) { continue }
        [void]$stack.Push($name)
        while ($stack.Count -gt 0) {
            $cur = $stack.Pop()
            if (-not $include.Add($cur)) { continue }
            foreach ($d in $depsOf[$cur]) { if (-not $include.Contains($d)) { [void]$stack.Push($d) } }
        }
    }

    # slnf yaz (yollar slnf dosyasının dizinine görelidir)
    $masterDir = Split-Path $master -Parent
    $slnfPath = Join-Path $masterDir "$filterName.slnf"
    $relProjects = $include | Sort-Object | ForEach-Object {
        [System.IO.Path]::GetRelativePath($masterDir, $byName[$_]).Replace('/', '\')
    }
    $slnf = [ordered]@{
        solution = [ordered]@{
            path     = [System.IO.Path]::GetRelativePath($masterDir, $master).Replace('/', '\')
            projects = @($relProjects)
        }
    }
    if ($DryRun) {
        Write-Warn2 "DryRun — $slnfPath yazılmadı. İçerik:"
        $slnf | ConvertTo-Json -Depth 4 | Write-Host
        return
    }
    $slnf | ConvertTo-Json -Depth 4 | Set-Content -Path $slnfPath -Encoding UTF8
    Write-Ok "Solution filter üretildi: $slnfPath ($($include.Count) proje)"
}

function Action-Help {
    Write-Host @"
Toltek build script

Kullanım: build.ps1 <action> [seçenekler]

  build               Incremental derleme (clean yok, pack yok — hızlı yol)
  pack                build + sadece değişen projeleri paketle + feed'e kopyala
                      + global cache'ten sadece onları düşür
  pack -All           Tüm projeleri paketle
  test                Ayarlardaki test projelerini çalıştır (önce testSolution derlenir — ref-flip)
                      Birim lane: build.ps1 test   (settings.testFilter = UnitTests/Importing/Security)
                      Entegrasyon lane: build.ps1 test -Filter "FullyQualifiedName!~.UnitTests.&FullyQualifiedName!~.Importing.&FullyQualifiedName!~.Security.&FullyQualifiedName!~.Legacy."
                      Tümü: build.ps1 test -Filter all
  clean               dotnet clean
  nuke                Tüm nupkg + cache girdilerini sil + clean (nükleer seçenek)
  restore             dotnet restore
  sln-all             Master solution üret (slnf ebeveyni)
  slnf -Module X      Modül için slnf üret (bağımlılıklar + test + sample)

Seçenekler:
  -Solution <yol>        settings.json'daki solution yerine kullan
  -Configuration <cfg>   Debug (varsayılan) / Release
  -DryRun                Ne yapılacağını göster, yapma
Ortam değişkenleri:
  TOLTEK_PACKAGE_FOLDER  Yerel feed klasörünü geçersiz kılar
"@
}

# ---------------------------------------------------------------------------
# Dispatch
# ---------------------------------------------------------------------------
Write-Info "Toltek Build | action=$Action | cfg=$Configuration | ci=$script:IsCi"
Write-Log  "solution=$script:SolutionPath"
Write-Log  "feed=$script:PackageFolder"

switch ($Action) {
    'build'   { Action-Build }
    'pack'    { Action-Pack }
    'test'    { Action-Test }
    'clean'   { Action-Clean }
    'nuke'    { Action-Nuke }
    'restore' { Action-Restore }
    'sln-all' { Action-SlnAll }
    'slnf'    { Action-Slnf }
    default   { Action-Help }
}

Write-Ok ("Bitti ({0:n1} sn)" -f ((Get-Date) - $script:StartedAt).TotalSeconds)
