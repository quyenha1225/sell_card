
pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
        timestamps()
    }

    environment {
        REPO_URL = 'https://github.com/quyenha1225/sell_card.git'
        BRANCH_NAME_BUILD = 'quyen/admin'

        SOLUTION = 'BBEcom.sln'
        PROJECT = 'BBEcom.csproj'

        NUGET = 'C:\\Tools\\NuGet\\nuget.exe'
        MSBUILD = 'C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\MSBuild\\Current\\Bin\\MSBuild.exe'

        PUBLISH_DIR = 'C:\\JenkinsDeploy\\BBEcom'
        IIS_DIR = 'C:\\inetpub\\wwwroot\\BBEcom'
        WEBSITE_URL = 'http://localhost/BBEcom/'
    }

    stages {

        stage('Checkout') {
            steps {
                echo '===== CHECKOUT GITHUB ====='

                git branch: "${env.BRANCH_NAME_BUILD}",
                    url: "${env.REPO_URL}"
            }
        }

        stage('Restore') {
            steps {
                echo '===== RESTORE NUGET ====='

                bat '"%NUGET%" restore "%SOLUTION%" -NonInteractive'
            }
        }

        stage('Build') {
            steps {
                echo '===== BUILD RELEASE ====='

                bat '"%MSBUILD%" "%SOLUTION%" /p:Configuration=Release /p:Platform="Any CPU" /m'
            }
        }

        stage('Test') {
            steps {
                echo '===== AUTOMATED TEST ====='

                bat '''
                @echo off
                if not exist "BBEcom.Tests\\BBEcom.Tests.csproj" (
                    echo ERROR: Chua co project BBEcom.Tests.
                    echo Can tao test project de dap ung yeu cau.
                    exit /b 1
                )

                if not exist "C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe" (
                    echo ERROR: Khong tim thay VSTest.
                    exit /b 1
                )

                if not exist "BBEcom.Tests\\bin\\Release\\BBEcom.Tests.dll" (
                    echo ERROR: Khong tim thay test assembly.
                    exit /b 1
                )

                "C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe" "BBEcom.Tests\\bin\\Release\\BBEcom.Tests.dll"

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

                "%MSBUILD%" "%PROJECT%" /p:Configuration=Release /p:WebPublishMethod=FileSystem /p:DeployOnBuild=true /p:PublishUrl="%PUBLISH_DIR%" /p:DeleteExistingFiles=true

                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Deploy') {
            steps {
                echo '===== DEPLOY IIS ====='

                bat '''
                @echo off

                if not exist "%PUBLISH_DIR%\\Web.config" (
                    echo ERROR: Publish output khong hop le.
                    exit /b 1
                )

                if not exist "%IIS_DIR%" mkdir "%IIS_DIR%"

                robocopy "%PUBLISH_DIR%" "%IIS_DIR%" /MIR /R:2 /W:2

                if errorlevel 8 exit /b 1

                exit /b 0
                '''
            }
        }

        stage('Verify') {
            steps {
                echo '===== VERIFY WEBSITE ====='

                powershell '''
                $ErrorActionPreference = "Stop"
                $url = $env:WEBSITE_URL

                try {
                    $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30

                    if ($response.StatusCode -eq 200) {
                        Write-Host "Website hoat dong thanh cong!"
                    }
                    else {
                        throw "HTTP status: $($response.StatusCode)"
                    }
                }
                catch {
                    Write-Error "Verify that bai: $_"
                    exit 1
                }
                '''
            }
        }
    }

    post {
        success {
            echo '==================================='
            echo 'CI/CD PIPELINE SUCCESS!'
            echo '==================================='
        }

        failure {
            echo '==================================='
            echo 'CI/CD PIPELINE FAILED!'
            echo 'Kiem tra Console Output.'
            echo '==================================='
        }

        always {
            echo 'Pipeline completed.'
        }
    }
}
