if (-Not (Test-Path -Path ".git/hooks")) {
    Write-Host ".git/hooks 디렉토리가 존재하지 않습니다."
    exit 1
}

$preCommitHook = @'
#!/bin/bash

current_branch=$(git branch --show-current)

if [[ "$current_branch" == "main" || "$current_branch" == "develop" ]]; then
    echo "경고: '$current_branch' 브랜치로 커밋하려고 합니다."
    read -p "정말로 커밋하시겠습니까? (y/n): " choice
    if [[ "$choice" != "y" ]]; then
        echo "커밋이 취소되었습니다."
        echo "깃 플로우 -> 새 기능 시작으로 커밋하세요."
        exit 1
    fi
fi
'@

$prePushHook = @'
#!/bin/bash

current_branch=$(git branch --show-current)

if [[ "$current_branch" == "main" || "$current_branch" == "develop" ]]; then
    echo "경고: '$current_branch' 브랜치로 푸시하려고 합니다."
    read -p "정말로 푸시하시겠습니까? (y/n): " choice
    if [[ "$choice" != "y" ]]; then
        echo "푸시가 취소되었습니다."
        echo "깃 플로우 -> 새 기능 시작으로 푸시하세요."
        exit 1
    fi
fi
'@

$preCommitHook | Out-File -FilePath ".git/hooks/pre-commit" -Encoding Ascii
$preCommitHookBytes = [System.Text.Encoding]::UTF8.GetBytes($preCommitHook)
[System.IO.File]::WriteAllBytes(".git/hooks/pre-commit", $preCommitHookBytes)

$prePushHook | Out-File -FilePath ".git/hooks/pre-push" -Encoding Ascii
$prePushHookBytes = [System.Text.Encoding]::UTF8.GetBytes($prePushHook)
[System.IO.File]::WriteAllBytes(".git/hooks/pre-push", $prePushHookBytes)

Write-Host "Git 훅 설정이 완료되었습니다."

Write-Host "아무 키나 눌러 종료하세요..."
[void][System.Console]::ReadKey($true)