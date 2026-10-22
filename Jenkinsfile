pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
        timestamps()
        disableConcurrentBuilds()
    }

    environment {
        REPO_URL = 'https://github.com/quyenha1225/sell_card.git'
        BUILD_BRANCH = 'quyen/admin'
        SOLUTION = 'BBEcom.sln'
        PROJECT = 'BBEcom.csproj'
        NUGET = 'C:\\Tools\\NuGet\\nuget.exe'
        NUGET_PACKAGES = 'C:\\JenkinsNuGetCache'
        PUBLISH_DIR = 'C:\\JenkinsDeploy\\BBEcom'
        IIS_DIR = 'C:\\inetpub\\wwwroot\\BBEcom'
        WEBSITE_URL = 'http://localhost/BBEcom/'
    }

    stages {
        stage('Checkout') {
            steps {
                echo '===== CHECKOUT GITHUB ====='
                git branch: "${env.BUILD_BRANCH}", url: "${env.REPO_URL}"
            }
        }

        stage('Find Build Tools') {
            steps {
                script {
                    env.MSBUILD_EXE = powershell(returnStdout: true, script: '''
                        $ErrorActionPreference = 'Stop'
                        $roots = @(
                            (Join-Path $env:ProgramFiles 'Microsoft Visual Studio'),
                            (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio')
                        )
                        $candidates = foreach ($root in $roots) {
                            if (Test-Path $root) {
                                Get-ChildItem -Path $root -Filter MSBuild.exe -File -Recurse -ErrorAction SilentlyContinue |
                                    Where-Object { $_.FullName.EndsWith('\\MSBuild\\Current\\Bin\\MSBuild.exe', [StringComparison]::OrdinalIgnoreCase) }
                            }
                        }
                        $found = $candidates | Sort-Object FullName -Descending | Select-Object -First 1
                        if (-not $found) { throw 'MSBuild.exe not found. Install Visual Studio Build Tools.' }
                        Write-Output $found.FullName
                    ''').trim()
                    env.MSBUILD_DIR = env.MSBUILD_EXE.substring(0, env.MSBUILD_EXE.lastIndexOf('\\'))
                    echo "MSBuild: ${env.MSBUILD_EXE}"
                }
                bat '''
                @echo off
                if not exist "%NUGET%" (echo ERROR: NuGet.exe not found & exit /b 1)
                if not exist "%MSBUILD_EXE%" (echo ERROR: MSBuild.exe not found & exit /b 1)
                '''
            }
        }

        stage('Restore') {
            steps {
                echo '===== RESTORE NUGET ====='
                bat '''
                @echo off
                if not exist "%NUGET_PACKAGES%" mkdir "%NUGET_PACKAGES%"
                "%NUGET%" restore "%SOLUTION%" -NonInteractive -MSBuildPath "%MSBUILD_DIR%"
                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Build') {
            steps {
                echo '===== BUILD RELEASE ====='
                bat '''
                @echo off
                "%MSBUILD_EXE%" "%SOLUTION%" /p:Configuration=Release /p:Platform="Any CPU" /m
                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Test') {
            steps {
                echo '===== AUTOMATED TEST ====='
                script {
                    env.VSTEST_EXE = powershell(returnStdout: true, script: '''
                        $ErrorActionPreference = 'Stop'
                        $roots = @(
                            (Join-Path $env:ProgramFiles 'Microsoft Visual Studio'),
                            (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio')
                        )
                        $candidates = foreach ($root in $roots) {
                            if (Test-Path $root) {
                                Get-ChildItem -Path $root -Filter vstest.console.exe -File -Recurse -ErrorAction SilentlyContinue
                            }
                        }
                        $found = $candidates | Sort-Object FullName -Descending | Select-Object -First 1
                        if (-not $found) { throw 'vstest.console.exe not found.' }
                        Write-Output $found.FullName
                    ''').trim()
                    echo "VSTest: ${env.VSTEST_EXE}"
                }
                bat '''
                @echo off
                set "TEST_DLL=BBEcom.Tests\\bin\\Release\\net472\\BBEcom.Tests.dll"
                if not exist "%TEST_DLL%" (echo ERROR: Test DLL not found: %TEST_DLL% & exit /b 1)
                "%VSTEST_EXE%" "%TEST_DLL%"
                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Publish') {
            steps {
                echo '===== PUBLISH WEBSITE ====='
                bat '''
                @echo off
                if not exist "%PUBLISH_DIR%" mkdir "%PUBLISH_DIR%"
                "%MSBUILD_EXE%" "%PROJECT%" /p:Configuration=Release /p:Platform=AnyCPU /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:PublishUrl="%PUBLISH_DIR%"
                if errorlevel 1 exit /b 1
                if not exist "%PUBLISH_DIR%\\Web.config" (echo ERROR: Web.config missing from publish directory & exit /b 1)
                '''
            }
        }

        stage('Deploy') {
            steps {
                echo '===== DEPLOY TO IIS TEST SITE ====='
                bat '''
                @echo off
                if not exist "%IIS_DIR%" (echo ERROR: IIS test folder does not exist: %IIS_DIR% & exit /b 1)
                if not exist "%PUBLISH_DIR%\\Web.config" (echo ERROR: No valid published website & exit /b 1)
                robocopy "%PUBLISH_DIR%" "%IIS_DIR%" /E /R:2 /W:2
                if errorlevel 8 exit /b 1
                exit /b 0
                '''
            }
        }

        stage('Verify') {
            steps {
                echo '===== VERIFY WEBSITE ====='
                powershell '''
                    $ErrorActionPreference = 'Stop'
                    try {
                        $response = Invoke-WebRequest -Uri $env:WEBSITE_URL -UseBasicParsing -TimeoutSec 30
                        if ($response.StatusCode -ne 200) { throw "HTTP $($response.StatusCode)" }
                        Write-Host 'VERIFY SUCCESS - HTTP 200'
                    } catch {
                        Write-Error "VERIFY FAILED: $_"
                        exit 1
                    }
                '''
            }
        }
    }

    post {
        success { echo 'CI/CD PIPELINE SUCCESS!' }
        failure { echo 'CI/CD PIPELINE FAILED! Check Console Output.' }
        always { echo 'Pipeline completed.' }
    }
}
