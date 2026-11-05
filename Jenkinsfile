
pipeline {
    agent {
        node {
            // This installation builds and deploys on its local Windows controller.
            label 'built-in'
            // Each build owns a fresh directory; locked reports from older builds
            // must not prevent checkout or cause stale test results to be reused.
            customWorkspace "sellcard-runs/${env.BUILD_TAG}"
        }
    }

    options {
        skipDefaultCheckout(true)
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
        timeout(time: 26, unit: 'HOURS')
        skipStagesAfterUnstable()
    }

    parameters {
        booleanParam(name: 'DEPLOY_IIS', defaultValue: false, description: 'Deploy the tested release to IIS after approval. Leave off for CI only.')
        booleanParam(name: 'BUILD_DOCKER', defaultValue: false, description: 'Also build a Windows Docker image; does not replace the running container.')
    }

    environment {
        // GITHUB
        BUILD_BRANCH = 'quyen/admin'
        NUGET_PACKAGES = 'C:\\JenkinsNuGetCache'
        // ASP.NET MVC 5 - .NET FRAMEWORK
        SOLUTION = 'BBEcom.sln'
        PROJECT = 'BBEcom.csproj'
        TEST_DLL = 'BBEcom.Tests\\bin\\Release\\net472\\BBEcom.Tests.dll'

        // BUILD TOOLS
        NUGET = 'C:\\Tools\\NuGet\\nuget.exe'

        // PUBLISH OUTPUT
        PUBLISH_DIR = "${WORKSPACE}\\release\\site"

        // IIS APPLICATION
        IIS_DIR = 'C:\\inetpub\\wwwroot\\BBEcom'
        WEBSITE_URL = 'http://localhost/BBEcom/Home/Index'
    }

    stages {

        // ========================================
        // STAGE 1 - CHECKOUT
        // ========================================
        stage('Checkout') {
            options { timeout(time: 12, unit: 'MINUTES') }
            steps {
                echo '===== CHECKOUT GITHUB ====='

                echo "Isolated build workspace: ${env.WORKSPACE}"

                script {
                    // Keep Jenkins' revision chooser/extensions so the selected
                    // commit is preserved, while avoiding full Git history/tags.
                    def remotes = scm.userRemoteConfigs
                    def releaseSpecs = [env.BUILD_BRANCH, '*/' + env.BUILD_BRANCH,
                        'origin/' + env.BUILD_BRANCH, 'refs/heads/' + env.BUILD_BRANCH]
                    if (!env.CHANGE_ID && !env.BRANCH_NAME && scm.branches.size() == 1 &&
                        releaseSpecs.contains(scm.branches[0].name)) {
                        remotes = scm.userRemoteConfigs.collect { remote ->
                            def remoteName = remote.name ?: 'origin'
                            [url: remote.url, credentialsId: remote.credentialsId,
                             name: remoteName,
                             refspec: "+refs/heads/${env.BUILD_BRANCH}:refs/remotes/${remoteName}/${env.BUILD_BRANCH}".toString()]
                        }
                    }
                    def checkoutInfo = checkout([
                        $class: 'GitSCM',
                        branches: scm.branches,
                        userRemoteConfigs: remotes,
                        extensions: scm.extensions + [[
                            $class: 'CloneOption',
                            shallow: true, depth: 20, noTags: true,
                            honorRefspec: true, timeout: 10
                        ]]
                    ])
                    env.SCM_BRANCH = checkoutInfo.GIT_BRANCH ?: ''
                    env.RELEASE_COMMIT = bat(returnStdout: true, script: '@git rev-parse HEAD').trim()
                    echo "SCM checkout branch: ${env.SCM_BRANCH ?: '(not reported)'}; multibranch: ${env.BRANCH_NAME ?: '(not set)'}"
                    currentBuild.description = "Commit ${env.RELEASE_COMMIT.take(12)}; ${params.DEPLOY_IIS ? 'IIS deployment requested' : 'CI only'}"
                }

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
            options { timeout(time: 5, unit: 'MINUTES') }
            steps {
                echo '===== FIND BUILD TOOLS ====='
                powershell '''
                    $ErrorActionPreference = 'Stop'
                    if ($env:OS -ne 'Windows_NT') { throw 'A Windows build agent is required.' }
                    Get-Command dotnet.exe -ErrorAction Stop | Out-Null
                    if (-not (Test-Path -LiteralPath $env:NUGET)) { throw 'Install NuGet at the configured NUGET path.' }
                '''

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
            options { timeout(time: 15, unit: 'MINUTES') }
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

                dotnet restore "BBEcom.Tests\\BBEcom.Tests.csproj" --packages "%NUGET_PACKAGES%"

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
            options { timeout(time: 20, unit: 'MINUTES') }
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
            options { timeout(time: 15, unit: 'MINUTES') }
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

                script {
                    int testExit = bat(returnStatus: true, script: '''
                        @echo off
                        if not exist "%TEST_DLL%" exit /b 1
                        if not exist "TestResults" mkdir "TestResults"
                        "%VSTEST_EXE%" "%TEST_DLL%" /ResultsDirectory:TestResults "/Logger:trx;LogFileName=SellCard.trx"
                        exit /b %ERRORLEVEL%
                    ''')
                    int reportExit = powershell(returnStatus: true, script: '& .\\scripts\\convert-test-results.ps1')
                    if (fileExists('TestResults/junit.xml')) {
                        junit testResults: 'TestResults/junit.xml', allowEmptyResults: false
                    }
                    if (testExit != 0 || reportExit != 0) {
                        error('Tests failed, were skipped, or no valid test report was produced. Deployment blocked.')
                    }
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'TestResults/**/*.trx,TestResults/junit.xml', allowEmptyArchive: true
                }
            }
        }

        // ========================================
        // STAGE 6 - PUBLISH
        // ========================================
        stage('Publish') {
            options { timeout(time: 10, unit: 'MINUTES') }
            steps {
                echo '===== PUBLISH ASP.NET MVC WEBSITE ====='

                bat '''
                @echo off

                if not exist "%PUBLISH_DIR%" mkdir "%PUBLISH_DIR%"

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

        stage('Release artifacts') {
            steps {
                powershell '''
                    $ErrorActionPreference = 'Stop'
                    $dll = Join-Path $env:PUBLISH_DIR 'bin\\BBEcom.dll'
                    $hash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
                    @("Commit: $env:RELEASE_COMMIT", "Build: $env:BUILD_URL", "BBEcom.dll SHA256: $hash") |
                        Set-Content -LiteralPath 'release-info.txt' -Encoding UTF8
                '''
                archiveArtifacts artifacts: 'release/site/bin/BBEcom.dll,release-info.txt', fingerprint: true, allowEmptyArchive: false
            }
        }

        stage('Docker Build') {
            options { timeout(time: 45, unit: 'MINUTES') }
            when { expression { params.BUILD_DOCKER } }
            steps {
                powershell '''
                    & "$env:WORKSPACE\\scripts\\build-docker.ps1" -PublishDir $env:PUBLISH_DIR -ImageTag "sellcard:ci-$env:BUILD_NUMBER"
                    if ($LASTEXITCODE -ne 0) { throw 'Docker build failed.' }
                '''
            }
        }

        stage('Release eligibility') {
            when { expression { params.DEPLOY_IIS } }
            steps {
                script {
                    if (env.CHANGE_ID || (env.BRANCH_NAME && env.BRANCH_NAME != env.BUILD_BRANCH)) {
                        error('Deployment is allowed only from the protected release branch, never from a pull request.')
                    }
                    def actualBranch = env.BRANCH_NAME ?: env.SCM_BRANCH ?: ''
                    for (String prefix : ['refs/remotes/', 'remotes/', 'refs/heads/', 'origin/']) {
                        if (actualBranch.startsWith(prefix)) {
                            actualBranch = actualBranch.substring(prefix.length())
                        }
                    }
                    if (actualBranch != env.BUILD_BRANCH) {
                        error("SCM branch '${actualBranch ?: '(not reported)'}' must be '${env.BUILD_BRANCH}'. Configure Branch Specifier as */${env.BUILD_BRANCH}.")
                    }
                    echo "Release branch accepted: ${actualBranch}; checking the release commit next."
                }
                bat '''
                    @echo off
                    git fetch origin "%BUILD_BRANCH%"
                    if errorlevel 1 exit /b 1
                    git rev-parse --verify FETCH_HEAD
                    if errorlevel 1 exit /b 1
                    for /f %%i in ('git rev-parse FETCH_HEAD') do if not "%%i"=="%RELEASE_COMMIT%" exit /b 1
                '''
                powershell '& .\\scripts\\test-iis-readiness.ps1'
            }
        }

        // ========================================
        // STAGE 7 - MANUAL RELEASE APPROVAL
        // A merged PR can trigger CI automatically; deployment awaits approval.
        // ========================================
        stage('Approval') {
            when { expression { params.DEPLOY_IIS } }
            steps {
                timeout(time: 24, unit: 'HOURS') {
                    script {
                        currentBuild.description = "Commit ${env.RELEASE_COMMIT.take(12)}; waiting for IIS approval"
                        env.APPROVED_BY = input message: "Deploy commit ${env.RELEASE_COMMIT} to ${env.WEBSITE_URL}? Review Test Result and Artifacts first.",
                            ok: 'Approve & Deploy', submitter: 'quyenha2512', submitterParameter: 'APPROVED_BY'
                        env.APPROVED_AT = powershell(returnStdout: true, script: '[DateTime]::UtcNow.ToString("o")').trim()
                        currentBuild.description = "Commit ${env.RELEASE_COMMIT.take(12)}; approved by ${env.APPROVED_BY}"
                        writeFile file: 'release-approval.txt', text: "Commit: ${env.RELEASE_COMMIT}\nApproved by: ${env.APPROVED_BY}\nApproved at (UTC): ${env.APPROVED_AT}\nBuild: ${env.BUILD_URL}\n"
                    }
                }
                archiveArtifacts artifacts: 'release-approval.txt', fingerprint: true
                echo 'DEPLOY APPROVED'
            }
        }

        stage('Deploy and Verify IIS') {
            when { expression { params.DEPLOY_IIS } }
            options { timeout(time: 15, unit: 'MINUTES') }
            steps {
                powershell '& .\\scripts\\invoke-iis-deployment.ps1'
                script {
                    env.IIS_DEPLOYED = 'true'
                    currentBuild.description = "Commit ${env.RELEASE_COMMIT.take(12)}; IIS verified; approved by ${env.APPROVED_BY}"
                }
            }
        }
    }

    // ========================================
    // PIPELINE RESULTS
    // ========================================
    post {
        success {
            echo '========================================'
            script {
                if (env.IIS_DEPLOYED == 'true') {
                    echo "CI/CD SUCCESS: IIS deployment verified by HTTP 200. Commit: ${env.RELEASE_COMMIT}."
                } else {
                    echo 'CI SUCCESS ONLY: IIS was not deployed. To request deployment, run with DEPLOY_IIS=true and approve the release.'
                }
            }
            echo '========================================'
        }

        aborted {
            echo 'Pipeline aborted or approval timed out. Check deployment stage if it had already started.'
        }

        failure {
            echo '========================================'
            echo 'CI/CD PIPELINE FAILED!'
            echo 'Check the failed stage in Console Output.'
            echo '========================================'
        }

        always {
            archiveArtifacts(
                artifacts: 'TestResults/**/*.trx,TestResults/junit.xml,release-approval.txt',
                fingerprint: true,
                allowEmptyArchive: true
            )

            echo 'Pipeline completed.'
        }

        cleanup {
            script {
                try {
                    deleteDir()
                } catch (Exception cleanupError) {
                    echo "Workspace cleanup could not finish: ${env.WORKSPACE}. Artifacts already archived remain available. Inspect file locks and permissions before removing this directory manually."
                }
            }
        }
    }
}
