
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
        MSBUILD = 'C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\MSBuild\\Current\\Bin\\MSBuild.exe'

        // Dung chung NuGet cache cho Restore va Build
        NUGET_PACKAGES = 'C:\\JenkinsNuGetCache'

        // Thu muc publish va IIS rieng cho moi truong thu nghiem
        PUBLISH_DIR = 'C:\\JenkinsDeploy\\BBEcom'
        IIS_DIR = 'C:\\inetpub\\wwwroot\\BBEcom'
        WEBSITE_URL = 'http://localhost/BBEcom/'
    }

    stages {

        stage('Checkout') {
            steps {
                echo '===== CHECKOUT GITHUB ====='

                git branch: "${env.BUILD_BRANCH}",
                    url: "${env.REPO_URL}"
            }
        }

        stage('Restore') {
            steps {
                echo '===== RESTORE NUGET ====='

                bat '''
                @echo off

                if not exist "%NUGET_PACKAGES%" (
                    mkdir "%NUGET_PACKAGES%"
                )

                "%NUGET%" restore "%SOLUTION%" -NonInteractive

                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Build') {
            steps {
                echo '===== BUILD RELEASE ====='

                bat '''
                @echo off

                "%MSBUILD%" "%SOLUTION%" /p:Configuration=Release /p:Platform="Any CPU" /m

                if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Test') {
            steps {
                echo '===== AUTOMATED TEST ====='

                bat '''
                @echo off

                set "TEST_PROJECT=BBEcom.Tests\\BBEcom.Tests.csproj"
                set "TEST_DLL=BBEcom.Tests\\bin\\Release\\net472\\BBEcom.Tests.dll"
                set "VSTEST=C:\\Program Files\\Microsoft Visual Studio\\18\\Insiders\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe"

                if not exist "%TEST_PROJECT%" (
                    echo ERROR: Khong tim thay project kiem thu.
                    exit /b 1
                )

                if not exist "%TEST_DLL%" (
                    echo ERROR: Khong tim thay test DLL.
                    exit /b 1
                )

                if not exist "%VSTEST%" (
                    echo ERROR: Khong tim thay VSTest.
                    exit /b 1
                )

                "%VSTEST%" "%TEST_DLL%"

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

                "%MSBUILD%" "%PROJECT%" /p:Configuration=Release /p:Platform=AnyCPU /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:PublishUrl="%PUBLISH_DIR%"

                if errorlevel 1 exit /b 1

                if not exist "%PUBLISH_DIR%\\Web.config" (
                    echo ERROR: Publish khong tao ra Web.config.
                    exit /b 1
                )
                '''
            }
        }

        stage('Deploy') {
            steps {
                echo '===== DEPLOY TO IIS ====='

                bat '''
                @echo off

                if not exist "%PUBLISH_DIR%\\Web.config" (
                    echo ERROR: Ban publish khong hop le.
                    exit /b 1
                )

                if not exist "%IIS_DIR%" (
                    echo ERROR: Chua co thu muc IIS dich.
                    echo Hay cau hinh website IIS truoc khi deploy.
                    exit /b 1
                )

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
                $ErrorActionPreference = "Stop"

                try {
                    $response = Invoke-WebRequest `
                        -Uri $env:WEBSITE_URL `
                        -UseBasicParsing `
                        -TimeoutSec 30

                    if ($response.StatusCode -ne 200) {
                        throw "HTTP status: $($response.StatusCode)"
                    }

                    Write-Host "VERIFY SUCCESS - HTTP 200"
                }
                catch {
                    Write-Error "VERIFY FAILED: $_"
                    exit 1
                }
                '''
            }
        }
    }

    post {
        success {
            echo '===================================='
            echo 'CI/CD PIPELINE SUCCESS!'
            echo '===================================='
        }

        failure {
            echo '===================================='
            echo 'CI/CD PIPELINE FAILED!'
            echo 'Build hoac Test loi thi khong Deploy.'
            echo 'Kiem tra Console Output.'
            echo '===================================='
        }

        always {
            echo 'Pipeline completed.'
        }
    }
}
