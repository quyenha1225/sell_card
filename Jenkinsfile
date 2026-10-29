
pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
        timeout(time: 45, unit: 'MINUTES')
    }

    environment {
        // GITHUB
        REPO_URL = 'https://github.com/quyenha1225/sell_card.git'
        BUILD_BRANCH = 'quyen/admin'
        NUGET_PACKAGES = 'C:\\JenkinsNuGetCache'
        // ASP.NET MVC 5 - .NET FRAMEWORK
        SOLUTION = 'BBEcom.sln'
        PROJECT = 'BBEcom.csproj'
        TEST_DLL = 'BBEcom.Tests\\bin\\Release\\net472\\BBEcom.Tests.dll'

        // BUILD TOOLS
        NUGET = 'C:\\Tools\\NuGet\\nuget.exe'

        // PUBLISH OUTPUT
        PUBLISH_DIR = 'C:\\JenkinsDeploy\\BBEcom'

        // IIS APPLICATION
        IIS_DIR = 'C:\\inetpub\\wwwroot\\BBEcom'
        WEBSITE_URL = 'http://localhost/BBEcom/'
    }

    stages {

        // ========================================
        // STAGE 1 - CHECKOUT
        // ========================================
        stage('Checkout') {
            steps {
                echo '===== CHECKOUT GITHUB ====='

                // Remove stale workspace files
                deleteDir()

                git branch: "${env.BUILD_BRANCH}",
                    url: "${env.REPO_URL}"

                bat '''
                @echo off
                echo ===== CURRENT COMMIT =====
                git rev-parse --short HEAD

                if not exist "%SOLUTION%" (
                    echo ERROR: Solution not found
                    exit /b 1
                )

                if not exist "%PROJECT%" (
                    echo ERROR: Project not found
                    exit /b 1
                )
                '''
            }
        }

        // ========================================
        // STAGE 2 - FIND BUILD TOOLS
        // ========================================
        stage('Find Build Tools') {
            steps {
                echo '===== FIND BUILD TOOLS ====='

                script {
                    env.MSBUILD_EXE = powershell(
                        returnStdout: true,
                        script: '''
                        $ErrorActionPreference = 'Stop'

                        $roots = @(
                            (Join-Path $env:ProgramFiles 'Microsoft Visual Studio'),
                            (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio')
                        )

                        $candidates = foreach ($root in $roots) {
                            if (Test-Path $root) {
                                Get-ChildItem -Path $root `
                                    -Filter MSBuild.exe `
                                    -File -Recurse `
                                    -ErrorAction SilentlyContinue |
                                Where-Object {
                                    $_.FullName.EndsWith(
                                        '\\MSBuild\\Current\\Bin\\MSBuild.exe',
                                        [StringComparison]::OrdinalIgnoreCase
                                    )
                                }
                            }
                        }

                        $found = $candidates |
                            Sort-Object FullName -Descending |
                            Select-Object -First 1

                        if (-not $found) {
                            throw 'MSBuild.exe not found.'
                        }

                        Write-Output $found.FullName
                        '''
                    ).trim()

                    env.MSBUILD_DIR = env.MSBUILD_EXE.substring(
                        0,
                        env.MSBUILD_EXE.lastIndexOf('\\')
                    )

                    echo "MSBuild: ${env.MSBUILD_EXE}"
                }

                bat '''
                @echo off
                if not exist "%NUGET%" (
                    echo ERROR: NuGet.exe not found
                    exit /b 1
                )

                if not exist "%MSBUILD_EXE%" (
                    echo ERROR: MSBuild.exe not found
                    exit /b 1
                )

                echo BUILD TOOLS READY
                '''
            }
        }

        // ========================================
        // STAGE 3 - RESTORE
        // ========================================
        
stage('Restore') {
    steps {
        echo '===== RESTORE NUGET PACKAGES ====='

        bat '''
        @echo off

        if not exist "%NUGET_PACKAGES%" (
            mkdir "%NUGET_PACKAGES%"
        )

        echo Restoring legacy ASP.NET MVC packages...

        "%NUGET%" restore "%SOLUTION%" -NonInteractive -MSBuildPath "%MSBUILD_DIR%"

        if errorlevel 1 (
            echo ERROR: SOLUTION RESTORE FAILED
            exit /b 1
        )

        echo Restoring MSTest project packages...

        dotnet restore "BBEcom.Tests\\BBEcom.Tests.csproj" --packages "%NUGET_PACKAGES%" --force --no-http-cache

        if errorlevel 1 (
            echo ERROR: TEST PACKAGE RESTORE FAILED
            exit /b 1
        )

        echo RESTORE SUCCESS
        '''
    }
}


        // ========================================
        // STAGE 4 - BUILD
        // ========================================
        stage('Build') {
            steps {
                echo '===== BUILD RELEASE ====='

                bat '''
                @echo off

                "%MSBUILD_EXE%" "%SOLUTION%" /t:Build /p:Configuration=Release /p:Platform="Any CPU" /m /nologo /verbosity:minimal

                if errorlevel 1 (
                    echo ERROR: BUILD FAILED
                    exit /b 1
                )

                echo BUILD SUCCESS
                '''
            }
        }

        // ========================================
        // STAGE 5 - AUTOMATED TEST
        // ========================================
        stage('Test') {
            steps {
                echo '===== AUTOMATED UNIT TEST ====='

                script {
                    env.VSTEST_EXE = powershell(
                        returnStdout: true,
                        script: '''
                        $ErrorActionPreference = 'Stop'

                        $roots = @(
                            (Join-Path $env:ProgramFiles 'Microsoft Visual Studio'),
                            (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio')
                        )

                        $candidates = foreach ($root in $roots) {
                            if (Test-Path $root) {
                                Get-ChildItem -Path $root `
                                    -Filter vstest.console.exe `
                                    -File -Recurse `
                                    -ErrorAction SilentlyContinue
                            }
                        }

                        $found = $candidates |
                            Sort-Object FullName -Descending |
                            Select-Object -First 1

                        if (-not $found) {
                            throw 'VSTest not found.'
                        }

                        Write-Output $found.FullName
                        '''
                    ).trim()

                    echo "VSTest: ${env.VSTEST_EXE}"
                }

                bat '''
                @echo off

                if not exist "%TEST_DLL%" (
                    echo ERROR: TEST DLL NOT FOUND
                    echo Expected: %TEST_DLL%
                    exit /b 1
                )

                if not exist "TestResults" mkdir "TestResults"

                "%VSTEST_EXE%" "%TEST_DLL%" "/Logger:trx;LogFileName=%WORKSPACE%\\TestResults\\SellCard.trx"

                if errorlevel 1 (
                    echo ERROR: UNIT TEST FAILED
                    exit /b 1
                )

                if not exist "TestResults\\SellCard.trx" (
                    echo ERROR: TEST REPORT NOT FOUND
                    exit /b 1
                )

                echo ALL TESTS PASSED
                '''
            }
        }

        // ========================================
        // STAGE 6 - PUBLISH
        // ========================================
        stage('Publish') {
            steps {
                echo '===== PUBLISH ASP.NET MVC WEBSITE ====='

                bat '''
                @echo off

                echo Cleaning old publish output...

                if exist "%PUBLISH_DIR%" (
                    rmdir /s /q "%PUBLISH_DIR%"

                    if exist "%PUBLISH_DIR%" (
                        echo ERROR: CANNOT CLEAN PUBLISH DIRECTORY
                        exit /b 1
                    )
                )

                mkdir "%PUBLISH_DIR%"

                if errorlevel 1 (
                    echo ERROR: CANNOT CREATE PUBLISH DIRECTORY
                    exit /b 1
                )

                echo Publishing to %PUBLISH_DIR%

                "%MSBUILD_EXE%" "%PROJECT%" /t:WebPublish /p:Configuration=Release /p:Platform=AnyCPU /p:WebPublishMethod=FileSystem /p:PublishProvider=FileSystem /p:publishUrl="%PUBLISH_DIR%" /p:DeleteExistingFiles=true /nologo /verbosity:minimal

                if errorlevel 1 (
                    echo ERROR: MSBUILD PUBLISH FAILED
                    exit /b 1
                )

                echo Checking published files...

                if not exist "%PUBLISH_DIR%\\Web.config" (
                    echo ERROR: Web.config missing from publish output
                    exit /b 1
                )

                if not exist "%PUBLISH_DIR%\\bin\\BBEcom.dll" (
                    echo ERROR: BBEcom.dll missing from publish output
                    exit /b 1
                )

                if not exist "%PUBLISH_DIR%\\Global.asax" (
                    echo ERROR: Global.asax missing
                    exit /b 1
                )

                echo PUBLISH SUCCESS
                '''
            }
        }

        // ========================================
        // STAGE 7 - DEPLOY TO IIS
        // ========================================
        stage('Deploy') {
            steps {
                echo '===== DEPLOY WEBSITE TO IIS ====='

                powershell '''
                    $ErrorActionPreference = 'Stop'

                    $source = $env:PUBLISH_DIR
                    $target = $env:IIS_DIR

                    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
                        throw "Publish directory not found: $source"
                    }

                    if (-not (Test-Path -LiteralPath $target -PathType Container)) {
                        throw "IIS directory not found: $target"
                    }

                    if (-not (Test-Path -LiteralPath (Join-Path $source 'Web.config'))) {
                        throw 'Published Web.config not found'
                    }

                    $offline = Join-Path $target 'app_offline.htm'

                    Write-Host 'Enabling temporary maintenance mode...'

                    Set-Content -LiteralPath $offline `
                        -Value 'Website is being updated.' `
                        -Encoding ASCII

                    try {
                        Write-Host 'Copying published website to IIS...'

                        & robocopy.exe $source $target /E /R:2 /W:2 /NP /NFL /NDL /XF app_offline.htm

                        $copyCode = $LASTEXITCODE

                        Write-Host "Robocopy exit code: $copyCode"

                        if ($copyCode -ge 8) {
                            throw "Robocopy failed with code $copyCode"

                        }
                        $global:LASTEXITCODE = 0
                        if (-not (Test-Path -LiteralPath (Join-Path $target 'Web.config'))) {
                            throw 'Web.config missing from IIS folder'
                        }

                        if (-not (Test-Path -LiteralPath (Join-Path $target 'bin\\BBEcom.dll'))) {
                            throw 'BBEcom.dll missing from IIS folder'
                        }

                        Write-Host 'DEPLOY SUCCESS'
                    }
                    finally {
                        if (Test-Path -LiteralPath $offline) {
                            Remove-Item -LiteralPath $offline -Force
                        }

                        Write-Host 'Maintenance mode removed.'
                    }
                '''
            }
        }

        // ========================================
        // STAGE 8 - VERIFY HTTP
        // ========================================
        stage('Verify') {
            steps {
                echo '===== VERIFY DEPLOYED WEBSITE ====='

                powershell '''
                    $ErrorActionPreference = 'Stop'

                    $url = $env:WEBSITE_URL
                    $maxAttempts = 5

                    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {

                        Write-Host "Checking website ($attempt/$maxAttempts): $url"

                        try {
                            $response = Invoke-WebRequest `
                                -Uri $url `
                                -UseBasicParsing `
                                -TimeoutSec 20 `
                                -MaximumRedirection 5

                            if ($response.StatusCode -ne 200) {
                                throw "Unexpected HTTP status: $($response.StatusCode)"
                            }

                            Write-Host 'VERIFY SUCCESS - HTTP 200'
                            Write-Host "Final URL: $($response.BaseResponse.ResponseUri)"
                            exit 0
                        }
                        catch {
                            Write-Host "Verification attempt failed: $_"

                            if ($attempt -eq $maxAttempts) {
                                throw "VERIFY FAILED after $maxAttempts attempts"
                            }

                            Start-Sleep -Seconds 3
                        }
                    }
                '''
            }
        }
    }

    // ========================================
    // PIPELINE RESULTS
    // ========================================
    post {
        success {
            echo '========================================'
            echo 'CI/CD PIPELINE SUCCESS!'
            echo 'BUILD: SUCCESS'
            echo 'TEST: SUCCESS'
            echo 'PUBLISH: SUCCESS'
            echo 'DEPLOY: SUCCESS'
            echo 'VERIFY: HTTP 200'
            echo '========================================'
        }

        failure {
            echo '========================================'
            echo 'CI/CD PIPELINE FAILED!'
            echo 'Check the failed stage in Console Output.'
            echo '========================================'
        }

        always {
            archiveArtifacts(
                artifacts: 'TestResults/**/*.trx',
                allowEmptyArchive: true
            )

            echo 'Pipeline completed.'
        }
    }
}
